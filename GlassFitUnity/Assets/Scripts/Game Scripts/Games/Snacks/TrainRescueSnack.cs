﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TrainRescueSnack : SnackBase {
	
	protected TrainController_Rescue train;
	protected ConstantVelocityPositionController trainPositionController;
	
	private int trainLevel = 0;
	public RYWorldObject trainObject = null;
	public RYWorldObject damsel = null;
	public CameraPath openingFlythroughPath = null;

	protected virtualTrack leftTrack = null;
	protected virtualTrack rightTrack = null;
	
	float junctionSpacing = 250.0f;
	bool bFailed = false;
	
	bool started = false;
	
	//list of track pieces we create for the flythrough.
	protected List<GameObject> extraTrackPieces;
	
	bool bWaitedForSubtitleTimeOut = false;
	
	float finishDistance = 350f;
	
	bool readyToStart = false;
	
	double playerStartDistance = 0;
	
	public GameObject flyCamera;

	// Use this for initialization
	public override void Start () {
		train = trainObject.gameObject.GetComponent<TrainController_Rescue>();
		trainPositionController = trainObject.gameObject.GetComponent<ConstantVelocityPositionController>();
	
		damsel.setScenePositionFrozen(true);

		//clear strings
		DataVault.Set("train_subtitle", " ");
		DataVault.Set("game_message", " ");
		
		//set flag so that trophy is shown if we win
		DataVault.Set("showFinishTrophy", true);
		
		UnityEngine.Debug.Log("Train: finish = " + finishDistance);
		
		//create some additional tracks to put on the flythrough
		extraTrackPieces = new List<GameObject>();
		float totalTrackDistCovered = 500.0f;	//half of one track obj
		float trackPiecePosition = 0.0f;
		
		GameObject rightTrackObj = GameObject.Find("rightTrack");
		if(rightTrack == null)
		{
			UnityEngine.Debug.Log("Train: couldn't find right hand track");
		}
		rightTrack = rightTrackObj.GetComponent<virtualTrack>();
		rightTrack.frozen = true;

		GameObject leftTrackObj = GameObject.Find("leftTrack");
		if(leftTrack == null)
		{
			UnityEngine.Debug.Log("Train: couldn't find left hand track");
		}
		leftTrack = leftTrackObj.GetComponent<virtualTrack>();
		leftTrack.frozen = true;

		while(totalTrackDistCovered <= finishDistance + 500.0f)
		{
			//create another one, 1km further on
			trackPiecePosition += 1000.0f;
			
			//duplicate existing track
			GameObject newTrackPiece = GameObject.Instantiate(rightTrackObj) as GameObject;
			newTrackPiece.transform.localPosition = newTrackPiece.transform.localPosition + new Vector3(0,0,trackPiecePosition);
			extraTrackPieces.Add(newTrackPiece);
			
			GameObject newTrackPieceLeft = GameObject.Instantiate(leftTrackObj) as GameObject;
			newTrackPieceLeft.transform.localPosition = newTrackPieceLeft.transform.localPosition + new Vector3(0,0,trackPiecePosition);
			extraTrackPieces.Add(newTrackPieceLeft);
			
			totalTrackDistCovered += 1000.0f;
		
		}
		
		UnityEngine.Debug.Log("TrainRescueSnack: the object is " + DataVault.Get("train_level").ToString());
		
		trainLevel = (int)DataVault.Get("train_level");
		
		SetLevel(trainLevel);
	}

	public void SetLevel(int level)
	{
		trainLevel = level;
		trainPositionController.setSpeed( 2.4f + (trainLevel * 0.5f) );
		UnityEngine.Debug.Log("TrainController: level is " + trainLevel.ToString());
	}

	protected double GetDistanceBehind ()
	{
		return train.GetDistanceBehindTarget();
	}
	
	public void SetReadyToStart (bool ready)
	{		
		if(openingFlythroughPath != null)
		{
			//freeze the damsel position
			openingFlythroughPath.StartFollowingPath();	
			SetMainCamera(false);
		}
		else
		{
			UnityEngine.Debug.LogError("Train: Don't have camera path set!");	
		}

		//start the music
		GameObject musicPlayer = GameObject.Find("MusicPlayer");
		AudioSource musicSource = (AudioSource)musicPlayer.GetComponent(typeof(AudioSource));
		musicSource.Play();
	}
	
	public override void Begin ()
	{
		base.Begin();
		
		SetTrack(false);
		SetReadyToStart(true);
		//transform.position = new Vector3(0, 0, (float)Platform.Instance.LocalPlayerPosition.Distance);
	}
	
	// Update is called once per frame
	public override void Update () {
		if(!finish && started)
		{
			//UnityEngine.Debug.Log("TrainRescueSnack: in finish loop");
			//check if the train has reached the end
			if(train.GetForwardDistance() - playerStartDistance > finishDistance && !finish)
			{
				//UnityEngine.Debug.Log("TrainRescueSnack: train has killed that woman");
				DataVault.Set("death_colour", "EA0000FF");
				DataVault.Set("snack_result", "You lost!");
				DataVault.Set("snack_result_desc", "the damsel is dead!");
				StartCoroutine(ShowBanner(3.0f));
				finish = true;
			}
			else if(GetPlayerDistanceTravelled() > finishDistance && !finish)
			{
				trainLevel++;
				DataVault.Set("death_colour", "12D400FF");
				StartCoroutine(SetWinningText());
				StartCoroutine(ShowBanner(6.0f));
				finish = true;
				DataVault.Set("train_level", trainLevel);
				DataVault.SaveToBlob();
			}
			
		}
		
		UpdateAhead(GetDistanceBehind());
		
		//check if the flythrough is complete
		if(!readyToStart)
		{
			//UnityEngine.Debug.Log("TrainRescueSnack: checking to see if flythrough finished");
			if(openingFlythroughPath.IsFinished())
			{
				SetMainCamera(true);
				flyCamera.GetComponentInChildren<Camera>().enabled = false;

				//unfreeze damsel
				damsel.setScenePositionFrozen(false);

				transform.position = new Vector3(0, 0, (float)Platform.Instance.LocalPlayerPosition.Distance);
				StartCountdown();
			}
		}
		
	}
	
	IEnumerator SetWinningText()
	{
		DataVault.Set("snack_result", "You won!");
		DataVault.Set("snack_result_desc", "you saved her life!");
		
		yield return new WaitForSeconds(3.0f);
		
		DataVault.Set("snack_result", "Train is now level " + trainLevel.ToString());
		DataVault.Set("snack_result_desc", "It's now harder to beat!");
	}
	
	public void StartCountdown()
	{
		//delete extra track pieces
		foreach(GameObject piece in extraTrackPieces)
		{
			Destroy(piece);	
		}
		
		readyToStart = true;
		
		if(train == null)
		{
			train = trainObject.GetComponent<TrainController_Rescue>();
		}
		train.BeginRace();
		playerStartDistance = Platform.Instance.LocalPlayerPosition.Distance;
		//progress flow to the normal HUD
		StartCoroutine(DoCountDown());
	}
	
	IEnumerator DoCountDown()
	{
		UnityEngine.Debug.Log("Train:Starting Countdown Coroutine");
		for(int i=3; i>=0; i--)
		{
			//go to subtitle card
			UnityEngine.Debug.Log("Train: Following 'subtitle' connector");
			FlowState.FollowFlowLinkNamed("Subtitle");
			//set value for subtitle. 0 = GO
			string displayString = (i==0) ? "GO !" : i.ToString();
			DataVault.Set("train_subtitle", displayString);
			
			//wait half a second
			yield return new WaitForSeconds(0.5f);
			
			//return to cam
			UnityEngine.Debug.Log("Train: Following 'toblank' connector");
			FlowState.FollowFlowLinkNamed("ToBlank");
			
			//wait a second more, except after GO!
			if(i!=0)
			{
				yield return new WaitForSeconds(0.5f);
			}
			
		}
		
		yield return new WaitForSeconds(0.1f);
		
		UnityEngine.Debug.Log("Train: Following 'begin' connector");
		FlowState.FollowFlowLinkNamed("Begin");

		//unfreeze the track
		leftTrack.frozen = false;
		rightTrack.frozen = false;
		
		started = true;
		//play the train's bell sound effect
		train.soundBell();	
		
	}
	
	public double GetPlayerDistanceTravelled()
	{
		return Platform.Instance.LocalPlayerPosition.Distance - playerStartDistance;
	}
}
