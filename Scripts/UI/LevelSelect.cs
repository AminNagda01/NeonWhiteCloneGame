using Godot;

public partial class LevelSelect : Control
{
	public void OnLevel1ButtonPressed()
	{
		SceneManager.Instance.ChangeScene(SceneManager.Instance.LevelOne);
	}

	public void OnBackButtonPressed()
	{
		SceneManager.Instance.ChangeScene(SceneManager.Instance.MainMenu);
	}

	public void RestoreDefaults()
	{
		PlayerGameplayDataHandler.Instance.RestoreDefaults(); 
	}
}

