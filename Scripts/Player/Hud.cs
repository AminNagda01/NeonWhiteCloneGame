using Godot;

public partial class Hud : CanvasLayer
{
	[Export] private Label _timer; 
	private static double timeElapsedThisLevel; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		timeElapsedThisLevel = 0.00;
		_timer.Text = $"{timeElapsedThisLevel:0.00}";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timeElapsedThisLevel += delta; 
		_timer.Text = $"{timeElapsedThisLevel:0.00}";
	}
}
