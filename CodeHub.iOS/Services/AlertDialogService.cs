using System;
using CodeHub.Core.Services;
using UIKit;
using System.Threading.Tasks;

namespace CodeHub.iOS.Services
{
    public class AlertDialogService : IAlertDialogService
    {
        public Task<bool> PromptYesNo(string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();
            var alert = new UIAlertView {Title = title, Message = message};
            alert.CancelButtonIndex = alert.AddButton("No");
            var ok = alert.AddButton("Yes");
            alert.Clicked += (sender, e) => tcs.SetResult(e.ButtonIndex == ok);
            alert.Show();
            return tcs.Task;
        }

        public Task Alert(string title, string message)
        {
            var tcs = new TaskCompletionSource<object>();
            ShowAlert(title, message, () => tcs.SetResult(null));
            return tcs.Task;
        }

        public static void ShowAlert(string title, string message, Action dismissed = null)
        {
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, x => {
                dismissed?.Invoke();
                alert.Dispose();
            }));
            UIApplication.SharedApplication.KeyWindow.GetVisibleViewController().PresentViewController(alert, true, null);
        }

        public Task<string> PromptTextBox(string title, string message, string defaultValue, string okTitle)
        {
            var tcs = new TaskCompletionSource<string>();
            var alert = new UIAlertView();
            alert.Title = title;
            alert.Message = message;
            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
            var cancelButton = alert.AddButton("Cancel".t());
            var okButton = alert.AddButton(okTitle);
            alert.CancelButtonIndex = cancelButton;
            alert.DismissWithClickedButtonIndex(cancelButton, true);
            alert.GetTextField(0).Text = defaultValue;
            alert.Clicked += (s, e) =>
            {
                if (e.ButtonIndex == okButton)
                    tcs.SetResult(alert.GetTextField(0).Text);
                else
                    tcs.SetCanceled();
            };
            alert.Show();
            return tcs.Task;
        }
    }
}

