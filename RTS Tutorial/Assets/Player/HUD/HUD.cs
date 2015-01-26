using UnityEngine;
using System.Collections.Generic;
using RTS;
using System;

public class HUD : MonoBehaviour {
	
	public GUISkin resourceSkin, ordersSkin, selectBoxSkin, /*mouseCursorSkin,*/ playerDetailsSkin;
	//public Texture2D activeCursor;
	//public Texture2D selectCursor, leftCursor, rightCursor; // upCursor, downCursor, rallyPointCursor;
	//public Texture2D[] attackCursors, harvestCursors, moveCursors;
	public Texture2D[] resources, resourceHealthBars;
	public Texture2D buttonHover, buttonClick, smallButtonHover, smallButtonClick;
	public Texture2D buildFrame, buildMask;
	public Texture2D healthy, damaged, critical;
	public AudioClip clickSound;
	public float clickVolume = 1.0f;
	
	private Player player;
//	private CursorState activeCursorState, previousCursorState;
	private int /*currentFrame = 0,*/ buildAreaHeight = 0;
	private Dictionary<ResourceType,int> resourceValues;
	private Dictionary<ResourceType,Texture2D> resourceImages;
	private WorldObject lastSelection;
	private float sliderValue;
	private AudioElement audioElement;
	
	private const int ORDERS_BAR_WIDTH = 250, RESOURCE_BAR_HEIGHT = 40;
	private const int SELECTION_NAME_HEIGHT = 15, SCROLL_BAR_WIDTH = 22, BUTTON_SPACING = 7;
	private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
	private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64, BUILD_IMAGE_PADDING = 8;
	
	private const int BAR_HEIGHT = 150;
	
	Camera miniMapCamera;
	Texture camTexture;
	
	/*** Game Engine Methods ***/
	
	void Start () {
		player = transform.root.GetComponent<Player>();
		resourceValues = new Dictionary<ResourceType, int>();
		//resourceLimits = new Dictionary<ResourceType, int>();
		resourceImages = new Dictionary<ResourceType, Texture2D>();
		for(int i=0; i<resources.Length; i++) {
			switch(resources[i].name) {
				case "Money":
					resourceImages.Add(ResourceType.Money, resources[i]);
					resourceValues.Add(ResourceType.Money, 0);
					//resourceLimits.Add(ResourceType.Money, 0);
					break;
				/*case "Power":
					resourceImages.Add(ResourceType.Power, resources[i]);
					resourceValues.Add(ResourceType.Power, 0);
					resourceLimits.Add(ResourceType.Power, 0);
					break;*/
				default: break;
			}
		}
		/*Dictionary<ResourceType, Texture2D> resourceHealthBarTextures = new Dictionary<ResourceType, Texture2D>();
		for(int i=0; i<resourceHealthBars.Length; i++) {
			switch(resourceHealthBars[i].name) {
			case "ore":
				resourceHealthBarTextures.Add(ResourceType.Ore, resourceHealthBars[i]);
				break;
			default: break;
			}
		}*/
		//ResourceManager.SetResourceHealthBarTextures(resourceHealthBarTextures);
		buildAreaHeight = 250;//Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;
		ResourceManager.StoreSelectBoxItems(selectBoxSkin, healthy, damaged, critical);
	//	SetCursorState(CursorState.Select);
		List<AudioClip> sounds = new List<AudioClip>();
		List<float> volumes = new List<float>();
		sounds.Add(clickSound);
		volumes.Add (clickVolume);
		audioElement = new AudioElement(sounds, volumes, "HUD", null);
	}

	void OnGUI () {
		//we only want to draw a GUI for human players
		if(player.human) {
			DrawPlayerDetails();
			DrawResourceBar();
			DrawOrdersBar();
			//call last to ensure that the custom mouse cursor is seen on top of everything
//			DrawMouseCursor();
		}
	}
	
	/*** Public methods for interacting with the HUD ***/
	
