using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JiraCli.Api;
using JiraCli.ViewModels;

namespace JiraCli
{
    public static class JiraApiCommands
    {
        public static async Task<ProjectViewModel> DownloadTimesheetAsync(this RestApiServiceClient client, Period period,
            params string[] users)
        {
            var issuesRequest = BuildRequest(period, users);

            if (users.Length != 1)
                throw new NotImplementedException();

            //downloading all info
            var issuesWithWorklogs = await client.GetIssuesAsync(issuesRequest);

            //mapping models to view models
            var authorViewModel = new AuthorViewModel(users[0]);
            var issuesViewModels = new List<IssueViewModel>();
            foreach (var issue in issuesWithWorklogs)
            {
                var issueViewModel = new IssueViewModel(issue);
                issueViewModel.Worklogs = issue.worklogs.Select(w =>
                {
                    var worklogViewModel = new WorklogViewModel(w)
                    {
                        Issue = issueViewModel,
                        Author = authorViewModel
                    };
                    return worklogViewModel;
                }).ToList();

                issuesViewModels.Add(issueViewModel);
            }

            var workDayViewModels = issuesViewModels.SelectMany(issue => issue.Worklogs)
                .GroupBy(w => w.StarteDate)
                .Select(g => new WorkDayViewModel {Worklogs = g.ToList()})
                .ToList();

            var projectViewModel = new ProjectViewModel(issuesViewModels, workDayViewModels, authorViewModel, period);
            return projectViewModel;
        }

        private static ApiSearchRequest BuildRequest(Period period, string[] users)
        {
            var issuesRequestBuilder = new SearchRequestBuilder()
                .WorklogStartAt(period.StartDate)
                .WorklogEndAt(period.EndDate)
                .IncludeFields("summary", "worklog", "assignee", "status", "key");

            foreach (var user in users)
            {
                issuesRequestBuilder.WorklogAuthor(user);
            }

            return issuesRequestBuilder.Build();
        }
    }
}