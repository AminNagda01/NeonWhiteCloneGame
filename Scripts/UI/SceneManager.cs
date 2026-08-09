using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#region Enums
public enum GameFlowState
{
	NavigatingMenu,
	Playing,
	Paused,
	EndScreen
}

public enum OverlayKind
{
	Pause,
	Settings,
	LevelSelect
}
#endregion


//NOTE:  |I kinda want to make a big psa. You see how i made scenes here for pause menus and stuff. But i may have something better: 
// I saw someone make every ui menu 1 scene. Just one big scene with many sqares, and teh camera would focus on one square at a time. could be WAAAY easier then all this stuff. 
// Then we could have gameplay -> ui scene -> gameplay. Only issue is that we would draw the every bit of ui, even those we dont use, when we hit pause in game. or we could just make the pause menu 
// not have settings, only main menu/restart/resume. 
//i think it might be good, think about it. 


public partial class SceneManager : Node
{
	#region Scenes 
	public static SceneManager Instance { get; private set; }
	
	[Export] public PackedScene MainMenu { get; private set; }
	[Export] public PackedScene LevelSelect { get; private set; }
	[Export] public PackedScene LevelOne { get; private set; }
	[Export] public PackedScene Settings { get; private set; }
	[Export] public PackedScene EndScene { get; private set; }
	
	//So with the above, it loads ALL scenes at the start. A better way is to give it a string and let it load when it needs to.
	//But since i dont want it hard coded, i got the following answer. This allows you to assign a path through the editor, and so it will 
	//update the path if say, mainmenu is moved to a different folder, while also not loading all the scenes on autoload like the above. 
	//[Export(PropertyHint.File, "*.tscn")]
	//public string MainMenu2;

	//there are more changes we can make but im not. for ex, make methods like GetLevelOne(), so that main menu and level select dont have to know what 
	//properties SceneManager has. All it has to ever do is call SceneManager.Instance.GetLevelOne(). In doing so, we could also make LevelOne private. 

	[Export] public PackedScene PauseMenu { get; private set; }
	[Export] private CanvasLayer OverlayLayer { get; set; }

	#endregion
    #region Other Instance Variables
	//adjust the inital state in the editor
	[Export] private GameFlowState _initialState = GameFlowState.NavigatingMenu;
	private GameFlowState _state;
	private bool endScreenFlag = false;
	private PackedScene _currentLevelScene; 
	#endregion

	private readonly Stack<OverlayEntry> _overlayStack = new();
	//Idea is our stack (above) will keep and DRAWN scenes tracked using this class. that is why we are using Control as our overlay entry. Ex: We pause: now Kind Pause, Control <PauseSceen we draw on canvas layer> then its a matter 
	// popping it when needed.
	private sealed class OverlayEntry
	{
		public OverlayKind Kind { get; }
		public Control CurrScene { get; } //differs from packedscnes since this will be drawn. 

		public OverlayEntry(OverlayKind kind, Control currentlyDrawnScene)
		{
			Kind = kind;
			CurrScene = currentlyDrawnScene;
		}
	}
	
	public override void _Ready()
	{
		Instance = this;
		_state = _initialState; 

		//THIS ERROR CHECKING IS DUMB BUT IDC ANYMORE 
		if (MainMenu == null)
		GD.PushError("SceneManager: MainMenu is not assigned.");

		if (LevelSelect == null)
			GD.PushError("SceneManager: LevelSelect is not assigned.");

		if (LevelOne == null)
			GD.PushError("SceneManager: LevelOne is not assigned.");

		if (Settings == null)
			GD.PushError("SceneManager: Settings is not assigned.");

		if (EndScene == null)
			GD.PushError("SceneManager: EndScene is not assigned.");

		if (PlayerGameplayDataHandler.Instance == null)
		{
			GD.PushError("PlayerGameplayDataHandler.Instance is null!");
		}
		else
		{
			PlayerGameplayDataHandler.Instance.newTimeEvent += OnNewTimeEvent;
		}
	}

