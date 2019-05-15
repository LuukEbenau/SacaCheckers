using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace FunctionalLayer.Scenarios
{
	/// <summary>
	/// Allows to load and save scenarios to the disk
	/// </summary>
	public class ScenarioManager
	{
		public string StoragePath { get; private set; }
		private const string SCENARIO_FILE_EXTENSION = ".json";
		public ScenarioManager(string scenarioStoragePath) {
			this.StoragePath = scenarioStoragePath;
			Directory.CreateDirectory(scenarioStoragePath);
		}

		private string GetPathForScenario(string scenarioName) => $"{StoragePath}{scenarioName}{SCENARIO_FILE_EXTENSION}";
		private string GetFileNameFromPath(string path) {
			var fileName = path.Split('/', '\\').Last();
			var extensionSeperatorIndex = fileName.LastIndexOf('.');

			var scenarioName = fileName;
			if(extensionSeperatorIndex != -1)
				scenarioName = scenarioName.Substring(0, extensionSeperatorIndex);

			return scenarioName;
		}
		public ScenarioMetaData GetMetadataFromFile() {
			return null;
		}

		public bool ScenarioExists(string scenarioName)
		{
			string path = GetPathForScenario(scenarioName);
			return File.Exists(path);
		}

		public string FindUnusedScenarioName(string scenarioName)
		{
			string nameExtension = "";
			int i = 0;

			//search for a not existing filepath
			string path;
			do {
				path = $"{StoragePath}{scenarioName}{nameExtension}{SCENARIO_FILE_EXTENSION}";
				nameExtension = $"_{++i}";
			}
			while(File.Exists(path));
			return scenarioName + nameExtension;
		}

		public IGame LoadScenario(string scenarioName) {
			if(!ScenarioExists(scenarioName)) {
				throw new Exception("Scenario does not exist!");
			}
			var fileContents = File.ReadAllText(GetPathForScenario(scenarioName));
			var settings = new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.Auto
			};
			IGame game = JsonConvert.DeserializeObject<Game>(fileContents, settings);
			return game;
		}
		public bool SaveScenario(string scenarioName, IGame scenarioToSave) => SaveScenario(scenarioName, scenarioToSave, out string s);
		public bool SaveScenario(string scenarioName, IGame scenarioToSave, out string savedPath) {

			var settings = new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.Auto
			};
			string serializedScenario = JsonConvert.SerializeObject(scenarioToSave, settings);

			try {
				string path = GetPathForScenario(scenarioName);
				File.WriteAllText(path, serializedScenario);
				savedPath = scenarioName + SCENARIO_FILE_EXTENSION;
			}
			catch(Exception) {
				savedPath = null;
				return false;
			}
			return true;
		}

		public IEnumerable<ScenarioMetaData> GetStoredScenarios() {
			var scenarios = new List<ScenarioMetaData>();
			
			var filePaths = Directory.EnumerateFiles(StoragePath);
			foreach(var path in filePaths) {
				var lastEdit = File.GetLastWriteTime(path);
				scenarios.Add(new ScenarioMetaData {
					Path = path,
					LastEdit = lastEdit,
					Name = GetFileNameFromPath(path)
				});
			}

			return scenarios.OrderBy(s=>s.LastEdit);
		}
	}
}
