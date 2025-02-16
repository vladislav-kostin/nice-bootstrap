using UnityEngine;
using System.IO;

namespace VK.Bootstrap
{
	public static class BootstrapSettings
	{
		private static readonly string SettingsPath = "Library/NiceBootstrap/settings.json";
		private static BootstrapSettingsData _settings = new BootstrapSettingsData();

		public static BootstrapSettingsData Settings => _settings;

		public static void LoadSettings()
		{
			if (File.Exists(SettingsPath))
			{
				Debug.Log("loading settings " + SettingsPath);
				string json = File.ReadAllText(SettingsPath);
				_settings = JsonUtility.FromJson<BootstrapSettingsData>(json);
			}
			else
			{
				_settings = new BootstrapSettingsData();
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
		public string BootstrapFolderAddress = "Bootstrap";
		public bool DontDestroyOnLoad = false;
	}
}