using Godot;

public partial class SceneManager : Node
{
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
	
	private bool endScreenFlag = false; 
	
	public override void _Ready()
	{
		Instance = this;
		
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
	
	public void ChangeScene(PackedScene scene)
	{
		if (scene == null)
		{
			GD.PushError("SceneManager received a null PackedScene.");
			return;
		}

		//ok I admit: I should probably make enums or some sort of data structure for scene and music, as raw calling and this if statement feel weird
		//.... 
		//but i am lazy ok 

		//the audiomanager is configured to keep playing this if it is already playing, no need to worry about that. 
		if (scene == MainMenu)
		{
			AudioManager.Instance.PlayMenuMusic();
		}

		if (scene == LevelOne)
		{
			AudioManager.Instance.PlayLevelOneMusic(); 
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

// Scene handles escape input. If user is in settings or level select (really should make this an enum, and classify these two under submenus?), make the user go back to the scene they came from 
// if in main menu, do nothing 
// if in game, show the pause menu, pause the game, muffle the music 
// if it is in the pause menu, resume the level. 

//with that said, should i go ahead and implement the state machine at this point? this seems easier if we know what state we are in. that way, settings for example would tell us what state to be in, and if in that state, escape works as a back button. 