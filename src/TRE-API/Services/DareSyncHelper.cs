﻿using BL.Models.APISimpleTypeReturns;
using BL.Models;
using Microsoft.EntityFrameworkCore;
using TRE_API.Repositories.DbContexts;

namespace TRE_API.Services
{
    public class DareSyncHelper: IDareSyncHelper
    {
        public ApplicationDbContext _DbContext { get; set; }
        public IDareClientWithoutTokenHelper _dareclientHelper { get; set; }
        public DareSyncHelper(ApplicationDbContext dbContext, IDareClientWithoutTokenHelper dareClient)
        {
            _DbContext = dbContext;
            _dareclientHelper = dareClient;
        }

        public async Task<BoolReturn> SyncSubmissionWithTre()
        {
            var subprojs = await _dareclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjectsForTre");
            var dbprojs = _DbContext.Projects.ToList();
            var projectAdds = subprojs.Where(x => !_DbContext.Projects.Any(y => y.SubmissionProjectId == x.Id));
            var projectArchives =
                dbprojs.Where(x => !subprojs.Any(y => y.Id == x.SubmissionProjectId));
            var projectUnarchives =
                dbprojs.Where(x => x.Archived && subprojs.Any(y => y.Id == x.SubmissionProjectId));
            foreach (var project in projectAdds)
            {
                _DbContext.Projects.Add(new TreProject()
                {
                    SubmissionProjectId = project.Id,
                    SubmissionProjectName = project.Name,
                    Description = project.ProjectDescription
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
    }
}
