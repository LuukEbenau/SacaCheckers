using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

using System.Security.Authentication;
using System.Linq;
namespace FunctionalLayer.Security
{
    public class OAuth
    {
        public OAuth(OAuthConfig config)
        {
            OAuthConfig = config;
        }

        public event Action<string> RedirectedToAuth;

        public OAuthConfig OAuthConfig { get; protected set; }
        /// <summary>
        /// Performs the authentication. If succesfull, writes the obtained tokens to the authenticationfile
        /// </summary>
        /// <returns></returns>
        public async Task<OAuthUserData> PerformAuthentication() {
            try
            {
                var response = await this.Authenticate();
                var exchangeResult = await this.ExchangeCodeForRefreshToken(response.Code, response.Code_Verifier, response.Code_Challenge, response.RedirectUri);

                var userdata = await this.GetUpdatedUserInfo(exchangeResult.AccessToken);
				return userdata;
			}
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
				throw ex;
            }
        }
        private async Task<string> RedirectToAuthProvider(string url,string redirectURI,string state) {
            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            http.Start();

            var browserprocess = System.Diagnostics.Process.Start(url);
            RedirectedToAuth?.Invoke(url);

            var context = await http.GetContextAsync();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            //<meta http-equiv='refresh' content='1;url=https://google.com'>
            string responseString = "<html><head></head><body><script>setTimeout(function(){window.open('','_self').close();},1000);</script></body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask =responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });
            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                throw new AuthenticationException($"OAuth authorization error: {context.Request.QueryString.Get("error")}.");
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                throw new AuthenticationException($"Malformed authorization response. {context.Request.QueryString}");
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state)
            {
                throw new AuthenticationException($"Received request with invalid state ({incoming_state})");
            }
            return code;
        }
        /// <summary>
        /// Authenticates this instance.
        /// </summary>
        /// <returns>the authorization code</returns>
        /// <exception cref="AuthenticationException">
        /// </exception>
        public async Task<AuthorizationResponse> Authenticate()
        {
            string state = OAuthHelpers.randomDataBase64url(32);
            string code_verifier = OAuthHelpers.randomDataBase64url(32);
            string code_challenge = OAuthHelpers.base64urlencodeNoPadding(OAuthHelpers.sha256(code_verifier));
            // Creates a redirect URI using an available port on the loopback address.

            string code=null;

            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, OAuthHelpers.GetRandomUnusedPort());
            string authorizationRequest = BuildAuthorizationRequestString(OAuthConfig.AuthorizationEndpoint,Uri.EscapeDataString(redirectURI),OAuthConfig.ClientID,state,code_challenge,OAuthConfig.Code_challenge_method,"offline");
            var promptRequest= RedirectToAuthProvider($"{authorizationRequest}&prompt=none", redirectURI, state);//TODO: prompt = none does not work yet, is it even possible?  and neccesary.
            try
            {
                code = await promptRequest;
            }
            catch (Exception) { };
            if (promptRequest.Exception != null)//Try catch cant run async in catch, so let's do it this way...
            {
                redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, OAuthHelpers.GetRandomUnusedPort());
                authorizationRequest = BuildAuthorizationRequestString(OAuthConfig.AuthorizationEndpoint, Uri.EscapeDataString(redirectURI), OAuthConfig.ClientID, state, code_challenge, OAuthConfig.Code_challenge_method, "offline");
                code = await RedirectToAuthProvider(authorizationRequest, redirectURI, state);
            }

            return new AuthorizationResponse(code, code_verifier, code_challenge, redirectURI);
        }

        /// <summary>
        /// Refreshes the access token using the refreshtoken in the auth file.
        /// </summary>
        /// <returns></returns>
        public bool TryRefreshAccessToken(string refreshToken, out OAuthRefreshAccessTokenResponse response) {
            RestClient client = new RestClient(OAuthConfig.TokenEndpoint);
            IRestRequest request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("Accept", "application/json");
            var stringToEncodeAsBody = $"" +
                $"client_id={OAuthConfig.ClientID}" +
                $"&client_secret={OAuthConfig.ClientSecret}" +
                $"&refresh_token={refreshToken}" +
                $"&grant_type=refresh_token";
            request.AddParameter("application/x-www-form-urlencoded", stringToEncodeAsBody,ParameterType.RequestBody);

            var resp = client.Execute(request);
            if (resp.IsSuccessful)
            {
                RefreshAccessTokenResponse d = JsonConvert.DeserializeObject<RefreshAccessTokenResponse>(resp.Content);
				var expirationDate = DateTime.UtcNow.AddSeconds(d.expires_in);
				response = new OAuthRefreshAccessTokenResponse(d.access_token, expirationDate);
                return true;
            }
            else {
				response = null;
                //_baseFL.Logger.Write($"OAuth RefreshAccessToken was not succesful with the folowing errorcode: {resp.ErrorMessage}");
                return false;
            }
        }

        public async Task<OAuthCodeExchangeResponse> ExchangeCodeForRefreshToken(string code, string code_verifier, string code_challenge, string redirectURI)
        {
            string tokenRequestBody = $"code={code}&redirect_uri={Uri.EscapeDataString(redirectURI)}&client_id={OAuthConfig.ClientID}" +
                $"&code_verifier={code_verifier}&client_secret={OAuthConfig.ClientSecret}&scope=&grant_type=authorization_code";

            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(OAuthConfig.TokenEndpoint);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();
            try
            {
                // gets the response
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
                {
                    string responseText = await reader.ReadToEndAsync();
                    Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);
                    string access_token = tokenEndpointDecoded["access_token"];
                    string refresh_token = tokenEndpointDecoded["refresh_token"];
					return new OAuthCodeExchangeResponse(OAuthConfig.ClientID, refresh_token, access_token);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            string responseText = await reader.ReadToEndAsync();
                            throw new AuthenticationException(responseText);
                        }
                    }
                }
                throw new AuthenticationException("Something Went whrong while Getting the access_token");
            }
        }

        public async Task<OAuthUserData> GetUpdatedUserInfo(string accessToken){
            
            HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(OAuthConfig.UserInfoEndpoint);
            userinfoRequest.Method = "GET";
            userinfoRequest.Headers.Add($"Authorization: Bearer {accessToken}");
            userinfoRequest.ContentType = "application/x-www-form-urlencoded";
            userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            // gets the response
            WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
            using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
            {
                string userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();
                OAuthUserData userData = JsonConvert.DeserializeObject<OAuthUserData>(userinfoResponseText);
                return userData;
            }
        }

        protected string BuildAuthorizationRequestString(string authorizationEndpoint, string redirectUri, string clientID, string state, string code_challenge, string code_challenge_method,string access_type)
        {
            return $"{authorizationEndpoint}?response_type=code&scope=openid%20profile&redirect_uri={redirectUri}" +
                $"&access_type={access_type}"+
                $"&client_id={clientID}&state={state}&code_challenge={code_challenge}&code_challenge_method={code_challenge_method}";
        }

        protected struct RefreshAccessTokenResponse {
            public string access_token;
            public int expires_in;
            public string token_type;
        }
    }
}