	public bool MouseInBounds() {
		//Screen coordinates start in the lower-left corner of the screen
		//not the top-right of the screen like the drawing coordinates do
		Vector3 mousePos = Input.mousePosition;
		bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width;
		bool insideHeight = mousePos.y >=BAR_HEIGHT && mousePos.y <= Screen.height;
		return insideWidth && insideHeight;
	}
	
	public Rect GetPlayingArea() {
		return new Rect(0, 0, Screen.width, Screen.height-BAR_HEIGHT);
	}
	
	/*public void SetCursorState(CursorState newState) {
		if(activeCursorState != newState) previousCursorState = activeCursorState;
		activeCursorState = newState;
		switch(newState) {
		case CursorState.Select:
			activeCursor = selectCursor;
			break;
		/*case CursorState.Attack:
			currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[currentFrame];
			break;
		case CursorState.Harvest:
			currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[currentFrame];
			break;
		case CursorState.Move:
			currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[currentFrame];
			break;
		case CursorState.PanLeft:
			activeCursor = leftCursor;
			break;
		case CursorState.PanRight:
			activeCursor = rightCursor;
			break;
		/*case CursorState.PanUp:
			activeCursor = upCursor;
			break;
		case CursorState.PanDown:
			activeCursor = downCursor;
			break;
		case CursorState.RallyPoint:
			activeCursor = rallyPointCursor;
			break;
		default: break;
		}
	}*/
	
	public void SetResourceValues(Dictionary<ResourceType, int> resourceValues/* Dictionary<ResourceType, int> resourceLimits*/) {
		this.resourceValues = resourceValues;
		//this.resourceLimits = resourceLimits;
	}
	
/*	public CursorState GetCursorState() {
		return activeCursorState;
	}
	
	public CursorState GetPreviousCursorState() {
		return previousCursorState;
	}*/
	
	/*** Private Worker Methods ***/
	
	private void DrawPlayerDetails() {
		GUI.skin = playerDetailsSkin;
		GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
		float height = ResourceManager.TextHeight;
		float leftPos = ResourceManager.Padding;
		float topPos = ResourceManager.Padding;

		float padding = ResourceManager.Padding;//7;
		float buttonWidth = height*4;
		float buttonHeight = height;

		Rect menuButtonPosition = new Rect(leftPos, topPos, buttonWidth, buttonHeight);
		if(GUI.Button(menuButtonPosition, "Menu")) {
			PlayClick();
			Time.timeScale = 0.0f;
			PauseMenu pauseMenu = GetComponent<PauseMenu>();
			if(pauseMenu) pauseMenu.enabled = true;
			UserInput userInput = player.GetComponent<UserInput>();
			if(userInput) userInput.enabled = false;
		}
		leftPos += buttonWidth + leftPos;

		Texture2D avatar = PlayerManager.GetPlayerAvatar();
		if(avatar) {
			//we want the texture to be drawn square at all times
			GUI.DrawTexture(new Rect(leftPos, topPos, height, height), avatar);
			leftPos += height + ResourceManager.Padding;
		}
		float minWidth = 0, maxWidth = 0;
		string playerName = PlayerManager.GetPlayerName();
		playerDetailsSkin.GetStyle("label").CalcMinMaxWidth(new GUIContent(playerName), out minWidth, out maxWidth);
		GUI.Label(new Rect(leftPos, topPos, maxWidth, height), playerName);
		GUI.EndGroup();
	}
	
