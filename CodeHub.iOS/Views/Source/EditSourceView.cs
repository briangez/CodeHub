using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using UIKit;
using CoreGraphics;
using Foundation;
using CodeHub.iOS.Utilities;
using System.Threading.Tasks;
using CodeHub.iOS.Services;

namespace CodeHub.iOS.Views.Source
{
    public class EditSourceView : BaseViewController
    {
        ComposerView _composerView;

        public EditSourceViewModel ViewModel { get; }
    
        public EditSourceView()
        {
            ViewModel = new EditSourceViewModel();
            EdgesForExtendedLayout = UIRectEdge.None;
            Title = "Edit";
        }
      
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _composerView = new ComposerView (ComputeComposerSize (CGRect.Empty));
            var saveButton = NavigationItem.RightBarButtonItem = new UIBarButtonItem { Image = Theme.CurrentTheme.SaveButton };

            View.AddSubview (_composerView);

            OnActivation(d =>
            {
                d(saveButton.GetClickedObservable().Subscribe(_ => Commit()));
                d(ViewModel.Bind(x => x.Text).Subscribe(x => _composerView.Text = x));
            });

            ViewModel.LoadCommand.Execute(null);
        }

        private void Commit()
        {
            var composer = new LiteComposer { Title = "Commit Message" };
            composer.Text = "Update " + ViewModel.Path.Substring(ViewModel.Path.LastIndexOf('/') + 1);
            var text = _composerView.Text;
            composer.ReturnAction += (s, e) => CommitThis(ViewModel, composer, text, e);
            _composerView.TextView.BecomeFirstResponder ();
            NavigationController.PushViewController(composer, true);
        }

        /// <summary>
        /// Need another function because Xamarin generates an Invalid IL if used inline above
        /// </summary>
        private async Task CommitThis(EditSourceViewModel viewModel, LiteComposer composer, string content, string message)
        {
            try
            {
                await this.DoWorkAsync("Commiting...", () => viewModel.Commit(content, message));
                NavigationController.DismissViewController(true, null);
            }
            catch (Exception ex)
            {
                AlertDialogService.ShowAlert("Error", ex.Message);
                composer.EnableSendButton = true;
            }
        }

        void KeyboardWillShow (NSNotification notification)
        {
            var nsValue = notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue;
            if (nsValue == null) return;
            var kbdBounds = nsValue.RectangleFValue;
            UIView.Animate(0.25f, 0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseIn, () =>
            _composerView.Frame = ComputeComposerSize(kbdBounds), null);
        }

        void KeyboardWillHide (NSNotification notification)
        {
            UIView.Animate(0.2, 0, UIViewAnimationOptions.BeginFromCurrentState | UIViewAnimationOptions.CurveEaseIn, () =>
            _composerView.Frame = ComputeComposerSize(CGRect.Empty), null);
        }

        CGRect ComputeComposerSize (CGRect kbdBounds)
        {
            var view = View.Bounds;
            return new CGRect (0, 0, view.Width, view.Height-kbdBounds.Height);
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillShowNotification"), KeyboardWillShow);
            NSNotificationCenter.DefaultCenter.AddObserver (new NSString("UIKeyboardWillHideNotification"), KeyboardWillHide);

            _composerView.TextView.BecomeFirstResponder ();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
        }

        private class ComposerView : UIView 
        {
            internal readonly UITextView TextView;

            public ComposerView (CGRect bounds) : base (bounds)
            {
                TextView = new UITextView (CGRect.Empty) {
                    Font = UIFont.SystemFontOfSize (14),
                };

                // Work around an Apple bug in the UITextView that crashes
                if (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR)
                    TextView.AutocorrectionType = UITextAutocorrectionType.No;

                AddSubview (TextView);
            }


            internal void Reset (string text)
            {
                TextView.Text = text;
            }

            public override void LayoutSubviews ()
            {
                Resize (Bounds);
            }

            void Resize (CGRect bounds)
            {
                TextView.Frame = new CGRect (0, 0, bounds.Width, bounds.Height);
            }

            public string Text { 
                get {
                    return TextView.Text;
                }
                set {
                    TextView.Text = value;
                    TextView.SelectedRange = new NSRange(0, 0);
                }
            }
        }

    }
}

