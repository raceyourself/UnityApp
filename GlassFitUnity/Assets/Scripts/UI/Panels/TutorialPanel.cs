using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class TutorialPanel : HexPanel
{
	protected float elapsedTime = 0.0f;
	
	protected bool shouldAdd = true;
	
	private float maxTime = 3.0f;
	
	private GameObject highlightText = null;
	
	private GestureHelper.ThreeFingerTap threeHandler = null;
    
	private GestureHelper.TwoFingerTap twoHandler = null;
	
	private GestureHelper.OnBack backHandler = null;
	
	private Camera camera;
	
	private UITexture firstSpeech;
	
    public TutorialPanel() { }
    public TutorialPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
    }

    /// <summary>
    /// Gets display name of the node, helps with node identification in editor
    /// </summary>
    /// <returns>name of the node</returns>
    public override string GetDisplayName()
    {
        //base.GetDisplayName();

        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "Tutorial Panel: " + gName.Value;
        }
        return "Tutorial Panel: UnInitialized";
    }
	
	public void QuitApp() {
		if(!IsPopupDisplayed()) {
			GestureHelper.onBack -= backHandler;
			Application.Quit();
		}
	}
	
	public bool IsPopupDisplayed() {
		HexInfoManager info = GameObject.FindObjectOfType(typeof(HexInfoManager)) as HexInfoManager;
		if(info != null) {
			if(info.IsInOpenStage()) {
				info.AnimExit();
				return true;
			}
		}
		return false;
	}
	
    /// <summary>
    /// Primary button preparation for panel entering state
    /// </summary>
    /// <returns></returns>
    public override void EnterStart()
    {
		//ResetButtonData();
		//UnityEngine.Debug.Log("TutorialPanel: setting two finger tap");
		base.EnterStart();
		
		DataVault.Set("tutorial_complete", true);
		DataVault.SaveToBlob();
		
		UnityEngine.Debug.Log("TutorialPanel: screen width is " + Screen.width + ", height is " + Screen.height);
		
		DataVault.Set("rp", (int)Platform.Instance.PlayerPoints.OpeningPointsBalance);
		DataVault.Set("metabolism", (int)Platform.Instance.PlayerPoints.CurrentMetabolism);
		
		DataVault.Set("race_type", "tutorial");
		
		backHandler = new GestureHelper.OnBack(() => {
			QuitApp();
		});
        GestureHelper.onBack += backHandler;


		Platform.Instance.GetPlayerOrientation().Reset();
		InitialButtons();
		elapsedTime = 0.0f;


		//UnityEngine.Debug.Log("TutorialPanel: Setting datavault stuff");
		DataVault.Set("tutorial_hint", " ");
		DataVault.Set("highlight", " ");
		
		//UnityEngine.Debug.Log("TutorialPanel: Setting three finger tap");
		threeHandler = new GestureHelper.ThreeFingerTap(() => {
			StraightToMenu();
		});
		
		GestureHelper.onThreeTap += threeHandler;
		
		//UnityEngine.Debug.Log("TutorialPanel: about to find camera");
		GameObject cam = GameObject.Find("Scene Camera");
		
		if(cam != null) {
			camera = cam.camera;
		}
		
		GameObject speechBubble = GameObject.Find("FirstSpeech");
		if(speechBubble != null) {
			firstSpeech = speechBubble.GetComponent<UITexture>();
		} else {
			firstSpeech = new UITexture();
		}
		
		firstSpeech.alpha = 0;
		
    }
	
	public void StraightToMenu() {
		GConnector gConnect = Outputs.Find(r => r.Name == "MenuExit");
		if(gConnect != null) 
		{
			GestureHelper.onThreeTap -= threeHandler;
			parentMachine.FollowConnection(gConnect);
		} else 
		{
			UnityEngine.Debug.Log("TutorialPanel: Error finding menu exit");
		}
	}
	
	public void UpdateSpeechBubble() {
		if(elapsedTime > 4.0f)
		{
			if(buttonData.Count == 2) 
			{
				if(firstSpeech != null) {
					firstSpeech.alpha += 1.0f * Time.deltaTime;
				}
			}
		} 
		else
		{
			if(firstSpeech != null) {
				firstSpeech.alpha = 0.0f;
			}
		}
	}
	
	public override void StateUpdate ()
	{
		base.StateUpdate ();

#if UNITY_EDITOR
        if (buttonData.Count == 0 && Input.GetMouseButtonDown(0))
        {
            InitialButtons();
            elapsedTime = 0.0f;
        }
#endif
				
		elapsedTime += Time.deltaTime;
		
		if(elapsedTime > maxTime && shouldAdd)
		{
			elapsedTime -= maxTime;
			AddButton();
		}
		
		if(elapsedTime > 10.0f && buttonData.Count == 7) 
		{
			AddFinalString();
		}
		
		UpdateSpeechBubble();
		
		if(camera != null) 
		{
			if((camera.transform.rotation.eulerAngles.x > 30 && camera.transform.rotation.eulerAngles.x < 330) || (camera.transform.rotation.eulerAngles.y > 40 && camera.transform.rotation.eulerAngles.y < 320)) {
				DataVault.Set("tutorial_hint", "Tap with two fingers to center view");
				LoadingTextComponent.SetVisibility(true);
			}
			else
			{
				string tutHint = (string)DataVault.Get("tutorial_hint");
				if(tutHint == "Tap with two fingers to center view") {
					DataVault.Set("tutorial_hint", " ");
				}
				
			}
		}
		
		if(buttonData.Count == 0) 
		{
			DataVault.Set("tutorial_hint", "Tap with two fingers to center view");
		}
		
	}
	
	public virtual void AddFinalString()
	{
		DataVault.Set("highlight", "Highlight the hex and tap to start");
	}
	
	public virtual void AddButton() 
	{
		if (buttonData.Count == 1)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 0;
            hbd.column = -1;
            hbd.buttonName = "LookHex";
            hbd.displayInfoData = false;
            hbd.textNormal = "Look at this tile to select it";
            hbd.allowEarlyHover = true;
			
			shouldAdd = false;
			
			maxTime = 10.0f;
			
            buttonData.Add(hbd);

        } else if (buttonData.Count == 3)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = -1;
            hbd.column = 0;
            hbd.buttonName = "TheseHex";
            hbd.displayInfoData = false;
            hbd.textNormal = "Tiles represent games";
            hbd.allowEarlyHover = true;

            buttonData.Add(hbd);
        } else if (buttonData.Count == 4)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 0;
            hbd.column = 1;
            hbd.buttonName = "ChallengeHex";
            hbd.displayInfoData = false;
            hbd.textNormal = "Try launching the one below";
            hbd.allowEarlyHover = true;

            buttonData.Add(hbd);
        } else if(buttonData.Count == 5) 
		{
			HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = 0;
            hbd.buttonName = "TryHex";
			hbd.imageName = "activity_run";
            hbd.displayInfoData = true;
			hbd.activityName = "First Race";
			hbd.activityContent = "Learn how to use Race Yourself";
			
			shouldAdd = false;
			buttonData.Add(hbd);
			
			GConnector gameExit = Outputs.Find(r => r.Name == "GameExit");
			
			GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
			
			GConnector gc = NewOutput(hbd.buttonName, "Flow");
            gc.EventFunction = "SetTutorial";
			
			if(gameExit.Link.Count > 0) {
				gComponent.Data.Connect(gc, gameExit.Link[0]);
			}
		}

        DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
        list.UpdateButtonList();
	}

    /// <summary>
    /// Initial button definitions
    /// </summary>
    /// <returns></returns>
    public virtual void InitialButtons()
    {
        HexButtonData hbd = new HexButtonData();
        hbd.row = 0;
        hbd.column = 0;
        hbd.buttonName = "tutorialButton1";
        hbd.displayInfoData = false;
        hbd.textBold = "Welcome";
        hbd.allowEarlyHover = true;
		hbd.backgroundTileColor = 0x00A30EFF;
        buttonData.Add(hbd);
		
//		DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
//		if(list != null) {
//       		list.UpdateButtonList();
//    	}
	}

    /// <summary>
    /// On hover activity opening further buttons. Example shows opening two buttons first and then one after another based on rollover action
    /// </summary>
    /// <param name="button">button which triggered action</param>
    /// <param name="justStarted">is ti hover in(true) or hover out(false)</param>
    /// <returns></returns>
    public override void OnHover(FlowButton button, bool justStarted)
    {
 	     base.OnHover(button, justStarted);

		if (justStarted == true && button != null)
        {                        
			if(button.name == "LookHex" && buttonData.Count == 2)
			{
				HexButtonData hbd = new HexButtonData();
	            hbd.row = -1;
	            hbd.column = -1;
	            hbd.buttonName = "NiceHex";
	            hbd.displayInfoData = false;
	            hbd.textNormal = "Now select this tile";
                hbd.allowEarlyHover = true;
				
				elapsedTime = 0f;
				shouldAdd = true;
				
	            buttonData.Add(hbd);
			} 
			else if (button.name == "NiceHex" && buttonData.Count == 3)
	        {
	            HexButtonData hbd = new HexButtonData();
	            hbd.row = -1;
	            hbd.column = 0;
	            hbd.buttonName = "TheseHex";
	            hbd.displayInfoData = false;
	            hbd.textNormal = "Tiles represent games";
                hbd.allowEarlyHover = true;
				
				elapsedTime = 0f;
	            buttonData.Add(hbd);
	        } else if (button.name == "TheseHex" && buttonData.Count == 4)
	        {
	            HexButtonData hbd = new HexButtonData();
	            hbd.row = 0;
	            hbd.column = 1;
	            hbd.buttonName = "ChallengeHex";
	            hbd.displayInfoData = false;
	            hbd.textNormal = "Try launching the one below";
                hbd.allowEarlyHover = true;
	
				elapsedTime = 0f;
	            buttonData.Add(hbd);
	        } else if(button.name == "ChallengeHex" && buttonData.Count == 5) 
			{
				HexButtonData hbd = new HexButtonData();
	            hbd.row = 1;
	            hbd.column = 0;
	            hbd.buttonName = "TryHex";
				hbd.imageName = "activity_run";
	            hbd.displayInfoData = true;
				hbd.activityName = "First Race";
				hbd.activityContent = "Learn how to use Race Yourself";
				
	            elapsedTime = 0f;
				shouldAdd = false;
				buttonData.Add(hbd);
				
				GConnector gameExit = Outputs.Find(r => r.Name == "GameExit");
				
				GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
				
				GConnector gc = NewOutput(hbd.buttonName, "Flow");
	            gc.EventFunction = "SetTutorial";
				
				if(gameExit.Link.Count > 0) {
					gComponent.Data.Connect(gc, gameExit.Link[0]);
				}
			}
			
	        
			DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
	        list.UpdateButtonList();
		}
         
    }       
	
	public override void Exited ()
	{
		base.Exited ();
		GestureHelper.onThreeTap -= threeHandler;
		GestureHelper.onTwoTap -= twoHandler;
		GestureHelper.onBack -= backHandler;
		DataVault.Set("highlight", " ");
	}
}
