using System;
using GitHubSharp.Models;
using ReactiveUI;
using System.Linq;
using GitHubSharp;
using System.Collections.Generic;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Gists
{
    public abstract class BaseGistsViewModel : BaseViewModel, IProvidesSearchKeyword, ILoadableViewModel, IPaginatableViewModel
    {
        protected ReactiveList<GistModel> InternalGists { get; private set; }
        public IReadOnlyReactiveList<GistItemViewModel> Gists { get; private set; }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        private IReactiveCommand<Unit> _loadMoreCommand;
        public IReactiveCommand<Unit> LoadMoreCommand
        {
            get { return _loadMoreCommand; }
            private set { this.RaiseAndSetIfChanged(ref _loadMoreCommand, value); }
        }

        protected BaseGistsViewModel()
        {
            InternalGists = new ReactiveList<GistModel>();
            Gists = InternalGists.CreateDerivedCollection(
                x => CreateGistItemViewModel(x),
                filter: x => x.Description.ContainsKeyword(SearchKeyword) || GetGistTitle(x).ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                await InternalGists.SimpleCollectionLoad(CreateRequest(), 
                    x => LoadMoreCommand = x == null ? null : ReactiveCommand.CreateAsyncTask(_ => x()));
            });
        }

        protected abstract GitHubRequest<List<GistModel>> CreateRequest();

        private GistItemViewModel CreateGistItemViewModel(GistModel gist)
        {
            var title = GetGistTitle(gist);
            var description = string.IsNullOrEmpty(gist.Description) ? "Gist " + gist.Id : gist.Description;
            var imageUrl = (gist.Owner == null) ? null : gist.Owner.AvatarUrl;

            return new GistItemViewModel(title, imageUrl, description, gist.UpdatedAt, _ =>
            {
                var vm = this.CreateViewModel<GistViewModel>();
                vm.Id = gist.Id;
                vm.Gist = gist;
                NavigateTo(vm);
            });
        }

        private static string GetGistTitle(GistModel gist)
        {
            var title = (gist.Owner == null) ? "Anonymous" : gist.Owner.Login;
            if (gist.Files.Count > 0)
                title = gist.Files.First().Key;
            return title;
        }
    }
}

