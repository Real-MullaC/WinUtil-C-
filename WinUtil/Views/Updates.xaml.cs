using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32.TaskScheduler;

namespace WinUtil.Views
{
    public partial class Updates
    {
        private void DefaultSettings(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteRegistryKeyTree(@"SOFTWARE\Policies\Microsoft\Windows", "WindowsUpdate");
                DeleteRegistryKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion", "DeliveryOptimization");
                DeleteRegistryKeyTree(@"SOFTWARE\Microsoft\WindowsUpdate\UX", "Settings");
                DeleteRegistryKeyTree(@"SOFTWARE\Policies\Microsoft\Windows", "DriverSearching");
                DeleteRegistryKeyTree(@"SOFTWARE\Policies\Microsoft\Windows", "Device Metadata");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registry Error: {ex.Message}");
            }

            RunServiceConfig("BITS", "demand");
            RunServiceConfig("wuauserv", "demand");
            RunServiceConfig("UsoSvc", "auto");
            RunServiceConfig("WaaSMedicSvc", "demand");

            string[] taskPaths = {
        @"\Microsoft\Windows\InstallService\",
        @"\Microsoft\Windows\UpdateOrchestrator\",
        @"\Microsoft\Windows\UpdateAssistant\",
        @"\Microsoft\Windows\WaaSMedic\",
        @"\Microsoft\Windows\WindowsUpdate\",
        @"\Microsoft\WindowsUpdate\"
    };

            foreach (string path in taskPaths)
            {
                EnableTasksNative(path);
            }

            ResetLocalPolicies();

            MessageBox.Show("Windows Update Defaults Restored!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DisableSettings(object sender, RoutedEventArgs e) 
        {
            try
            {
                NewRegistrySubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "AU");
                NewRegistryKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", RegistryValueKind.DWord, 1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registry Error: {ex.Message}");
            }
        }
        private void SecuritySettings(object sender, RoutedEventArgs e) {  }

        private void DeleteRegistryKeyTree(string parentPath, string subKey)
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(parentPath, true);
            if (key != null)
            {
                key.DeleteSubKeyTree(subKey, false);
            }
        }

        private void NewRegistrySubKey(string parentPath, string subKey)
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(parentPath, true);
            if (key != null)
            {
                key.CreateSubKey(subKey, false);
            }
        }

        private void NewRegistryKey(string subKey, string subValue, RegistryValueKind subType, object subData)
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(subKey, true);
            if (key != null)
            {
                object formattedData = subType switch
                {
                    RegistryValueKind.DWord => Convert.ToInt32(subData),

                    RegistryValueKind.QWord => Convert.ToInt64(subData),

                    RegistryValueKind.MultiString => (subData is string[] array) ? array : new[] { subData.ToString() ?? "" },

                    _ => subData.ToString() ?? ""
                };

                key.SetValue(subValue, formattedData, subType);
            }
        }

        private void RunServiceConfig(string serviceName, string startType)
        {
            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "sc.exe",
                    Arguments = $"config {serviceName} start= {startType}",
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Service Error ({serviceName}): {ex.Message}");
            }
        }
        private void ResetLocalPolicies()
        {
            try
            {
                string? winDir = Environment.GetEnvironmentVariable("SystemRoot");

                if (string.IsNullOrEmpty(winDir)) return;

                string arguments = $"/configure /cfg \"{winDir}\\inf\\defltbase.inf\" /db defltbase.sdb /quiet";

                ProcessStartInfo psi = new()
                {
                    FileName = "secedit.exe",
                    Arguments = arguments,
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };

                Process? process = Process.Start(psi);

                process?.WaitForExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Secedit Error: {ex.Message}");
            }
        }

        private void EnableTasksNative(string rootPath)
        {
            try
            {
                using (Microsoft.Win32.TaskScheduler.TaskService ts = new Microsoft.Win32.TaskScheduler.TaskService())
                {
                    string cleanPath = rootPath.TrimEnd('*').TrimEnd('\\');

                    Microsoft.Win32.TaskScheduler.TaskFolder folder = ts.GetFolder(cleanPath);
                    if (folder != null)
                    {
                        foreach (Microsoft.Win32.TaskScheduler.Task task in folder.Tasks)
                        {
                            task.Enabled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Native Task Error on {rootPath}: {ex.Message}");
            }
        }
    }
}