using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using LitJson;
using System.Windows.Controls;
using System.ComponentModel;

namespace GBCLV2.Controls
{
    public partial class ForgeInstall : Grid
    {
        private const string ForgeListUrl = "http://bmclapi2.bangbang93.com/forge/minecraft/";
        private static readonly HttpClient client = new HttpClient { Timeout = new TimeSpan(0, 0, 5) };
        private KMCCC.Launcher.Version version;

        class ForgeInfo
        {
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
            version = App.Versions[App.Config.VersionIndex];
            if(version.InheritsVersion != null)
            {
                version = App.Core.GetVersion(version.InheritsVersion);
            }

            string json;
            try
            {
                statusBox.Text = $"正在获取{version.ID}版本Forge列表";
                json = await client.GetStringAsync(ForgeListUrl + version.ID);
            }
            catch
            {
                statusBox.Text = $"获取{version.ID}版本Forge列表失败";
                return;
            }

            VersionForges.Clear();

            foreach(var _forge in JsonMapper.ToObject(json))
            {
                var forge = _forge as JsonData;
                VersionForges.Add(new ForgeInfo
                {
                    Version = forge["version"].ToString(),
                    ModifiedTime = forge["modified"].ToString()
                });
            }

            if(VersionForges.Count != 0)
            {
                statusBox.Text = $"获取{version.ID}版本Forge列表成功";
            }
            else
            {
                statusBox.Text = $"{version.ID}版本并没有可用的Forge";
            }
            
            ForgeList.Items.SortDescriptions.Add(new SortDescription("ModifiedTime", ListSortDirection.Descending));
            ForgeList.Items.Refresh();
        }

        private async void InstallForgeAsync(object sender, RoutedEventArgs e)
        {

        }
    }
}
