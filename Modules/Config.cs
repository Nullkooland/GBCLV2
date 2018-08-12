namespace GBCLV2.Modules
{
    using System.IO;
    using LitJson;
    using System.ComponentModel;
    using KMCCC.Tools;
    using KMCCC.Launcher;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class ConfigArgs : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region 私有字段

        private string _javaPath;
        private ObservableCollection<Version> _versions = new ObservableCollection<Version>();
        private int _versionIndex;
        private bool _isVersionSplit;
        private uint _maxMemory;
        private bool _isOfflineMode;
        private string _userName;
        private string _passWord;
        private bool _isRememberPassWord;
        private ushort _gameWinWidth;
        private ushort _gameWinHeight;
        private string _gameWinTitle;
        private bool _isFullScreen;
        private string _serverAddress;
        private bool _isLoginToServer;
        private string _advancedArgs;
        private string _themeColor;
        private bool _isUseSystemThemeColor;
        private bool _isUseImageBackground;
        private string _imageFilePath;
        private int _downloadSource;
        private int _afterLaunchBehavior;

        #endregion

        #region 属性访问器

        public string JavaPath
        {
            get => _javaPath;
            set
            {
                _javaPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(JavaPath)));
            }
        }

        public int VersionIndex
        {
            get => _versionIndex;
            set
            {
                _versionIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VersionIndex)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasAnyVersion)));
            }
        }

        [JsonIgnore]
        public ObservableCollection<Version> Versions { get => _versions; set => _versions = value; }

        [JsonIgnore]
        public Version SelectedVersion { get => _versions[_versionIndex]; }

        [JsonIgnore]
        public bool HasAnyVersion { get => (_versionIndex != -1); }

        public bool IsVersionSplit
        {
            get => _isVersionSplit; set => _isVersionSplit = value;
        }

        public uint MaxMemory
        {
            get => _maxMemory;
            set
            {
                if (value < 1024) value = 1024;
                if (value > SystemTools.GetAvailableMemory()) value = SystemTools.GetAvailableMemory();
                _maxMemory = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxMemory)));
            }
        }

        public bool IsOfflineMode
        {
            get => _isOfflineMode;
            set
            {
                _isOfflineMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOfflineMode)));
            }
        }

        public string UserName
        {
            get => _userName; set => _userName = value;
        }

        public string PassWord
        {
            get => _passWord; set => _passWord = value;
        }

        public bool IsRememberPassWord
        {
            get => _isRememberPassWord; set => _isRememberPassWord = value;
        }

        public ushort GameWinWidth
        {
            get => _gameWinWidth; set => _gameWinWidth = value;
        }

        public ushort GameWinHeight
        {
            get => _gameWinHeight; set => _gameWinHeight = value;
        }

        public bool IsFullScreen
        {
            get => _isFullScreen; set => _isFullScreen = value;
        }

        public string ServerAddress
        {
            get => _serverAddress; set => _serverAddress = value;
        }

        public bool IsLoginToServer
        {
            get => _isLoginToServer; set => _isLoginToServer = value;
        }

        public string AdvancedArgs
        {
            get => _advancedArgs; set => _advancedArgs = value;
        }

        public string GameWinTitle
        {
            get => _gameWinTitle; set => _gameWinTitle = value;
        }

        public string ThemeColor
        {
            get => _themeColor; set => _themeColor = value;
        }

        public bool IsUseSystemThemeColor
        {
            get => _isUseSystemThemeColor; set => _isUseSystemThemeColor = value;
        }

        public bool IsUseImageBackground
        {
            get => _isUseImageBackground;
            set
            {
                if (App.Current.MainWindow != null)
                {
                    if (value)
                    {
                        MainWindow.ChangeImageBackgroundAsync(_imageFilePath);
                    }
                    else
                    {
                        App.Current.MainWindow.Background = null;
                    }
                }

                _isUseImageBackground = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUseImageBackground)));
            }
        }

        public string ImageFilePath
        {
            get => _imageFilePath;
            set
            {
                if (_isUseImageBackground)
                {
                    MainWindow.ChangeImageBackgroundAsync(value);
                }

                _imageFilePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageFilePath)));
            }
        }

        public int DownloadSource
        {
            get => _downloadSource; set { _downloadSource = value; DownloadHelper.SetDownloadSource(value); }
        }

        public int AfterLaunchBehavior
        {
            get => _afterLaunchBehavior; set => _afterLaunchBehavior = value;
        }

        #endregion
    }

    public static class Config
    {
        public static ConfigArgs Args { get; set; }

        public static string LauncherVersion { get; } = "2.0.7";

        public static void Load()
        {
            if (File.Exists("GBCL.json"))
            {
                Args = JsonMapper.ToObject<ConfigArgs>(File.ReadAllText("GBCL.json"));
                Args.PassWord = UsefulTools.DecryptString(Args.PassWord);
                if (!File.Exists(Args.JavaPath))
                {
                    Args.JavaPath = SystemTools.FindJava();
                }
            }
            else
            {
                Args = new ConfigArgs
                {
                    MaxMemory = 2048,
                    GameWinWidth = 854,
                    GameWinHeight = 480,
                    JavaPath = SystemTools.FindJava(),
                    DownloadSource = 1,
                };
            }
        }

        public static void Save()
        {
            if (Args.IsRememberPassWord)
            {
                Args.PassWord = UsefulTools.EncryptString(Args.PassWord);
            }
            else
            {
                Args.PassWord = null;
            }

            File.WriteAllText("GBCL.json", JsonMapper.ToJson(Args));
        }
    }
}
