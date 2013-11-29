using UnityEngine;
using System.Collections;

/// <summary>
/// Rotates the camera in-game and sets the grid
/// </summary>
public class MinimalSensorCamera : MonoBehaviour {
	
	public Quaternion offsetFromStart;
	
	private bool started;
	private float scaleX;
	private float scaleY;
	public GameObject grid;
	private bool gridOn = false;
	private float gridTimer = 0.0f;
	private bool timerActive = false;

	// Set the grid and scale values
	void Start () {
		grid.SetActive(false);
		scaleX = (float)Screen.width / 800.0f;
		scaleY = (float)Screen.height / 500.0f;
	}
	
	/// <summary>
	/// Raises the GU event. Creates a reset gyro button
	/// </summary>
	void OnGUI()
	{
		// Set the offset if it hasn't been set already, doesn't work in Start() function
		if(!started)
		{
			offsetFromStart = Platform.Instance.GetOrientation();
			offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
			Platform.Instance.ResetGyro();
			started = true;
		}
		
		// Set the new GUI matrix based on scale and the depth
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleX, scaleY, 1));		
		GUI.depth = 7;
		
		if(!started) {
			if(GUI.Button(new Rect(200, 0, 400, 500), "", GUIStyle.none)) {
				started = true;
			}
		} else {
			// Check if the button is being held
			if(GUI.RepeatButton(new Rect(200, 0, 400, 250), "", GUIStyle.none))
			{ 
				// Activates the grid and reset the gyros if the timer is off, turns it off if the timer is on
				if(timerActive) {
					gridOn = false;
				} else {
					offsetFromStart = Platform.Instance.GetOrientation();
					Platform.Instance.ResetGyro();
					gridOn = true;
				}
				gridTimer = 5.0f;
			
			}
			else if(Event.current.type == EventType.Repaint)
			{
				// If the grid is on when the button is released, activate timer, else reset the timer and switch it off
				if(gridOn)
				{
					timerActive = true;
				} else
				{
					gridTimer = 0.0f;
					timerActive = false;
				}
			}	
		}
		GUI.matrix = Matrix4x4.identity;
	}
	
	/// <summary>
	/// Update this instance. Updates the rotation
	/// </summary>
	void Update () {
		// Set the new rotation of the camera
		Quaternion newOffset = Quaternion.Inverse(offsetFromStart) * Platform.Instance.GetOrientation();
		
		// If the timer and grid are on, countdown the timer and switch it off if the timer runs out
		if(timerActive && gridOn)
		{
			gridTimer -= Time.deltaTime;
			if(gridTimer < 0.0f)
			{
				gridOn = false;
				timerActive = false;
			}
		}
		
		grid.SetActive(gridOn);

		transform.rotation =  newOffset;		
	}
}