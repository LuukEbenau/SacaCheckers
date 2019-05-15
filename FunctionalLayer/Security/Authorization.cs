using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunctionalLayer.Api;

using ServiceStack;

namespace FunctionalLayer.Security
{
    public class Authorization
    {
        private CheckersApi Api { get; }
        public OAuthUserData OAuthUserData { get; set; }

		public OAuth OAuthProvider { get; }
			

		public Authorization(string apiRoot,OAuthConfig config) {
			this.Api = new CheckersApi(apiRoot);
			OAuthProvider = new OAuth(config);
		}

        public async Task<bool> CheckAccountExists(string id) {
            var resp = await Api.GetFullPath(ApiPath.AccountExists)
                .PostToUrlAsync(new { id });
            var exists = resp.FromJson<bool>();
            return exists;
        }

        public bool TryGetAccountID(string externID,string providerID,out int userID) {
            var url= Api[ApiPath.GetAccountID];
            var resp= url.PostToUrl(new { externID, providerID });

            var id = resp.FromJson<int?>();
            if (id == null) {
                userID = 0;
                return false;
            }
            userID = id.Value;
            return true;
        }
        /// <summary>
        /// Sends a request to the api to check wherether the user with the given device exists.
        /// </summary>
        /// <param name="userID">The user identifier.</param>
        /// <param name="deviceID">The device identifier.</param>
        /// <returns>true if exists</returns>
        public async Task<bool> CheckDeviceExists(int accountID, string deviceID) {
            var resp = await Api[ApiPath.DeviceExists]
                .PostToUrlAsync(new { accountID, deviceID});
            var exists = resp.FromJson<bool>();
            return exists;
        }
		/// <summary>
		/// Registers a new account, and returns the accountid.
		/// </summary>
		/// <param name="externID">The extern identifier.</param>
		/// <param name="providerID">The provider identifier.</param>
		/// <returns></returns>
		public async Task<int> Register(string externID, string providerID)
		{
            var resp = await Api[ApiPath.Register]
                .PostToUrlAsync(new { externID, providerID });
            int id = resp.FromJson<int>();
            return id;
        }
        /// <summary>
        /// Registers a new device for the account
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="deviceID">The device identifier.</param>
        /// <returns>exception of not successful</returns>
        public async Task RegisterDevice(int accountID, string deviceID,string deviceName) {
            var resp = await Api[(ApiPath.RegisterDevice)]
                .PostToUrlAsync(formData: new { accountID, deviceID, deviceName = deviceName });
        }

        public async Task Login(string externID,string providerID) {
            var resp = await Api[ApiPath.Login].PostToUrlAsync(new { ExternID = externID, ProviderID = providerID });
        }
    }

    public enum OAuthProviders:UInt32 {
        Google = 1
    }

    public class OauthProviderTokenPair {
        string Access_token { get; set; }
        OAuthProviders Provider { get; set; }
        public OauthProviderTokenPair(string access_token, OAuthProviders provider) {
            this.Access_token = access_token;
            this.Provider = provider;
        }
    }
}
