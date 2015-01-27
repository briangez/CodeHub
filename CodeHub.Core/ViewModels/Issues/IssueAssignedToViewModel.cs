using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.ViewModels.Users;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueAssignedToViewModel : BaseViewModel, ILoadableViewModel
    {
        private Octokit.User _selectedUser;
        public Octokit.User SelectedUser
        {
            get { return _selectedUser; }
            set { this.RaiseAndSetIfChanged(ref _selectedUser, value); }
        }

        public IReadOnlyReactiveList<UserItemViewModel> Users { get; private set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public int IssueId { get; set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand SaveCommand { get; private set; }

        public IssueAssignedToViewModel(IApplicationService applicationService)
        {
            Title = "Assignees";

            var users = new ReactiveList<Octokit.User>();
            Users = users.CreateDerivedCollection(x => new UserItemViewModel(x.Name, x.AvatarUrl, false, () => {}));

            SaveCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                await applicationService.GitHubClient.Issue.Update(RepositoryOwner, RepositoryName, IssueId, new Octokit.IssueUpdate {
                    Assignee = SelectedUser != null ? SelectedUser.Login : null
                });

                Dismiss();
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => 
                users.Reset(await applicationService.GitHubClient.Issue.Assignee.GetForRepository(RepositoryOwner, RepositoryName)));
        }
    }
}

