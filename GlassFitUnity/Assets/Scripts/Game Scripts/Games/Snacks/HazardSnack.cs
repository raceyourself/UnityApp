﻿using UnityEngine;
using System.Collections;

public class HazardSnack : SnackBase {
	
	GestureHelper.ThreeFingerTap threeHandler = null;
	
	// Use this for initialization
	void Start () {
	
	}
	
	public override void Begin() {
		base.Begin();
		
		SetTrack(false);
		
		SetMainCamera(false);
		
		threeHandler = new GestureHelper.ThreeFingerTap(() => {
			Finish();
		});
		
		GestureHelper.onThreeTap += threeHandler;
		
		SetThirdPerson(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnDestroy()
	{
		GestureHelper.onThreeTap -= threeHandler;
	}
}