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
        private const string ForgeListUrl = "http://bmclapi2.bangbang93.com/forge/minecraft/";
        private static readonly HttpClient client = new HttpClient { Timeout = new TimeSpan(0, 0, 5) };
        private string mcVersion;

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
            VersionBox.ItemsSource = App.Versions;
            VersionBox.DataContext = App.Config;
            ForgeList.ItemsSource = VersionForges;
        }

        private async void GetVersionForgeListAsync(object sender, SelectionChangedEventArgs e)
        {
            if (App.Config.VersionIndex == -1) return;

            mcVersion = App.Versions[App.Config.VersionIndex].JarID;

            string json;
            try
            {
                statusBox.Text = $"正在获取{mcVersion}版本Forge列表";
                json = await client.GetStringAsync(ForgeListUrl + mcVersion);
            }
            catch
            {
                statusBox.Text = $"获取{mcVersion}版本Forge列表失败";
                return;
            }

            VersionForges.Clear();

            var allForge = JsonMapper.ToObject(json);
            for(int i = allForge.Count - 1; i >=0; i--)
            {
                VersionForges.Add(new ForgeInfo
                {
                    Branch = allForge[i]["branch"]?.ToString(),
                    Version = allForge[i]["version"].ToString(),
                    ModifiedTime = allForge[i]["modified"].ToString()
                });
            }

            if(VersionForges.Count != 0)
            {
                statusBox.Text = $"获取{mcVersion}版本Forge列表成功";
            }
            else
            {
                statusBox.Text = $"{mcVersion}版本并没有可用的Forge";
            }

            ForgeList.Items.Refresh();
        }

        private async void InstallForgeAsync(object sender, RoutedEventArgs e)
        {
            if(ForgeList.SelectedIndex == -1)
            {
                MessageBox.Show("请选择要安装的Forge版本!", "(｡•ˇ‸ˇ•｡)", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            download_btn.IsEnabled = false;
            var core = App.Core;

            var forge = VersionForges[ForgeList.SelectedIndex];
            var forgeName = $"{mcVersion}-{forge.Version}" + (forge.Branch == null ? null : $"-{forge.Branch}");

            var forgeJarPath = $"{core.GameRootPath}\\libraries\\net\\minecraftforge\\forge\\{forgeName}\\forge-{forgeName}.jar";

            var forgeDownload = new List<DownloadInfo>()
            {
                new DownloadInfo
                {
                    Path = forgeJarPath,
                    Url = $"{DownloadHelper.BaseUrl.ForgeBaseUrl}{forgeName}/forge-{forgeName}-universal.jar"
                }
            };

            var downloadPage = new Pages.DownloadPage(forgeDownload, "下载Forge");
            (Application.Current.MainWindow.FindName("frame") as Frame).Navigate(downloadPage);
            await Task.Run(() => downloadPage.DownloadComplete.WaitOne());

            if(!downloadPage.Succeeded)
            {
                MessageBox.Show($"下载{mcVersion}版本Forge失败");
                download_btn.IsEnabled = true;
                return;
            }

            var newVersionID = $"{mcVersion}-forge{forgeName}";
            var newVersionPath = $"{core.GameRootPath}\\versions\\{newVersionID}";
            try
            {
                if(!Directory.Exists(newVersionPath))
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
                MessageBox.Show($"安装{mcVersion}版本Forge失败");
                download_btn.IsEnabled = true;
                return;
            }

            var newVersion = core.GetVersion(newVersionID);
            App.Versions.Add(newVersion);
            App.Config.VersionIndex = App.Versions.IndexOf(newVersion);

            MessageBox.Show($"安装{mcVersion}版本Forge成功");
            download_btn.IsEnabled = true;
        }
    }
}
