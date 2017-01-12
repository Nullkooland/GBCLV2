namespace KMCCC.Tools
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualBasic.Devices;
    using Microsoft.VisualBasic.FileIO;
    using Microsoft.Win32;

    public class SystemTools
	{
        /// <summary>
        ///     从注册表中查找可能的javaw.exe位置
        /// </summary>
        /// <returns>JAVA地址列表</returns>
        public static string FindJava()
		{
			try
			{
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment\"))
                {
                    string currentVersion = key.GetValue("CurrentVersion").ToString();
                    using (var subkey = key.OpenSubKey(currentVersion))
                    {
                        return subkey.GetValue("JavaHome").ToString() + @"\bin\javaw.exe";
                    }
                }
            }
			catch
			{
                return null;
			}
		}

		/// <summary>
		///     取物理内存
		/// </summary>
		/// <returns>物理内存</returns>
		public static uint GetTotalMemory()
		{
			return (uint)(new ComputerInfo().TotalPhysicalMemory >> 20);
		}

        /// <summary>
        /// 获取系统剩余内存
        /// </summary>
        /// <returns>剩余内存</returns>
        public static uint GetAvailableMemory()
        {
            return (uint)(new ComputerInfo().AvailablePhysicalMemory >> 20);
        }

        /// <summary>
        ///     获取x86 or x64
        /// </summary>
        /// <returns>32 or 64</returns>
		public static bool Is_X64()
        {
            return Environment.Is64BitOperatingSystem;
        }

        public static void DeleteFileAsync(string Path)
        {
            Task.Run(() => {
                FileSystem.DeleteDirectory(Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            });
        }
    }
}