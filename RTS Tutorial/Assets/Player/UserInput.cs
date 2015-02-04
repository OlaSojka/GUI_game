using UnityEngine;
using System.Collections;
using RTS;

public class UserInput : MonoBehaviour {
	
	private Player player;
	
	// Use this for initialization
	void Start () {
		player = transform.root.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
		if(player && player.human) {
			if(Input.GetKeyDown(KeyCode.Escape)) OpenPauseMenu();
			MoveCamera();
			RotateCamera();
			MouseActivity();
		}
	}
	
	private void OpenPauseMenu() {
		Time.timeScale = 0.0f;
		GetComponentInChildren<PauseMenu>().enabled = true;
		GetComponent<UserInput>().enabled = false;
		//Screen.showCursor = true;
		ResourceManager.MenuOpen = true;
	}
	
	private void MoveCamera() {
		float xpos = Input.mousePosition.x;
		//float ypos = Input.mousePosition.y;
		Vector3 movement = new Vector3(0,0,0);

		float maxCameraX = 66.5f;
		float minCameraX = -66.5f;		
		
		//horizontal camera movement
		if(xpos >= 0 && xpos < ResourceManager.ScrollWidth) {
			movement.x -= ResourceManager.ScrollSpeed;
		} else if(xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) {
			movement.x += ResourceManager.ScrollSpeed;
		}
		
		//make sure movement is in the direction the camera is pointing
		//but ignore the vertical tilt of the camera to get sensible scrolling
		movement = Camera.main.transform.TransformDirection(movement);
		movement.y = 0;
		
		//away from ground movement
		//movement.y -= ResourceManager.ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");
		
		//calculate desired camera position based on received input
		Vector3 origin = Camera.main.transform.position;
		Vector3 destination = origin;
		if(destination.x + movement.x < minCameraX) {
			destination.x = minCameraX;
		}
		else if(destination.x + movement.x > maxCameraX) {
			destination.x = maxCameraX;
		}
		else {
			destination.x += movement.x;
		}
		destination.y += movement.y;
		destination.z += movement.z;
		
		
		//if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
		}
	}
	
	private void RotateCamera() {
		Vector3 origin = Camera.main.transform.eulerAngles;
		Vector3 destination = origin;
		
		//detect rotation amount if ALT is being held and the Right mouse button is down
		if((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetMouseButton(1)) {
			destination.x -= Input.GetAxis("Mouse Y") * ResourceManager.RotateAmount;
			destination.y += Input.GetAxis("Mouse X") * ResourceManager.RotateAmount;
		}
		
		//if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
		}
	}
	
	private void MouseActivity() {
		if(Input.GetMouseButtonDown(0)) LeftMouseClick();
		else if(Input.GetMouseButtonDown(1)) RightMouseClick();
		MouseHover();
	}
	
	private void LeftMouseClick() {
		if(player.hud.MouseInBounds()) {
			if(player.IsFindingBuildingLocation()) {
				if(player.CanPlaceBuilding()) player.StartConstruction();
			} else {
				GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
				Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
				if(hitObject && hitPoint != ResourceManager.InvalidPosition) {
					if(player.SelectedObject) player.SelectedObject.MouseClick(hitObject, hitPoint, player);
					else if(hitObject.name != "Ground") {
						WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
						if(worldObject) {
							//we already know the player has no selected object
							player.SelectedObject = worldObject;
							worldObject.SetSelection(true, player.hud.GetPlayingArea());
						}
					}
				}
			}
		}
	}
	
	private void RightMouseClick() {
		if(player.hud.MouseInBounds() && player.SelectedObject) {
			if(player.IsFindingBuildingLocation()) {
				player.CancelBuildingPlacement();
			} else {
				player.SelectedObject.SetSelection(false, player.hud.GetPlayingArea());
				player.SelectedObject = null;
			}
		}
	}
	
	private void MouseHover() {
		if(player.hud.MouseInBounds()) {
			if(player.IsFindingBuildingLocation()) {
				player.FindBuildingLocation();
			} /*else {
				GameObject hoverObject = WorkManager.FindHitObject(Input.mousePosition);
				if(hoverObject) {
					if(player.SelectedObject) player.SelectedObject.SetHoverState(hoverObject);
					else if(hoverObject.name != "Ground") {
						Player owner = hoverObject.transform.root.GetComponent<Player>();
						if(owner) {
						//	Unit unit = hoverObject.transform.parent.GetComponent<Unit>();
						//	Building building = hoverObject.transform.parent.GetComponent<Building>();
						}
					}
				}
			}*/
		}
	}
}