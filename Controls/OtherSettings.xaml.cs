using System.Windows.Controls;

namespace GBCLV2.Controls
{
    public partial class OtherSettings : Grid
    {
        public OtherSettings()
        {
            InitializeComponent();
            this.DataContext = App.Config;

            DownloadSourceBox.ItemsSource = new string[2] { "官方","BMCLAPI" };
            AfterLaunchBehaviorBox.ItemsSource = new string[3] { "隐藏并后台运行", "直接退出", "保持窗体可见" };
        }
    }
}
