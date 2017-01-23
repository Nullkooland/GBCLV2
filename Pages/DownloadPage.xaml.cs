using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using GBCLV2.Modules;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace GBCLV2.Pages
{
    /// <summary>
    /// DownloadPage.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadPage : Page
    {
        public List<DownloadInfo> FilesToDownload;
        private static int count;
        private static int fails;

        public DownloadPage()
        {
            InitializeComponent();
        }

        private static async void DownloadFileAsync(string URL, string path)
        {
            //if (File.Exists(path)) return;

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
                httpWebRequest.Timeout = 6000;
                httpWebRequest.ReadWriteTimeout = 18000;
                httpWebRequest.Method = "GET";
                httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 6.4; .NET CLR 1.0.3705)";
                HttpWebResponse httpWebResponse = await httpWebRequest.GetResponseAsync() as HttpWebResponse;

                var responseStream = httpWebResponse.GetResponseStream();
                var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[1024];
                int size = responseStream.Read(buffer, 0, 1024);

                while (size > 0)
                {
                    fileStream.Write(buffer, 0, size);
                    size = responseStream.Read(buffer, 0, 1024);
                }

                responseStream.Close();
                fileStream.Close();
            }
            catch (WebException e)
            {
                count++;
                fails++;
                return;
            }
            System.Threading.Interlocked.Increment(ref count);
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
