using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KMCCC.Tools;

namespace GBCLV2.Pages
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            launch_settings.Save_Settings();
            resourcepack_manager.SavePackOptions();

            NavigationService.GoBack();
        }

    }
}
