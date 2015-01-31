using UnityEngine;
using RTS;

public class ScienceFacility : Building {

	protected override void Start () {
		base.Start();
		actions = new string[] { "Goliath" };
	}
	
	public override void PerformAction(string actionToPerform) {
		base.PerformAction(actionToPerform);
		CreateUnit(actionToPerform);
	}
	
	protected override bool ShouldMakeDecision () {
		return false;
	}
}
	