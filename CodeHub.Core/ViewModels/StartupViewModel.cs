﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.CrossCore;
using CodeFramework.Core.Services;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Data;

namespace CodeHub.Core.ViewModels
{
    public class StartupViewModel : BaseViewModel
    {

        /// <summary>
        /// Gets the default account. If there is not one assigned it will pick the first in the account list.
        /// If there isn't one, it'll just return null.
        /// </summary>
        /// <returns>The default account.</returns>
        private void GetDefaultAccount()
        {
//            var accounts = Mvx.Resolve<IAccountsService>()
//            var defaultAccount = Application.Accounts.GetDefault();
//            if (defaultAccount == null)
//            {
//                defaultAccount = Application.Accounts.FirstOrDefault();
//                Application.Accounts.SetDefault(defaultAccount);
//            }
//            return defaultAccount;
        }

        private void Login()
        {
//            var defaultAccount = GetDefaultAccount();
//
//            //There's no accounts... or something bad has happened to the default
//            if (Application.Accounts.Count == 0 || defaultAccount == null)
//            {
//                var login = new AccountTypeViewController();
//                login.NavigationItem.LeftBarButtonItem = null;
//                var navCtrl = new CustomNavigationController(this, login);
//                Transitions.TransitionToController(navCtrl);
//                return;
//            }
//
//            //Don't remember, prompt for password
//            if (defaultAccount.DontRemember)
//            {
//                ShowAccountsAndSelectedUser(defaultAccount);
//            }
//            //If the user wanted to remember the account
//            else
//            {
//                try
//                {
//                    await Utils.Login.LoginAccount(defaultAccount, this);
//                }
//                catch (Exception e)
//                {
//                    //Wow, what a surprise that there's issues using await and a catch here...
//                    MonoTouch.Utilities.ShowAlert("Error".t(), e.Message, () => ShowAccountsAndSelectedUser(defaultAccount));
//                }
//            }
        }

        private void ShowAccountsAndSelectedUser(GitHubAccount account)
        {
//            var accountsController = new AccountsViewController();
//            accountsController.NavigationItem.LeftBarButtonItem = null;
//            var login = new LoginViewController(account);
//
//            var navigationController = new CustomNavigationController(this, accountsController);
//            navigationController.PushViewController(login, false);
//            Transitions.TransitionToController(navigationController);
        }
    }
}
