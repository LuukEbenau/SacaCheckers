using FunctionalLayer.Security;
using SacaDev.Api;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace FunctionalLayer.Api
{
    public class CheckersApi : RestApiBase<ApiPath>
    {
		private string AccessToken { get; set; }
		public CheckersApi(string siteRoot): base(siteRoot) {

		}

        public async Task<HttpWebResponse> Post(ApiPath path, string accessToken, object formData = null, InterceptDelegate customIntercept = null)
        {
			AccessToken = accessToken;
			var resp = await Post(GetFullPath(path), formData);
            return resp;
        }

        #region overrides
        private IList<RequestInterceptionObject> _requestIntercepts;
        protected override IList<RequestInterceptionObject> RequestIntercepts => _requestIntercepts = _requestIntercepts ?? new List<RequestInterceptionObject> {
            new RequestInterceptionObject(HttpStatusCode.Unauthorized, async a => {
				//TODO: we need to rewrite this using an event i think, since the logic for reauthorizing shouldn't be here.

                //baseFL.Logger.Write($"the request '{a.ResponseUri}' got intercepted with a 401 statuscode, so try to request a new accesstoken",LogType.INFO);

                //try
                //{
                //    bool succes = Auth.CurrentOAuthProvider.TryRefreshAccessToken(,out OAuthRefreshAccessTokenResponse resp);
                //}
                //catch (Exception ex)
                //{
                //    //baseFL.Logger.Write($"the requesting of a new accesstoken failed, so now lets try if the refreshtoken has expired with the following error: '{ex.Message}',\n stacktrace: \n\n{ex.StackTrace}\n\n ",LogType.ERROR);
                //    try{
                //        Console.WriteLine(ex.Message);
                //        await baseFL.Authorization.CurrentOAuthProvider.PerformAuthentication();
                //        baseFL.Authorization.CurrentOAuthProvider.RefreshAccessToken();
                //    }
                //    catch(Exception ex2) {
                //        //baseFL.Logger.Write($"the requesting of a new refreshtoken has failed to. no clue how this could happen, but here is the error: '{ex.Message}',\n stacktrace: \n\n{ex.StackTrace}\n\n ",LogType.ERROR);
                //        throw ex2;
                //    }
                //}
            },retryRequest:true),
            new RequestInterceptionObject(0, codeToRun : async c => {
                //baseFL.Logger.Write($"Tried to send a request to '{c.ResponseUri}', but the request timed out.",LogType.ERROR);
                //TODO: log this?
            },retryRequest : true)
        };

        protected override Dictionary<ApiPath, string> ApiPaths => new Dictionary<ApiPath, string>
        {
            {ApiPath.Login,"account/Login"},
            { ApiPath.Logout,"account/Logout"},
            { ApiPath.Register,"account/Register"},
            { ApiPath.AccountExists, "account/Exists"},
            { ApiPath.GetAccountID, "account/GetID"},
            { ApiPath.PollStillLoggedIn,"login/Poll"}
        };

		protected override Dictionary<string, string> Headers => new Dictionary<string, string> {
			{ "Authorization", $"{AccessToken}"}
		};

		#endregion overrides
	}
}