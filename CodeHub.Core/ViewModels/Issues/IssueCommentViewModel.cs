﻿using System;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueCommentViewModel : MarkdownComposerViewModel
    {
        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }

        public int Id { get; set; }

        public IReactiveCommand<Octokit.IssueComment> SaveCommand { get; private set; }

        public IssueCommentViewModel(IApplicationService applicationService, IImgurService imgurService, 
            IMediaPickerFactory mediaPicker, IAlertDialogFactory alertDialogFactory) 
            : base(imgurService, mediaPicker, alertDialogFactory)
        {
            Title = "Add Comment";
            SaveCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Text).Select(x => !string.IsNullOrEmpty(x)),
                t => applicationService.GitHubClient.Issue.Comment.Create(RepositoryOwner, RepositoryName, Id, Text));
            SaveCommand.Subscribe(x => Dismiss());
        }
    }
}

