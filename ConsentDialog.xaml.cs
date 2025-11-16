using System.Windows;

namespace RemoteAdminClientUI
{
    public partial class ConsentDialog : Window
    {
        public ConsentDialog(string operatorName, string sessionId)
        {
            InitializeComponent();
            MessageText.Text = 
                $"Operator '{operatorName}' is requesting remote control.\n\nSession ID: {sessionId}";
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Decline_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
