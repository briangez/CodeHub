using ReactiveUI;
using CodeHub.iOS.Cells;
using CodeHub.Core.ViewModels.Gists;
using UIKit;

namespace CodeHub.iOS.TableViewSources
{
    public class GistTableViewSource : ReactiveTableViewSource<GistItemViewModel>
    {
        public GistTableViewSource(UITableView tableView, IReactiveNotifyCollectionChanged<GistItemViewModel> collection) 
            : base(tableView, collection, GistCellView.Key, UITableView.AutomaticDimension, 60f)
        {
            tableView.RegisterNibForCellReuse(GistCellView.Nib, GistCellView.Key);
        }
    }
}

