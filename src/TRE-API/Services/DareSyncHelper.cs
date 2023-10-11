using BL.Models.APISimpleTypeReturns;
using BL.Models;
using BL.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TRE_API.Repositories.DbContexts;
using BL.Models.ViewModels;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EasyNetQ.Management.Client.Model;
using static Npgsql.PostgresTypes.PostgresCompositeType;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Sentry;

namespace TRE_API.Services
{
    public class DareSyncHelper : IDareSyncHelper
    {
        public ApplicationDbContext _DbContext { get; set; }
        public IDareClientWithoutTokenHelper _dareclientHelper { get; set; }
        private readonly MinioSettings _minioSettings;
        private readonly IMinioHelper _minioHelper;
        public DareSyncHelper(ApplicationDbContext dbContext, IDareClientWithoutTokenHelper dareClient, MinioSettings minioSettings, IMinioHelper minioHelper)
        {
            _DbContext = dbContext;
            _dareclientHelper = dareClient;
            _minioSettings = minioSettings;
            _minioHelper = minioHelper;

        }

        public async Task<BoolReturn> SyncSubmissionWithTre()
        {
            if (!_dareclientHelper.CheckCredsAreAvailable())
            {
                Log.Error("{Function} Credential not yet entered for synching with Dare", "SyncSubmissionWithTre");
                return new BoolReturn()
                {
                    Result = false
                };
            }
            var subprojs = await _dareclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjectsForTre");
            var dbprojs = _DbContext.Projects.ToList();
            var projectAdds = subprojs.Where(x => !_DbContext.Projects.Any(y => y.SubmissionProjectId == x.Id));
            var projectArchives =
                dbprojs.Where(x => !subprojs.Any(y => y.Id == x.SubmissionProjectId));
            var projectUnarchives =
                dbprojs.Where(x => x.Archived && subprojs.Any(y => y.Id == x.SubmissionProjectId));
            foreach (var project in projectAdds)
            {
                var submission = project.SubmissionBucket.ToLower() + "tre";
                var output = project.OutputBucket.ToLower() + "tre";
                var submissionBucket = await _minioHelper.CreateBucket(_minioSettings, submission);
                if (!submissionBucket)
                {
                    Log.Error("{Function} S3GetListObjects: Failed to create bucket {name}.", "SyncSubmissionWithTre", submission);
                    submission = "";
                }
                var outputBucket = await _minioHelper.CreateBucket(_minioSettings, output);
                if (!outputBucket)
                {
                    Log.Error("{Function} S3GetListObjects: Failed to create bucket {name}.", "SyncSubmissionWithTre", output);
                    output = "";
                }

                _DbContext.Projects.Add(new TreProject()
                {
                    SubmissionProjectId = project.Id,
                    SubmissionProjectName = project.Name,
                    Description = project.ProjectDescription,
                    SubmissionBucketTre = submission,
                    OutputBucketTre = output,

                });
            }

            foreach (var treProject in projectArchives)
            {
                treProject.Archived = true;
                foreach (var treProjectMemberDecision in treProject.MemberDecisions)
                {
                    treProjectMemberDecision.Archived = true;
                }
            }

            foreach (var projectUnarchive in projectUnarchives)
            {
                projectUnarchive.Archived = false;
            }

            await _DbContext.SaveChangesAsync();
            var users = subprojs.SelectMany(x => x.Users).Distinct();
            var dbusers = _DbContext.Users.ToList();
            var userAdds = users.Where(x => !_DbContext.Users.Any(y => y.SubmissionUserId == x.Id));
            var userArchives = dbusers.Where(x => !users.Any(y => y.Id == x.SubmissionUserId));
            var userUnarchives = dbusers.Where(x => x.Archived && users.Any(y => y.Id == x.SubmissionUserId));
            foreach (var user in userAdds)
            {
                _DbContext.Users.Add(new TreUser()
                {
                    SubmissionUserId = user.Id,
                    Username = user.Name,
                    Email = user.Email,
                });
            }

            foreach (var userArchive in userArchives)
            {
                userArchive.Archived = true;
            }

            foreach (var userUnarchive in userUnarchives)
            {
                userUnarchive.Archived = false;
            }
            await _DbContext.SaveChangesAsync();
            var projectUserPairs = subprojs
                .SelectMany(project => project.Users, (project, user) => new
                {
                    ProjectId = project.Id,
                    UserId = user.Id
                }).ToList();
            var dbmembers = _DbContext.MembershipDecisions.ToList();
            var memberAdds = projectUserPairs.Where(x => !_DbContext.MembershipDecisions.Any(y =>
                y.Project.SubmissionProjectId == x.ProjectId && y.User.SubmissionUserId == x.UserId));
            var memberArchives = dbmembers.Where(x =>
                !projectUserPairs.Any(y =>
                    y.ProjectId == x.Project.SubmissionProjectId && y.UserId == x.User.SubmissionUserId));
            var memberUnarchives = dbmembers.Where(x =>
                x.Archived && projectUserPairs.Any(y =>
                    y.ProjectId == x.Project.SubmissionProjectId && y.UserId == x.User.SubmissionUserId));

            foreach (var memberAdd in memberAdds)
            {
                var project = _DbContext.Projects.First(x => x.SubmissionProjectId == memberAdd.ProjectId);
                var user = _DbContext.Users.First(x => x.SubmissionUserId == memberAdd.UserId);
                _DbContext.MembershipDecisions.Add(new TreMembershipDecision()
                {
                    User = user,
                    Project = project
                });
            }

            foreach (var treMembershipDecision in memberArchives)
            {
                treMembershipDecision.Archived = true;
            }

            foreach (var treMembershipDecision in memberUnarchives)
            {
                treMembershipDecision.Archived = false;
            }

            await _DbContext.SaveChangesAsync();
            return new BoolReturn()
            {
                Result = true
            };

        }

