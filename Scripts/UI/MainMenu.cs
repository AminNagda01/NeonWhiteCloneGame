using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		AudioManager.Instance.PlayMenuMusic(); 
	}

	public void OnPlayButtonPressed()
	{
		SceneManager.Instance.ChangeScene(SceneManager.Instance.LevelOne);
	}

	public void OnLevelSelectButtonPressed()
	{
		SceneManager.Instance.ChangeScene(SceneManager.Instance.LevelSelect);
	}

	public void OnSettingsButtonPressed()
	{
		SceneManager.Instance.ChangeScene(SceneManager.Instance.Settings);
	}

	public void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}
