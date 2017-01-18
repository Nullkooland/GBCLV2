using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KMCCC.Authentication;
using KMCCC.Launcher;
using KMCCC.Tools;
using System.Windows.Media;

namespace GBCLV2.Pages
{
    /// <summary>
    /// Page1.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Page
    {
        private Random rand = new Random();
        private string[] Excited = new string[]{
            "(⇀‸↼‶)","(๑˘•◡•˘๑)","( Ծ ‸ Ծ )","_( '-' _)⌒)_","(●—●)","~( ´•︵•` )~","( *・ω・)✄╰ひ╯","(╯>д<)╯┻━┻","_(-ω-`_)⌒)_",
            "ᕦ(･ㅂ･)ᕤ","(◞‸◟ )","(ㅎ‸ㅎ)","(= ᵒᴥᵒ =)","(๑乛◡乛๑)","( ,,ÒωÓ,, )","ε=ε=(ノ≧∇≦)ノ","(･∀･)","Σ( ￣□￣||)","(。-`ω´-)",
            "(´• ᗜ •`)","(๑╹∀╹๑)","(´• ᵕ •`)*✲","┑(￣Д ￣)┍","(｡•ˇ‸ˇ•｡)","\\(•ㅂ•)/","(´･ᆺ･`)","ԅ(¯﹃¯ԅ)","୧(๑•∀•๑)૭","ʕ•ﻌ•ʔ"
        };

        public MainPage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(Config.UserName))
                {
                    tb.Text = Excited[rand.Next(Excited.Length)];
                }
                else
                {
                    tb.Text = "Hello " + Config.UserName;
                }

                if(Config.VersionIndex == -1)
                {
                    LaunchButton.IsEnabled = false;
                    LaunchButton.Content = "没有版本";
                }
                else
                {
                    LaunchButton.IsEnabled = true;
                    LaunchButton.Content = "启动";

                    VersionBox.ItemsSource = App.Versions;
                    VersionBox.SelectedIndex = Config.VersionIndex;
                }
            };
        }

        private void Launch(object sender, RoutedEventArgs e)
        {
            var Core = App.Core;
            var Versions = App.Versions;

            var Result = Core.Launch(new LaunchOptions()
            {
                Version = Versions[VersionBox.SelectedIndex],
                Authenticator = (Config.Offline) ?
                (IAuthenticator)new OfflineAuthenticator(Config.UserName) : new YggdrasilLogin(Config.Email, Config.PassWord, false),
                MaxMemory = Config.MaxMemory,

                Size = new WindowSize
                {
                    Width = Config.WinWidth,
                    Height = Config.WinHeight,
                    FullScreen = Config.FullScreen
                },

                ServerAddress = Config.ServerAddress

            }, x => x.AdvencedArguments.Add(Config.AdvancedArgs));

            if(Result.Success)
            {
                tb.Text = "(。-`ω´-) 启动中...";
                LaunchButton.IsEnabled = false;
                LaunchButton.Content = "启动中";
            }
            else
            {
                MessageBox.Show(Result.ErrorMessage, Result.ErrorType.ToString());
                return;
            }
        }

        private void Goto_SettingsPage(object sender, RoutedEventArgs e)
        {
            var settings_page = new SettingsPage();
            if(Config.UseImageBackground)
            {
                settings_page.Background = new SolidColorBrush(Color.FromArgb(100,150,150,150));
            }
            NavigationService.Navigate(settings_page);
        }

        private void Goto_SkinPage(object sender, RoutedEventArgs e)
        {
            var skin_page = new SkinPage();
            NavigationService.Navigate(skin_page);
        }

        private void SelectVersionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.VersionIndex = VersionBox.SelectedIndex;
        }
    }
}
