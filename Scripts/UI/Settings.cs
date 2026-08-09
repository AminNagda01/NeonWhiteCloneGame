using Godot;

public partial class Settings : Control
{
	[Export] private HSlider _fovSlider;
	[Export] private HSlider _sensitivitySlider;
	[Export] private Label _fovLabel;
	[Export] private Label _sensitivityLabel;

	public override void _Ready()
	{
		_fovSlider.Value = PlayerSettings.Instance.BaseFov;
		_fovLabel.Text = $"{PlayerSettings.Instance.BaseFov:0}";
   		_sensitivitySlider.Value = PlayerSettings.Instance.MouseSensitivity;
		_sensitivityLabel.Text = $"{PlayerSettings.Instance.MouseSensitivity:0.0000}";
	}
	
	public void OnFovChange(float newValue) {
		PlayerSettings.Instance.BaseFov = newValue;
		_fovLabel.Text = $"{newValue:0}";
	}
	public void OnFovChangeEnded(bool changed) {
		if (changed){
        	PlayerSettings.Instance.Save();
		}
	}
	public void OnSensitivityChange(float newValue) {
		PlayerSettings.Instance.MouseSensitivity = newValue;
		_sensitivityLabel.Text = $"{newValue:0.0000}";
	}
	public void OnSensitivityChangeEnded(bool changed) {
		if (changed){
        	PlayerSettings.Instance.Save();
		}
	}

	public void OnDefaultsPressed()
	{
		PlayerSettings.Instance.RestoreDefaults(); 
		_Ready(); 
	}

	public void OnBackButtonPressed()
	{
		SceneManager.Instance.OnBack();
	}
}
