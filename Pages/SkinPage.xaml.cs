using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace GBCLV2.Pages
{
    public partial class SkinPage : Page
    {
        private static Color Temp_Color;

        public SkinPage()
        {
            InitializeComponent();
            this.DataContext = App.Config;
            PresetColorList.ItemsSource = PresetColors;

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

            PresetColorList.SelectionChanged += (s, e) =>
             {
                 var SelectedColor = PresetColorList.SelectedItem as MyColor;
                 Temp_Color = (Color)ColorConverter.ConvertFromString(SelectedColor.Color);
                 Slider_A.Value = Temp_Color.A;
                 Slider_R.Value = Temp_Color.R;
                 Slider_G.Value = Temp_Color.G;
                 Slider_B.Value = Temp_Color.B;
                 App.Update_ThemeColorBrush(Temp_Color);
             };

            goback_btn.Click += (s, e) => NavigationService.GoBack();
        }

        private static void Update_ThemeColor()
        {
            Application.Current.Resources["Theme_Color"] = Temp_Color;
        }

        private void Slider_Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
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

            if (dialog.ShowDialog() ?? false)
            {
                App.Config.ImagePath = dialog.FileName;
            }
        }

        private class MyColor
        {
            public string Name { get; set; }
            public string Color { get; set; }
        }

        private static MyColor[] PresetColors = new MyColor[]
        {
            new MyColor{Name = "土豪金", Color = "#7DEBAF5A"},
            new MyColor{Name = "滑稽黄", Color = "#96F1C40F"},
            new MyColor{Name = "橙子橙", Color = "#96FF7722"},
            new MyColor{Name = "春节红", Color = "#9DE0392B"},
            new MyColor{Name = "桃花粉", Color = "#9DFF96AF"},
            new MyColor{Name = "蟑螂红", Color = "#C8350C14"},
            new MyColor{Name = "鲜草绿", Color =" #582ECC71"},
            new MyColor{Name = "老草绿", Color = "#96649105"},
            new MyColor{Name = "吃土棕", Color = "#D2563C18"},
            new MyColor{Name = "竹林绿", Color = "#AF00320F"},
            new MyColor{Name = "泰瑞绿", Color = "#961ABC9C"},
            new MyColor{Name = "上天蓝", Color = "#8C3498DB"},
            new MyColor{Name = "墨水蓝", Color = "#B400284B"},
            new MyColor{Name = "闪电蓝", Color = "#284BC8EB"},
            new MyColor{Name = "茄子紫", Color = "#B06E1E8C"},
            new MyColor{Name = "雾霾灰", Color = "#4D7F8C8D"},
            new MyColor{Name = "高冷黑", Color = "#B41E2321"},
        };

    }
}
