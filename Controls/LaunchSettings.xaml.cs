using System.Windows;
using System.Windows.Controls;
using KMCCC.Tools;

namespace GBCLV2.Controls
{
    /// <summary>
    /// LaunchSettings.xaml 的交互逻辑
    /// </summary>
    public partial class LaunchSettings : Grid
    {
        private string versionID;
        private uint AvailableMemory = SystemTools.GetAvailableMemory();

        public LaunchSettings()
        {
            InitializeComponent();

            VersionComboBox.ItemsSource = App.Versions;
            VersionComboBox.SelectedIndex = Config.VersionIndex;
            Offline_CheckBox.IsChecked = Config.Offline;
            if (Config.Offline) OfflineMode();
            else OnlineMode();

            Offline_CheckBox.Checked += (s, e) => OfflineMode();
            Offline_CheckBox.Unchecked += (s, e) => OnlineMode();

            JavaPathBox.Text = Config.JavaPath;
            MaxMemoryBox.Text = Config.MaxMemory.ToString();
            WinWidthBox.Text = Config.WinWidth.ToString();
            WinHeightBox.Text = Config.WinHeight.ToString();
            FullScreen_CheckBox.IsChecked = Config.FullScreen;
        }

        private void OnlineMode()
        {
            Config.Offline = false;
            PassWordBox.IsEnabled = true;
            RememberPassword_CheckBox.IsEnabled = true;
            PassWord_TextBlcok.Opacity = 1.0;
            UserName_TextBlcok.Text = "邮箱";
            UserNameBox.Text = Config.Email;
            PassWordBox.Password = Config.PassWord;
            RememberPassword_CheckBox.IsChecked = Config.RememberPassWord;
        }

        private void OfflineMode()
        {
            Config.Offline = true;
            PassWordBox.Password = null;
            PassWordBox.IsEnabled = false;
            PassWord_TextBlcok.Opacity = 0.3;
            RememberPassword_CheckBox.IsChecked = false;
            RememberPassword_CheckBox.IsEnabled = false;
            UserName_TextBlcok.Text = "游戏名";
            UserNameBox.Text = Config.UserName;
        }

        public void Save_Settings()
        {
            App.Core.JavaPath = JavaPathBox.Text;
            Config.VersionIndex = VersionComboBox.SelectedIndex;
            Config.FullScreen = FullScreen_CheckBox.IsChecked ?? false;
            if (Config.Offline)
            {
                Config.UserName = UserNameBox.Text;
            }
            else
            {
                Config.Email = UserNameBox.Text;
                Config.PassWord = PassWordBox.Password;
                Config.RememberPassWord = RememberPassword_CheckBox.IsChecked ?? false;
            }
        }

        private void ShowVersionOptions(object sender, RoutedEventArgs e)
        {
            if (VersionComboBox.SelectedIndex != -1)
            {
                versionID = App.Versions[VersionComboBox.SelectedIndex].ID;
            }
            else
            {
                Menu_OpenFolder.IsEnabled = false;
                Menu_OpenJson.IsEnabled = false;
                Menu_Delete.IsEnabled = false;
            }
            VersionOptions.IsOpen = true;
        }

        private void Update_MaxMemory(object sender, RoutedEventArgs e)
        {
            try
            {
                Config.MaxMemory = uint.Parse(MaxMemoryBox.Text);
                Config.MaxMemory = Config.MaxMemory > 1024 ? Config.MaxMemory : 1024;
                Config.MaxMemory = Config.MaxMemory < AvailableMemory ? Config.MaxMemory : AvailableMemory;
                MaxMemoryBox.Text = Config.MaxMemory.ToString();
            }
            catch
            {
                MaxMemoryBox.Text = Config.MaxMemory.ToString();
            }
        }

        private void Update_WindowSize(object sender, RoutedEventArgs e)
        {
            try
            {
                Config.WinWidth = ushort.Parse(WinWidthBox.Text);
            }
            catch
            {
                WinWidthBox.Text = Config.WinWidth.ToString();
            }

            try
            {
                Config.WinHeight = ushort.Parse(WinHeightBox.Text);
            }
            catch
            {
                WinHeightBox.Text = Config.WinHeight.ToString();
            }
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
                VersionComboBox.SelectedIndex = count - 1;
                Menu_OpenFolder.IsEnabled = true;
                Menu_OpenJson.IsEnabled = true;
                Menu_Delete.IsEnabled = true;
            }
        }

        private void OpenVersionFolder(object sender, RoutedEventArgs e)
        {
            if (VersionComboBox.SelectedIndex != -1)
            {
                string Path = string.Format(@"{0}\versions\{1}\", App.Core.GameRootPath, versionID);
                System.Diagnostics.Process.Start("explorer.exe", Path);
            }
        }

        private void OpenVersionJson(object sender, RoutedEventArgs e)
        {
            if (VersionComboBox.SelectedIndex != -1)
            {
                string Path = string.Format(@"{0}\versions\{1}\{1}.json", App.Core.GameRootPath, versionID);
                try
                {
                    System.Diagnostics.Process.Start(Path);
                }
                catch { }
            }
        }

        private void DeleteVersion(object sender, RoutedEventArgs e)
        {
            if (VersionComboBox.SelectedIndex != -1)
            {
                string Path = string.Format(@"{0}\versions\{1}\", App.Core.GameRootPath, versionID);
                SystemTools.DeleteDirectoryAsync(Path);

                App.Versions.RemoveAt(VersionComboBox.SelectedIndex);
                VersionComboBox.SelectedIndex = 0;
            }

        }

        private void Update_CurrentAvailableMemory(object sender, ToolTipEventArgs e)
        {
            AvailableMemory = SystemTools.GetAvailableMemory();
            MaxMemoryBox.ToolTip = string.Format("当前可用物理内存：{0} MB", AvailableMemory);
        }

        private void GetJavaPathFromDisk(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "请选择Java路径",
                Filter = "Java运行环境|javaw.exe",
            };

            if (dialog.ShowDialog() ?? false)
            {
                JavaPathBox.Text = dialog.FileName;
            }
        }
    }
}
