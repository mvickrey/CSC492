using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpiker :  PT_MonoBehaviour, Enemy {
	[SerializeField]
	private float			_touchDamage = 1;
	public float			touchDamage{
		get{ return(_touchDamage);}
		set{_touchDamage = value;}
	}
	//the pos property is already implemented in PT_MonoBehaviour
	public float			speed = 5f;
	public string			roomXMLString = "{";
	public AudioClip		bounceSound;
	public string			typeString {
		get{ return(roomXMLString);}
		set{ roomXMLString = value;}
	}

	public bool _______________;

	public Vector3			moveDir;
	public Transform		characterTrans;

	void Awake(){
		characterTrans = transform.Find ("CharacterTrans");
	}

	// Use this for initialization
	void Start () {
		//set the mvoe direction based on the character in rooms.xml
		switch (roomXMLString) {
		case "^":
			moveDir = Vector3.up;
			break;
		case "v":
			moveDir = Vector3.down;
			break;
		case "{":
			moveDir = Vector3.left;
			break;
		case "}":
			moveDir = Vector3.right;
			break;
		}
	}

	void FixedUpdate(){//happens every physics step (i.e. 50 times/sec)
		rigidbody.velocity = moveDir * speed;
	}

	//this has the same structure as the damage method in enemybug
	public void Damage(float amt, ElementType eT, bool damageOverTime = false){
		//nothing damages the EnemySpiker
	}

	void OnTriggerEnter(Collider other){
		//check to see if a wall was hit
		GameObject go = Utils.FindTaggedParent (other.gameObject);
		if (go == null)
			return;//in case nothing is tagged

		if (go.tag == "Ground") {
			//make sure that the ground tile is in the direction we're moving
			//a dot product will help us with this
			float dot = Vector3.Dot (moveDir, go.transform.position - pos);
			if (dot > 0) {//if spiker is moving towards the block it hit
				moveDir *= -1;//reverse direction
				AudioSource.PlayClipAtPoint(bounceSound, pos);
			}
		}
	}

}