using CommunityToolkit.Mvvm.ComponentModel;

namespace GameTracker.App.Models.Notifications
{
    public partial class ToastMessage : ObservableObject
    {
        public string Text { get; }
        public ToastSeverity Severity { get; }

       public ToastMessage(string text, ToastSeverity severity = ToastSeverity.Info)
        {
            Text = text;
            Severity = severity;
        }
    }
}

public enum ToastSeverity { Info, Success, Error}
