using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class PlayerGameplayDataHandler : Node
{
	public static PlayerGameplayDataHandler Instance { get; private set; }

	/// <summary>
    /// This is the CURRENT/PLAYER's save file, not the defaults. Refer to DefaultSavePaths in DefaultSavePaths.cs for those paths 
    /// </summary>
	[Export] private string playerGameplayDataFile; 

	private GameplayData _data; 

	public string CurrentLevelId { get; private set; }
	public bool newTime { get; private set; }

	//may need more attention, this was set to Action<bool>? by chat by default, im not doing it though 
	public event Action<bool> newTimeEvent;

	public override void _Ready()
	{
		Instance = this;

		if (!FileAccess.FileExists(playerGameplayDataFile))
		{
			RestoreDefaults(); 
		}
		 
		newTime = false; 

		// initialize the levels dict for our current save file  
		using var file = FileAccess.Open(playerGameplayDataFile, FileAccess.ModeFlags.Read);

		_data = JsonSerializer.Deserialize<GameplayData>(file.GetAsText(), new JsonSerializerOptions {
			PropertyNameCaseInsensitive = true
		});

		//in the future, maybe initalizing all this at the start is bad, and should be done on a level by level basis? or if you open a timings page? idk 
	}

	public void SubmitData(string levelId, double time)
	{
		if (!_data.levels.ContainsKey(levelId))
		{
			GD.PrintErr($"Wat the frick how (Your level id is wrong when calling this)");
			return;
		}

		var currLevel = _data.levels.GetValueOrDefault(levelId); 
		CurrentLevelId = levelId; 

		if (time < currLevel.bestTime)
		{
			currLevel.bestTime = time;
			newTime = true;  
			newTimeEvent?.Invoke(true); //if new time, also send event so that scene manager knwos what audio to play. made after new time, these coulda been one i think 
		}
		else
		{
			newTimeEvent?.Invoke(false);
		}

		currLevel.lastTime = time; 

		string json = JsonSerializer.Serialize(_data, new JsonSerializerOptions
		{
			WriteIndented = true
		});

		using var file = FileAccess.Open(playerGameplayDataFile, FileAccess.ModeFlags.Write);
		file.StoreString(json);

	}

	public LevelData ReadData(string levelId)
	{
		//read json here and return for the given id 
		if (!_data.levels.ContainsKey(levelId))
		{
			GD.PrintErr($"Wat the frick how READ (Your level id is wrong when calling this)");
			return null;
		}

		//re-read it every tiem this is called to make sure we have it updated? 
		using var file = FileAccess.Open(playerGameplayDataFile, FileAccess.ModeFlags.Read);
		string json = file.GetAsText();
		_data = JsonSerializer.Deserialize<GameplayData>(json, new JsonSerializerOptions {
			PropertyNameCaseInsensitive = true
		}); 

		return _data.levels.GetValueOrDefault(levelId);
	}

	public void ClearNewTime()
	{
		newTime = false;
	}

	public void RestoreDefaults()
	{
		//this should make the file if it doesnt exist, but the directory (so the Live folder) must already exist
		using var defaultFile = FileAccess.Open(DefaultSavePaths.DefaultGameplayData, FileAccess.ModeFlags.Read);

		_data = JsonSerializer.Deserialize<GameplayData>(defaultFile.GetAsText(), new JsonSerializerOptions {
			PropertyNameCaseInsensitive = true
		});

		string writeJson = JsonSerializer.Serialize(_data, new JsonSerializerOptions
		{
			WriteIndented = true
		});

		using var file2 = FileAccess.Open(playerGameplayDataFile, FileAccess.ModeFlags.Write);
		file2.StoreString(writeJson);
	}

	public class LevelData
	{
		public double bestTime { get; set; }
		public double lastTime { get; set; }
	}

	private class GameplayData
	{
		public Dictionary<string, LevelData> levels { get; set; } = new();

		//public bool initialized { get; set; } 
		//note: JSON does not serialze fields by default, so we could made this a property.
		//we can add a flag to the json writer, but im worried i would have to add that flag everywhere, so im not gonna do it lol (IncludeFields = true) 
		
		//if we later add more stuff other than Levels, we would add it here 
	}

}