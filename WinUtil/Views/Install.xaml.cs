using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace WinUtil.Views
{
    public partial class Install : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<AppItem> AllApps { get; set; } = new();
        public ObservableCollection<AppItem> SelectedApps { get; set; } = new();

        private List<AppItem> FeaturedApps = new();
        private HashSet<string> BlockedIds = new();
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        public Install()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += async (s, e) => {
                await LoadBlockedApps(); // Load blocked first
                await LoadFeaturedApps();
            };
        }

        private async Task LoadBlockedApps()
        {
            try
            {
                string url = "https://raw.githubusercontent.com/Real-MullaC/WinUtil-C-/refs/heads/main/Data/blockedapp.json";
                string json = await _http.GetStringAsync(url);
                var ids = JsonConvert.DeserializeObject<List<string>>(json);
                if (ids != null) BlockedIds = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
            }
            catch { /* Block list failed to load */ }
        }

        private async Task LoadFeaturedApps()
        {
            try
            {
                string url = "https://raw.githubusercontent.com/Real-MullaC/WinUtil-C-/refs/heads/main/Data/featuredapps.json";
                string json = await _http.GetStringAsync(url);
                var ids = JsonConvert.DeserializeObject<List<string>>(json);

                FeaturedApps.Clear();
                if (ids != null)
                {
                    foreach (var id in ids)
                    {
                        // Even featured apps check the block list for safety
                        if (!BlockedIds.Contains(id))
                            FeaturedApps.Add(new AppItem { Id = id, Name = id, Parent = this });
                    }
                }
                if (string.IsNullOrWhiteSpace(SearchBox.Text)) ShowFeatured();
            }
            catch { /* Silently fail or use defaults */ }
        }

        private void ShowFeatured()
        {
            AllApps.Clear();
            foreach (var app in FeaturedApps)
            {
                app.IsSelected = SelectedApps.Any(s => s.Id == app.Id);
                AllApps.Add(app);
            }
            StatusLabel.Text = "Featured Apps";
        }

        public void UpdateGlobalSelection(AppItem item)
        {
            if (item.IsSelected)
            {
                if (!SelectedApps.Any(a => a.Id == item.Id))
                    SelectedApps.Add(item);
            }
            else
            {
                var existing = SelectedApps.FirstOrDefault(a => a.Id == item.Id);
                if (existing != null) SelectedApps.Remove(existing);
            }
        }

        private void RemoveSelection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is AppItem item)
            {
                item.IsSelected = false;
                // Force UI update for the grid if the item is currently visible
                var visible = AllApps.FirstOrDefault(a => a.Id == item.Id);
                if (visible != null) visible.IsSelected = false;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) ShowFeatured();
        }

        private async void Search_Click(object sender, RoutedEventArgs e) => await PerformWingetSearch();

        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) await PerformWingetSearch();
        }

        private async Task PerformWingetSearch()
        {
            string query = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(query)) { ShowFeatured(); return; }

            StatusLabel.Text = $"Results for '{query}'";
            AllApps.Clear();

            await Task.Run(async () =>
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = $"search \"{query}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8
                    };

                    using (Process? process = Process.Start(psi))
                    {
                        if (process == null) return;
                        string output = await process.StandardOutput.ReadToEndAsync();
                        process.WaitForExit();

                        var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                        await Dispatcher.InvokeAsync(() =>
                        {
                            bool startParsing = false;
                            foreach (var line in lines)
                            {
                                if (line.Contains("---")) { startParsing = true; continue; }
                                if (!startParsing) continue;

                                var parts = line.Split(new[] { "  " }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(p => p.Trim()).ToList();

                                if (parts.Count >= 2)
                                {
                                    string id = parts[1];

                                    // CHECK: Blocked List
                                    if (BlockedIds.Contains(id)) continue;

                                    var app = new AppItem { Name = parts[0], Id = id, Parent = this };
                                    app.IsSelected = SelectedApps.Any(s => s.Id == app.Id);
                                    AllApps.Add(app);
                                }
                            }
                        });
                    }
                }
                catch { /* Handle Error */ }
            });
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            if (!SelectedApps.Any()) return;
            string ids = string.Join(" ", SelectedApps.Select(a => $"\"{a.Id}\""));
            Process.Start(new ProcessStartInfo { FileName = "powershell", Arguments = $"-NoExit -Command \"winget install {ids} --accept-source-agreements --accept-package-agreements\"", UseShellExecute = true });
        }

        private void UpgradeAll_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = "powershell", Arguments = "-NoExit -Command \"winget upgrade --all\"", UseShellExecute = true });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class AppItem : INotifyPropertyChanged
    {
        public string? Name { get; set; }
        public string? Id { get; set; }
        public Install? Parent { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                Parent?.UpdateGlobalSelection(this);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}