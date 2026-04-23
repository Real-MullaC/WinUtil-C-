using System;
using System.Net.NetworkInformation;
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

        private void CheckInternetAndArrangeTabs()
        {
            bool hasInternet = IsConnected();

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

        /// <summary>
        /// Attempts to ping Cloudflare's DNS to verify active internet connection.
        /// </summary>
        private bool IsConnected()
        {
            try
            {
                using (Ping p = new Ping())
                {
                    // 1.1.1.1 is Cloudflare's DNS; 1500ms timeout
                    PingReply reply = p.Send("1.1.1.1", 1500);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                // If ping fails or network is unreachable
                return false;
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