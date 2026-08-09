using Godot;

public partial class PauseMenu : Control
{
	public void OnRestartButtonPressed()
	{
		SceneManager.Instance.RestartCurrentLevel(); 
	}

	public void OnResumeButtonPressed()
	{
		SceneManager.Instance.ResumeGame(); 
	}

	public void OnSettingsButtonPressed()
	{
		SceneManager.Instance.OpenSettings(); 
	}
	public void OnLevelSelectButtonPressed()
	{
		SceneManager.Instance.OpenLevelSelect(); 
	}
	public void OnMainMenuButtonPressed()
	{
		SceneManager.Instance.ChangeScene(SceneManager.Instance.MainMenu); 
	}

}
