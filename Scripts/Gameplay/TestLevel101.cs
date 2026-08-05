using Godot;

public partial class TestLevel101 : Node3D
{
	//read more into statics they seem to make persistance 
	private double timeElapsedThisLevel;

	private static readonly string levelId = "level1"; //THIS IS BAD AND THERE IS A BETTER WAY BUT WHATEVES 

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timeElapsedThisLevel += delta;
	}

	public void WhenEndReached(Node3D body)
	{
		if (body is not Player player)
		{
			return;
		}
        //order is important here. We HAVE To submit data before switching to eend scene, otherwise 
        //gameplay manager has no clue what level we were at. maybe its better to make a new mehtod 
        // called updateCurrentlevel or some bs but eh thats future me's problem. i love tech debt 
        PlayerGameplayDataHandler.Instance.SubmitData(levelId, timeElapsedThisLevel);

        ToggleMouseCapture();

		SceneManager.Instance.CallDeferred(
			nameof(SceneManager.ChangeScene),
			SceneManager.Instance.EndScene
		);
	}

	//duplicate of the one in player. later, delete both this and one from player move both into one loc 
	private void ToggleMouseCapture()
	{
		Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
			? Input.MouseModeEnum.Visible
			: Input.MouseModeEnum.Captured;
	}
}
