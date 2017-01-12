using System;
using System.Threading;
using System.Threading.Tasks;
using KMCCC.Modules.Yggdrasil;

namespace KMCCC.Authentication
{

	#region Login

	/// <summary>
	///     正版验证器（直接登陆）
	/// </summary>
	public class YggdrasilLogin : IAuthenticator
	{
		/// <summary>
		///     新建正版验证器
		/// </summary>
		/// <param name="email">电子邮件地址</param>
		/// <param name="password">密码</param>
		/// <param name="twitchEnabled">是否启用Twitch</param>
		/// <param name="clientToken">clientToken</param>
		/// <param name="authServer">验证服务器</param>
		public YggdrasilLogin(string email, string password, bool twitchEnabled, Guid clientToken, string token = null, string authServer = null)
		{
			Email = email;
			Password = password;
			TwitchEnabled = twitchEnabled;
			ClientToken = clientToken;
			AuthServer = authServer;
            Token = token;
        }

		/// <summary>
		///     新建正版验证器(随机的新ClientToken)
		/// </summary>
		/// <param name="email">电子邮件地址</param>
		/// <param name="password">密码</param>
		/// <param name="twitchEnabled">是否启用Twitch</param>
		/// <param name="authServer">验证服务器</param>
		public YggdrasilLogin(string email, string password, bool twitchEnabled, string token = null, string authServer = null)
		{
            Email = email;
            Password = password;
            TwitchEnabled = twitchEnabled;
            AuthServer = authServer;
            Token = token;
        }

		/// <summary>
		///     电子邮件地址
		/// </summary>
		public string Email { get; }

		/// <summary>
		///     密码
		/// </summary>
		public string Password { get; }

		/// <summary>
		///     是否启用Twitch
		/// </summary>
		public bool TwitchEnabled { get; }

		/// <summary>
		/// </summary>
		public Guid ClientToken { get; }

		/// <summary>
        ///     第三方服务器
		/// </summary>
		public string AuthServer { get; set; }

        /// <summary>
        ///     第三方验证服务器的一些验证Token（伪正版）
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        ///     返回Yggdrasil验证器类型
        /// </summary>
        public string Type => "KMCCC.Yggdrasil";

		public AuthenticationInfo Do()
		{
            var r = new System.Text.RegularExpressions.Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");

            if(!r.IsMatch(Email))
            {
                return new AuthenticationInfo
                {
                    Error = "不是有效的邮箱地址"
                };
            }
            var client = new YggdrasilClient(AuthServer, ClientToken);
            var ErrorMessage = client.Authenticate(Email, Password, Token, TwitchEnabled);
            if (ErrorMessage == null)
			{
				return new AuthenticationInfo
				{
					AccessToken = client.AccessToken,
					UserType = client.AccountType,
					DisplayName = client.DisplayName,
					Properties = client.Properties,
					UUID = client.UUID
				};
			}
			return new AuthenticationInfo
			{
				Error = ErrorMessage
            };
		}

		public Task<AuthenticationInfo> DoAsync(CancellationToken token)
		{
			var client = new YggdrasilClient(AuthServer, ClientToken);
			return client.AuthenticateAsync(Email, Password, Token, TwitchEnabled, token).ContinueWith(task =>
			{
				if ((task.Exception == null) && (task.Result))
				{
					return new AuthenticationInfo
					{
						AccessToken = client.AccessToken,
						UserType = client.AccountType,
						DisplayName = client.DisplayName,
						Properties = client.Properties,
						UUID = client.UUID
					};
				}
				return new AuthenticationInfo
				{
					Error = "验证错误"
				};
			}, token);
		}
	}

	#endregion

}