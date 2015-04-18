using UnityEngine;
using System.Collections;

public class Portal : PT_MonoBehaviour {

	public string 		toRoom;
	public bool 		justArrived = false;//true if _PC has just teleported here

	void OnTriggerEnter(Collider other){
		if (justArrived) //don't immediately teleport the PC back
			return;

		//get the gameobject of the collider
		GameObject go = other.gameObject;
		//search up for a tagged parent
		GameObject goP = Utils.FindTaggedParent (go);
		if (goP != null)
			go = goP;

		//if this isn't the PC, return
		if (go.tag != "PC")
			return;

		//build the next room
		LayoutTiles.S.BuildRoom (toRoom);
	}

	void OnTriggerExit(Collider other){
		//once the mage leaves this portal, set justArrived to false
		if (other.gameObject.tag == "PC")
			justArrived = false;
	}
}
