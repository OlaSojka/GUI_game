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
//	private int /*currentFrame = 0,*/ buildAreaHeight = 0;
	private Dictionary<ResourceType,int> resourceValues;
	private Dictionary<ResourceType,Texture2D> resourceImages;
	private WorldObject lastSelection;
	private float sliderValue;
	private AudioElement audioElement;
	
	private const int ORDERS_BAR_WIDTH = 250, RESOURCE_BAR_HEIGHT = 40;
	private const int SELECTION_NAME_HEIGHT = 15, SCROLL_BAR_WIDTH = 22, BUTTON_SPACING = 7;
	private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
	private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64, BUILD_IMAGE_PADDING = 8;
	
	public const int BAR_HEIGHT = 150;
	
	Camera miniMapCamera;
	Texture camTexture;
	
	/*** Game Engine Methods ***/
	
	void Start () {
		player = transform.root.GetComponent<Player>();
		resourceValues = new Dictionary<ResourceType, int>();
		resourceImages = new Dictionary<ResourceType, Texture2D>();
			switch(resources[0].name) {
				case "Money":
					resourceImages.Add(ResourceType.Money, resources[0]);
					resourceValues.Add(ResourceType.Money, 0);
					break;
				default: break;
			}
//		buildAreaHeight = 250;
		ResourceManager.StoreSelectBoxItems(selectBoxSkin, healthy, damaged, critical);
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

	public void SetResourceValues(Dictionary<ResourceType, int> resourceValues/* Dictionary<ResourceType, int> resourceLimits*/) {
		this.resourceValues = resourceValues;
	}
	
	/*** Private Worker Methods ***/
	
	private void DrawPlayerDetails() {
		GUI.skin = playerDetailsSkin;
		GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
		float height = ResourceManager.TextHeight;
		float leftPos = ResourceManager.Padding;
		float topPos = ResourceManager.Padding;

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
		string selectionName = "";
		if(player.SelectedObject) {
			selectionName = player.SelectedObject.objectName;
			if(player.SelectedObject.IsOwnedBy(player)){
				//reset slider value if the selected object has changed
				if(lastSelection && lastSelection != player.SelectedObject) sliderValue = 0.0f;
				//store the current selection
				lastSelection = player.SelectedObject;
				Building selectedBuilding = lastSelection.GetComponent<Building>();
				if(!selectedBuilding) {
					DrawActions(player.SelectedObject.GetActions());
				}
				if(selectedBuilding && !selectedBuilding.name.Equals("Nexus")) {
					DrawStandardBuildingOptions(selectedBuilding);
				}
			}
		}
		if(!selectionName.Equals("")) {
			int leftPos = Screen.width/8 + 50;//BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH / 2;
			int topPos = BAR_HEIGHT - SELECTION_NAME_HEIGHT - 10;
			GUI.Label(new Rect(leftPos,topPos,ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), selectionName.ToUpper());
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
		string text = resourceValues[type].ToString();
		GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
		GUI.Label (new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
	}

	
	private void DrawActions(string[] actions) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = buttonHover;
		buttons.active.background = buttonClick;
		GUI.skin.button = buttons;
		int numActions = actions.Length;
		GUI.BeginGroup (new Rect (Screen.width/8, 3, Screen.width/4, BAR_HEIGHT));
		//display possible actions as buttons and handle the button click for each
		for(int i=0; i<numActions; i++) {
			int column = i % 6;
			int row = i / 6;
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
			if(!actions[i].Equals("ScienceFacility"))
				GUI.Label(new Rect(pos.x-5, pos.y+50, pos.width+10, TEXT_HEIGHT), text);
			else
			   	GUI.Label(new Rect(pos.x-5, pos.y+50, pos.width+35, TEXT_HEIGHT), text);
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
		float left = 10 + column * (BUILD_IMAGE_WIDTH*1.5f);
		float top = 10 + row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
		return new Rect(left,top,BUILD_IMAGE_WIDTH,BUILD_IMAGE_HEIGHT);
	}
	
	private void DrawSlider(int groupHeight, float numRows) {
		//slider goes from 0 to the number of rows that do not fit on screen
		sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight),sliderValue,0.0f,numRows-MaxNumRows(groupHeight));
	}
	
	private Rect GetScrollPos(int groupHeight) {
		return new Rect(BUTTON_SPACING,BUTTON_SPACING,SCROLL_BAR_WIDTH,groupHeight - 2 * BUTTON_SPACING);
	}
	
	private void DrawStandardBuildingOptions(Building building) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = smallButtonHover;
		buttons.active.background = smallButtonClick;
		GUI.skin.button = buttons;
		int leftPos = 10;
		int topPos = 10;
		int width = BUILD_IMAGE_WIDTH / 2;
		int height = BUILD_IMAGE_HEIGHT / 2;
		if(GUI.Button(new Rect(leftPos, topPos, width, height), building.sellImage)) {
			PlayClick();
			building.Sell();
		}
	}
}
