// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CodeHub.iOS.ViewControllers.Accounts
{
	[Register ("EnterpriseOAuthTokenLoginView")]
	partial class EnterpriseOAuthTokenLoginViewController
	{
		[Outlet]
		UIKit.UILabel DescriptionLabel { get; set; }

		[Outlet]
		UIKit.UITextField DomainText { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint ImageHeight { get; set; }

		[Outlet]
		UIKit.UIButton LoginButton { get; set; }

		[Outlet]
		UIKit.UIImageView LogoImageView { get; set; }

		[Outlet]
		UIKit.UIScrollView ScrollView { get; set; }

		[Outlet]
		UIKit.UITextField TokenText { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (DescriptionLabel != null) {
				DescriptionLabel.Dispose ();
				DescriptionLabel = null;
			}

			if (DomainText != null) {
				DomainText.Dispose ();
				DomainText = null;
			}

			if (LoginButton != null) {
				LoginButton.Dispose ();
				LoginButton = null;
			}

			if (LogoImageView != null) {
				LogoImageView.Dispose ();
				LogoImageView = null;
			}

			if (ScrollView != null) {
				ScrollView.Dispose ();
				ScrollView = null;
			}

			if (TokenText != null) {
				TokenText.Dispose ();
				TokenText = null;
			}

			if (ImageHeight != null) {
				ImageHeight.Dispose ();
				ImageHeight = null;
			}
		}
	}
}
