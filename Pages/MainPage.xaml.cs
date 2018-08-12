using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KMCCC.Authentication;
using KMCCC.Launcher;
using GBCLV2.Modules;
using GBCLV2.Helpers;
using System.Threading.Tasks;

namespace GBCLV2.Pages
{
    public partial class MainPage : Page
    {
        private static bool _isLaunching;

        public MainPage()
        {
            InitializeComponent();
            this.DataContext = Config.Args;

            Loaded += (s, e) =>
            {
                if (!_isLaunching && string.IsNullOrWhiteSpace(Config.Args.UserName))
                {
                    tb.Text = TextFacesHelper.GetTextFace();
                }
                else
                {
                    tb.Text = "Hello " + Config.Args.UserName;
                }
            };
        }

        private async void Launch(object sender, RoutedEventArgs e)
        {
            if (Config.Args.JavaPath == null)
            {
                if (MessageBox.Show("好气哦，Java在哪里啊 Σ( ￣□￣||)!!\n需要给您打开下载页面吗？", "吓得我喝了杯82年的Java",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://www.java.com/zh_CN/download/manual.jsp");
                }
                return;
            }
            else
            {
                App.Core.JavaPath = Config.Args.JavaPath;
            }

            App.Core.GameLaunch += OnGameLaunch;

            var lostEssentials = DownloadHelper.GetLostEssentials(App.Core, Config.Args.SelectedVersion);
            if (lostEssentials.Any())
            {
                var downloadPage = new DownloadPage();
                NavigationService.Navigate(downloadPage);
                bool hasDownloadSucceeded = await downloadPage.StartDownloadAsync(lostEssentials, "下载依赖库");

                if (!hasDownloadSucceeded)
                {
                    if (MessageBox.Show("依赖库未全部下载成功，可能无法正常启动\n是否继续启动", "Σ( ￣□￣||)",
                        MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                    {
                        return;
                    }
                }
            }

            var lostAssets = DownloadHelper.GetLostAssets(App.Core, Config.Args.SelectedVersion);

            if (lostAssets.Any() && MessageBox.Show("资源文件缺失，是否补齐", "(σﾟ∀ﾟ)σ",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var downloadPage = new DownloadPage();
                this.NavigationService?.Navigate(downloadPage);
                bool hasDownloadSucceeded = await downloadPage.StartDownloadAsync(lostAssets, "下载资源文件");

                if (!hasDownloadSucceeded)
                {
                    if (MessageBox.Show("资源文件未全部下载成功，游戏可能没有声效\n是否继续启动", "(´･ᆺ･`)",
                        MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                    {
                        return;
                    }
                }
            }

            var Result = App.Core.Launch(new LaunchOptions
            {
                Version = Config.Args.SelectedVersion,
                VersionSplit = Config.Args.IsVersionSplit,

                Authenticator = (Config.Args.IsOfflineMode) ?
                (IAuthenticator)new OfflineAuthenticator(Config.Args.UserName) : new YggdrasilLogin(Config.Args.UserName, Config.Args.PassWord, false),
                MaxMemory = Config.Args.MaxMemory,

                Size = new WindowSize
                {
                    Width = Config.Args.GameWinWidth,
                    Height = Config.Args.GameWinHeight,
                    FullScreen = Config.Args.IsFullScreen
                },

                ServerAddress = Config.Args.ServerAddress,
                VersionType = $"GBCL-v{Config.LauncherVersion}",

            }, x => x.AdvencedArguments.Add(Config.Args.AdvancedArgs));

            if (Result.Success)
            {
                _isLaunching = true;
                LaunchButton.IsEnabled = false;
                tb.Text = "(。-`ω´-) 启动中...";
                LaunchButton.Content = "启动中";
            }
            else
            {
                MessageBox.Show(Result.ErrorMessage, Result.ErrorType.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                LaunchButton.IsEnabled = true;
            }
        }

        private void GotoPage(object sender, RoutedEventArgs e)
        {
            var page = "Pages/" + (sender as Button).Name + ".xaml";
            NavigationService.Navigate(new Uri(page, UriKind.Relative));
        }

        private void OnGameLaunch(LaunchHandle handle)
        {
            if (!string.IsNullOrWhiteSpace(Config.Args.GameWinTitle))
            {
                handle.SetTitle(Config.Args.GameWinTitle);
            }

            switch (Config.Args.AfterLaunchBehavior)
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
                        if (string.IsNullOrWhiteSpace(Config.Args.UserName))
                        {
                            tb.Text = TextFacesHelper.GetTextFace();
                        }
                        else
                        {
                            tb.Text = "Hello " + Config.Args.UserName;
                        }

                        LaunchButton.IsEnabled = true;
                        _isLaunching = false;
                    });
                    break;
            }
        }
    }
}
