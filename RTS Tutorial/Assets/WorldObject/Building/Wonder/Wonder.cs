using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wonder : Building {

	protected override bool ShouldMakeDecision () {
		return false;
	}
}