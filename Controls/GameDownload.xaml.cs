using LitJson;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GBCLV2.Modules;

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
        }

        private List<VersionInfo> AllVersions = new List<VersionInfo>();

        public GameDownload()
        {
            InitializeComponent();
            VersionList.ItemsSource = AllVersions;
            VersionList.SelectionChanged += (s, e) => e.Handled = true;
            refresh_btn.Click += async (s, e) => await GetVersionListFromNetAsync();
        }

        public async Task GetVersionListFromNetAsync()
        {
            if (refresh_btn.IsEnabled)
            {
                refresh_btn.IsEnabled = false;
            }
            else return;

            string json;
            try
            {
                json = await client.GetStringAsync(DownloadHelper.BaseUrl.VersionListUrl);
            }
            catch (Exception ex)
            {
                if(this.IsVisible) MessageBox.Show("加载版本列表失败","",MessageBoxButton.OK,MessageBoxImage.Information);
                refresh_btn.IsEnabled = true;
                return;
            }

            AllVersions.Clear();

            foreach (var x in JsonMapper.ToObject(json)["versions"])
            {
                var version = x as JsonData;
                var info = new VersionInfo
                {
                    ID = version["id"].ToString(),
                    Type = version["type"].ToString(),
                    ReleaseTime = version["releaseTime"].ToString()
                };
                AllVersions.Add(info);
            }

            refresh_btn.IsEnabled = true;
            VersionList.Items.Refresh();
        }
    }
}
