using System;
using CodeHub.Core.ViewModels.Users;
using Foundation;
using CoreGraphics;
using UIKit;
using System.Reactive.Linq;
using ReactiveUI;

namespace CodeHub.iOS.Cells
{
    public class UserTableViewCell : ReactiveTableViewCell<UserItemViewModel>
    {
        public static NSString Key = new NSString("usercell");
        private const float ImageSpacing = 10f;
        private static CGRect ImageFrame = new CGRect(ImageSpacing, 6f, 32, 32);

        public UserTableViewCell(IntPtr handle)
            : base(handle)
        { 
            SeparatorInset = new UIEdgeInsets(0, ImageFrame.Right + ImageSpacing, 0, 0);
            ImageView.Layer.CornerRadius = ImageFrame.Height / 2f;
            ImageView.Layer.MasksToBounds = true;
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            ContentView.Opaque = true;

            this.WhenAnyValue(x => x.ViewModel)
                .IsNotNull()
                .Subscribe(x =>
                {
                    TextLabel.Text = x.Name;
                    ImageView.SetAvatar(x.Avatar);
                });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ImageView.Frame = ImageFrame;
            TextLabel.Frame = new CGRect(ImageFrame.Right + ImageSpacing, TextLabel.Frame.Y, TextLabel.Frame.Width, TextLabel.Frame.Height);
        }
    }
}

