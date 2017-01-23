using System;
using System.Windows.Controls;
using GBCLV2.Modules;

namespace GBCLV2.Controls
{
    public partial class OtherSettings : Grid
    {
        public OtherSettings()
        {
            InitializeComponent();
            this.DataContext = App.Config;

            DownloadSourceComboBox.ItemsSource = Enum.GetValues(typeof(DownloadSource));
            AfterLaunchBehaviorComboBox.ItemsSource = Enum.GetValues(typeof(AfterLaunchBehavior));

            DownloadSourceComboBox.SelectionChanged += (s, e) => DownloadHelper.SetDownloadSource();
        }
    }
}
