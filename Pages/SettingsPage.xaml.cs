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
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            launch_settings.Save_Settings();
            NavigationService.GoBack();
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
