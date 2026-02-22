using System.Windows;

namespace ReserV7.Views.Windows
{
    public partial class DialogWindow : Window
    {
        public enum DialogType
        {
            Information,
            Warning,
            Error,
            Confirmation
        }

        public enum CustomDialogResult
        {
            None,
            Yes,
            No,
            Cancel
        }

        public CustomDialogResult DialogResult { get; set; }

        public DialogWindow(string title, string message, DialogType type = DialogType.Information, bool showSecondaryButton = false)
        {
            InitializeComponent();
            Title = title;
            DataContext = new { Title = title, Message = message };

            // Customize button labels based on dialog type
            PrimaryButton.Content = type == DialogType.Confirmation ? "Oui" : "OK";
            PrimaryButton.Focus();

            if (showSecondaryButton)
            {
                SecondaryButton.Visibility = Visibility.Visible;
                SecondaryButton.Content = type == DialogType.Confirmation ? "Non" : "Annuler";
            }

            // Set initial DialogResult
            DialogResult = CustomDialogResult.None;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = CustomDialogResult.Cancel;
            Close();
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = CustomDialogResult.Yes;
            Close();
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = CustomDialogResult.No;
            Close();
        }
    }
}
