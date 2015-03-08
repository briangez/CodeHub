using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GitHubSharp.Models;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Contents
{
    public class EditFileViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly ISubject<ContentUpdateModel> _sourceChangedSubject = new Subject<ContentUpdateModel>();
        private DateTime _lastLoad, _lastEdit;

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

		private string _text;
		public string Text
		{
			get { return _text; }
			set { this.RaiseAndSetIfChanged(ref _text, value); }
		}

        private string _commitMessage;
        public string CommitMessage
        {
            get { return _commitMessage; }
            set { this.RaiseAndSetIfChanged(ref _commitMessage, value); }
        }

        private string _path;
		public string Path 
        {
            get { return _path; }
            set { this.RaiseAndSetIfChanged(ref _path, value); }
        }

		public string BlobSha { get; set; }

		public string Branch { get; set; }

        public IObservable<ContentUpdateModel> SourceChanged
        {
            get { return _sourceChangedSubject; }
        }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<Unit> SaveCommand { get; private set; }

        public IReactiveCommand<bool> DismissCommand { get; private set; }

        public EditFileViewModel(ISessionService applicationService, IAlertDialogFactory alertDialogFactory)
	    {
            Title = "Edit";

            this.WhenAnyValue(x => x.Path)
                .IsNotNull()
                .Subscribe(x => CommitMessage = "Updated " + x.Substring(x.LastIndexOf('/') + 1));

            this.WhenAnyValue(x => x.Text)
                .Subscribe(x => _lastEdit = DateTime.Now);

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
    	        {
    	            var path = Path;
                    if (!path.StartsWith("/", StringComparison.Ordinal))
                        path = "/" + path;

    	            var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetContentFile(path, Branch ?? "master");
    			    request.UseCache = false;
    			    var data = await applicationService.Client.ExecuteAsync(request);
    			    BlobSha = data.Data.Sha;
    	            var content = Convert.FromBase64String(data.Data.Content);
                    Text = System.Text.Encoding.UTF8.GetString(content, 0, content.Length) ?? string.Empty;
                    _lastLoad = DateTime.Now;
    	        });

            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.CommitMessage).Select(x => !string.IsNullOrEmpty(x)),
                async _ =>
            {
                var path = Path.StartsWith("/", StringComparison.Ordinal) ? Path : string.Concat("/", Path);
                var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName]
                    .UpdateContentFile(path, CommitMessage, Text, BlobSha, Branch);

                using (alertDialogFactory.Activate("Commiting..."))
                {
                    var response = await applicationService.Client.ExecuteAsync(request);
                    _sourceChangedSubject.OnNext(response.Data);
                }

                Dismiss();
            });

            DismissCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                if (string.IsNullOrEmpty(Text)) return true;
                if (_lastEdit <= _lastLoad) return true;
                return await alertDialogFactory.PromptYesNo("Discard Edit?", "Are you sure you want to discard these changes?");
            });
            DismissCommand.Where(x => x).Subscribe(_ => Dismiss());
	    }
    }
}

