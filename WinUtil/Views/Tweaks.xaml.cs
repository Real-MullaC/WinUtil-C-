using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using WinUtil.Models;

namespace WinUtil.Views
{
    public partial class Tweaks : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<Tweak> TweakList { get; set; } = new();
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private readonly string _cache = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tweaks_cache.json");

        public Tweaks()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += async (s, e) => { if (TweakList.Count == 0) await LoadTweaks(); };
        }

        // Logic for splitting the JSON data into the UI columns
        public IEnumerable<Tweak> EssentialTweaksMaster => TweakList.Where(t => t.category.Contains("Essential"));
        public IEnumerable<Tweak> EssentialTweaksLeft => EssentialTweaksMaster.Take((EssentialTweaksMaster.Count() + 1) / 2);
        public IEnumerable<Tweak> EssentialTweaksRight => EssentialTweaksMaster.Skip((EssentialTweaksMaster.Count() + 1) / 2);

        public IEnumerable<Tweak> AdvancedTweaksMaster => TweakList.Where(t => t.category.Contains("Advanced"));
        public IEnumerable<Tweak> AdvancedTweaksLeft => AdvancedTweaksMaster.Take((AdvancedTweaksMaster.Count() + 1) / 2);
        public IEnumerable<Tweak> AdvancedTweaksRight => AdvancedTweaksMaster.Skip((AdvancedTweaksMaster.Count() + 1) / 2);

        public IEnumerable<Tweak> PreferenceTweaks => TweakList.Where(t => t.category.Contains("Customize") || t.category.Contains("Preference"));

        private async Task LoadTweaks()
        {
            string json = "";
            try { json = await _http.GetStringAsync("https://raw.githubusercontent.com/Real-MullaC/WinUtil-C-/refs/heads/main/Data/tweaks.json"); File.WriteAllText(_cache, json); }
            catch { if (File.Exists(_cache)) json = File.ReadAllText(_cache); }

            if (!string.IsNullOrEmpty(json))
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, Tweak>>(json);
                if (data != null)
                {
                    await Dispatcher.InvokeAsync(() => {
                        TweakList.Clear();
                        foreach (var v in data.Values) TweakList.Add(v);
                        RefreshAllLists();
                    });
                }
            }
        }

        private void RefreshAllLists()
        {
            OnPropertyChanged(nameof(EssentialTweaksLeft)); OnPropertyChanged(nameof(EssentialTweaksRight));
            OnPropertyChanged(nameof(AdvancedTweaksLeft)); OnPropertyChanged(nameof(AdvancedTweaksRight));
            OnPropertyChanged(nameof(PreferenceTweaks));
        }

        private void ClearTweaks_Click(object sender, RoutedEventArgs e) { foreach (var t in TweakList) t.IsSelected = false; }
        private void AddPerformancePlan_Click(object sender, RoutedEventArgs e) { }
        private void RemovePerformancePlan_Click(object sender, RoutedEventArgs e) { }
        private void ApplyTweaks_Click(object sender, RoutedEventArgs e) { }
        private void UndoTweaks_Click(object sender, RoutedEventArgs e) { }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}