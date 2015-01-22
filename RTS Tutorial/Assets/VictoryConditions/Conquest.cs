using UnityEngine;
using System.Collections;

public class Conquest : VictoryCondition {
	
	public override string GetDescription () {
		return "Conquest";
	}

	/*public override bool GameFinished () {
		if(players == null) return true;
		int playersLeft = players.Length;
		foreach(Player player in players) {
			if(!PlayerMeetsConditions(player)) playersLeft--;
		}
		return playersLeft == 1;
	}*/
	
	public override bool PlayerMeetsConditions (Player player) {
		bool won = false;
		//if (!player.human) {
			Wonder wonder = player.GetComponentInChildren<Wonder>();
		if(!wonder){
				won = true;
			}
	//	}
		return player && !player.IsDead () && won;
	}
	
}