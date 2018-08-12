using System.Windows;
using System.Windows.Media;
using System.Threading;
using KMCCC.Launcher;
using System.IO;
using GBCLV2.Modules;
using System.Linq;
using System.Collections.ObjectModel;

namespace GBCLV2
{
    public partial class App : Application
    {
        public static LauncherCore Core { get; private set; }
 
        private static Mutex _mutex;
        private static StreamWriter _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, "GBCLV2", out bool ret);
            if (!ret)
            {
                MessageBox.Show("已经有一个我在运行了", "(>ㅂ< )", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Shutdown(0);
            }

            Config.Load();
            Config.Args.Versions = new ObservableCollection<Version>();

            InitializeLauncherCore();
            InitializeThemeColor();

            Dispatcher.UnhandledException += UnhandledExceptionHandler;

            base.OnStartup(e);
        }

        private void InitializeLauncherCore()
        {
            Core = LauncherCore.Create();
            Core.GameExit += OnGameExit;
            Core.GameLog += OnGameLog;

            LoadVersions();

            var logPath = Core.GameRootPath + @"\logs\";
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            _logger = new StreamWriter(new FileStream(logPath + "mcrun.log", FileMode.Create));
        }

        private void InitializeThemeColor()
        {
            Color ThemeColor;

            if (Config.Args.IsUseSystemThemeColor || string.IsNullOrEmpty(Config.Args.ThemeColor))
            {
                ThemeColor = SystemParameters.WindowGlassColor;
                ThemeColor.A = 150;
            }
            else
            {
                ThemeColor = (Color)ColorConverter.ConvertFromString(Config.Args.ThemeColor);
            }
            Resources["ThemeColor"] = ThemeColor;
            UpdateThemeColorBrush(ThemeColor);
        }

        public static void LoadVersions()
        {
            Config.Args.Versions.Clear();
            foreach (var ver in Core.GetVersions())
            {
                Config.Args.Versions.Add(ver);
            }

            if (!Config.Args.Versions.Any())
            {
                Config.Args.VersionIndex = -1;
            }
            else if (Config.Args.VersionIndex == -1 || Config.Args.VersionIndex >= Config.Args.Versions.Count)
            {
                Config.Args.VersionIndex = 0;
            }
        }

        public static void UpdateThemeColorBrush(Color _col)
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
            Current.Resources["ThemeColorBrush"] = new SolidColorBrush(_col);
        }

        private void OnGameLog(string line)
        {
            _logger.WriteLine(line);
        }

        private void OnGameExit(int ExitCode)
        {
            if (ExitCode != 0)
            {
                if (MessageBox.Show($"Minecraft异常退出了,Exit Code: {ExitCode}\n是否查看log文件？", "（/TДT)/",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(Core.GameRootPath + @"\logs\mcrun.log");
                }
            }

            Current.Dispatcher.Invoke(() =>
            {
                if (Config.Args.AfterLaunchBehavior == 0)
                {
                    Current.Shutdown();
                }
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Config.Args.ThemeColor = Resources["ThemeColor"].ToString();
            Config.Save();
            base.OnExit(e);
        }

        void UnhandledExceptionHandler(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"异常信息：{e.Exception.Message}\n异常源：{e.Exception.StackTrace}", "程序发生了无法处理的异常！", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
            e.Handled = true;
        }
    }
}
