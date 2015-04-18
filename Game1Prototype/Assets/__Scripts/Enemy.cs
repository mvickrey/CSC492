using UnityEngine;
using System.Collections;

public interface Enemy{
	//these are declarations of properties that will be implemented by
	//all classes that implement the Enemy interface
	Vector3 	pos{get;set;}//the enemy's transform.position
	float		touchDamage{get; set;}//damage done by touching the enemy
	string		typeString{ get; set; }//the type string from Rooms.xml

	//the following are already implemented by all MonoBehaviour subclasses
	GameObject 	gameObject{ get; }
	Transform	transform{ get; }
}