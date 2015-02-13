using System;
using ReactiveUI;
using System.Reactive.Linq;
using UIKit;
using CodeHub.Core.ViewModels.Source;
using CodeHub.WebViews;
using Foundation;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.Views.Source
{
    public abstract class FileSourceView<TViewModel> : BaseWebView<TViewModel> where TViewModel : ContentViewModel
    {
        private readonly IAlertDialogFactory _alertDialogFactory;
        private bool _fullScreen;

        protected FileSourceView(IAlertDialogFactory alertDialogFactory)
        {
            _alertDialogFactory = alertDialogFactory;

            this.WhenViewModel(x => x.ShowMenuCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Action))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenViewModel(x => x.OpenWithCommand)
                .Switch()
                .Subscribe(_ =>
                {
                    UIDocumentInteractionController ctrl = UIDocumentInteractionController.FromUrl(new NSUrl(ViewModel.SourceItem.FileUri.AbsoluteUri));
                    ctrl.Delegate = new UIDocumentInteractionControllerDelegate();
                    var couldOpen = ctrl.PresentOpenInMenu(NavigationItem.RightBarButtonItem, true);
                    if (!couldOpen)
                    {
                        alertDialogFactory.ShowError("Nothing to open with");
                    }
                });
        }

        protected override void OnLoadError(object sender, UIWebErrorArgs e)
        {
            base.OnLoadError(sender, e);
            if (e.Error.Code == 102)
            {
                _alertDialogFactory.Alert("Oh no!", "Looks like CodeHub cannot display this type of file. Sorry about that :(");
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel.WhenAnyValue(x => x.Theme)
                .Select(_ => ViewModel.SourceItem)
                .Skip(1)
                .Where(x => x != null && !x.IsBinary)
                .Subscribe(x => LoadSource(x.FileUri));

            ViewModel.WhenAnyValue(x => x.SourceItem)
                .Where(x => x != null)
                .SubscribeSafe(x =>
                {
                    if (x.IsBinary)
                    {
                        GoUrl(new NSUrl(x.FileUri.AbsoluteUri));
                    }
                    else
                    {
                        LoadSource(x.FileUri);
                    }
                });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (_fullScreen && NavigationController != null)
                SetFullScreen(false);
        }

        private void SetFullScreen(bool fullScreen)
        {
            _fullScreen = fullScreen;
            UIApplication.SharedApplication.SetStatusBarHidden(fullScreen, UIStatusBarAnimation.Slide);
            NavigationController.SetNavigationBarHidden(fullScreen, true);
        }

        protected virtual void LoadSource(Uri fileUri)
        {
            if (ViewModel.IsMarkdown)
            {
                var content = System.IO.File.ReadAllText(fileUri.LocalPath, System.Text.Encoding.UTF8);
                var htmlContent = new MarkdownView { Model = content };
                LoadContent(htmlContent.GenerateString());
            }
            else
            {
                var content = System.IO.File.ReadAllText(fileUri.LocalPath, System.Text.Encoding.UTF8);
                var contentView = new SyntaxHighlighterView { Model = new SourceBrowserModel(content, ViewModel.Theme ?? "idea", fileUri.LocalPath) };
                LoadContent(contentView.GenerateString());
            }
        }

        protected override bool ShouldStartLoad(NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            if (request.Url.AbsoluteString.StartsWith("file://", StringComparison.Ordinal))
                return base.ShouldStartLoad(request, navigationType);
            ViewModel.GoToUrlCommand.ExecuteIfCan(request.Url.AbsoluteString);
            return false;
        }
    }
}

