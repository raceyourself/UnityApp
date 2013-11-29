using UnityEngine;
using System.Collections;

public class DemoController : TargetController {

	private Animator anim; 
	private float speed;
	private bool started = false;
	
	// Use this for initialization
	void Start () {
		//target = Platform.Instance.getTargetTracker();
		base.SetAttribs(0, 135, -254.6f, 50);
		anim = GetComponent<Animator>();
		
	}
	
	void OnEnable() {
		//base.OnEnable();
		base.SetAttribs(0, 135, -254.6f, 50);
	}
	
	void Update () {
		if (target == null) return;
				
		if(!started) {
			started = true;
			anim.speed = target.PollCurrentSpeed() / 2.2f;
			speed = target.PollCurrentSpeed();
			anim.SetFloat("Speed", speed);
		}
		
		base.Update();
		float newSpeed = target.PollCurrentSpeed();
		if(speed != newSpeed)
		{
			speed = newSpeed;
			anim.SetFloat("Speed", speed);
			if(speed > 2.2f && speed < 4.0f) {
				anim.speed = newSpeed / 2.2f;
			} else if(speed > 4.0f) {
				anim.speed = Mathf.Clamp(newSpeed / 4.0f, 1, 2);
			} else {
				anim.speed = newSpeed / 1.25f;
			}
		}
	}
}