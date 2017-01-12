using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace GBCLV2.Pages
{
    /// <summary>
    /// SkinPage.xaml 的交互逻辑
    /// </summary>
    public partial class SkinPage : Page
    {
        private static Color Temp_Color;

        public SkinPage()
        {
            InitializeComponent();
            Temp_Color = (Color)Application.Current.Resources["Theme_Color"];

            Slider_A.Value = Temp_Color.A;
            Slider_R.Value = Temp_Color.R;
            Slider_G.Value = Temp_Color.G;
            Slider_B.Value = Temp_Color.B;

            Slider_A.ValueChanged += (s, e) =>
            {
                if (e.NewValue == 0) return;
                Temp_Color.A = (byte)Slider_A.Value; Update_ThemeColor();
            };
            Slider_R.ValueChanged += (s, e) => { Temp_Color.R = (byte)Slider_R.Value; Update_ThemeColor(); };
            Slider_G.ValueChanged += (s, e) => { Temp_Color.G = (byte)Slider_G.Value; Update_ThemeColor(); };
            Slider_B.ValueChanged += (s, e) => { Temp_Color.B = (byte)Slider_B.Value; Update_ThemeColor(); };

            SystemColor_CheckBox.IsChecked = Config.UseSystemThemeColor;
            Img_CheckBox.IsChecked = Config.UseImageBackground;

            if(Config.UseImageBackground)
            {
                Img_PathBox.Text = Config.ImagePath;
            }
            else
            {
                Img_PathBox.IsEnabled = false;
                GetImage_Button.IsEnabled = false;
            }

            Img_CheckBox.Checked += (s, e) =>
            {
                MainWindow.ChangeImageBackgroundAsync(Img_PathBox.Text);
                Img_PathBox.IsEnabled = true;
                GetImage_Button.IsEnabled = true;
            };
            Img_CheckBox.Unchecked += (s, e) =>
            {
                Application.Current.MainWindow.Background = null;
                Img_PathBox.IsEnabled = false;
                GetImage_Button.IsEnabled = false;
            };
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            Config.UseSystemThemeColor = SystemColor_CheckBox.IsChecked ?? false;
            Config.UseImageBackground = Img_CheckBox.IsChecked ?? false;
            Config.ImagePath = Img_PathBox.Text;
            NavigationService.GoBack();
        }

        private static void Update_ThemeColor()
        {
            Application.Current.Resources["Theme_Color"] = Temp_Color;
        }

        private void Update_ThemeColorBrush(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            App.Update_ThemeColorBrush(Temp_Color);
        }

        private void GetImageFromDisk(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "请选择图片文件",
                Filter = "图像文件| *.png; *.jpg; *.bmp; *.gif",
            };

            if(dialog.ShowDialog() ?? false)
            {
                Img_PathBox.Text = dialog.FileName;
                MainWindow.ChangeImageBackgroundAsync(dialog.FileName);
            }
        }
    }
}
