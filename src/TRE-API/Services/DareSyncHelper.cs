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
        
        private readonly IMinioTreHelper _minioTreHelper;
        private readonly OPASettings _opaSettings;
        private readonly OpaService _opaService;
        public DareSyncHelper(ApplicationDbContext dbContext, IDareClientWithoutTokenHelper dareClient,  IMinioTreHelper minioTreHelper, OPASettings opasettings, OpaService opaservice)
        {
            _DbContext = dbContext;
            _dareclientHelper = dareClient;     
            _minioTreHelper = minioTreHelper;
            _opaSettings = opasettings;
            _opaService = opaservice;
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
                var submissionBucket = await _minioTreHelper.CreateBucket(submission);
                if (!submissionBucket)
                {
                    Log.Error("{Function} S3GetListObjects: Failed to create bucket {name}.", "SyncSubmissionWithTre", submission);
                    submission = "";
                }

                var outputBucket = await _minioTreHelper.CreateBucket(output);
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
                    OutputBucketSub = project.OutputBucket.ToLower()
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
            await SyncProjectDecisions();
            await SyncMembershipDecisions();

            DateTime today = DateTime.Today;
            var treusers = _DbContext.Users.ToList();
            var treprojects = _DbContext.Projects.ToList();
            var trememberships = _DbContext.MembershipDecisions.ToList();
            var resultList = new List<TreProject>();
            foreach (var user in treusers )
            {           
            
                foreach (var membership in trememberships)
                {
                    DateTime membershipExpiryDate = membership.ProjectExpiryDate;

                    DateTime projectExpiryDate = treprojects.FirstOrDefault(p => p.Id == membership.Project.Id)?.ProjectExpiryDate ?? DateTime.MinValue;

                    DateTime selectedExpiryDate = membershipExpiryDate < projectExpiryDate ? membershipExpiryDate : projectExpiryDate;
                    var project = treprojects.FirstOrDefault(p => p.Id == membership.Project.Id);
                    if(project != null)
                    { 
                        if (selectedExpiryDate > today)
                        {
                            project.ProjectExpiryDate = DateTime.Now.AddMinutes(_opaSettings.ExpiryDelayMinutes);
                            resultList.Add(project);
                            bool hasAccess = await _opaService.CheckAccess(user.Username, selectedExpiryDate, resultList);
                            if (hasAccess)
                            {
                                Log.Information("{Function} User Access Allowed", "CheckUserAccess");
                            }
                            else
                            {
                                Log.Information("{Function} User Access Denied", "CheckUserAccess");
                            }
                        }

                    }


                }
            }

            return new BoolReturn()
            {
                Result = true
            };

        }

        public async Task<bool> SyncProjectDecisions()
        {

            var dbprojs = _DbContext.Projects.ToList();            
            var synclist = dbprojs.Select(x => new ProjectTreDecisionsDTO() { ProjectId = x.SubmissionProjectId, Decision = x.Decision }).ToList();
            var result = await _dareclientHelper.CallAPI<List<ProjectTreDecisionsDTO>, BoolReturn>("/api/Project/SyncTreProjectDecisions", synclist);
            


            return result.Result;

        }

        public async Task<bool> SyncMembershipDecisions()
        {

            var dbMemberships = _DbContext.MembershipDecisions.ToList();



            var synclist = dbMemberships.Select(x => new MembershipTreDecisionDTO() { ProjectId = x.Project.SubmissionProjectId, UserId = x.User.SubmissionUserId, Decision = x.Decision }).ToList();
            var result = await _dareclientHelper.CallAPI<List<MembershipTreDecisionDTO>, BoolReturn>("/api/Project/SyncTreMembershipDecisions", synclist);



            return result.Result;

        }
    }
}
