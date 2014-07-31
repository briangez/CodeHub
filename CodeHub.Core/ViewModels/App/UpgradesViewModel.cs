using CodeHub.Core.Services;
using System.Linq;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.App
{
    public class UpgradesViewModel : BaseViewModel, ILoadableViewModel
    {
        private string[] _keys;

        public string[] Keys
        {
            get { return _keys; }
            private set { this.RaiseAndSetIfChanged(ref _keys, value); }
        }

        public IReactiveCommand LoadCommand { get; private set; }

        public UpgradesViewModel(IFeaturesService featuresService)
        {
            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                Keys = (await featuresService.GetAvailableFeatureIds()).ToArray();
            });
        }
    }
}

