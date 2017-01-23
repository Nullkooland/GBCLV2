using LitJson;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GBCLV2.Modules;

namespace GBCLV2.Controls
{

    public partial class GameDownload : Grid
    {
        class Downloader : WebClient
        {
            public int Timeout { get; set; }

            public Downloader(int timeout)
            {
                Timeout = timeout;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
                request.Timeout = Timeout;
                request.ReadWriteTimeout = Timeout;
                return request;
            }
        }

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
            GetVersionListFromInternet();
            refresh_btn.Click += (s,e) => GetVersionListFromInternet();
        }

        private void GetVersionListFromInternet()
        {
            refresh_btn.IsEnabled = false;
            Task.Run(() =>
            {
                string json;
                try
                {
                    json = new Downloader(3000).DownloadString(DownloadHelper.BaseUrl.VersionListUrl);
                }
                catch (WebException ex)
                {
                    MessageBox.Show(ex.Message, "获取版本列表失败");
                    Dispatcher.Invoke(() => refresh_btn.IsEnabled = true);
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

                Dispatcher.Invoke(() =>
                {
                    VersionList.Items.Refresh();
                    refresh_btn.IsEnabled = true;
                });
            });
        }
    }
}
