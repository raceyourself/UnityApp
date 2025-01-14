﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	
	private GestureHelper.TwoFingerTap twoHandler = null;
	
	private float startPitch;
	
	private bool lookingUp = false;
	private bool lookingDown = false;
	
	private bool countingDown = false;
	
	private float maxTime = 1.0f;
	
	private float currentTime = 0.0f;
	
	private bool jump = false;
	
	private new Collider collider;
	
	// Use this for initialization
	void Start () {
		//Platform.Instance.GetPlayerOrientation().Reset();
		
		startPitch = Platform.Instance.GetPlayerOrientation().AsPitch() * Mathf.Rad2Deg;
		
		twoHandler = new GestureHelper.TwoFingerTap(() => {
			Platform.Instance.GetPlayerOrientation().Reset();
			startPitch = Platform.Instance.GetPlayerOrientation().AsPitch() * Mathf.Rad2Deg;
		});
		
		UnityEngine.Debug.Log("PlayerController: started");
		
		rigidbody.centerOfMass = new Vector3(0, 0, 0);
		
		collider = GetComponentInChildren<Collider>();
		
		GestureHelper.onTwoTap += twoHandler;
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyDown(KeyCode.Space))
		{
			jump = true;
		}
		// TODO: once yaw/roll issue is fixed change back to roll
		float roll =  Platform.Instance.GetPlayerOrientation().AsRoll();
		
		roll = Mathf.Rad2Deg * roll;
		
		float xPos = Mathf.Clamp(roll / 10f, -1f, 1f);
		
		Vector3 pos = transform.position;
		pos.x = -xPos;
		transform.position = pos;
		
		float pitch = Platform.Instance.GetPlayerOrientation().AsPitch();
		pitch *= Mathf.Rad2Deg;
		
		if(startPitch - pitch > 10f && !lookingUp)
		{
			UnityEngine.Debug.Log("PlayerController: possible jump...");
			countingDown = true;
			lookingUp = true;
			jump = true;
		}
		else if(startPitch - pitch < -10f && !lookingDown)
		{
			UnityEngine.Debug.Log("PlayerController: possible slide...");
			countingDown = true;
			lookingDown = true;
			
			DoSlide();
		}
		
		if(countingDown)
		{
			currentTime += Time.deltaTime;
			if(currentTime < maxTime)
			{
				if(lookingUp)
				{
					if(pitch > startPitch - 5f)
					{
						lookingUp = false;
						countingDown = false;
						currentTime = 0.0f;
						UnityEngine.Debug.Log("PlayerController: jump actually detected!");
					}
				}
				else if(lookingDown)
				{
					if(pitch < startPitch + 5f)
					{
						lookingDown = false;
						countingDown = false;
						currentTime = 0.0f;
						UnityEngine.Debug.Log("PlayerController: slide actually detected!");
					}
				}
			}
			else
			{
				UnityEngine.Debug.Log("PlayerController: stopping the countdown");
				countingDown = false;
				currentTime = 0.0f;
			}
		}
		
		if(lookingUp && pitch > startPitch - 5f)
		{
			lookingUp = false;
		}
		else if(lookingDown && pitch < startPitch + 5f)
		{
			lookingDown = false;
		}
	}
	
	void FixedUpdate()
	{
		if(jump)
		{
			jump = false;
			//if(Physics.Raycast(transform.position, -Vector3.up, collider.bounds.extents.y + 0.1f))
//			if(collider != null)
//			{
//				if(Physics.Raycast(transform.position, -Vector3.up, collider.bounds.extents.y))
//				{
					DoJump();
//				}
//			}
			
		}
	}
	
	void DoJump()
	{
		//rigidbody.isKinematic = false;
		rigidbody.AddForce(new Vector3(0, 5, 0), ForceMode.Impulse);
		//rigidbody.isKinematic = true;
	}
	
	void DoSlide()
	{
		
	}
	
	void OnDestroy()
	{
		GestureHelper.onTwoTap -= twoHandler;
	}
}
