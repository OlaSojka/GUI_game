using UnityEngine;
using RTS;
using System;

public class WarFactory : Building {
	
	
	protected override void Start () {
		base.Start();
		actions = new string[] { "Tank", "Tank2" };
		addMoneyVal = 1;
	}
	
	public override void PerformAction(string actionToPerform) {
		base.PerformAction(actionToPerform);
		CreateUnit(actionToPerform);
		
	}
	
	protected override bool ShouldMakeDecision () {
		return false;
	}
	
}
