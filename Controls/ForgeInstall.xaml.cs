using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using LitJson;
using System.Windows.Controls;
using GBCLV2.Modules;

namespace GBCLV2.Controls
{
    public partial class ForgeInstall : Grid
    {
        private const string _forgeListUrl = "https://bmclapi2.bangbang93.com/forge/minecraft/";
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = new TimeSpan(0, 0, 5) };
        private string _mcVersion;

        class ForgeInfo
        {
            public string Branch { get; set; }
            public string Version { get; set; }
            public string ModifiedTime { get; set; }
        }

        private static List<ForgeInfo> VersionForges = new List<ForgeInfo>();

        public ForgeInstall()
        {
            InitializeComponent();
            this.DataContext = Config.Args;
            ForgeList.ItemsSource = VersionForges;
        }

        private async void GetVersionForgeListAsync(object sender, SelectionChangedEventArgs e)
        {
            if (Config.Args.VersionIndex == -1) return;

            _mcVersion = Config.Args.SelectedVersion.JarID;

            string json;
            try
            {
                statusBox.Text = $"正在获取 {_mcVersion} 版本Forge列表";
                json = await _httpClient.GetStringAsync(_forgeListUrl + _mcVersion);
            }
            catch
            {
                statusBox.Text = $"获取 {_mcVersion} 版本Forge列表失败";
                return;
            }

            VersionForges.Clear();

            var allForge = JsonMapper.ToObject(json);
            for (int i = allForge.Count - 1; i >= 0; i--)
            {
                VersionForges.Add(new ForgeInfo
                {
                    Branch = allForge[i]["branch"]?.ToString(),
                    Version = allForge[i]["version"].ToString(),
                    ModifiedTime = allForge[i]["modified"].ToString()
                });
            }

            if (VersionForges.Count != 0)
            {
                statusBox.Text = $"获取 {_mcVersion} 版本Forge列表成功";
            }
            else
            {
                statusBox.Text = $"{_mcVersion} 版本并没有可用的Forge";
            }

            ForgeList.Items.Refresh();
        }

        private async void InstallForgeAsync(object sender, RoutedEventArgs e)
        {
            if (ForgeList.SelectedIndex == -1)
            {
                MessageBox.Show("请选择要安装的Forge版本!", "(｡•ˇ‸ˇ•｡)", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            download_btn.IsEnabled = false;
            var core = App.Core;

            var forge = VersionForges[ForgeList.SelectedIndex];
            var forgeName = $"{_mcVersion}-{forge.Version}" + (forge.Branch == null ? null : $"-{forge.Branch}");

            var forgeJarPath = $"{core.GameRootPath}\\libraries\\net\\minecraftforge\\forge\\{forgeName}\\forge-{forgeName}.jar";

            var forgeDownload = new List<DownloadInfo>()
            {
                new DownloadInfo
                {
                    Path = forgeJarPath,
                    Url = $"{DownloadHelper.BaseUrl.ForgeBaseUrl}{forgeName}/forge-{forgeName}-universal.jar",
                }
            };

            var downloadPage = new Pages.DownloadPage();
            (Application.Current.MainWindow.FindName("Frame") as Frame).Navigate(downloadPage);
            bool hasDownloadSucceeded = await downloadPage.StartDownloadAsync(forgeDownload, "下载Forge");

            if (!hasDownloadSucceeded)
            {
                MessageBox.Show($"下载 {_mcVersion} 版本Forge失败");
                download_btn.IsEnabled = true;
                return;
            }

            var newVersionID = $"{_mcVersion}-forge{forgeName}";
            var newVersionPath = $"{core.GameRootPath}\\versions\\{newVersionID}";
            try
            {
                if (!Directory.Exists(newVersionPath))
                {
                    Directory.CreateDirectory(newVersionPath);
                }

                using (var archive = ZipFile.OpenRead(forgeJarPath))
                {
                    archive.GetEntry("version.json").ExtractToFile($"{newVersionPath}\\{newVersionID}.json");
                }
            }
            catch
            {
                MessageBox.Show($"安装 {_mcVersion} 版本Forge失败");
                download_btn.IsEnabled = true;
                return;
            }

            var newVersion = core.GetVersion(newVersionID);
            Config.Args.Versions.Add(newVersion);
            Config.Args.VersionIndex = Config.Args.Versions.IndexOf(newVersion);

            MessageBox.Show($"安装{_mcVersion}版本Forge成功");
            download_btn.IsEnabled = true;
        }
    }
}
