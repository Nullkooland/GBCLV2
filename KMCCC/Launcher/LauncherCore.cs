namespace KMCCC.Launcher
{
	#region

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Modules.JVersion;
	using Tools;

	#endregion

	/// <summary>
	///     启动器核心
	/// </summary>
	public partial class LauncherCore
	{
		private IVersionLocator _versionLocator;

		/// <summary>
		///     返回包含全部版本集合
		/// </summary>
		/// <returns>版本集合/returns>
		public IEnumerable<Version> GetVersions()
		{
			return _versionLocator.GetAllVersions();
		}

		/// <summary>
		///     游戏根目录
		/// </summary>
		public string GameRootPath { get; private set; }

		/// <summary>
		///     JAVA目录
		/// </summary>
		public string JavaPath { get; set; }

		/// <summary>
		///     版本定位器
		/// </summary>
		public IVersionLocator VersionLocator
		{
			get { return _versionLocator; }
			set { (_versionLocator = value).Core = this; }
		}

		public static LauncherCore Create(string gameRootPath = null,string javaPath = null)
		{
            var launcherCore = new LauncherCore
            {
                GameRootPath = new DirectoryInfo(gameRootPath ?? ".minecraft").FullName,
                JavaPath = javaPath ?? SystemTools.FindJava(),
                VersionLocator = new JVersionLocator(),
            };

            if (!Directory.Exists(launcherCore.GameRootPath))
            {
                Directory.CreateDirectory(launcherCore.GameRootPath);
            }

            return launcherCore;

		}

		/// <summary>
		///     启动函数
		///     过程：
		///     1. 运行验证器(authenticator)，出错返回null
		///     2. 继续构造启动参数
		///     3. 遍历Operators对启动参数进行修改
		///     4. 启动
		/// </summary>
		/// <param name="options">启动选项</param>
		/// <param name="argumentsOperators">启动参数的修改器</param>
		/// <returns>启动结果</returns>
		public LaunchResult Launch(LaunchOptions options, params Action<MinecraftLaunchArguments>[] argumentsOperators)
		{
			return this.Report(LaunchInternal(options, argumentsOperators), options);
		}

        /// <summary>
        ///     游戏启动事件
        /// </summary>
        public event Action GameLaunch;

        /// <summary>
        ///     游戏退出事件
        /// </summary>
        public event Action<int> GameExit;

		/// <summary>
		///     游戏Log事件
		/// </summary>
		public event Action<string> GameLog;
	}


	/// <summary>
	///     启动后返回的启动结果
	/// </summary>
	public class LaunchResult
	{
		/// <summary>
		///     获取是否启动成功
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		///     获取发生的错误类型
		/// </summary>
		public ErrorType ErrorType { get; set; }

		/// <summary>
		///     获取错误信息
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		///     启动时发生异常
		/// </summary>
		public Exception Exception { get; set; }

		/// <summary>
		///     获取启动句柄
		/// </summary>
		public LaunchHandle Handle { get; set; }
	}

	public enum ErrorType
	{
		/// <summary>
		///     没有错误
		/// </summary>
		None,

		/// <summary>
		///     没有找到JAVA
		/// </summary>
		NoJAVA,

		/// <summary>
		///     验证失败
		/// </summary>
		AuthenticationFailed,

		/// <summary>
		///     操作器出现故障
		/// </summary>
		OperatorException,

		/// <summary>
		///     未知
		/// </summary>
		Unknown,

		/// <summary>
		///     解压错误
		/// </summary>
		UncompressingFailed
	}
}