using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FunctionalLayer;
using SacaDev.Configuration;

namespace FunctionalLayer.Security
{
    public class AuthorizationFile : ConfigFileBase<IConfigItem<AuthPropertyEnum>, AuthPropertyEnum, AuthPropertyGroupEnum>
    {
        public AuthorizationFile(string filePath) : base(filePath: filePath) {}
		public AuthorizationFile(string filePath, string encryptionKey) : base(filePath, encryptionKey) { }
		public override IDictionary<AuthPropertyGroupEnum, ICollection<IConfigItem<AuthPropertyEnum>>> DefaultItems => new Dictionary<AuthPropertyGroupEnum, ICollection<IConfigItem<AuthPropertyEnum>>> {
        {
            AuthPropertyGroupEnum.Oauth, new HashSet<IConfigItem<AuthPropertyEnum>> {
                new AuthConfigItem(AuthPropertyEnum.AccessToken, null),
                new AuthConfigItem(AuthPropertyEnum.RefreshToken,null),
                new AuthConfigItem(AuthPropertyEnum.Sub,null),
                new AuthConfigItem(AuthPropertyEnum.ProviderId,null),
                new AuthConfigItem(AuthPropertyEnum.AccessTokenValidUntil,null)
            }
        }
    };
        protected override string RootElement => "Authentication";
    }
    public enum AuthPropertyEnum
    {
        None = 0,
        RefreshToken = 1,
        AccessToken=2,
        Sub=3,
        AccessTokenValidUntil=5,
        ProviderId=6

    }
    public enum AuthPropertyGroupEnum
    {
        Oauth = 0
    }

    public class AuthConfigItem : IConfigItem<AuthPropertyEnum>
    {
        public AuthConfigItem(AuthPropertyEnum name, string value)
        {
            Name = name;
            Value = value;
        }
        public AuthPropertyEnum Name { get; set; }
        public string Value { get; set; }
    }
}
