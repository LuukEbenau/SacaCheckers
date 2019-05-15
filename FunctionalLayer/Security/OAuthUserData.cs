using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace FunctionalLayer.Security
{
    public class OAuthUserData
    {
        [JsonProperty("sub")]
        public string ExternID { get; set; }
        [JsonProperty("name")]
        public string FullName { get; set; }//name
        [JsonProperty("given_name")]
        public string FirstName { get; set; }//given_name
        [JsonProperty("family_name")]
        public string FamilyName { get; set; }
        [JsonProperty("picture")]
        public string Picture { get; set; }
        [JsonProperty("locale")]
        public string Locale { get; set; }
    }
}
