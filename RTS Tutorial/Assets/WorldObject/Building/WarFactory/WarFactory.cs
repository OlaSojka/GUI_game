using UnityEngine;
using RTS;

public class WarFactory : Building {

	protected override void Start () {
		base.Start();
		actions = new string[] { "Tank", "Tank2" };
		player.startMoney -= this.addMoneyVal;
	}
	
	public override void PerformAction(string actionToPerform) {
		base.PerformAction(actionToPerform);
		CreateUnit(actionToPerform);
	}
	
	protected override bool ShouldMakeDecision () {
		return false;
	}
}
