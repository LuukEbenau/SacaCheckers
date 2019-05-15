using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalLayer.Security
{
	public class OAuthCodeExchangeResponse
	{
		public string AccessToken { get; }
		public string RefreshToken { get; }
		public string ClientId { get; }
		public OAuthCodeExchangeResponse(string accessToken, string refreshToken, string clientId)
		{
			this.AccessToken = accessToken;
			this.RefreshToken = refreshToken;
			this.ClientId = clientId;
		}
	}
}