	private void DrawOrdersBar() {
		GUI.skin = ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width/2,Screen.height - BAR_HEIGHT,Screen.width/2,BAR_HEIGHT));
		GUI.Box(new Rect(0,0,Screen.width/2,BAR_HEIGHT),"");
		
		//GUI.BeginGroup(new Rect(Screen.width-ORDERS_BAR_WIDTH-BUILD_IMAGE_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH+BUILD_IMAGE_WIDTH, Screen.height-RESOURCE_BAR_HEIGHT));
		//GUI.Box(new Rect(BUILD_IMAGE_WIDTH+SCROLL_BAR_WIDTH, 0, ORDERS_BAR_WIDTH, Screen.height-RESOURCE_BAR_HEIGHT),"");

		string selectionName = "";
		if(player.SelectedObject) {
			selectionName = player.SelectedObject.objectName;
			if(player.SelectedObject.IsOwnedBy(player)){
				//reset slider value if the selected object has changed
				if(lastSelection && lastSelection != player.SelectedObject) sliderValue = 0.0f;
			//	DrawActions(player.SelectedObject.GetActions());
				//store the current selection
				lastSelection = player.SelectedObject;
				Building selectedBuilding = lastSelection.GetComponent<Building>();
				if(!selectedBuilding)
					DrawActions(player.SelectedObject.GetActions());
				if(selectedBuilding && !selectedBuilding.name.Equals("Nexus")) {
					//DrawBuildQueue(selectedBuilding.getBuildQueueValues(), selectedBuilding.getBuildPercentage());
					DrawStandardBuildingOptions(selectedBuilding);
				}
			}
		}
		if(!selectionName.Equals("")) {
			int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH / 2;
			int topPos = buildAreaHeight + BUTTON_SPACING;
			GUI.Label(new Rect(leftPos,topPos,ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), selectionName);
		}
		GUI.EndGroup();
	}

	private void DrawResourceBar() {
		GUI.skin = resourceSkin;
		GUI.BeginGroup(new Rect(0,Screen.height - BAR_HEIGHT,Screen.width/2,BAR_HEIGHT));
		GUI.Box(new Rect(0,0,Screen.width/2,BAR_HEIGHT),"");

		int topPos = 4, iconLeft = 4, textLeft = 20;
		DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);

		miniMapCamera = Camera.allCameras[1];
		miniMapCamera.Render();
		GUI.EndGroup();
	}
	
	private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos) {
		Texture2D icon = resourceImages[type];
		string text = resourceValues[type].ToString();// + "/" + resourceLimits[type].ToString();
		GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
		GUI.Label (new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
	}

	
	private void DrawActions(string[] actions) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = buttonHover;
		buttons.active.background = buttonClick;
		GUI.skin.button = buttons;
		int numActions = actions.Length;
		Debug.Log (actions.Length);
		//define the area to draw the actions inside
		//GUI.BeginGroup(new Rect(BUILD_IMAGE_WIDTH,0,ORDERS_BAR_WIDTH,buildAreaHeight));
		GUI.BeginGroup (new Rect (0, 3, Screen.width/2, BAR_HEIGHT));
		//draw scroll bar for the list of actions if need be
	//	if(numActions >= MaxNumRows(buildAreaHeight)) DrawSlider(buildAreaHeight, numActions / 2.0f);
		//display possible actions as buttons and handle the button click for each
		for(int i=0; i<numActions; i++) {
			int column = i % 3;
			int row = i / 3;
			Rect pos = GetButtonPos(row, column);
			Texture2D action = ResourceManager.GetBuildImage(actions[i]);
			if(action) {
				//create the button and handle the click of that button
				if(GUI.Button(pos, action)) {
					if(player.SelectedObject) {
						PlayClick();
						player.SelectedObject.PerformAction(actions[i]);
					}
				}
			}
			string text = actions[i];
			GUI.Label(new Rect(pos.x-2, pos.y+45, pos.width+5, TEXT_HEIGHT), text);
		}
		GUI.EndGroup();
	}
	
	private void PlayClick() {
		if(audioElement != null) audioElement.Play(clickSound);
	}
	
	private int MaxNumRows(int areaHeight) {
		return areaHeight / BUILD_IMAGE_HEIGHT;
	}
	
	private Rect GetButtonPos(int row, int column) {
		int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
		float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
		return new Rect(left,top,BUILD_IMAGE_WIDTH,BUILD_IMAGE_HEIGHT);
	}
	
	private void DrawSlider(int groupHeight, float numRows) {
		//slider goes from 0 to the number of rows that do not fit on screen
		sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight),sliderValue,0.0f,numRows-MaxNumRows(groupHeight));
	}
	
	private Rect GetScrollPos(int groupHeight) {
		return new Rect(BUTTON_SPACING,BUTTON_SPACING,SCROLL_BAR_WIDTH,groupHeight - 2 * BUTTON_SPACING);
	}
	
	/*private void DrawBuildQueue(string[] buildQueue, float buildPercentage) {
		for(int i=0; i<buildQueue.Length; i++) {
			float topPos = i * BUILD_IMAGE_HEIGHT - (i+1) * BUILD_IMAGE_PADDING;
			Rect buildPos = new Rect(BUILD_IMAGE_PADDING,topPos,BUILD_IMAGE_WIDTH,BUILD_IMAGE_HEIGHT);
			GUI.DrawTexture(buildPos,ResourceManager.GetBuildImage(buildQueue[i]));
			GUI.DrawTexture(buildPos,buildFrame);
			topPos += BUILD_IMAGE_PADDING;
			float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
			float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
			if(i==0) {
				//shrink the build mask on the item currently being built to give an idea of progress
				topPos += height * buildPercentage;
				height *= (1 - buildPercentage);
			}
			GUI.DrawTexture(new Rect(2 * BUILD_IMAGE_PADDING,topPos,width,height),buildMask);
		}
	}*/
	
	private void DrawStandardBuildingOptions(Building building) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = smallButtonHover;
		buttons.active.background = smallButtonClick;
		GUI.skin.button = buttons;
		int leftPos = 10;//BUILD_IMAGE_WIDTH;// + /*SCROLL_BAR_WIDTH +*/ BUTTON_SPACING;
		int topPos = 10;//250 - BUILD_IMAGE_HEIGHT*2;//buildAreaHeight - BUILD_IMAGE_HEIGHT;// 2;
		int width = BUILD_IMAGE_WIDTH / 2;
		int height = BUILD_IMAGE_HEIGHT / 2;
		if(GUI.Button(new Rect(leftPos, topPos, width, height), building.sellImage)) {
			PlayClick();
			building.Sell();
		}
		/*if(building.hasSpawnPoint()) {
			leftPos += width + BUTTON_SPACING;
			if(GUI.Button(new Rect(leftPos, topPos, width, height), building.rallyPointImage)) {
				PlayClick();
				if(activeCursorState != CursorState.RallyPoint && previousCursorState != CursorState.RallyPoint) SetCursorState(CursorState.RallyPoint);
				else {
					//dirty hack to ensure toggle between RallyPoint and not works ...
					SetCursorState(CursorState.PanRight);
					SetCursorState(CursorState.Select);
				}
			}
		}*/
	}
	

	/*private void DrawMouseCursor() {
//		bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;
		if(mouseOverHud) {
			Screen.showCursor = true;
		} else {
			Screen.showCursor = false;
			if(!player.IsFindingBuildingLocation()) {
				GUI.skin = mouseCursorSkin;
				GUI.BeginGroup(new Rect(0,0,Screen.width,Screen.height));
//				UpdateCursorAnimation();
				Rect cursorPosition = GetCursorDrawPosition();
				GUI.Label(cursorPosition, activeCursor);
				GUI.EndGroup();
			}
		}
	}*/
	
	/*private void UpdateCursorAnimation() {
		//sequence animation for cursor (based on more than one image for the cursor)
		//change once per second, loops through array of images
		if(activeCursorState == CursorState.Move) {
			currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[currentFrame];
		} else if(activeCursorState == CursorState.Attack) {
			currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[currentFrame];
		} else if(activeCursorState == CursorState.Harvest) {
			currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[currentFrame];
		}
	}*/
	
	/*private Rect GetCursorDrawPosition() {
		//set base position for custom cursor image
		float leftPos = Input.mousePosition.x;
		float topPos = Screen.height - Input.mousePosition.y; //screen draw coordinates are inverted
		//adjust position base on the type of cursor being shown
		if(activeCursorState == CursorState.PanRight) leftPos = Screen.width - activeCursor.width;
		else if(activeCursorState == CursorState.PanDown) topPos = Screen.height - activeCursor.height;
		else if(activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
			topPos -= activeCursor.height / 2;
			leftPos -= activeCursor.width / 2;
		} else if(activeCursorState == CursorState.RallyPoint) topPos -= activeCursor.height;
		return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
	}*/
}
