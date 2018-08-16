using LitJson;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GBCLV2.Modules;
using System.IO;

namespace GBCLV2.Controls
{

    public partial class GameDownload : Grid
    {
        private static readonly HttpClient client = new HttpClient { Timeout = new TimeSpan(0, 0, 5) };

        class VersionDownloadInfo
        {
            public string ID { get; set; }
            public string ReleaseTime { get; set; }
            public string Type { get; set; }
            public string JsonUrl { get; set; }
        }

        private static List<VersionDownloadInfo> _availableVersions = new List<VersionDownloadInfo>();

        public GameDownload()
        {
            InitializeComponent();

            if (_availableVersions.Count != 0)
            {
                _statusBox.Text = "准备下载";
            }

            _downloadVersionList.ItemsSource = _availableVersions;
            _downloadVersionList.SelectionChanged += (s, e) => e.Handled = true;
            _refreshButton.Click += async (s, e) => await GetVersionListFromNetAsync();
        }

        public async Task GetVersionListFromNetAsync()
        {
            if (_refreshButton.IsEnabled && _availableVersions.Count == 0)
            {
                _refreshButton.IsEnabled = false;
            }
            else return;

            string json;
            try
            {
                _statusBox.Text = "正在获取所有版本列表";
                json = await client.GetStringAsync(DownloadHelper.BaseUrl.VersionListUrl);
            }
            catch (HttpRequestException ex)
            {
                _statusBox.Text = "获取版本列表失败：" + ex.Message;
                _refreshButton.IsEnabled = true;
                return;
            }

            _availableVersions.Clear();

            foreach (var ver in JsonMapper.ToObject(json)["versions"])
            {
                var version = ver as JsonData;
                var info = new VersionDownloadInfo
                {
                    ID = version["id"].ToString(),
                    Type = version["type"].ToString(),
                    ReleaseTime = version["releaseTime"].ToString(),
                    JsonUrl = DownloadHelper.BaseUrl.JsonBaseUrl + version["url"].ToString().Substring(32)
                };
                _availableVersions.Add(info);
            }

            _refreshButton.IsEnabled = true;
            _statusBox.Text = "准备下载";
            _downloadVersionList.Items.Refresh();
        }

        private async void DownloadVersionAsync(object sender, RoutedEventArgs e)
        {
            if (_downloadVersionList.SelectedIndex == -1)
            {
                MessageBox.Show("未选取任何版本!", "(｡•ˇ‸ˇ•｡)", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _downloadButton.IsEnabled = false;

            var core = App.Core;
            var versionID = _availableVersions[_downloadVersionList.SelectedIndex].ID;
            var jsonUrl = _availableVersions[_downloadVersionList.SelectedIndex].JsonUrl;
            var versionDir = $"{core.GameRootPath}\\versions\\{versionID}";
            if (!Directory.Exists(versionDir))
            {
                Directory.CreateDirectory(versionDir);
            }

            var jsonPath = $"{versionDir}\\{versionID}.json";

            if (File.Exists(jsonPath) && File.Exists(jsonPath.Replace("json", "jar")))
            {
                MessageBox.Show("所选版本已经躺在你的硬盘里了", "(｡•ˇ‸ˇ•｡)", MessageBoxButton.OK, MessageBoxImage.Information);
                _downloadButton.IsEnabled = true;
                return;
            }

            _statusBox.Text = $"正在请求{versionID}版本json文件";
            try
            {
                File.WriteAllText(jsonPath, await client.GetStringAsync(jsonUrl));
            }
            catch
            {
                _statusBox.Text = $"获取{versionID}版本json文件失败";
                Directory.Delete(versionDir, true);
                _downloadButton.IsEnabled = true;
                return;
            }

            var version = core.GetVersion(versionID);
            Config.Args.Versions.Add(version);
            Config.Args.VersionIndex = Config.Args.Versions.IndexOf(version);

            var filesToDownload = DownloadHelper.GetLostEssentials(version);

            var downloadPage = new Pages.DownloadPage();
            (Application.Current.MainWindow as MainWindow).Frame.Navigate(downloadPage);
            bool hasDownloadSucceeded = await downloadPage.StartDownloadAsync(filesToDownload, "下载新的Minecraft版本");

            if (hasDownloadSucceeded)
            {
                MessageBox.Show($"{versionID}版本下载成功");
            }
            else
            {
                MessageBox.Show($"{versionID}版本下载失败");
                Directory.Delete(versionDir, true);
                Config.Args.Versions.Remove(version);
                Config.Args.VersionIndex = 0;
            }

            _downloadButton.IsEnabled = true;
            _statusBox.Text = "准备下载";
        }
    }
}
