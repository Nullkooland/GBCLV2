using System.Windows;
using System.Windows.Controls;
using KMCCC.Tools;
using KMCCC.Launcher;
using GBCLV2.Modules;

namespace GBCLV2.Controls
{
    public partial class LaunchSettings : Grid
    {
        private Version version;

        public LaunchSettings()
        {
            InitializeComponent();
        }

        private void LaunchSettings_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = Config.Args;
            VersionBox.ItemsSource = App.Versions;
            PassWordBox.Password = Config.Args.PassWord;
        }

        private void ShowVersionOptions(object sender, RoutedEventArgs e)
        {
            if (VersionBox.SelectedIndex != -1)
            {
                version = App.Versions[Config.Args.VersionIndex];
            }
            else
            {
                Menu_OpenFolder.IsEnabled = false;
                Menu_OpenJson.IsEnabled = false;
                Menu_Delete.IsEnabled = false;
            }
            VersionOptions.IsOpen = true;
        }

        private void RefreshVersion(object sender, RoutedEventArgs e)
        {
            int count = 0;
            App.Versions.Clear();
            foreach (var version in App.Core.GetVersions())
            {
                App.Versions.Add(version);
                count++;
            }
            if (count != 0)
            {
                Config.Args.VersionIndex = count - 1;
                Menu_OpenFolder.IsEnabled = true;
                Menu_OpenJson.IsEnabled = true;
                Menu_Delete.IsEnabled = true;
            }
        }

        private void OpenVersionFolder(object sender, RoutedEventArgs e)
        {
            if (Config.Args.VersionIndex != -1)
            {
                string DirPath = $"{App.Core.GameRootPath}\\versions\\{version.ID}\\";
                System.Diagnostics.Process.Start("explorer.exe", DirPath);
            }
        }

        private void OpenVersionJson(object sender, RoutedEventArgs e)
        {
            if (Config.Args.VersionIndex != -1)
            {
                string JsonPath = $"{App.Core.GameRootPath}\\versions\\{version.ID}\\{version.ID}.json";
                try
                {
                    System.Diagnostics.Process.Start(JsonPath);
                }
                catch { }
            }
        }

        private void DeleteVersion(object sender, RoutedEventArgs e)
        {
            if (Config.Args.VersionIndex != -1)
            {
                string DirPath = $"{App.Core.GameRootPath}\\versions\\{version.ID}\\";
                UsefulTools.DeleteDirectoryAsync(DirPath);

                if (version.ID.Contains("forge"))
                {
                    var forgeDir = $"{App.Core.GameRootPath}\\libraries\\{System.IO.Path.GetDirectoryName(version.Libraries[0].Path)}";
                    UsefulTools.DeleteDirectoryAsync(forgeDir);
                }

                App.Versions.RemoveAt(Config.Args.VersionIndex);
                Config.Args.VersionIndex = 0;
            }

        }

        private void Update_CurrentAvailableMemory(object sender, ToolTipEventArgs e)
        {
            (sender as TextBox).ToolTip = $"当前可用物理内存：{SystemTools.GetAvailableMemory()} MB";
        }

        private void GetJavaPathFromDisk(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "请选择Java路径",
                Filter = "Java运行环境| javaw.exe; java.exe",
            };

            if (dialog.ShowDialog() ?? false)
            {
                Config.Args.JavaPath = dialog.FileName;
            }
        }

        private void UpdatePassWordToConfig(object sender, RoutedEventArgs e)
        {
            Config.Args.PassWord = (sender as PasswordBox).Password;
        }
    }
}
