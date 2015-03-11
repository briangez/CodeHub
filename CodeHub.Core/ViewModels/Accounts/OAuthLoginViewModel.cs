﻿using System;
using CodeHub.Core.Data;
using ReactiveUI;
using CodeHub.Core.Services;
using CodeHub.Core.Messages;
using CodeHub.Core.Factories;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class OAuthLoginViewModel : BaseViewModel
    {
        private readonly static string ApiDomain = "https://api.github.com";
        private readonly static string WebDomain = "https://github.com";

        public IReactiveCommand<GitHubAccount> LoginCommand { get; private set; }

        private string _token;
        public string Token
        {
            get { return _token; }
            set { this.RaiseAndSetIfChanged(ref _token, value); }
        }

        public OAuthLoginViewModel(
            ILoginService loginFactory, 
            IAccountsRepository accountsRepository,
            IAlertDialogFactory alertDialogFactory)
        {
            Title = "Login";

            var canLogin = this.WhenAnyValue(y => y.Token, (x) => !string.IsNullOrEmpty(x));
            LoginCommand = ReactiveCommand.CreateAsyncTask(canLogin, async _ => 
            {
                var account = await loginFactory.Authenticate(ApiDomain, WebDomain, Token, false);
                await accountsRepository.SetDefault(account);
                return account;
            });

            LoginCommand.IsExecuting.Skip(1).Subscribe(x =>
            {
                if (x)
                    alertDialogFactory.Show("Logging in...");
                else
                    alertDialogFactory.Hide();
            });

            LoginCommand.Subscribe(x => MessageBus.Current.SendMessage(new LogoutMessage()));
        }
    }
}

