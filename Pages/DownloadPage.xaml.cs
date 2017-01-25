using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using GBCLV2.Modules;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace GBCLV2.Pages
{
    /// <summary>
    /// DownloadPage.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadPage : Page
    {
        public List<DownloadInfo> FilesToDownload;
        private int total;
        private int count;
        private int fails;
        private static CancellationTokenSource cts = new CancellationTokenSource();

        public DownloadPage()
        {
            InitializeComponent();
            ServicePointManager.DefaultConnectionLimit = 256;
            Loaded += (s, e) =>
            {
                total = FilesToDownload.Count;
                progressBar.Maximum = total;
            };
        }

        private async Task DownloadFileAsync(DownloadInfo download)
        {
            if (!Directory.Exists(Path.GetDirectoryName(download.Path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(download.Path));
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(download.Url);
                request.Timeout = 6000;
                request.ReadWriteTimeout = 18000;
                request.Method = "GET";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";

                HttpWebResponse httpWebResponse = await request.GetResponseAsync() as HttpWebResponse;

                if (cts.IsCancellationRequested)
                {
                    Interlocked.Increment(ref count);
                    Debug.WriteLine("第{0}/{1}个 下载取消辣", count, total);
                    return;
                }

                var responseStream = httpWebResponse.GetResponseStream();
                var fileStream = new FileStream(download.Path, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[1024];
                int size = responseStream.Read(buffer, 0, 1024);

                while (!cts.IsCancellationRequested && size > 0)
                {
                    fileStream.Write(buffer, 0, size);
                    size = responseStream.Read(buffer, 0, 1024);
                }
                responseStream.Close();
                fileStream.Close();

                if (cts.IsCancellationRequested)
                {
                    Debug.WriteLine("第{0}/{1}个 下载取消辣", count, total);
                    return;
                }
            }
            catch(Exception e)
            {
                Interlocked.Increment(ref fails);
                Debug.WriteLine(download.Url);
                Debug.WriteLine(e.Message);
                Dispatcher.Invoke(() => failsBox.Text = string.Format("{0}个下载失败", fails));
            }

            Interlocked.Increment(ref count);
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = count;
                statusBox.Text = string.Format("{0}/{1}个文件下载完成", count, total);
            });
        }

        private void Cancle(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            statusBox.Text = "下载已取消";
        }

        private async void Start(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < total; i++)
            {
                await DownloadFileAsync(FilesToDownload[i]).ConfigureAwait(false);
            }
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
