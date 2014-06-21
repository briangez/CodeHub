using System;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Issues;
using System.Linq;
using CodeHub.iOS.Elements;
using MonoTouch.UIKit;
using ReactiveUI;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueLabelsView : ViewModelCollectionView<IssueLabelsViewModel>
    {
		public IssueLabelsView()
		{
			EnableSearch = false;
		}

        public override void ViewDidLoad()
        {
            Title = "Labels";
            NoItemsText = "No Labels";

            base.ViewDidLoad();

			NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.BackButton, UIBarButtonItemStyle.Plain,
			    (s, e) =>
			    {
			        if (ViewModel.SaveOnSelect)
                        ViewModel.SelectLabelsCommand.ExecuteIfCan();
			    });


            Bind(ViewModel.WhenAnyValue(x => x.Labels), label =>
            {
                var element = new LabelElement(label);
                element.Tapped += () =>
                {
                    if (ViewModel.SelectedLabels.Contains(label))
                        ViewModel.SelectedLabels.Remove(label);
                    else
                        ViewModel.SelectedLabels.Add(label);
                };
                element.Accessory = ViewModel.SelectedLabels.Contains(label)
                           ? UITableViewCellAccessory.Checkmark
                           : UITableViewCellAccessory.None;
                return element;
            });

            ViewModel.SelectedLabels.Changed.Subscribe(x => ViewModel.Labels.Reset());
        }
    }
}

