﻿using System;
using ReactiveUI;
using GitHubSharp.Models;
using CodeHub.Core.Utilities;
using Humanizer;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class CommitItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        private readonly Lazy<string> _description;

        public string Name { get; private set; }

        public GitHubAvatar Avatar { get; private set; }

        public string Description
        {
            get { return _description.Value; }
        }

        public string Time { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        internal CommitModel Commit { get; private set; }

        internal CommitItemViewModel(CommitModel commit, Action<CommitItemViewModel> action)
        {
            var msg = commit.With(x => x.Commit).With(x => x.Message, () => string.Empty);
            var firstLine = msg.IndexOf("\n", StringComparison.Ordinal);
            var description = firstLine > 0 ? msg.Substring(0, firstLine) : msg;
            _description = new Lazy<string>(() => Emojis.FindAndReplace(description));

            var time = DateTimeOffset.MinValue;
            if (commit.Commit.Committer != null)
                time = commit.Commit.Committer.Date;
            Time = time.UtcDateTime.Humanize();

            Name = commit.GenerateCommiterName();
            Avatar = new GitHubAvatar(commit.GenerateGravatarUrl());
            Commit = commit;
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => action(this));
        }
    }
}

