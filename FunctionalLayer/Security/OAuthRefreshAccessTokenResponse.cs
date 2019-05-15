using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalLayer.Security
{
	public class OAuthRefreshAccessTokenResponse
	{
		public string AccessToken { get; }
		public DateTime ExpirationDate { get; }
		public OAuthRefreshAccessTokenResponse(string accessToken, DateTime expirationDate)
		{
			this.AccessToken = accessToken;
			this.ExpirationDate = expirationDate;
		}
	}
}
