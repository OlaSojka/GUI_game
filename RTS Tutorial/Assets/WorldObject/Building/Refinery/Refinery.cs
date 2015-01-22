using UnityEngine;

public class Refinery : Building {

	protected override void Start () {
		base.Start();
	}
	
	protected override bool ShouldMakeDecision () {
		return false;
	}
}