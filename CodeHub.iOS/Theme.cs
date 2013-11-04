﻿using CodeFramework.iOS;
using MonoTouch.UIKit;

namespace CodeHub.iOS
{
    public class Theme : ICodeFrameworkTheme
    {
        public static Theme CurrentTheme { get; private set; }

        public static void Setup()
        {
            var theme = new Theme();
            CurrentTheme = theme;
            CodeFramework.iOS.Theme.CurrentTheme = theme;

//            CodeFramework.Elements.NewsFeedElement.LinkColor = theme.MainTitleColor;
//            CodeFramework.Elements.NewsFeedElement.TextColor = theme.MainTextColor;
//            CodeFramework.Elements.NewsFeedElement.NameColor = theme.MainTitleColor;
        }

        public UITextAttributes SegmentedControlText
        {
            get
            {
                return new UITextAttributes
                { 
                    Font = UIFont.SystemFontOfSize(14f), 
                    TextColor = UIColor.FromRGB(87, 85, 85), 
                    TextShadowColor = UIColor.FromRGBA(255, 255, 255, 125), 
                    TextShadowOffset = new UIOffset(0, 1) 
                };
            }
        }

        public UIImage BackButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/back"); } }
        public UIImage ThreeLinesButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/three_lines"); } }
        public UIImage CancelButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/cancel"); } }
        public UIImage EditButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/edit"); } }
        public UIImage SaveButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/save"); } }
        public UIImage AddButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/add"); } }
        public UIImage FilterButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/filter"); } }
        public UIImage GearButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/gear"); } }
        public UIImage ViewButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/view"); } }
        public UIImage ForkButton { get { return UIImageHelper.FromFileAuto("Images/Buttons/fork"); } }
        public UIImage WebBackButton { get { return UIImage.FromFile("Images/Web/back_button@2x.png"); } }
        public UIImage WebFowardButton { get { return UIImage.FromFile("Images/Web/forward_button@2x.png"); } }

        public UIImage AnonymousUserImage { get { return Images.Images.Anonymous; } }

        public UIColor ViewBackgroundColor { get { return UIColor.FromRGB(238, 238, 238); } }

        public UIImage MenuSectionBackground { get { return UIImageHelper.FromFileAuto("Images/Controls/menu_section_bg"); } }
        public UIImage MenuNavbarBackground { get { return UIImageHelper.FromFileAuto("Images/Controls/menu_navbar"); } }
        public UIImage WarningImage { get { return UIImageHelper.FromFileAuto("Images/Controls/warning"); } }

        public UIImage DropbarBackground { get { return UIImageHelper.FromFileAuto("Images/Controls/dropbar"); } }

        public UIImage TableViewSectionBackground { get { return Images.Images.Searchbar; } }

        //Cache these because we make a smaller size of them
        private UIImage _issueCell1, _issueCell2, _issueCell3, _issueCell4;
        private UIImage _repoCell1, _repoCell2, _repoCell3;

        public UIImage IssueCellImage1
        {
            get { return _issueCell1 ?? (_issueCell1 = new UIImage(Images.Images.Cog.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage IssueCellImage2
        {
            get { return _issueCell2 ?? (_issueCell2 = new UIImage(Images.Images.Comments.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage IssueCellImage3
        {
            get { return _issueCell3 ?? (_issueCell3 = new UIImage(Images.Images.Person.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage IssueCellImage4
        {
            get { return _issueCell4 ?? (_issueCell4 = new UIImage(Images.Images.Pencil.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage RepositoryCellFollowers
        {
            get { return _repoCell1 ?? (_repoCell1 = new UIImage(Images.Images.Star.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage RepositoryCellForks
        {
            get { return _repoCell2 ?? (_repoCell2 = new UIImage(Images.Images.Fork.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIImage RepositoryCellUser
        {
            get { return _repoCell3 ?? (_repoCell3 = new UIImage(Images.Images.Person.CGImage, 1.3f, UIImageOrientation.Up)); }
        }

        public UIColor NavigationTextColor { get { return UIColor.FromRGB(97, 95, 95); } }

        public UIColor MainTitleColor { get { return UIColor.FromRGB(0x41, 0x83, 0xc4); } }
        public UIColor MainSubtitleColor { get { return UIColor.FromRGB(81, 81, 81); } }
        public UIColor MainTextColor { get { return UIColor.FromRGB(41, 41, 41); } }

        public UIColor IssueTitleColor { get { return MainTitleColor; } }
        public UIColor RepositoryTitleColor { get { return MainTitleColor; } }
        public UIColor HeaderViewTitleColor { get { return MainTitleColor; } }
        public UIColor HeaderViewDetailColor { get { return MainSubtitleColor; } }

        public UIColor WebButtonTint { get { return UIColor.FromRGB(127, 125, 125); } }
    }
}
