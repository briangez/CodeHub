﻿using System;
using UIKit;
using CodeHub.Core.ViewModels.Accounts;
using System.Reactive.Linq;
using CoreGraphics;
using ReactiveUI;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.Views.Accounts
{
    public partial class OAuthLoginView : BaseViewController<OAuthLoginViewModel>
    {
        private readonly IAlertDialogFactory _alertDialogFactory;

        public OAuthLoginView(IAlertDialogFactory alertDialogFactory)
            : base("OAuthLoginView", null)
        {
            _alertDialogFactory = alertDialogFactory;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel.LoginCommand.IsExecuting.Skip(1).Subscribe(x =>
            {
                if (x)
                    _alertDialogFactory.Show("Logging in...");
                else
                    _alertDialogFactory.Hide();
            });

            TokenText.EditingChanged += (sender, args) => ViewModel.Token = TokenText.Text;
            ViewModel.WhenAnyValue(x => x.Token).Subscribe(x => TokenText.Text = x);

            LoginButton.TouchUpInside += (sender, args) => ViewModel.LoginCommand.ExecuteIfCan();
            ViewModel.LoginCommand.CanExecuteObservable.Subscribe(x => LoginButton.Enabled = x);

            View.BackgroundColor = UIColor.FromRGB(239, 239, 244);
            LogoImageView.Image = Images.Logos.GitHub;

            LoginButton.SetBackgroundImage(Images.Buttons.GreyButton.CreateResizableImage(new UIEdgeInsets(18, 18, 18, 18)), UIControlState.Normal);

            //Set some generic shadowing
            LoginButton.Layer.ShadowColor = UIColor.Black.CGColor;
            LoginButton.Layer.ShadowOffset = new CGSize(0, 1);
            LoginButton.Layer.ShadowOpacity = 0.3f;

            TokenText.ShouldReturn = delegate {
                TokenText.ResignFirstResponder();
                LoginButton.SendActionForControlEvents(UIControlEvent.TouchUpInside);
                return true;
            };

            this.ViewportObservable().Subscribe(x => ScrollView.Frame = x);
        }
    }
}

