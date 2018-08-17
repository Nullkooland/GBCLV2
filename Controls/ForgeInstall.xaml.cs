using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Net.Http;
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
            _forgeList.ItemsSource = VersionForges;
        }

        private async void GetVersionForgeListAsync(object sender, SelectionChangedEventArgs e)
        {
            if (Config.Args.VersionIndex == -1) return;

            _mcVersion = Config.Args.SelectedVersion.JarID;

            string json;
            try
            {
                _statusBox.Text = $"正在获取 {_mcVersion} 版本Forge列表";
                json = await _httpClient.GetStringAsync(_forgeListUrl + _mcVersion);
            }
            catch
            {
                _statusBox.Text = $"获取 {_mcVersion} 版本Forge列表失败";
                return;
            }

            VersionForges.Clear();

            var allForge = JsonMapper.ToObject(json);
            for (int i = allForge.Count - 1; i >= 0; i--)
            {
                VersionForges.Add(new ForgeInfo
                {
                    Branch = allForge[i]["branch"]?.ToString(),
                    Version = allForge[i]["version"].ToString().ToLower(),
                    ModifiedTime = allForge[i]["modified"].ToString()
                });
            }

            if (VersionForges.Count != 0)
            {
                _statusBox.Text = $"获取 {_mcVersion} 版本Forge列表成功";
            }
            else
            {
                _statusBox.Text = $"{_mcVersion} 版本并没有可用的Forge";
            }

            _forgeList.Items.Refresh();
        }

        private async void InstallForgeAsync(object sender, RoutedEventArgs e)
        {
            if (_forgeList.SelectedIndex == -1)
            {
                MessageBox.Show("请选择要安装的Forge版本!", "(｡•ˇ‸ˇ•｡)", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _downloadButton.IsEnabled = false;
            var core = App.Core;

            var forge = VersionForges[_forgeList.SelectedIndex];
            var forgeName = $"{_mcVersion}-{forge.Version}";
            var newVersionID = $"{_mcVersion}-forge{forgeName}";

            foreach(var version in Config.Args.Versions)
            {
                if(version.ID == newVersionID)
                {
                    MessageBox.Show($"{forgeName} 版本的forge已经安装！", "┑(￣Д ￣)┍");
                    return;
                }
            }

            var newVersionPath = $"{core.GameRootPath}\\versions\\{newVersionID}";
            var forgeJarPath = $"{core.GameRootPath}\\libraries\\net\\minecraftforge\\forge\\{forgeName}\\forge-{forgeName}.jar";

            var downloadName = forgeName + (forge.Branch == null ? null : $"-{forge.Branch}");
            var forgeDownload = new List<DownloadInfo>()
            {
                new DownloadInfo
                {
                    Path = forgeJarPath,
                    Url = $"{DownloadHelper.BaseUrl.ForgeBaseUrl}{downloadName}/forge-{downloadName}-universal.jar",
                }
            };

            var downloadPage = new Pages.DownloadPage();
            (Application.Current.MainWindow as MainWindow).Frame.Navigate(downloadPage);
            bool hasDownloadSucceeded = await downloadPage.StartDownloadAsync(forgeDownload, "下载Forge");

            if (!hasDownloadSucceeded)
            {
                MessageBox.Show($"下载 {_mcVersion} 版本Forge失败");
                _downloadButton.IsEnabled = true;
                return;
            }

            try
            {
                if (!Directory.Exists(newVersionPath))
                {
                    Directory.CreateDirectory(newVersionPath);
                }

                JsonData jsonData;
                string jsonText;

                using (var archive = ZipFile.OpenRead(forgeJarPath))
                {
                    var entry = archive.GetEntry("version.json");
                    using (var sr = new StreamReader(entry.Open(), System.Text.Encoding.UTF8))
                    {
                        jsonData = JsonMapper.ToObject(sr.ReadToEnd());
                    }
                }

                jsonData["id"] = newVersionID;
                jsonText = jsonData.ToJson();

                if (!jsonData.ContainsKey("inheritsFrom"))
                {
                    jsonText = jsonText.Substring(0, jsonText.Length - 1) + $",\"inheritsFrom\": \"{_mcVersion}\"}}";
                }

                File.WriteAllText($"{newVersionPath}\\{newVersionID}.json", jsonText, System.Text.Encoding.UTF8);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"安装 {_mcVersion} 版本Forge失败\n{ex.Message}");
                _downloadButton.IsEnabled = true;
                return;
            }

            var newVersion = core.GetVersion(newVersionID);
            Config.Args.Versions.Add(newVersion);
            Config.Args.VersionIndex = Config.Args.Versions.IndexOf(newVersion);

            MessageBox.Show($"安装{_mcVersion}版本Forge成功");
            _downloadButton.IsEnabled = true;
        }
    }
}
