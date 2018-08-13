using System.Windows;
using System.Windows.Controls;
using KMCCC.Tools;
using GBCLV2.Modules;
using System.Linq;

namespace GBCLV2.Controls
{
    public partial class LaunchSettings : Grid
    {
        public LaunchSettings()
        {
            InitializeComponent();
        }

        private void LaunchSettings_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = Config.Args;
            _passWordBox.Password = Config.Args.PassWord;
        }

        private void ShowVersionOptions(object sender, RoutedEventArgs e)
        {
            VersionOptionsMenu.PlacementTarget = (sender as Button);
            VersionOptionsMenu.IsOpen = true;
        }

        private void RefreshVersion(object sender, RoutedEventArgs e)
        {
            App.LoadVersions();
        }

        private void OpenVersionFolder(object sender, RoutedEventArgs e)
        {
            string DirPath = $"{App.Core.GameRootPath}\\versions\\{Config.Args.SelectedVersion.ID}\\";
            System.Diagnostics.Process.Start("explorer.exe", DirPath);
        }

        private void OpenVersionJson(object sender, RoutedEventArgs e)
        {
            string JsonPath = $"{App.Core.GameRootPath}\\versions\\{Config.Args.SelectedVersion.ID}\\{Config.Args.SelectedVersion.ID}.json";
            try
            {
                System.Diagnostics.Process.Start(JsonPath);
            }
            catch { }
        }

        private void DeleteVersion(object sender, RoutedEventArgs e)
        {
            string DirPath = $"{App.Core.GameRootPath}\\versions\\{Config.Args.SelectedVersion.ID}\\";
            UsefulTools.DeleteDirectoryAsync(DirPath);

            if (Config.Args.SelectedVersion.ID.Contains("forge"))
            {
                var forgeDir = $"{App.Core.GameRootPath}\\libraries\\{System.IO.Path.GetDirectoryName(Config.Args.SelectedVersion.Libraries[0].Path)}";
                UsefulTools.DeleteDirectoryAsync(forgeDir);
            }

            Config.Args.Versions.RemoveAt(Config.Args.VersionIndex);
            Config.Args.VersionIndex = Config.Args.Versions.Any() ? 0 : -1;          
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
            Config.Args.PassWord = _passWordBox.Password;
        }
    }
}
