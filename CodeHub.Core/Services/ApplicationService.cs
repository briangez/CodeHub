using CodeHub.Core.Data;
using CodeHub.Core.Services;
using GitHubSharp;
using System;
using MvvmCross.Core.Views;

namespace CodeHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IMvxViewDispatcher _viewDispatcher;
        private readonly IPushNotificationsService _pushNotifications;
        private readonly IFeaturesService _features;
        private readonly IAlertDialogService _alertDialogService;

        public Client Client { get; private set; }
        public GitHubAccount Account { get; private set; }
        public IAccountsService Accounts { get; private set; }

        public Action ActivationAction { get; set; }

        public ApplicationService(IAccountsService accounts, IMvxViewDispatcher viewDispatcher, 
            IFeaturesService features, IPushNotificationsService pushNotifications,
            IAlertDialogService alertDialogService)
        {
            _viewDispatcher = viewDispatcher;
            _pushNotifications = pushNotifications;
            Accounts = accounts;
            _features = features;
            _alertDialogService = alertDialogService;
        }

        public void DeactivateUser()
        {
            Accounts.SetActiveAccount(null);
            Accounts.SetDefault(null);
            Account = null;
            Client = null;
        }

        public void ActivateUser(GitHubAccount account, Client client)
        {
            Accounts.SetActiveAccount(account);
            Account = account;
            Client = client;

            //Set the default account
            Accounts.SetDefault(account);
        }

        public void SetUserActivationAction(Action action)
        {
            if (Account != null)
                action();
            else
                ActivationAction = action;
        }

    }
}
