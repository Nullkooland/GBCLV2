using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GBCLV2.Pages;
using GBCLV2.Modules;

namespace GBCLV2
{
    public partial class MainWindow : Window
    { 
        public MainWindow()
        {
            InitializeComponent();
            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Config.UseImageBackground)
            {
                ChangeImageBackgroundAsync(App.Config.ImagePath);
            }
            var mp = new MainPage();
            frame.Navigate(mp);

            if(Environment.OSVersion.Version.Minor > 1)
            {
                Win10BlurHelper.EnableBlur(this);
            }
            else
            {
                this.BorderThickness = new Thickness(0.5);
                this.BorderBrush = Brushes.DarkGray;
                Win7BlurHelper.EnableAeroGlass(this);
            }
        }

        private void ShutDown(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public static async void ChangeImageBackgroundAsync(string FilePath)
        {
            if (!File.Exists(FilePath))
            {
                if (!Directory.Exists("bg\\")) return;

                string[] img_files = Directory.EnumerateFiles("bg\\")
                .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".bmp")).ToArray();

                if (img_files.Any())
                {
                    FilePath = AppDomain.CurrentDomain.BaseDirectory + img_files[new Random().Next(img_files.Length)];
                }
                else return;
            }

            BitmapImage bg = await Task.Run(() =>
            {
                try
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(FilePath, UriKind.Absolute);
                    img.DecodePixelWidth = 688;
                    img.DecodePixelHeight = 387;
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();
                    img.Freeze();
                    return img;
                }
                catch
                {
                    return null;
                }

            });

            Application.Current.MainWindow.Background = new ImageBrush() { ImageSource = bg };
        }

    }
}
