using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalLayer.Security
{
    public class OAuthConfig
    {
        public string ClientID { get; private set; }
        public string ClientSecret { get; private set; }

        public string AuthorizationEndpoint { get; private set; }

        public string TokenEndpoint { get; private set; }
        public string UserInfoEndpoint { get; private set; }

        public string Code_challenge_method { get; set; } = "S256";

        public OAuthConfig(string clientID, string clientSecret, string authorizationEndpoint, string tokenEndpoint, string userInfoEndpoint) {
            this.ClientID = clientID;
            this.ClientSecret = clientSecret;
            this.AuthorizationEndpoint = authorizationEndpoint;
            this.TokenEndpoint = tokenEndpoint;
            this.UserInfoEndpoint = userInfoEndpoint;
        }
    }
}
