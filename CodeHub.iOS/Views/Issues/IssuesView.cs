using System;
using Cirrious.CrossCore;
using CodeFramework.iOS.Views;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Issues;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.Issues
{
    public class IssuesView : BaseIssuesView
    {
        private UISegmentedControl _viewSegment;
        private UIBarButtonItem _segmentBarButton;

        public new IssuesViewModel ViewModel
        {
            get { return (IssuesViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add, (s, e) => ViewModel.GoToNewIssueCommand.Execute(null));

            base.ViewDidLoad();

            _viewSegment = new UISegmentedControl(new string[] { "Open".t(), "Closed".t(), "Mine".t(), "Custom".t() });
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
            BindCollection(ViewModel.Issues, CreateElement);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);

            //Before we select which one, make sure we detach the event handler or silly things will happen
            _viewSegment.ValueChanged -= SegmentValueChanged;

            var application = Mvx.Resolve<IApplicationService>();

            //Select which one is currently selected
            if (ViewModel.Issues.Filter.Equals(IssuesFilterModel.CreateOpenFilter()))
                _viewSegment.SelectedSegment = 0;
            else if (ViewModel.Issues.Filter.Equals(IssuesFilterModel.CreateClosedFilter()))
                _viewSegment.SelectedSegment = 1;
            else if (ViewModel.Issues.Filter.Equals(IssuesFilterModel.CreateMineFilter(application.Account.Username)))
                _viewSegment.SelectedSegment = 2;
            else
                _viewSegment.SelectedSegment = 3;

            _viewSegment.ValueChanged += SegmentValueChanged;
        }

        void SegmentValueChanged (object sender, EventArgs e)
        {
            var application = Mvx.Resolve<IApplicationService>();

            if (_viewSegment.SelectedSegment == 0)
            {
                ViewModel.Issues.ApplyFilter(IssuesFilterModel.CreateOpenFilter(), true);
            }
            else if (_viewSegment.SelectedSegment == 1)
            {
                ViewModel.Issues.ApplyFilter(IssuesFilterModel.CreateClosedFilter(), true);
            }
            else if (_viewSegment.SelectedSegment == 2)
            {
                ViewModel.Issues.ApplyFilter(IssuesFilterModel.CreateMineFilter(application.Account.Username), true);
            }
            else if (_viewSegment.SelectedSegment == 3)
            {
//                var filter = new IssuesFilterViewController(ViewModel.User, ViewModel.Slug, ViewModel.Issues);
//                var nav = new UINavigationController(filter);
//                PresentViewController(nav, true, null);
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}

