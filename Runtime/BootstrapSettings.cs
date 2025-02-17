using UnityEngine;
using System.IO;

namespace VK.Bootstrap
{
	public static class BootstrapSettings
	{
		private static readonly string SettingsPath = "ProjectSettings/BootstrapSettings.json";
		private static BootstrapSettingsData _settings;

		public static BootstrapSettingsData Settings
		{
			get
			{
				if (_settings == null)
				{
					LoadSettings();
				}
				return _settings;
			}
		}

		public static void LoadSettings()
		{
			if (File.Exists(SettingsPath))
			{
				string json = File.ReadAllText(SettingsPath);
				_settings = JsonUtility.FromJson<BootstrapSettingsData>(json);
			}
			else
			{
				_settings = new BootstrapSettingsData();
				SaveSettings();
			}
		}

		public static void SaveSettings()
		{
			string json = JsonUtility.ToJson(_settings, true);
			Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
			File.WriteAllText(SettingsPath, json);
		}
	}
	
	[System.Serializable]
	public class BootstrapSettingsData
	{
		public string BootstrapFolderAddress = "Assets/Bootstrap";
		public bool DontDestroyOnLoad = false;
	}
}