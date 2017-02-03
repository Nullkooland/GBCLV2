﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KMCCC.Authentication;
using KMCCC.Launcher;
using GBCLV2.Modules;
using System.Threading.Tasks;

namespace GBCLV2.Pages
{
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

                if(App.Versions.Any())
                {
                    LaunchButton.IsEnabled = true;
                    LaunchButton.Content = "启动";

                    VersionBox.ItemsSource = App.Versions;
                }
                else
                {
                    LaunchButton.IsEnabled = false;
                    LaunchButton.Content = "没有版本";
                }
            };
        }

        private async void Launch(object sender, RoutedEventArgs e)
        {
            LaunchButton.IsEnabled = false;

            var Core = App.Core;
            var Config = App.Config;

            Core.GameLaunch += OnGameLaunch;

            var LaunchVersion = App.Versions[Config.VersionIndex];

            var lostEssentials = DownloadHelper.GetLostEssentials(Core, LaunchVersion);
            if(lostEssentials.Any())
            {
                var downloadPage = new DownloadPage(lostEssentials , "下载依赖库");
                NavigationService.Navigate(downloadPage);
                await Task.Run(() => downloadPage.DownloadComplete.WaitOne());
                if(!downloadPage.Succeeded)
                {
                    if(MessageBox.Show("依赖库未全部下载成功，可能无法正常启动\n是否继续启动", "Σ( ￣□￣||)",
                        MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                    {
                        return;
                    }
                    
                }
            }

            var lostAssets = DownloadHelper.GetLostAssets(Core, LaunchVersion);

            if(lostAssets.Any() && MessageBox.Show("资源文件缺失，是否补齐", "(σﾟ∀ﾟ)σ",
                MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var downloadPage = new DownloadPage(lostAssets, "下载资源文件");
                NavigationService.Navigate(downloadPage);
                await Task.Run(()=> downloadPage.DownloadComplete.WaitOne());
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

                ServerAddress = Config.ServerAddress,
                VersionType = "GBCL-v2.0.3"

            }, x => x.AdvencedArguments.Add(Config.AdvancedArgs));

            if(Result.Success)
            {
                tb.Text = "(。-`ω´-) 启动中...";
                LaunchButton.Content = "启动中";
            }
            else
            {
                MessageBox.Show(Result.ErrorMessage, Result.ErrorType.ToString(),MessageBoxButton.OK,MessageBoxImage.Error);
                LaunchButton.IsEnabled = true;
            }
        }

        private void Goto_Page(object sender, RoutedEventArgs e)
        {
            var page = "Pages/" + (sender as Button).Name + ".xaml";
            NavigationService.Navigate(new Uri(page,UriKind.Relative));
        }

        private void OnGameLaunch(LaunchHandle handle)
        {
            if(!string.IsNullOrWhiteSpace(App.Config.WindowTitle))
            {
                handle.SetTitle(App.Config.WindowTitle);
            }
            
            switch (App.Config.AfterLaunchBehavior)
            {
                case 0:
                    Dispatcher.Invoke(() =>
                    {
                        Application.Current.MainWindow.Hide();
                    });
                    break;
                case 1:
                    Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                    });
                    break;
                case 2:
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
