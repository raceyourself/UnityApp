using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;


public class RaceGame : GameBase {
	 
	public bool end = false;
	
	// Enums for the actor types
	public enum ActorType
	{
		Runner			= 1,
		Cyclist			= 2
	}
	
	private ActorType currentActorType = ActorType.Runner;
    public List<GameObject> actors = new List<GameObject>();

	protected GameObject opponent;

	private float targSpeed = 2.4f;

	// Holds actor templates
	public GameObject cyclistHolder;
	public GameObject runnerHolder;
		
	public override void Start () {
		base.Start();
		
		//instantiate the appropriate actor
		string tar = (string)DataVault.Get("type");
		if(tar == null)
		{
			tar = "Runner";
		}

		opponent = null;

		switch(tar)
		{
		case "Runner":
			currentActorType = ActorType.Runner;
			opponent = runnerHolder;
			targSpeed = 3.0f;
			break;
			
		case "Cyclist":
			currentActorType = ActorType.Cyclist;
			opponent = cyclistHolder;
			targSpeed = 2.4f;
			break;
		}
		
		// Set templates' active status
		cyclistHolder.SetActive(false);
		runnerHolder.SetActive(false);
		
		Platform.Instance.ResetTargets();

		opponent.SetActive(true);

		TargetTracker tracker;
		if(selectedTrack != null) {
			//create a target tracker position controller component and add it to the runner
			tracker = Platform.Instance.CreateTargetTracker(selectedTrack.deviceId, selectedTrack.trackId);
			TargetTrackerPositionController posController = opponent.AddComponent<TargetTrackerPositionController>();
			posController.tracker = tracker;
		} 
		else {
			//create a fixed velocity target tracker
			ConstantVelocityPositionController posController = opponent.AddComponent<ConstantVelocityPositionController>();
			posController.velocity = new Vector3(0,0,targSpeed);
		}

		//Platform.Instance.LocalPlayerPosition.SetIndoor(true);
		//SetReadyToStart(true);
		SetVirtualTrackVisible(true);

	}
	
	public void SetActorType(ActorType targ) {
		currentActorType = targ;
	}
	
	protected void UpdateLeaderboard() {
		double distance = Platform.Instance.LocalPlayerPosition.Distance;
		// TODO: Decide if we are allowed to sort in place or need to make a copy
		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
		int position = 1;
		
		if(trackers != null){
			trackers.Sort(delegate(TargetTracker x, TargetTracker y) {
				return y.GetTargetDistance().CompareTo(x.GetTargetDistance());
			} );
		
			foreach (TargetTracker tracker in trackers) {
				if (tracker.GetTargetDistance() > distance) position++;
		}
		}
		DataVault.Set("ahead_col_box", UIColour.red);
		
		DataVault.Set("leader_header", "You are");
		if (position == 1) { 
			DataVault.Set("ahead_leader", "in the lead!");
			DataVault.Set("ahead_col_box", UIColour.green);
		}  else {
			DataVault.Set("ahead_leader", "behind by " + UnitsHelper.SiDistance(trackers[0].GetDistanceBehindTarget()));
		}
		
		DataVault.Set("position_header", "Position");
		string nth = position.ToString();
		if (position == 1) nth += "st";
		if (position == 2) nth += "nd";
		if (position == 3) nth += "rd";
		if (position >= 4) nth += "th";
		if (position > 2 && position == trackers.Count + 1) nth = "Last!";
		DataVault.Set("position_box", nth);
		
		// Find closest (abs) target
		TargetTracker nemesis = null;
		TargetTracker upstream = null;
		if (position > 1) upstream = trackers[position - 2]; // 1->0 indexing
		TargetTracker downstream = null;
		if (position < trackers.Count + 1) 
		{
			downstream = trackers[position - 1]; // 1->0 indexing
		}
			
		if (upstream != null && downstream != null) {
			if (Math.Abs(upstream.GetDistanceBehindTarget()) <= Math.Abs(downstream.GetDistanceBehindTarget())) nemesis = upstream;
			else nemesis = downstream;
		}  
		else if (upstream != null) nemesis = upstream;
		else if (downstream != null) nemesis = downstream;		
		
		if (nemesis != null) {
			double d = nemesis.GetDistanceBehindTarget();
			string which = " behind";
			if (d > 0) which = " ahead";
			DataVault.Set("follow_header", nemesis.name + " is"); 
			DataVault.Set("follow_box", UnitsHelper.SiDistance(Math.Abs(d)) + which);
		}  else {
			DataVault.Set("follow_header", "Solo");
			DataVault.Set("follow_box", "round!");
		}
	}
	

	
	public override void Update () {
		base.Update ();
	}
}

