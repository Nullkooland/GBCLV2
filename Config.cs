namespace GBCLV2
{
    using System.IO;
    using LitJson;
    using System;
    using System.Text;

    public class Config
    {

        private static Config data;

        [JsonPropertyName("JavaPath")]              public string  _JavaPath;
        [JsonPropertyName("VersionIndex")]          public int     _VersionIndex;
        [JsonPropertyName("MaxMemory")]             public uint    _MaxMemory;
        [JsonPropertyName("Offline")]               public bool    _Offline;
        [JsonPropertyName("UserName")]              public string  _UserName;
        [JsonPropertyName("Email")]                 public string  _Email;
        [JsonPropertyName("PassWord")]              public string  _PassWord;
        [JsonPropertyName("RememberPassWord")]      public bool    _RememberPassWord;
        [JsonPropertyName("WinWidth")]              public ushort  _WinWidth;
        [JsonPropertyName("WinHeight")]             public ushort  _WinHeight;
        [JsonPropertyName("FullScreen")]            public bool    _FullScreen;
        [JsonPropertyName("AdvancedArgs")]          public string  _AdvancedArgs;
        [JsonPropertyName("ThemeColor")]            public string  _ThemeColor;
        [JsonPropertyName("UseSystemThemeColor")]   public bool    _UseSystemThemeColor;
        [JsonPropertyName("UseImageBackground")]    public bool    _UseImageBackground;
        [JsonPropertyName("ImagePath")]             public string  _ImagePath;

        #region 属性访问器

        public static string JavaPath
        {
            get => data._JavaPath; set => data._JavaPath = value;
        }

        public static int VersionIndex
        {
            get => data._VersionIndex; set => data._VersionIndex = value;
        }

        public static uint MaxMemory
        {
            get => data._MaxMemory; set => data._MaxMemory = value;
        }

        public static bool Offline
        {
            get => data._Offline; set => data._Offline = value;
        }

        public static string UserName
        {
            get => data._UserName; set => data._UserName = value;
        }

        public static string Email
        {
            get => data._Email; set => data._Email = value;
        }

        public static string PassWord
        {
            get => data._PassWord; set => data._PassWord = value;
        }

        public static bool RememberPassWord
        {
            get => data._RememberPassWord; set => data._RememberPassWord = value;
        }

        public static ushort WinWidth
        {
            get => data._WinWidth; set => data._WinWidth = value;
        }

        public static ushort WinHeight
        {
            get => data._WinHeight; set => data._WinHeight = value;
        }

        public static bool FullScreen
        {
            get => data._FullScreen; set => data._FullScreen = value;
        }

        public static string AdvancedArgs
        {
            get => data._AdvancedArgs; set => data._AdvancedArgs = value;
        }

        public static string ThemeColor
        {
            get => data._ThemeColor; set => data._ThemeColor = value;
        }

        public static bool UseSystemThemeColor
        {
            get => data._UseSystemThemeColor; set => data._UseSystemThemeColor = value;
        }

        public static bool UseImageBackground
        {
            get => data._UseImageBackground; set => data._UseImageBackground = value;
        }

        public static string ImagePath
        {
            get => data._ImagePath; set => data._ImagePath = value;
        }

        #endregion

        static Config()
        {
            Load();
        }

        public static void Save()
        {
            if(!string.IsNullOrEmpty(data._PassWord) && data._RememberPassWord)
                data._PassWord = Convert.ToBase64String(Encoding.Default.GetBytes(data._PassWord));
            else
                data._PassWord = null;

            File.WriteAllText("GBCL.json", JsonMapper.ToJson(data));
        }

        private static void Load()
        {
            try
            {
                data = JsonMapper.ToObject<Config>(File.ReadAllText("GBCL.json"));
                if (data._RememberPassWord && !string.IsNullOrEmpty(data._PassWord))
                    data._PassWord = Encoding.Default.GetString(Convert.FromBase64String(data._PassWord));
            }
            catch
            {
                data = new Config()
                {
                    _MaxMemory = 2048,
                    _WinWidth = 854,
                    _WinHeight = 480,
                };
            }
        }
    }
}
