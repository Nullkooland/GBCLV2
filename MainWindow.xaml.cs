using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GBCLV2.Pages;
using GBCLV2.Helpers;
using System.Windows.Controls;

namespace GBCLV2
{
    public partial class MainWindow : Window
    {
        public Frame Frame { get => _frame; }

        public MainWindow()
        {
            InitializeComponent();
            MouseLeftButtonDown += (s, e) => DragMove();
            _minimizeButton.Click += (s, e) => this.WindowState = WindowState.Minimized;
            _shutdownButton.Click += (s, e) => Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _frame.Navigate(new MainPage());

            if (Environment.OSVersion.Version.Major == 10)
            {
                Win10BlurHelper.EnableBlur(this);
            }
            else
            {
                this.BorderThickness = new Thickness(1);
                this.BorderBrush = Brushes.DarkGray;
                Win7BlurHelper.EnableAeroGlass(this);
            }
        }

        public static async void ChangeImageBackgroundAsync(string imageFilePath)
        {
            if (!File.Exists(imageFilePath))
            {
                if (!Directory.Exists("bg\\")) return;

                string[] imageFiles = Directory.EnumerateFiles("bg\\")
                .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".bmp")).ToArray();

                if (imageFiles.Any())
                {
                    imageFilePath = AppDomain.CurrentDomain.BaseDirectory + imageFiles[new Random().Next(imageFiles.Length)];
                }
                else return;
            }

            BitmapImage bg = await Task.Run(() =>
            {
                try
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(imageFilePath, UriKind.Absolute);
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
