using CodeHub.Core.Services;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesStarredViewModel : RepositoriesViewModel
    {
        public RepositoriesStarredViewModel(IApplicationService applicationService) : base(applicationService)
        {
            ShowRepositoryOwner = true;

            LoadCommand.RegisterAsyncTask(t =>
                Repositories.SimpleCollectionLoad(
                    applicationService.Client.AuthenticatedUser.Repositories.GetStarred(), t as bool?));
        }
    }
}