	// Escape and Restart so far 
	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent.IsActionPressed("pause"))
		{
			HandleBackOrPauseInput();
		}

		if (inputEvent.IsActionPressed("restart_level") &&
			_state == GameFlowState.Playing)
		{
			RestartCurrentLevel();
		}
	}

	private void HandleBackOrPauseInput()
    {
        if (_overlayStack.Count > 0)
		{
			if (_state == GameFlowState.Paused &&
		    _overlayStack.Peek().Kind == OverlayKind.Pause)
			{
				ResumeGame(); 
				return;
			}

			CloseTopOverlay();
			return;
		}
		
		if (_state == GameFlowState.Playing)
		{
			PauseGame();  
		}

		//any other time this method is called, like in main menu, it should do nothing. 
    }

    private void PauseGame()
    {
		if (_state != GameFlowState.Playing)
		{
			return;
		}

		_state = GameFlowState.Paused;
		GetTree().Paused = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		AudioManager.Instance.MuffleMusic(); 

		OpenOverlay(PauseMenu, OverlayKind.Pause);
    }

	public void ResumeGame()
    {
		if (_state != GameFlowState.Paused)
		{
			return;
		}

		CloseTopOverlay(); 

		Input.MouseMode = Input.MouseModeEnum.Captured;
		_state = GameFlowState.Playing;
    }

    private void OpenOverlay(PackedScene sceneToDraw, OverlayKind overlayKind)
    {
        //draw onot canvas layer
		//update the stack 

		Control scene = sceneToDraw.Instantiate<Control>();
		scene.ProcessMode = ProcessModeEnum.Always; //allows menu drawn to work while tree is paused. 

		OverlayLayer.AddChild(scene);
		OverlayEntry entryToPush = new OverlayEntry(overlayKind, scene); 
		_overlayStack.Push(entryToPush); 
    }

    private void CloseTopOverlay()
    {
        if (_overlayStack.Count == 0)
		{
			return;
		}

		OverlayEntry entry = _overlayStack.Pop();

		if (entry.Kind == OverlayKind.Pause)
		{
			GetTree().Paused = false;
			AudioManager.Instance.UnmuffleMusic();
		}

		entry.CurrScene.QueueFree();
    }

	//This one i made its own function since I possibly want to re-use this when someone dies. it works great cause music should be continuous on restart or death. only way music restarts is if we go back to main menu. 
    public void RestartCurrentLevel()
    { 
		if (_state == GameFlowState.Paused)
		{
			ResumeGame();
		}

		ChangeScene(_currentLevelScene);
    }

	public void OpenSettings()
	{
		OpenOverlay(Settings, OverlayKind.Settings); 
	}

	public void OpenLevelSelect()
	{
		OpenOverlay(LevelSelect, OverlayKind.LevelSelect);
	}

	public void OnBack()
	{
		CloseTopOverlay(); 
	}

    public void ChangeScene(PackedScene scene)
	{
		if (scene == null)
		{
			GD.PushError("SceneManager received a null PackedScene.");
			return;
		}

		//we need to clear any stacked screens before changing a scene using this. Ideally, this func is only used for main menu, going into a level, and end screen. 
		if (_overlayStack.Count > 0)
		{
			while (_overlayStack.Count > 0)
			{
				CloseTopOverlay(); 
			}
		}

		//ok I admit: I should probably make enums or some sort of data structure for scene and music, as raw calling and this if statement feel weird
		//.... 
		//but i am lazy ok 

		//the audiomanager is configured to keep playing this if it is already playing, no need to worry about that. 
		if (scene == MainMenu)
		{
			_state = GameFlowState.NavigatingMenu; 
			AudioManager.Instance.PlayMenuMusic();
		}

		if (scene == LevelOne)
		{
			AudioManager.Instance.PlayLevelOneMusic(); 
			_state = GameFlowState.Playing; 
		}

		if (scene == EndScene)
		{
			if (endScreenFlag)
			{
				AudioManager.Instance.PlayEndSceneMusicNewTime();
			}
			else
			{
				AudioManager.Instance.PlayEndSceneMusicNormal();
			}
			 
		}

		Error error = GetTree().ChangeSceneToPacked(scene);
		_currentLevelScene = scene; 

		if (error != Error.Ok)
		{
			GD.PushError($"Failed to change scene. Error: {error}");
		}
	}

	private void OnNewTimeEvent (bool isNewBest)
	{
		if (isNewBest)
		{
			endScreenFlag = true;
		}
		else
		{
			endScreenFlag = false; 
		}
	}

}


//settings doesnt work cause our player never refreshes to get the new values. it is paused, so its state remains as it was when we started the level. Thus, new settings are not propogated. 
