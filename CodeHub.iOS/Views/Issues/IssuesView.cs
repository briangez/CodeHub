using System;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.ViewComponents;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Issues
{
    public class IssuesView : BaseTableViewController<IssuesViewModel>
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;

        public IssuesView()
        {
            _viewSegment = new CustomUISegmentedControl(new [] { "Open", "Closed", "Mine", "Custom" }, 3);
            _segmentBarButton = new UIBarButtonItem(_viewSegment);

            ToolbarItems = new [] { 
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), 
                _segmentBarButton, 
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) 
            };

            this.WhenAnyValue(x => x.ViewModel.GoToNewIssueCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Add))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.IssueOpened.ToImage(64f), "There are no issues."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            TableView.Source = new IssueTableViewSource(TableView, ViewModel.Issues);
            _segmentBarButton.Width = View.Frame.Width - 10f;
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            _segmentBarButton.Width = View.Frame.Width - 10f;
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);

            //Before we select which one, make sure we detach the event handler or silly things will happen
            _viewSegment.ValueChanged -= SegmentValueChanged;
            _viewSegment.SelectedSegment = (int)ViewModel.FilterSelection;
            _viewSegment.ValueChanged += SegmentValueChanged;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }

        void SegmentValueChanged (object sender, EventArgs e)
        {
            switch (_viewSegment.SelectedSegment)
            {
                case 0:
                    ViewModel.FilterSelection = IssuesViewModel.IssueFilterSelection.Open;
                    break;
                case 1:
                    ViewModel.FilterSelection = IssuesViewModel.IssueFilterSelection.Closed;
                    break;
                case 2:
                    ViewModel.FilterSelection = IssuesViewModel.IssueFilterSelection.Mine;
                    break;
                case 3:
                    ViewModel.GoToCustomFilterCommand.ExecuteIfCan();
                    break;
            }
        }

        private class CustomUISegmentedControl : UISegmentedControl
        {
            readonly int _multipleTouchIndex;
            public CustomUISegmentedControl(string[] args, int multipleTouchIndex)
                : base(args)
            {
                this._multipleTouchIndex = multipleTouchIndex;
            }

            public override void TouchesEnded(Foundation.NSSet touches, UIEvent evt)
            {
                var previousSelected = SelectedSegment;
                base.TouchesEnded(touches, evt);
                if (previousSelected == SelectedSegment && SelectedSegment == _multipleTouchIndex)
                    SendActionForControlEvents(UIControlEvent.ValueChanged);
            }
        }
    }
}

