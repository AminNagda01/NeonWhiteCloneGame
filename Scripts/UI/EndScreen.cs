using Godot;

public partial class EndScreen : Control
{
	[Export] private Label finalTime; 

	[Export] private Label bestTime; 

	[Export] private Label congrats; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var temp = PlayerGameplayDataHandler.Instance.ReadData(PlayerGameplayDataHandler.Instance.CurrentLevelId);
		finalTime.Text = $"{temp.lastTime:0.00}";

		bestTime.Text = $"{temp.bestTime:0.00}";

		if (PlayerGameplayDataHandler.Instance.newTime)
		{
			//create text at the top that says congrats
			congrats.Visible = true; 

			//then clear it back to false val 
			PlayerGameplayDataHandler.Instance.ClearNewTime();
		}
	}

	public void OnMainMenuPressed()
	{
		SceneManager.Instance.ChangeScene(SceneManager.Instance.MainMenu);
	}
}
