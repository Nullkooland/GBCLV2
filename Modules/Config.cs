namespace GBCLV2.Modules
{
    using System.IO;
    using LitJson;
    using System;
    using System.Text;
    using System.Security.Cryptography;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public enum AfterLaunchBehavior { 隐藏启动器, 退出启动器, 保持启动器可见 }

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
        private string   _ThemeColor;
        private bool     _UseSystemThemeColor;
        private bool     _UseImageBackground;
        private string   _ImagePath;
        private DownloadSource _DownloadSource;
        private AfterLaunchBehavior _AfterLaunch;

        #endregion

        #region 属性访问器

        public string JavaPath
        {
            get => _JavaPath; set => _JavaPath = value;
        }

        public int VersionIndex
        {
            get => _VersionIndex; set { _VersionIndex = value; NotifyPropertyChanged(); }
        }

        public uint MaxMemory
        {
            get => _MaxMemory; set { _MaxMemory = value; NotifyPropertyChanged(); }
        }

        public bool Offline
        {
            get => _Offline;  set { _Offline = value; NotifyPropertyChanged(); }
        }

        public string UserName
        {
            get => _UserName; set { _UserName = value; NotifyPropertyChanged(); }
        }

        public string Email
        {
            get => _Email; set { _Email= value; NotifyPropertyChanged(); }
        }

        public string PassWord
        {
            get => _PassWord; set { _PassWord = value; NotifyPropertyChanged(); }
        }

        public bool RememberPassWord
        {
            get => _RememberPassWord; set { _RememberPassWord = value; NotifyPropertyChanged(); }
        }

        public ushort WinWidth
        {
            get => _WinWidth; set { _WinWidth = value; NotifyPropertyChanged(); }
        }

        public ushort WinHeight
        {
            get => _WinHeight; set { _WinHeight = value; NotifyPropertyChanged(); }
        }

        public bool FullScreen
        {
            get => _FullScreen; set { _FullScreen = value; NotifyPropertyChanged(); }
        }

        public string AdvancedArgs
        {
            get => _AdvancedArgs; set => _AdvancedArgs = value;
        }

        public string ServerAddress
        {
            get => _ServerAddress; set => _ServerAddress = value;
        }

        public string ThemeColor
        {
            get => _ThemeColor; set => _ThemeColor = value;
        }

        public bool UseSystemThemeColor
        {
            get => _UseSystemThemeColor; set { _UseSystemThemeColor = value; NotifyPropertyChanged(); }
        }

        public bool UseImageBackground
        {
            get => _UseImageBackground; set { _UseImageBackground = value; NotifyPropertyChanged(); }
        }

        public string ImagePath
        {
            get => _ImagePath; set { _ImagePath = value; NotifyPropertyChanged(); }
        }

        public DownloadSource DownloadSource
        {
            get => _DownloadSource; set { _DownloadSource = value; NotifyPropertyChanged(); }
        }

        public AfterLaunchBehavior AfterLaunch
        {
            get => _AfterLaunch; set => _AfterLaunch = value;
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
                    _DownloadSource = DownloadSource.BMCLAPI
                };
            }
            config._JavaPath = File.Exists(config._JavaPath) ? config._JavaPath : KMCCC.Tools.SystemTools.FindJava();
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
                cs.FlushFinalBlock();
                return Encoding.Default.GetString(ms.ToArray());
            }
        }
    }
}
