namespace GBCLV2.Modules
{
    using System.IO;
    using LitJson;
    using System;
    using System.Text;
    using System.Security.Cryptography;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using KMCCC.Tools;

    public class ConfigModule : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName]string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region 私有字段

        const string encryptKey = "无可奉告";

        private string   _JavaPath;
        private int      _VersionIndex;
        private uint     _MaxMemory;
        private bool     _Offline;
        private string   _UserName;
        private string   _Email;
        private string   _PassWord;
        private bool     _RememberPassWord;
        private ushort   _WinWidth;
        private ushort   _WinHeight;
        private bool     _FullScreen;
        private string   _ServerAddress;
        private string   _AdvancedArgs;
        private string   _WindowTitle;
        private string   _ThemeColor;
        private bool     _UseSystemThemeColor;
        private bool     _UseImageBackground;
        private string   _ImagePath;
        private int      _DownloadSource;
        private int      _AfterLaunchBehavior;

        #endregion

        #region 属性访问器

        public string JavaPath
        {
            get => _JavaPath;
            set
            {
                if(!File.Exists(value))
                {
                    value = SystemTools.FindJava();
                }
                _JavaPath = value;
                NotifyPropertyChanged();
            }
        }

        public int VersionIndex
        {
            get => _VersionIndex; set { _VersionIndex = value; NotifyPropertyChanged(); }
        }

        public uint MaxMemory
        {
            get => _MaxMemory;
            set
            {
                if (value < 1024) value = 1024;
                if (value > SystemTools.GetAvailableMemory()) value = SystemTools.GetAvailableMemory();
                _MaxMemory = value;
                NotifyPropertyChanged();
            }
        }

        public bool Offline
        {
            get => _Offline; set => _Offline = value;
        }

        public string UserName
        {
            get => _UserName; set => _UserName = value;
        }

        public string Email
        {
            get => _Email; set => _Email= value;
        }

        public string PassWord
        {
            get => _PassWord; set => _PassWord = value;
        }

        public bool RememberPassWord
        {
            get => _RememberPassWord; set => _RememberPassWord = value;
        }

        public ushort WinWidth
        {
            get => _WinWidth; set => _WinWidth = value;
        }

        public ushort WinHeight
        {
            get => _WinHeight; set => _WinHeight = value;
        }

        public bool FullScreen
        {
            get => _FullScreen; set => _FullScreen = value;
        }

        public string ServerAddress
        {
            get => _ServerAddress; set => _ServerAddress = value;
        }

        public string AdvancedArgs
        {
            get => _AdvancedArgs; set => _AdvancedArgs = value;
        }

        public string WindowTitle
        {
            get => _WindowTitle; set => _WindowTitle = value;
        }

        public string ThemeColor
        {
            get => _ThemeColor; set => _ThemeColor = value;
        }

        public bool UseSystemThemeColor
        {
            get => _UseSystemThemeColor; set => _UseSystemThemeColor = value;
        }

        public bool UseImageBackground
        {
            get => _UseImageBackground;
            set
            {
                if (App.Current.MainWindow != null)
                {
                    if (value)
                    {
                        MainWindow.ChangeImageBackgroundAsync(_ImagePath);
                    }
                    else
                    {
                        App.Current.MainWindow.Background = null;
                    }
                }

                _UseImageBackground = value;
                NotifyPropertyChanged();
            }
        }

        public string ImagePath
        {
            get => _ImagePath;
            set
            {
                if(_UseImageBackground)
                {
                    MainWindow.ChangeImageBackgroundAsync(value);
                }

                _ImagePath = value;
                NotifyPropertyChanged();
            }
        }

        public int DownloadSource
        {
            get => _DownloadSource; set { _DownloadSource = value; DownloadHelper.SetDownloadSource(value); }
        }

        public int AfterLaunchBehavior
        {
            get => _AfterLaunchBehavior; set => _AfterLaunchBehavior = value;
        }

        #endregion


        public void Save()
        {
            if(_RememberPassWord)
            {
                _PassWord = Encrypt(_PassWord);
            }
            else
            {
                _PassWord = null;
            }
                
            File.WriteAllText("GBCL.json", JsonMapper.ToJson(this));
        }

        public static ConfigModule LoadConfig()
        {
            ConfigModule config;

            if (File.Exists("GBCL.json"))
            {
                config = JsonMapper.ToObject<ConfigModule>(File.ReadAllText("GBCL.json"));
                config.PassWord = Decrypt(config.PassWord);
            }
            else
            {
                config = new ConfigModule
                {
                    _MaxMemory = 2048,
                    _WinWidth = 854,
                    _WinHeight = 480,
                    _JavaPath = SystemTools.FindJava(),
                    _DownloadSource = 1,
                };
            }
            return config;
        }

        private static string Encrypt(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();

            byte[] key = Encoding.Unicode.GetBytes(encryptKey);
            byte[] data = Encoding.Default.GetBytes(str);

            using (var ms = new MemoryStream())
            {
                CryptoStream cs = new CryptoStream(ms, descsp.CreateEncryptor(key, key), CryptoStreamMode.Write);
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private static string Decrypt(string str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return null;
            }

            DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();

            byte[] key = Encoding.Unicode.GetBytes(encryptKey);
            byte[] data = Convert.FromBase64String(str);

            using (var ms = new MemoryStream())
            {
                CryptoStream cs = new CryptoStream(ms, descsp.CreateDecryptor(key, key), CryptoStreamMode.Write);
                cs.Write(data, 0, data.Length);
                try
                {
                    cs.FlushFinalBlock();
                }
                catch
                {
                    return null;
                }
                return Encoding.Default.GetString(ms.ToArray());
            }
        }
    }
}
