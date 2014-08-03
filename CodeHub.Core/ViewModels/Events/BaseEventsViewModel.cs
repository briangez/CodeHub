using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Source;
using CodeHub.Core.ViewModels.Users;
using GitHubSharp;
using GitHubSharp.Models;
using CodeFramework.Core.Utils;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Events
{
    public abstract class BaseEventsViewModel : BaseViewModel, ILoadableViewModel
    {
        protected readonly IApplicationService ApplicationService;

        public ReactiveList<Tuple<EventModel, EventBlock>> Events { get; private set; }

        public IReactiveCommand<object> GoToRepositoryCommand { get; private set; }

        public IReactiveCommand<object> GoToGistCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public bool ReportRepository
        {
            get;
            private set;
        }

        protected BaseEventsViewModel(IApplicationService applicationService)
        {
            ApplicationService = applicationService;
            Events = new ReactiveList<Tuple<EventModel, EventBlock>>();
            ReportRepository = true;

            GoToRepositoryCommand = ReactiveCommand.Create();
            GoToRepositoryCommand.OfType<EventModel.RepoModel>().Subscribe(x =>
            {
                var repoId = new RepositoryIdentifier(x.Name);
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = repoId.Owner;
                vm.RepositoryName = repoId.Name;
                ShowViewModel(vm);
            });

            GoToGistCommand = ReactiveCommand.Create();
            GoToGistCommand.OfType<EventModel.GistEvent>().Subscribe(x =>
            {
                var vm = CreateViewModel<GistViewModel>();
                vm.Id = x.Gist.Id;
                vm.Gist = x.Gist;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t => 
                this.RequestModel(CreateRequest(0, 100), t as bool?, response =>
                {
                    this.CreateMore(response, m => { }, d => Events.AddRange(CreateDataFromLoad(d)));
                    Events.Reset(CreateDataFromLoad(response.Data));
                }));
        }

		private IEnumerable<Tuple<EventModel, EventBlock>> CreateDataFromLoad(List<EventModel> events)
        {
			var transformedEvents = new List<Tuple<EventModel, EventBlock>>(events.Count);
			foreach (var e in events)
			{
				try
				{
					transformedEvents.Add(new Tuple<EventModel, EventBlock>(e, CreateEventTextBlocks(e)));
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.ToString());
				}
			}
			return transformedEvents;
        }
        
        protected abstract GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage);

        private void GoToCommits(EventModel.RepoModel repoModel, string branch)
        {
            var repoId = new RepositoryIdentifier(repoModel.Name);
            var vm = CreateViewModel<ChangesetsViewModel>();
            vm.RepositoryOwner = repoId.Owner;
            vm.RepositoryName = repoId.Name;
            vm.Branch = branch;
            ShowViewModel(vm);
        }

        private void GoToUser(string username)
        {
            if (string.IsNullOrEmpty(username)) return;
            var vm = CreateViewModel<ProfileViewModel>();
            vm.Username = username;
            ShowViewModel(vm);
        }

        private void GoToBranches(RepositoryIdentifier repoId)
        {
            var vm = CreateViewModel<BranchesAndTagsViewModel>();
            vm.RepositoryOwner = repoId.Owner;
            vm.RepositoryName = repoId.Name;
            vm.SelectedFilter = BranchesAndTagsViewModel.ShowIndex.Branches;
            ShowViewModel(vm);
        }

        private void GoToTags(EventModel.RepoModel eventModel)
        {
            var repoId = new RepositoryIdentifier(eventModel.Name);
            var vm = CreateViewModel<BranchesAndTagsViewModel>();
            vm.RepositoryOwner = repoId.Owner;
            vm.RepositoryName = repoId.Name;
            vm.SelectedFilter = BranchesAndTagsViewModel.ShowIndex.Tags;
            ShowViewModel(vm);
        }

        private void GoToIssue(RepositoryIdentifier repo, long id)
        {
            if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
                return;
            var vm = CreateViewModel<IssueViewModel>();
            vm.RepositoryOwner = repo.Owner;
            vm.RepositoryName = repo.Name;
            vm.IssueId = id;
        }

        private void GoToPullRequest(RepositoryIdentifier repo, long id)
        {
            if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
                return;
            var vm = CreateViewModel<PullRequestViewModel>();
            vm.RepositoryOwner = repo.Owner;
            vm.RepositoryName = repo.Name;
            vm.PullRequestId = id;
            ShowViewModel(vm);
        }

        private void GoToPullRequests(RepositoryIdentifier repo)
        {
            if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
                return;
            var vm = CreateViewModel<PullRequestsViewModel>();
            vm.RepositoryOwner = repo.Owner;
            vm.RepositoryName = repo.Name;
            ShowViewModel(vm);
        }

        private void GoToChangeset(RepositoryIdentifier repo, string sha)
        {
            if (repo == null || string.IsNullOrEmpty(repo.Name) || string.IsNullOrEmpty(repo.Owner))
                return;
            var vm = CreateViewModel<ChangesetViewModel>();
            vm.RepositoryOwner = repo.Owner;
            vm.RepositoryName = repo.Name;
            vm.Node = sha;
            ShowViewModel(vm);
        }

        private EventBlock CreateEventTextBlocks(EventModel eventModel)
        {
            var eventBlock = new EventBlock();
            var repoId = eventModel.Repo != null ? new RepositoryIdentifier(eventModel.Repo.Name) : new RepositoryIdentifier();
            var username = eventModel.Actor != null ? eventModel.Actor.Login : null;

            // Insert the actor
			eventBlock.Header.Add(new AnchorBlock(username, () => GoToUser(username)));

            var commitCommentEvent = eventModel.PayloadObject as EventModel.CommitCommentEvent;
            if (commitCommentEvent != null)
            {
				var node = commitCommentEvent.Comment.CommitId.Substring(0, commitCommentEvent.Comment.CommitId.Length > 6 ? 6 : commitCommentEvent.Comment.CommitId.Length);
				eventBlock.Tapped = () => GoToChangeset(repoId, commitCommentEvent.Comment.CommitId);
                eventBlock.Header.Add(new TextBlock(" commented on commit "));
                eventBlock.Header.Add(new AnchorBlock(node, eventBlock.Tapped));

                if (ReportRepository)
                {
                    eventBlock.Header.Add(new TextBlock(" in "));
                    eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                }

                eventBlock.Body.Add(new TextBlock(commitCommentEvent.Comment.Body));
                return eventBlock;
            }

            var createEvent = eventModel.PayloadObject as EventModel.CreateEvent;
            if (createEvent != null)
            {
                if (createEvent.RefType.Equals("repository"))
                {
                    if (ReportRepository)
                    {
                        eventBlock.Tapped = () => GoToRepositoryCommand.Execute(eventModel.Repo);
						eventBlock.Header.Add(new TextBlock(" created repository "));
                        eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                    }
                    else
						eventBlock.Header.Add(new TextBlock(" created this repository"));
                }
                else if (createEvent.RefType.Equals("branch"))
                {
                    eventBlock.Tapped = () => GoToBranches(repoId);
					eventBlock.Header.Add(new TextBlock(" created branch "));
                    eventBlock.Header.Add(new AnchorBlock(createEvent.Ref, eventBlock.Tapped));

                    if (ReportRepository)
                    {
                        eventBlock.Header.Add(new TextBlock(" in "));
                        eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                    }
                }
                else if (createEvent.RefType.Equals("tag"))
                {
					eventBlock.Tapped = () => GoToTags(eventModel.Repo);
					eventBlock.Header.Add(new TextBlock(" created tag "));
                    eventBlock.Header.Add(new AnchorBlock(createEvent.Ref, eventBlock.Tapped));

                    if (ReportRepository)
                    {
                        eventBlock.Header.Add(new TextBlock(" in "));
                        eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                    }
                }
            }


            var deleteEvent = eventModel.PayloadObject as EventModel.DeleteEvent;
            if (deleteEvent != null)
            {
                if (deleteEvent.RefType.Equals("branch"))
                {
                    eventBlock.Tapped = () => GoToBranches(repoId);
					eventBlock.Header.Add(new TextBlock(" deleted branch "));
                }
                else if (deleteEvent.RefType.Equals("tag"))
                {
					eventBlock.Tapped = () => GoToTags(eventModel.Repo);
					eventBlock.Header.Add(new TextBlock(" deleted tag "));
                }
                else
                    return null;

                eventBlock.Header.Add(new AnchorBlock(deleteEvent.Ref, eventBlock.Tapped));
                if (!ReportRepository) return eventBlock;
                eventBlock.Header.Add(new TextBlock(" in "));
                eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                return eventBlock;
            }


            if (eventModel.PayloadObject is EventModel.DownloadEvent)
            {
                // Don't show the download event for now...
                return null;
            }


            var followEvent = eventModel.PayloadObject as EventModel.FollowEvent;
            if (followEvent != null)
            {
                eventBlock.Tapped = () => GoToUser(followEvent.Target.Login);
				eventBlock.Header.Add(new TextBlock(" started following "));
                eventBlock.Header.Add(new AnchorBlock(followEvent.Target.Login, eventBlock.Tapped));
                return eventBlock;
            }
            /*
             * FORK EVENT
             */
            else if (eventModel.PayloadObject is EventModel.ForkEvent)
            {
                var forkEvent = (EventModel.ForkEvent)eventModel.PayloadObject;
                var forkedRepo = new EventModel.RepoModel {Id = forkEvent.Forkee.Id, Name = forkEvent.Forkee.FullName, Url = forkEvent.Forkee.Url};
                eventBlock.Tapped = () => GoToRepositoryCommand.Execute(forkedRepo);
				eventBlock.Header.Add(new TextBlock(" forked "));
                eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                eventBlock.Header.Add(new TextBlock(" to "));
                eventBlock.Header.Add(CreateRepositoryTextBlock(forkedRepo));
            }
            /*
             * FORK APPLY EVENT
             */
            else if (eventModel.PayloadObject is EventModel.ForkApplyEvent)
            {
                var forkEvent = (EventModel.ForkApplyEvent)eventModel.PayloadObject;
                eventBlock.Tapped = () => GoToRepositoryCommand.Execute(eventModel.Repo);
				eventBlock.Header.Add(new TextBlock(" applied fork to "));
                eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                eventBlock.Header.Add(new TextBlock(" on branch "));
                eventBlock.Header.Add(new AnchorBlock(forkEvent.Head, () => GoToBranches(repoId)));
            }
            /*
             * GIST EVENT
             */
            else if (eventModel.PayloadObject is EventModel.GistEvent)
            {
                var gistEvent = (EventModel.GistEvent)eventModel.PayloadObject;
                eventBlock.Tapped = () => GoToGistCommand.ExecuteIfCan(gistEvent);

                if (string.Equals(gistEvent.Action, "create", StringComparison.OrdinalIgnoreCase))
					eventBlock.Header.Add(new TextBlock(" created Gist #"));
                else if (string.Equals(gistEvent.Action, "update", StringComparison.OrdinalIgnoreCase))
					eventBlock.Header.Add(new TextBlock(" updated Gist #"));
				else if (string.Equals(gistEvent.Action, "fork", StringComparison.OrdinalIgnoreCase))
					eventBlock.Header.Add(new TextBlock(" forked Gist #"));

                eventBlock.Header.Add(new AnchorBlock(gistEvent.Gist.Id, eventBlock.Tapped));
                eventBlock.Body.Add(new TextBlock(gistEvent.Gist.Description.Replace('\n', ' ').Replace("\r", "").Trim()));
            }
            /*
             * GOLLUM EVENT (WIKI)
             */
            else if (eventModel.PayloadObject is EventModel.GollumEvent)
            {
				var gollumEvent = eventModel.PayloadObject as EventModel.GollumEvent;
				eventBlock.Header.Add(new TextBlock(" modified the wiki in "));
				eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));

				if (gollumEvent != null && gollumEvent.Pages != null)
				{
					foreach (var page in gollumEvent.Pages)
					{
						var p = page;
						eventBlock.Body.Add(new AnchorBlock(page.PageName, () => GoToUrlCommand.Execute(p.HtmlUrl)));
						eventBlock.Body.Add(new TextBlock(" - " + page.Action + "\n"));
					}
				}
            }
            /*
             * ISSUE COMMENT EVENT
             */
            else if (eventModel.PayloadObject is EventModel.IssueCommentEvent)
            {
                var commentEvent = (EventModel.IssueCommentEvent)eventModel.PayloadObject;

				if (commentEvent.Issue.PullRequest != null && !string.IsNullOrEmpty(commentEvent.Issue.PullRequest.HtmlUrl))
				{
					eventBlock.Tapped = () => GoToPullRequest(repoId, commentEvent.Issue.Number);
					eventBlock.Header.Add(new TextBlock(" commented on pull request "));
				}
				else
				{
					eventBlock.Tapped = () => GoToIssue(repoId, commentEvent.Issue.Number);
					eventBlock.Header.Add(new TextBlock(" commented on issue "));
				}

                eventBlock.Header.Add(new AnchorBlock("#" + commentEvent.Issue.Number, eventBlock.Tapped));
                eventBlock.Header.Add(new TextBlock(" in "));
                eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));

                eventBlock.Body.Add(new TextBlock(commentEvent.Comment.Body.Replace('\n', ' ').Replace("\r", "").Trim()));
            }
            /*
             * ISSUES EVENT
             */
            else if (eventModel.PayloadObject is EventModel.IssuesEvent)
            {
                var issueEvent = (EventModel.IssuesEvent)eventModel.PayloadObject;
                eventBlock.Tapped  = () => GoToIssue(repoId, issueEvent.Issue.Number);

                if (string.Equals(issueEvent.Action, "opened", StringComparison.OrdinalIgnoreCase))
                    eventBlock.Header.Add(new TextBlock(" opened issue "));
                else if (string.Equals(issueEvent.Action, "closed", StringComparison.OrdinalIgnoreCase))
					eventBlock.Header.Add(new TextBlock(" closed issue "));
                else if (string.Equals(issueEvent.Action, "reopened", StringComparison.OrdinalIgnoreCase))
					eventBlock.Header.Add(new TextBlock(" reopened issue "));

                eventBlock.Header.Add(new AnchorBlock("#" + issueEvent.Issue.Number, eventBlock.Tapped));
                eventBlock.Header.Add(new TextBlock(" in "));
                eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                eventBlock.Body.Add(new TextBlock(issueEvent.Issue.Title.Trim()));
            }
            /*
             * MEMBER EVENT
             */
            else if (eventModel.PayloadObject is EventModel.MemberEvent)
            {
                var memberEvent = (EventModel.MemberEvent)eventModel.PayloadObject;
                eventBlock.Tapped = () => GoToRepositoryCommand.Execute(eventModel.Repo);

                if (memberEvent.Action.Equals("added"))
                    eventBlock.Header.Add(new TextBlock(" added as a collaborator"));
                else if (memberEvent.Action.Equals("removed"))
                    eventBlock.Header.Add(new TextBlock(" removed as a collaborator"));

                if (ReportRepository)
                {
                    eventBlock.Header.Add(new TextBlock(" to "));
                    eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                }
            }
            /*
             * PUBLIC EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PublicEvent)
            {
                eventBlock.Tapped = () => GoToRepositoryCommand.Execute(eventModel.Repo);
                if (ReportRepository)
                {
                    eventBlock.Header.Add(new TextBlock(" has open sourced "));
                    eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                }
                else
                    eventBlock.Header.Add(new TextBlock(" has been open sourced this repository!"));
            }
            /*
             * PULL REQUEST EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PullRequestEvent)
            {
                var pullEvent = (EventModel.PullRequestEvent)eventModel.PayloadObject;
                eventBlock.Tapped = () => GoToPullRequest(repoId, pullEvent.Number);

                if (pullEvent.Action.Equals("closed"))
                    eventBlock.Header.Add(new TextBlock(" closed pull request "));
                else if (pullEvent.Action.Equals("opened"))
                    eventBlock.Header.Add(new TextBlock(" opened pull request "));
                else if (pullEvent.Action.Equals("synchronize"))
                    eventBlock.Header.Add(new TextBlock(" synchronized pull request "));
                else if (pullEvent.Action.Equals("reopened"))
                    eventBlock.Header.Add(new TextBlock(" reopened pull request "));

				eventBlock.Header.Add(new AnchorBlock("#" + pullEvent.PullRequest.Number, eventBlock.Tapped));
                eventBlock.Header.Add(new TextBlock(" in "));
                eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));

                eventBlock.Body.Add(new TextBlock(pullEvent.PullRequest.Title));
            }
            /*
             * PULL REQUEST REVIEW COMMENT EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PullRequestReviewCommentEvent)
            {
                var commentEvent = (EventModel.PullRequestReviewCommentEvent)eventModel.PayloadObject;
                eventBlock.Tapped = () => GoToPullRequests(repoId);
                eventBlock.Header.Add(new TextBlock(" commented on pull request "));
                if (ReportRepository)
                {
                    eventBlock.Header.Add(new TextBlock(" in "));
                    eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                }

                eventBlock.Body.Add(new TextBlock(commentEvent.Comment.Body.Replace('\n', ' ').Replace("\r", "").Trim()));
            }
            /*
             * PUSH EVENT
             */
            else if (eventModel.PayloadObject is EventModel.PushEvent)
            {
                var pushEvent = (EventModel.PushEvent)eventModel.PayloadObject;

				string branchRef = null;
				if (!string.IsNullOrEmpty(pushEvent.Ref))
				{
					var lastSlash = pushEvent.Ref.LastIndexOf("/", StringComparison.Ordinal) + 1;
					branchRef = pushEvent.Ref.Substring(lastSlash);
				}

                if (eventModel.Repo != null)
					eventBlock.Tapped = () => GoToCommits(eventModel.Repo, pushEvent.Ref);

                eventBlock.Header.Add(new TextBlock(" pushed to "));
				if (branchRef != null)
					eventBlock.Header.Add(new AnchorBlock(branchRef, () => GoToBranches(repoId)));

                if (ReportRepository)
                {
                    eventBlock.Header.Add(new TextBlock(" at "));
                    eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                }

				if (pushEvent.Commits != null)
				{
					foreach (var commit in pushEvent.Commits)
					{
						var desc = (commit.Message ?? "");
						var sha = commit.Sha;
						var firstNewLine = desc.IndexOf("\n");
						if (firstNewLine <= 0)
							firstNewLine = desc.Length;

						desc = desc.Substring(0, firstNewLine);
						var shortSha = commit.Sha;
						if (shortSha.Length > 6)
							shortSha = shortSha.Substring(0, 6);

						eventBlock.Body.Add(new AnchorBlock(shortSha, () => GoToChangeset(repoId, sha)));
						eventBlock.Body.Add(new TextBlock(" - " + desc + "\n"));
					}
				}
            }


            var teamAddEvent = eventModel.PayloadObject as EventModel.TeamAddEvent;
            if (teamAddEvent != null)
            {
                eventBlock.Header.Add(new TextBlock(" added "));

                if (teamAddEvent.User != null)
                    eventBlock.Header.Add(new AnchorBlock(teamAddEvent.User.Login, () => GoToUser(teamAddEvent.User.Login)));
                else if (teamAddEvent.Repo != null)
                    eventBlock.Header.Add(CreateRepositoryTextBlock(new EventModel.RepoModel { Id = teamAddEvent.Repo.Id, Name = teamAddEvent.Repo.FullName, Url = teamAddEvent.Repo.Url }));
                else
                    return null;

                if (teamAddEvent.Team == null) return eventBlock;
                eventBlock.Header.Add(new TextBlock(" to team "));
                eventBlock.Header.Add(new AnchorBlock(teamAddEvent.Team.Name, () => { }));
                return eventBlock;
            }


            var watchEvent = eventModel.PayloadObject as EventModel.WatchEvent;
            if (watchEvent != null)
            {
				eventBlock.Tapped = () => GoToRepositoryCommand.Execute(eventModel.Repo);
                eventBlock.Header.Add(watchEvent.Action.Equals("started") ? 
					new TextBlock(" starred ") : new TextBlock(" unstarred "));
                eventBlock.Header.Add(CreateRepositoryTextBlock(eventModel.Repo));
                return eventBlock;
            }

			var releaseEvent = eventModel.PayloadObject as EventModel.ReleaseEvent;
			if (releaseEvent != null)
			{
				eventBlock.Tapped = () => GoToUrlCommand.Execute(releaseEvent.Release.HtmlUrl);
				eventBlock.Header.Add(new TextBlock(" " + releaseEvent.Action + " release " + releaseEvent.Release.Name));
				return eventBlock;
			}

            return eventBlock;
        }

        private TextBlock CreateRepositoryTextBlock(EventModel.RepoModel repoModel)
        {
            //Most likely indicates a deleted repository
            if (repoModel == null)
                return new TextBlock("Unknown Repository");
            if (repoModel.Name == null)
                return new TextBlock("<Deleted Repository>");

            var repoSplit = repoModel.Name.Split('/');
            if (repoSplit.Length < 2)
                return new TextBlock(repoModel.Name);

//            var repoOwner = repoSplit[0];
//            var repoName = repoSplit[1];
			return new AnchorBlock(repoModel.Name, () => GoToRepositoryCommand.Execute(repoModel));
        }


        public class EventBlock
        {
            public IList<TextBlock> Header { get; private set; }
            public IList<TextBlock> Body { get; private set; } 
            public Action Tapped { get; set; }

            public EventBlock()
            {
                Header = new List<TextBlock>(6);
                Body = new List<TextBlock>();
            }
        }

        public class TextBlock
        {
            public string Text { get; set; }

            public TextBlock()
            {
            }

            public TextBlock(string text)
            {
                Text = text;
            }
        }

        public class AnchorBlock : TextBlock
        {
            public AnchorBlock(string text, Action tapped) : base(text)
            {
                Tapped = tapped;
            }

            public Action Tapped { get; set; }

            public AnchorBlock(Action tapped)
            {
                Tapped = tapped;
            }
        }
    }
}