        public async Task<ProjectTreDecision> ProcessSubmissionIdList()
        {

            var dbprojs = _DbContext.Projects.ToList();
            var subprojs = await _dareclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjectsForTre");

            //adds every projectid to a list
            List<int> submissionProjectIdList = new List<int>();
            foreach (var projs in dbprojs)
            {
                submissionProjectIdList.Add(projs.SubmissionProjectId);
            }
            //goes through each item in the id list and gets the user name of the person
            //foreach (var item in submissionProjectIdList)
            //{
            //    var submissionLayerProj = subprojs.Where(x => item == x.Id);

            //    var usersName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            //    var submissionTre = _DbContext.User.FirstOrDefault(x => x.AdminUsername.ToLower() == usersName.ToLower());
            //    if (submissionTre == null)
            //    {
            //        return submissionTre == false;
            //    }
            //    return submissionTre;

            //}
            //sees if there is an existing record matching 
            var projTreDecisionProj = _DbContext.ProjectTreDecisions.ToList();

            //var submissionProjects = projTreDecisionProj.Select(x => x.Id).Distinct();

            var submissionProjectsAdds = projTreDecisionProj.Where(x => !_DbContext.ProjectTreDecisions.Any(y => y.SubmissionProj.Id == x.Id));
            var submissionProjectsAdds1 = projTreDecisionProj.Where(x => !_DbContext.ProjectTreDecisions.Any(y => y.Tre.Id == x.Id));

            var treAndProj = submissionProjectsAdds.Intersect(submissionProjectsAdds1);

            foreach (var projects in treAndProj)
            {

                _DbContext.ProjectTreDecisions.Add(new ProjectTreDecision()
                {
                    Id = projects.Id,
                    Decision = projects.Decision
                });
            }

            return null;

        }

        public async Task<ProjectTreDecision> ProcessTreMembershipDecision()
        {
            var subprojs = await _dareclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjectsForTre");
            var dbmembers = _DbContext.MembershipDecisions.ToList();

            //gets the submissionuserid
            List<int> submissionUserIdList = new List<int>();
            foreach (var users in dbmembers)
            {
                submissionUserIdList.Add(users.Id);
            }
            //gets the submissionprojectid
            List<int> submissionProjectId = new List<int>();
            foreach (var projs in dbmembers)
            {
                submissionProjectId.Add(projs.Id);
            }

            var projectUserPairs = subprojs
                 .SelectMany(project => project.Users, (project, user) => new
                 {
                     ProjectId = project.Id,
                     UserId = user.Id
                 }).ToList();
            var memberAdds = projectUserPairs.Where(x => !_DbContext.MembershipDecisions.Any(y =>
                y.Project.SubmissionProjectId == x.ProjectId && y.User.SubmissionUserId == x.UserId));
            var memberArchives = dbmembers.Where(x =>
                !projectUserPairs.Any(y =>
                    y.ProjectId == x.Project.SubmissionProjectId && y.UserId == x.User.SubmissionUserId));
            var memberUnarchives = dbmembers.Where(x =>
                x.Archived && projectUserPairs.Any(y =>
                    y.ProjectId == x.Project.SubmissionProjectId && y.UserId == x.User.SubmissionUserId));

            foreach (var memberAdd in memberAdds)
            {
                var project = _DbContext.Projects.First(x => x.SubmissionProjectId == memberAdd.ProjectId);
                var user = _DbContext.Users.First(x => x.SubmissionUserId == memberAdd.UserId);
                _DbContext.MembershipDecisions.Add(new TreMembershipDecision()
                {
                    User = user,
                    Project = project
                });
            }

            foreach (var treMembershipDecision in memberArchives)
            {
                treMembershipDecision.Archived = true;
            }

            foreach (var treMembershipDecision in memberUnarchives)
            {
                treMembershipDecision.Archived = false;
            }

            await _DbContext.SaveChangesAsync();

            return null;
        }
    }
}
