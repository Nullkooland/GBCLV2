using System.Windows;
using System.Windows.Media;
using System.Threading;
using KMCCC.Launcher;
using System.Collections.ObjectModel;
using System.IO;
using GBCLV2.Modules;

namespace GBCLV2
{
    public partial class App : Application
	{
        public static ConfigModule Config;
        public static LauncherCore Core;
        public static ObservableCollection<Version> Versions = new ObservableCollection<Version>();

        private static Mutex mutex;
        private static FileStream log_FileStream;
        private static StreamWriter Logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new Mutex(true, "GBCLV2", out bool ret);
            if (!ret)
            {
                MessageBox.Show("已经有一个我在运行了", "(>ㅂ< )", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Environment.Exit(0);
            }

            Config = ConfigModule.LoadConfig();
            Initialize_LauncherCore();
            Initialize_ThemeColor();

            Dispatcher.UnhandledException += UnhandledExceptionHandler;

            base.OnStartup(e);
        }

        private void Initialize_LauncherCore()
        {
            Core = LauncherCore.Create();
            Core.GameExit += OnGameExit;
            Core.GameLog += OnGameLog;

            log_FileStream = new FileStream(Core.GameRootPath + @"\mcrun.log", FileMode.Create);
            Logger = new StreamWriter(log_FileStream);

            if (Core.JavaPath == null)
            {
                if (MessageBox.Show("好气哦，Java在哪里啊 Σ( ￣□￣||)!!\n需要给您打开下载页面吗？", "吓得我喝了杯82年的Java",
                    MessageBoxButton.YesNo,MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("http://www.java.com/zh_CN/download/manual.jsp");
                }
            }
            
            uint count = 0;

            foreach(Version ver in Core.GetVersions())
            {
                Versions.Add(ver);
                count++;
            }

            if(count == 0)
            {
                Config.VersionIndex = -1;
            }
            else if (Config.VersionIndex == -1 || Config.VersionIndex >= count)
            {
                Config.VersionIndex = 0;
            }
        }

        private void Initialize_ThemeColor()
        {
            Color Theme_Color;

            if (Config.UseSystemThemeColor || string.IsNullOrEmpty(Config.ThemeColor))
            {
                Theme_Color = SystemParameters.WindowGlassColor;
                Theme_Color.A = 150;
            }
            else
            {
                Theme_Color = (Color)ColorConverter.ConvertFromString(Config.ThemeColor);
            }
            Resources["Theme_Color"] = Theme_Color;
            Update_ThemeColorBrush(Theme_Color);
        }

        public static void Update_ThemeColorBrush(Color _col)
        {
            float Gray = _col.R * 0.299f + _col.G * 0.577f + _col.B * 0.124f;

            if (Gray < 10.0f)
            {
                _col = Color.FromRgb(90, 90, 90);
            }
            else if (Gray < 100.0f)
            {
                _col = Color.Multiply(_col, 23.0f - Gray * 0.22f);
            }
            else if (Gray > 175.0f)
            {
                _col = Color.Multiply(_col, 2.75f - Gray * 0.01f);
            }

            _col.A = 255;
            Current.Resources["Theme_Brush"] = new SolidColorBrush(_col);
        }

        private void OnGameLog(string line)
        {
            Logger.WriteLine(line);
        }

        private void OnGameExit(int ExitCode)
        {
            if (ExitCode != 0)
            {
                if (MessageBox.Show(string.Format("Minecraft异常退出了,Exit Code: {0}\n是否查看log文件？", ExitCode), "（/TДT)/", 
                    MessageBoxButton.YesNo,MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(Core.GameRootPath + @"\mcrun.log");
                }
            }

            Current.Dispatcher.Invoke(() =>
            {
                if(Config.AfterLaunchBehavior == 0)
                {
                    Current.Shutdown();
                }
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Config.ThemeColor = Resources["Theme_Color"].ToString();
            Config.Save();
            base.OnExit(e);
        }

        void UnhandledExceptionHandler(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(string.Format("异常信息：{0}\n异常源：{1}",e.Exception.Message,e.Exception.StackTrace), "程序发生了无法处理的异常！",MessageBoxButton.OK,MessageBoxImage.Error);
            //Shutdown(1);
            e.Handled = true;
        }
    }
}
