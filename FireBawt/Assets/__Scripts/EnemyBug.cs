using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyBug : PT_MonoBehaviour, Enemy {
	[SerializeField]
	private float			_touchDamage = 1;
	public float			touchDamage{
		get{ return(_touchDamage);}
		set{_touchDamage = value;}
	}
	//the pos property is already implemented in PT_MonoBehaviour
	public string			typeString {
		get{ return(roomXMLString);}
		set{ roomXMLString = value;}
	}
	public string			roomXMLString;
	public float			speed = 1.5f;
	public float			health = 20;
	public float			damageScale = .8f;
	public float			damageScaleDuration = .25f;
	public AudioClip		walkSound;
	public AudioClip		hurtSound;
	public AudioClip		attackSound;
	public AudioClip		deathSound;

	public bool _________________;

	private float			damageScaleStartTime;
	public float			_maxHealth;
	public Vector3			walkTarget;
	public bool				walking;
	public Transform		characterTrans;
	//Stores damage for each element each frame
	public Dictionary<ElementType, float>	damageDict;
	//NOTE: Dictionaries do not appear in the Unity inspector

	void Awake () {
		characterTrans = transform.Find ("CharacterTrans");
		ResetDamageDict ();

	}

	// Update is called once per frame
	void Update () {
		WalkTo (PC.S.pos);
		_maxHealth = health;//used to put a top cap on healing
	}

	//------------------Walking Code--------------------//
	//All of this wakling Code is copied directly from mage

	//Walk to a specific position. The position.z is always 0
	public void WalkTo(Vector3 xTarget){
		walkTarget = xTarget;	//set the point to walk to
		walkTarget.z = 0;		//Force z = 0
		walking = true;			//Now the mage is walking
		Face (walkTarget);		//Look in the direction of the walkTarget
	}
	
	public void Face(Vector3 poi){//Face toward a point of interest
		Vector3 delta = poi - pos;//Find vector to the point of interest
		//Use Atan2 to get the rotation around Z that points the x-axis of
		//_PC:CharacterTrans toward poi
		float rZ = Mathf.Rad2Deg * Mathf.Atan2 (delta.y, delta.x);
		//Set the rotation of characterTrans (doesn't actually rotate _PC)
		characterTrans.rotation = Quaternion.Euler (0, 0, rZ);
	}
	
	public void StopWalking(){//Stops the _PC from walking
		walking = false;
		rigidbody.velocity = Vector3.zero;
	}

	void FixedUpdate(){//Happens every physics step
		if (walking) {
			if ((walkTarget - pos).magnitude < speed * Time.fixedDeltaTime) {
				//If the PC is very close to walkTarget, just stop there
				pos = walkTarget;
				StopWalking ();
			} else {
				//otherwise, move toward walktarget
				rigidbody.velocity = (walkTarget - pos).normalized * speed;
			}
		} else {
			//if not walking, velocity should be zero
			rigidbody.velocity = Vector3.zero;
		}
	}

	//-------------------END OF WALKING CODE-----------------------------//


	//-------------------Damage code-------------------------------------//
	//Resets the values for the damageDict
	void ResetDamageDict(){
		if (damageDict == null) {
			damageDict = new Dictionary<ElementType, float> ();
		}
		damageDict.Clear ();
		damageDict.Add (ElementType.earth, 0);
		damageDict.Add (ElementType.water, 0);
		damageDict.Add (ElementType.air, 0);
		damageDict.Add (ElementType.fire, 0);
		damageDict.Add (ElementType.aether, 0);
		damageDict.Add (ElementType.none, 0);
	}

	/**Damage this instance. By default, the damage is instant, but it can
	 * also be treated as damage over time, where the amt value would be
	 * the amount of damage done per second
	 */
	public void Damage(float amt, ElementType eT, bool damageOverTime = false){
		//if it's DOT, then only damage the fractional amount for this frame
		if (damageOverTime) {
			amt *= Time.deltaTime;
		}

		//Treat different damage types differently (most are default)
		switch (eT) {
		case ElementType.fire:
			//only the max damage from one fire source affects this instance
			damageDict [eT] = Mathf.Max (amt, damageDict [eT]);
			break;

		case ElementType.air:
			//air doesn't damage EnemyBugs, so do nothing
			break;
		default:
			//by default, damage is added to the other damage by same element
			damageDict [eT] += amt;
			break;
		}
	}

	/*LateUpdate() is automatically called by Unity every frame. Once all the
	 * Updates on all instances have been called, then LateUpdate() is called
	 * on all instances
	 */ 
	void LateUpdate(){
		//Apply damage from the different element types

		//Iteration through a dictionary uses a KeyValuePair
		//Entry.key is the elementType, while entry.value is the float
		float dmg = 0;
		foreach (KeyValuePair<ElementType,float> entry in damageDict) {
			dmg += entry.Value;
		}
		if (dmg > 0) {//if this took damage
			//and if it is at full scale now (& not already scaling)
			if (characterTrans.localScale == Vector3.one) {
				//start the damage scale animation
				damageScaleStartTime = Time.time;
				AudioSource.PlayClipAtPoint(hurtSound, pos);
			}
		}

		//the damage scale animation
		float damU = (Time.time - damageScaleStartTime) / damageScaleDuration;
		damU = Mathf.Min (1, damU);//limit the max localScale to 1
		float scl = (1-damU) * damageScale + damU*1;
		characterTrans.localScale = scl * Vector3.one;

		health -= dmg;
		health = Mathf.Min (_maxHealth, health);//limit health if healing

		ResetDamageDict ();//prepare for next frame

		if (health <= 0)
			Die ();
	}


	//Making Die a separate function allows us to add things later like 
	//different death animations, dropping something for the player, etc
	public void Die(){
	//	print ("BUG DESTROYED!");
		AudioSource.PlayClipAtPoint (deathSound, pos);
		Destroy (gameObject);
	}
}
