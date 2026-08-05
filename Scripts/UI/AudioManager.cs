using System;
using Godot;

public partial class AudioManager : Node
{
	public static AudioManager Instance { get; private set; }

	//still not quite sure on the diff between properites and fields, i think the best case to make properites are booleans. But trying a field here. 
	private AudioStreamPlayer _mainMenuMusic; 
	private AudioStreamPlayer _levelOneMusic; 
	private AudioStreamPlayer _endCardNormalMusic; 
	private AudioStreamPlayer _endCardNewTimeMusic; 
	#nullable enable
	private AudioStreamPlayer? currentMusic; 


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this; 

		//Note: Chat told me that you should NOT do this if your audio streams are elsewhere. in this project, the audio streams are children of the manager, so this works. 
		//_mainMenuMusic = GetNode<AudioStreamPlayer>("%MainMenuMusic"); //the percent means if we later move mainmenumusic in to a subfolder under audiomanager, the path isnt lost 
		_mainMenuMusic = GetNode<AudioStreamPlayer>("MainMenuMusic");
		_levelOneMusic = GetNode<AudioStreamPlayer>("LevelOneMusic");
		_endCardNormalMusic = GetNode<AudioStreamPlayer>("EndCardNormalMusic");
		_endCardNewTimeMusic = GetNode<AudioStreamPlayer>("EndCardNewTimeMusic");
	}

	public void PlayMenuMusic()
	{
		if (currentMusic is null)
		{
			currentMusic = _mainMenuMusic;
			currentMusic.Play(); 
		}
		else if (currentMusic != _mainMenuMusic) {
			StopCurrent();
			currentMusic = _mainMenuMusic;
			currentMusic.Play(); 
		}
	}
	public void PlayLevelOneMusic()
	{
		if (currentMusic is null)
		{
			currentMusic = _levelOneMusic;
			currentMusic.Play(); 
		}
		else if (currentMusic != _levelOneMusic) {
			StopCurrent();
			currentMusic = _levelOneMusic;
			currentMusic.Play(); 
		}
	}
	public void StopCurrent()
	{
		if (currentMusic != null && currentMusic.Playing)
		{
			currentMusic.Stop(); 
		}
	}

    public void PlayEndSceneMusicNewTime()
    {
        if (currentMusic is null)
		{
			currentMusic = _endCardNewTimeMusic;
			currentMusic.Play(); 
		}
		else if (currentMusic != _endCardNewTimeMusic) {
			StopCurrent();
			currentMusic = _endCardNewTimeMusic;
			currentMusic.Play(); 
		}
    }

	public void PlayEndSceneMusicNormal()
    {
        if (currentMusic is null)
		{
			currentMusic = _endCardNormalMusic;
			currentMusic.Play(); 
		}
		else if (currentMusic != _endCardNormalMusic) {
			StopCurrent();
			currentMusic = _endCardNormalMusic;
			currentMusic.Play(); 
		}
    }

}
