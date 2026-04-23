using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WinUtil
{
    public partial class MainWindow : Window
    {
        // This is the source for your UI items
        public ObservableCollection<WingetApp> Apps { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this; // Necessary for {Binding} to work
            LoadData();
        }

        private void LoadData()
        {
            Apps.Add(new WingetApp { Name = "Google Chrome", AppId = "Google.Chrome", IconPath = "https://www.google.com/chrome/static/images/chrome-logo.svg" });
            Apps.Add(new WingetApp { Name = "Discord", AppId = "Discord.Discord", IconPath = "https://assets-global.website-files.com/6257adef93867e3c84519eb1/6257adef93867e0763519f43_847541504914fd33810e70a0ea73177e.ico" });
            Apps.Add(new WingetApp { Name = "Visual Studio Code", AppId = "Microsoft.VisualStudioCode", IconPath = "https://code.visualstudio.com/apple-touch-icon.png" });
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: WingetApp app })
            {
                app.IsInstalling = true;
                app.IsIndeterminate = true;

                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = $"install --id {app.AppId} --silent --accept-source-agreements --accept-package-agreements",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process? process = Process.Start(startInfo))
                    {
                        if (process != null)
                        {
                            await Task.Delay(2000); // Simulated delay for visual feedback
                            app.IsIndeterminate = false;
                            app.InstallProgress = 100;
                            await process.WaitForExitAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to start winget: {ex.Message}");
                }
                finally
                {
                    app.IsInstalling = false;
                }
            }
        }
    }

    // This class MUST be public so XAML can see it
    public class WingetApp : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;

        private bool _isInstalling;
        public bool IsInstalling
        {
            get => _isInstalling;
            set
            {
                _isInstalling = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ButtonVisibility));
                OnPropertyChanged(nameof(ProgressVisibility));
            }
        }

        public Visibility ButtonVisibility => IsInstalling ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ProgressVisibility => IsInstalling ? Visibility.Visible : Visibility.Collapsed;

        private double _installProgress;
        public double InstallProgress { get => _installProgress; set { _installProgress = value; OnPropertyChanged(); } }

        private bool _isIndeterminate;
        public bool IsIndeterminate { get => _isIndeterminate; set { _isIndeterminate = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}