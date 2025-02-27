using System;
using System.IO;
using UnityEngine;

namespace NiceBootstrap
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
				var json = File.ReadAllText(SettingsPath);
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
			var json = JsonUtility.ToJson(_settings, true);
			Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
			File.WriteAllText(SettingsPath, json);
		}
	}

	[Serializable]
	public class BootstrapSettingsData
	{
		public string BootstrapFolderAddress = "Assets/Bootstrap";
		public bool DontDestroyOnLoad;
	}
}