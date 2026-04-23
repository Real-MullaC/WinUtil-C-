using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WinUtil.Models
{
    public class WingetApp : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        private bool _isInstalling;
        public bool IsInstalling { get => _isInstalling; set { _isInstalling = value; OnPropertyChanged(); OnPropertyChanged(nameof(ButtonVisibility)); OnPropertyChanged(nameof(ProgressVisibility)); } }
        public Visibility ButtonVisibility => IsInstalling ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ProgressVisibility => IsInstalling ? Visibility.Visible : Visibility.Collapsed;
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class Tweak : INotifyPropertyChanged
    {
        private bool _isSelected;
        public string Content { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string ps1FileName { get; set; } = string.Empty;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}