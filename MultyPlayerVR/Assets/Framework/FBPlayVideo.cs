using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System;

public class FBPlayVideo : MonoBehaviour {

	public RawImage image;
	private VideoPlayer videoPlayer;
	private VideoSource videoSource;
	private AudioSource audioSource;

	// Use this for initialization
	void Start () {	
		Application.runInBackground = true;
		StartCoroutine(playVideo());
	}

	//Use this for play video
	IEnumerator playVideo()
	{    		
		videoPlayer = gameObject.AddComponent<VideoPlayer>();		      
		audioSource = gameObject.AddComponent<AudioSource>();

		videoPlayer.playOnAwake = false;
		audioSource.playOnAwake = false;
		audioSource.Pause();	

	
		if (FBUtils.isValidUrl(FBUtils.videoUrl)){	//play video which have url online
				videoPlayer.source = VideoSource.Url;    
				videoPlayer.url = FBUtils.videoUrl;
		} else{		//play video load from device
				videoPlayer.source = VideoSource.VideoClip;
				videoPlayer.clip = (VideoClip)Resources.Load( FBUtils.videoUrl,typeof(VideoClip));
		}	


		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;		  
		videoPlayer.EnableAudioTrack(0, true);
		videoPlayer.SetTargetAudioSource(0, audioSource);

		videoPlayer.Prepare();

		WaitForSeconds waitTime = new WaitForSeconds(1);
		while (!videoPlayer.isPrepared)
		{
			Debug.Log("Preparing Video");         
			yield return waitTime;        
			break;
		}

		Debug.Log("Done Preparing Video");

		image.texture = videoPlayer.texture;

		videoPlayer.Play();

		audioSource.Play();

		Debug.Log("Playing Video");
		while (videoPlayer.isPlaying)
		{
			Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
			yield return null;
		}
		Application.LoadLevel ("startScene");
		Debug.Log("Done Playing Video");

		//move to previous scene
		Application.LoadLevel (FBUtils.playVideo_previousScene);
	}
}
