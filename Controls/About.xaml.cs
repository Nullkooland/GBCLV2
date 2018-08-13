using System.Windows.Controls;
using GBCLV2.Modules;

namespace GBCLV2.Controls
{
    public partial class About : Grid
    {
        public About()
        {
            InitializeComponent();
            _aboutBox.Text = $"关于 GBCL V{Config.LauncherVersion}";
        }

        private void Open_Link(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            System.Diagnostics.Process.Start((sender as TextBlock).Text);
        }
    }
}
