using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KMCCC.Authentication;
using KMCCC.Launcher;
using KMCCC.Tools;
using System.Windows.Media;
using GBCLV2.Modules;

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
            this.DataContext = App.Config;

            Loaded += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(App.Config.UserName))
                {
                    tb.Text = Excited[rand.Next(Excited.Length)];
                }
                else
                {
                    tb.Text = "Hello " + App.Config.UserName;
                }

                if(App.Config.VersionIndex == -1)
                {
                    LaunchButton.IsEnabled = false;
                    LaunchButton.Content = "没有版本";
                }
                else
                {
                    LaunchButton.IsEnabled = true;
                    LaunchButton.Content = "启动";

                    VersionBox.ItemsSource = App.Versions;
                }
            };
        }

        private void Launch(object sender, RoutedEventArgs e)
        {
            var Core = App.Core;
            var Config = App.Config;

            Core.GameLaunch += OnGameLaunch;

            var LaunchVersion = App.Versions[VersionBox.SelectedIndex];

            var lostEssentials = DownloadHelper.GetLostEssentials(Core, LaunchVersion).ToList();
            if(lostEssentials.Any())
            {
                var downloadPage = new DownloadPage{ FilesToDownload = lostEssentials };
                NavigationService.Navigate(downloadPage);
                return;
            }

            var lostAssets = DownloadHelper.GetLostAssets(Core, LaunchVersion).ToList();
            if(lostAssets.Any())
            {
                var downloadPage = new DownloadPage{ FilesToDownload = lostAssets };
                NavigationService.Navigate(downloadPage);
                return;
            }

            var Result = Core.Launch(new LaunchOptions()
            {
                Version = LaunchVersion,
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
                MessageBox.Show(Result.ErrorMessage, Result.ErrorType.ToString(),MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
        }

        private void Goto_Page(object sender, RoutedEventArgs e)
        {
            var page = "Pages/" + (sender as Button).Name + ".xaml";
            NavigationService.Navigate(new Uri(page,UriKind.Relative));
        }

        private void OnGameLaunch()
        {
            switch (App.Config.AfterLaunch)
            {
                case AfterLaunchBehavior.隐藏并后台运行:
                    Dispatcher.Invoke(() =>
                    {
                        Application.Current.MainWindow.Hide();
                    });
                    break;
                case AfterLaunchBehavior.直接退出:
                    Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                    });
                    break;
                case AfterLaunchBehavior.保持可见:
                    Dispatcher.Invoke(() =>
                    {
                        if (string.IsNullOrWhiteSpace(App.Config.UserName))
                        {
                            tb.Text = Excited[rand.Next(Excited.Length)];
                        }
                        else
                        {
                            tb.Text = "Hello " + App.Config.UserName;
                        }
                        LaunchButton.IsEnabled = true;
                        LaunchButton.Content = "启动";
                    });
                    break;
            }
        }
    }
}
