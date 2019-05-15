using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FunctionalLayer
{
	public class Settings : ISettings
	{
		[JsonIgnore]
		public bool TimerEnabled { get; private set; }
		public int TimerInterval { get; private set; }
		public Settings(int timerInterval) {
			if(timerInterval > 0) {
				TimerEnabled = true;
				this.TimerInterval = timerInterval;
			}
		}
	}
}
