using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace GBCLV2.Pages
{
    public partial class SettingsPage : Page
    {

        public SettingsPage()
        {
            InitializeComponent();
            BackButton.Click += (s,e) => NavigationService.GoBack();
        }

        private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabcontrol.SelectedIndex == 1)
            {
                await game_download.GetVersionListFromNetAsync();
            }
            e.Handled = true;
        }
    }
}
