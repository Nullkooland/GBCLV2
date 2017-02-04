using System.Windows;
using System.Windows.Controls;
using KMCCC.Tools;

namespace GBCLV2.Controls
{
    public partial class LaunchSettings : Grid
    {
        private string versionID;

        public LaunchSettings()
        {
            InitializeComponent();
            this.DataContext = App.Config;
            VersionBox.ItemsSource = App.Versions;

            if (App.Config.Offline) OfflineMode();
            else OnlineMode();

            Offline_CheckBox.Checked += (s, e) => OfflineMode();
            Offline_CheckBox.Unchecked += (s, e) => OnlineMode();

            if(!string.IsNullOrEmpty(App.Config.ServerAddress))
            {
                DirectLoginServer_CheckBox.IsChecked = true;
                ServerAddressBox.Text = App.Config.ServerAddress;
            }
        }

        private void OnlineMode()
        {
            App.Config.Offline = false;
            PassWordBox.IsEnabled = true;
            RememberPassword_CheckBox.IsEnabled = true;
            PassWord_TextBlcok.IsEnabled = true;
            UserName_TextBlcok.Text = "邮箱";
            UserNameBox.Text = App.Config.Email;
            PassWordBox.Password = App.Config.PassWord;

        }

        private void OfflineMode()
        {
            App.Config.Offline = true;
            PassWordBox.Password = null;
            PassWordBox.IsEnabled = false;
            PassWord_TextBlcok.IsEnabled = false;
            RememberPassword_CheckBox.IsEnabled = false;
            UserName_TextBlcok.Text = "游戏名";
            UserNameBox.Text = App.Config.UserName;
        }

        public void Save_Settings()
        {
            if (App.Config.Offline)
            {
                App.Config.UserName = UserNameBox.Text;
            }
            else
            {
                App.Config.Email = UserNameBox.Text;
                App.Config.PassWord = PassWordBox.Password;
            }

            if(DirectLoginServer_CheckBox.IsChecked ?? false)
            {
                App.Config.ServerAddress = ServerAddressBox.Text;
            }
            else
            {
                App.Config.ServerAddress = null;
            }
        }

        private void ShowVersionOptions(object sender, RoutedEventArgs e)
        {
            if (VersionBox.SelectedIndex != -1)
            {
                versionID = App.Versions[App.Config.VersionIndex].ID;
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
                App.Config.VersionIndex = count - 1;
                Menu_OpenFolder.IsEnabled = true;
                Menu_OpenJson.IsEnabled = true;
                Menu_Delete.IsEnabled = true;
            }
        }

        private void OpenVersionFolder(object sender, RoutedEventArgs e)
        {
            if (App.Config.VersionIndex != -1)
            {
                string DirPath = $"{App.Core.GameRootPath}\\versions\\{versionID}\\";
                System.Diagnostics.Process.Start("explorer.exe", DirPath);
            }
        }

        private void OpenVersionJson(object sender, RoutedEventArgs e)
        {
            if (App.Config.VersionIndex != -1)
            {
                string JsonPath = $"{App.Core.GameRootPath}\\versions\\{versionID}\\{versionID}.json";
                try
                {
                    System.Diagnostics.Process.Start(JsonPath);
                }
                catch { }
            }
        }

        private void DeleteVersion(object sender, RoutedEventArgs e)
        {
            if (App.Config.VersionIndex != -1)
            {
                string DirPath = $"{App.Core.GameRootPath}\\versions\\{versionID}\\";
                SystemTools.DeleteDirectoryAsync(DirPath);

                App.Versions.RemoveAt(App.Config.VersionIndex);
                App.Config.VersionIndex = 0;
            }

        }

        private void Update_CurrentAvailableMemory(object sender, ToolTipEventArgs e)
        {
            MaxMemoryBox.ToolTip = $"当前可用物理内存：{SystemTools.GetAvailableMemory()} MB";
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
                App.Config.JavaPath = dialog.FileName;
            }
        }
    }
}
