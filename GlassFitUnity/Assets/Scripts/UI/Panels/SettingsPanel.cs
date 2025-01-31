using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class SettingsPanel : Panel {
	
	public SettingsPanel(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) {
	}
	
	public SettingsPanel() {}
	
	protected override void Initialize()
    {
        base.Initialize();
	}
	
	public override void OnClick(FlowButton button)
	{
		base.OnClick(button);
		
		RaceGame ss = (RaceGame) GameObject.FindObjectOfType(typeof(RaceGame));
		if(ss != null) 
		{
			switch(button.name)
			{
			// These two buttons set the target to the runner and cyclist respectively
			case "RunnerButton":
				ss.SetActorType(RaceGame.ActorType.Runner);
				break;
				
			case "CyclistButton":
				ss.SetActorType(RaceGame.ActorType.Cyclist);
				break;
				
			// Sets indoor mode
			case "IndoorButton":
				//ss.SetIndoor();
				bool indoor = Platform.Instance.LocalPlayerPosition.IsIndoor();
				Platform.Instance.LocalPlayerPosition.SetIndoor(!indoor);
				
				break;
				
			// Syncs to the server to authenticate the user.
			case "ServerButton":
//				Debug.Log("SettingsPanel: ServerButton clicked");
	            GConnector gConect = Outputs.Find(r => r.Name == button.name);
				// Follow connection once authentication has returned asynchronously
                NetworkMessageListener.OnAuthenticated handler = null;
                handler = new NetworkMessageListener.OnAuthenticated((authenticated) => {
//					Debug.Log("SettingsPanel: ServerButton authenticated");
					if (authenticated) {
						Platform.Instance.SyncToServer();
						parentMachine.FollowConnection(gConect);
					}
                    Platform.Instance.NetworkMessageListener.onAuthenticated -= handler;
				});
                Platform.Instance.NetworkMessageListener.onAuthenticated += handler;	
				// Trigger authentication
				Platform.Instance.Authorize("any", "login");				
//				Debug.Log("SettingsPanel: ServerButton run");
				break;
				
			// Gets the tracks for the user
			case "GetTrackButton":
				Platform.Instance.GetTracks();
				GameObject.Find("TrackSelect").renderer.enabled = true;
				break;
				
			// Goes back to the game
			case "BackMainButton":
				//ss.Back();
				GameObject h = GameObject.Find("blackPlane");
				h.renderer.enabled = false;
				
				h = GameObject.Find("minimap");
				h.renderer.enabled = true;
				
				break;	
				
			case "FriendButton": 
				Debug.Log("FriendButton clicked");
				break;	
			
			}
		}
	}
}
