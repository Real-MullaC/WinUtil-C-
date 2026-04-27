using System.Net.Http;
using System.Windows;
using System.Windows.Input;

namespace WinUtil
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Run the connection check after the UI has initialized
            CheckInternetAndArrangeTabs();
        }

        private async void CheckInternetAndArrangeTabs()
        {
            bool hasInternet = await IsConnected();

            if (hasInternet)
            {
                // If internet is found, default to the INSTALL tab (Index 0)
                if (MainTabs != null)
                {
                    MainTabs.SelectedIndex = 0;
                }
            }
            else
            {
                // If no internet, remove the Install tab entirely
                // This ensures the user can only see and use the TWEAKS tab
                if (MainTabs != null && InstallTab != null)
                {
                    MainTabs.Items.Remove(InstallTab);

                    // After removal, the Tweaks tab becomes the new Index 0
                    MainTabs.SelectedIndex = 0;
                }
            }
        }
        private async Task<bool> IsConnected()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    // We send a request to a reliable endpoint
                    var response = await client.GetAsync("https://google.com");

                    // Returns true if the status code is 200-299
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    // If the request fails (no internet, DNS issues, etc.)
                    return false;
                }
            }
        }

        // --- Window Control Logic ---

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allows the user to drag the custom window
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}