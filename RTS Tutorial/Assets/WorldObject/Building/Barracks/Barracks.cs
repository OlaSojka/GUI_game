using UnityEngine;
using RTS;

public class Barracks : Building {
	
	protected override void Start () {
		base.Start();
		actions = new string[] { "Coś co robią baraki" };
	}
	
	public override void PerformAction(string actionToPerform) {
		base.PerformAction(actionToPerform);
		CreateUnit(actionToPerform);
	}
	
	protected override bool ShouldMakeDecision () {
		return false;
	}
}
