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
        private static readonly HttpClient client = new HttpClient { Timeout=new TimeSpan(0,0,5) };

        class VersionInfo
        {
            public string ID { get; set; }
            public string ReleaseTime { get; set; }
            public string Type { get; set; }
            public string JsonUrl { get; set; }
        }

        private static List<VersionInfo> AllVersions = new List<VersionInfo>();

        public GameDownload()
        {
            InitializeComponent();

            if(AllVersions.Count != 0)
            {
                statusBox.Text = "准备下载";
            }

            VersionList.ItemsSource = AllVersions;
            VersionList.SelectionChanged += (s, e) => e.Handled = true;
            refresh_btn.Click += async (s, e) => await GetVersionListFromNetAsync();
        }

        public async Task GetVersionListFromNetAsync()
        {
            if (refresh_btn.IsEnabled && AllVersions.Count == 0)
            {
                refresh_btn.IsEnabled = false;
            }
            else return;

            string json;
            try
            {
                statusBox.Text = "正在获取所有版本列表";
                json = await client.GetStringAsync(DownloadHelper.BaseUrl.VersionListUrl);
            }
            catch
            {
                statusBox.Text = "获取版本列表失败";
                refresh_btn.IsEnabled = true;
                return;
            }

            AllVersions.Clear();

            foreach (var ver in JsonMapper.ToObject(json)["versions"])
            {
                var version = ver as JsonData;
                var info = new VersionInfo
                {
                    ID = version["id"].ToString(),
                    Type = version["type"].ToString(),
                    ReleaseTime = version["releaseTime"].ToString(),
                    JsonUrl = DownloadHelper.BaseUrl.JsonBaseUrl + version["url"].ToString().Substring(32)
                };
                AllVersions.Add(info);
            }

            refresh_btn.IsEnabled = true;
            statusBox.Text = "准备下载";
            VersionList.Items.Refresh();
        }

        private async void DownloadVersionAsync(object sender, RoutedEventArgs e)
        {
            if(VersionList.SelectedIndex == -1)
            {
                MessageBox.Show("未选取任何版本!", "(｡•ˇ‸ˇ•｡)", MessageBoxButton.OK,MessageBoxImage.Information);
                return;
            }

            download_btn.IsEnabled = false;

            var core = App.Core;
            var versionID = AllVersions[VersionList.SelectedIndex].ID;
            var jsonUrl = AllVersions[VersionList.SelectedIndex].JsonUrl;
            var versionDir = $"{core.GameRootPath}\\versions\\{versionID}";
            if(!Directory.Exists(versionDir))
            {
                Directory.CreateDirectory(versionDir);
            }

            var jsonPath = $"{versionDir}\\{versionID}.json";

            if(File.Exists(jsonPath) && File.Exists(jsonPath.Replace("json","jar")))
            {
                MessageBox.Show("所选版本已经躺在你的硬盘里了", "(｡•ˇ‸ˇ•｡)", MessageBoxButton.OK, MessageBoxImage.Information);
                download_btn.IsEnabled = true;
                return;
            }

            statusBox.Text = $"正在请求{versionID}版本json文件";
            try
            {
                File.WriteAllText(jsonPath, await client.GetStringAsync(jsonUrl));
            }
            catch
            {
                statusBox.Text = $"获取{versionID}版本json文件失败";
                Directory.Delete(versionDir, true);
                download_btn.IsEnabled = true;
                return;
            }

            var version =  core.GetVersion(versionID);
            App.Versions.Add(version);
            App.Config.VersionIndex = App.Versions.IndexOf(version);

            var FilesToDownload = DownloadHelper.GetLostEssentials(core, version);

            var downloadPage = new Pages.DownloadPage(FilesToDownload, "下载新Minecraft版本");
            (Application.Current.MainWindow.FindName("frame") as Frame).Navigate(downloadPage);
            await Task.Run(() => downloadPage.DownloadComplete.WaitOne());

            if(downloadPage.Succeeded)
            {
                MessageBox.Show($"{versionID}版本下载成功");
            }
            else
            {
                MessageBox.Show($"{versionID}版本下载失败");
                Directory.Delete(versionDir, true);
                App.Versions.Remove(version);
            }

            download_btn.IsEnabled = true;
        }
    }
}
