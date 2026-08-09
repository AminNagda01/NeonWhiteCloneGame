using Godot;
using System.Collections.Generic;
using System.Text.Json;
using System; 


public partial class PlayerSettings : Node
{
	public static PlayerSettings Instance { get; private set; }

	//Select the json file from the inspector 
	[Export] private string playerSettingsFile; 

	public float BaseFov { get; set; }
	public float MouseSensitivity { get; set; }

	private SettingsFile _settings; 
	public event Action playerSettingsChanged;

	public override void _Ready()
	{
		Instance = this;

		if (!FileAccess.FileExists(playerSettingsFile))
		{
			RestoreDefaults(); 
		}

		using var file = FileAccess.Open(playerSettingsFile, FileAccess.ModeFlags.Read);

		_settings = JsonSerializer.Deserialize<SettingsFile>(file.GetAsText());

		BaseFov = _settings.playerSettings[0].fov;
		MouseSensitivity = _settings.playerSettings[0].sensitivity;
	}

	public void Save()
	{

		_settings.playerSettings[0].fov = BaseFov;
		_settings.playerSettings[0].sensitivity = MouseSensitivity;


		string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
		{
			WriteIndented = true
		});

		using var file = FileAccess.Open(playerSettingsFile, FileAccess.ModeFlags.Write);
		file.StoreString(json);

		playerSettingsChanged?.Invoke(); 
	}

	public void RestoreDefaults()
	{
		using var defaultFile = FileAccess.Open(DefaultSavePaths.DefaultPlayerSettings, FileAccess.ModeFlags.Read);
		_settings = JsonSerializer.Deserialize<SettingsFile>(defaultFile.GetAsText(), new JsonSerializerOptions {
			PropertyNameCaseInsensitive = true
		});

		string writeJson = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
		{
			WriteIndented = true
		});

		using var file2 = FileAccess.Open(playerSettingsFile, FileAccess.ModeFlags.Write);
		file2.StoreString(writeJson);

		BaseFov = _settings.playerSettings[0].fov; 
		MouseSensitivity = _settings.playerSettings[0].sensitivity; 
	}
}

public partial class SettingsFile
{
	public List<PlayerSetting> playerSettings { get; set; }
}

public class PlayerSetting
{
	public float fov { get; set; }
	public float sensitivity { get; set; }
}