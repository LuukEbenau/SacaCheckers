//using SacaDev.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Dammen
//{
//    internal class GameSettings : ConfigFileBase<GameSetting, GameSettingEnum, GameSettingGroupEnum>
//    {
//        protected override string RootElement { get; }
//        public override IDictionary<GameSettingGroupEnum, ICollection<GameSetting>> DefaultItems => new Dictionary<GameSettingGroupEnum, ICollection<GameSetting>> {
//        };

//        internal GameSettings(string path):base(path){
//        }
//    }
//    public enum GameSettingEnum{ }
//    public enum GameSettingGroupEnum { }
//}