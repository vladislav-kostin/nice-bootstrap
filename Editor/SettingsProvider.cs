using UnityEditor;

namespace NiceBootstrap
{
	public static class BootstrapSettingsProvider
	{
		[SettingsProvider]
		public static SettingsProvider CreateBootstrapSettingsProvider()
		{
			return new SettingsProvider("Project/BootstrapSettings", SettingsScope.Project)
			{
				label = "Bootstrap Settings",
				guiHandler = (searchContext) =>
				{
					// Access the settings from the static class
					var settings = BootstrapSettings.Settings;

					// Draw the GUI fields for settings
					EditorGUI.BeginChangeCheck();
					settings.BootstrapFolderAddress = EditorGUILayout.TextField("Bootstrap Folder Address", settings.BootstrapFolderAddress);
					settings.DontDestroyOnLoad = EditorGUILayout.Toggle("Don't Destroy On Load", settings.DontDestroyOnLoad);
					if (EditorGUI.EndChangeCheck())
					{
						// Save settings when there are changes
						BootstrapSettings.SaveSettings();
					}
				}
			};
		}
	}
}