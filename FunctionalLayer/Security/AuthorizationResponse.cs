using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalLayer.Security
{
    public class AuthorizationResponse
    {
        public string Code { get; private set; }
        public string Code_Verifier { get; private set; }
        public string Code_Challenge { get; private set; }
        public string RedirectUri { get; private set; }
        public AuthorizationResponse(string code, string code_verifier, string code_challenge, string redirectUri)
        {
            Code = code;
            Code_Verifier = code_verifier;
            Code_Challenge = code_challenge;
            RedirectUri = redirectUri;
        }
    }
}
