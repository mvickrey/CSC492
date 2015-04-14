using UnityEngine;
using System.Collections;
using System.Collections.Generic; //Enables list<>s
using System.Linq; //enables linq queries

//The ElementType enum
public enum ElementType{
	earth,
	water,
	air,
	fire,
	aether,
	none
}


public class PC : PT_MonoBehaviour {
	static public PC S;

	public float			activeScreenWidth = 1;//% of the screen to use

	public float			speed = 3f;//The speed at which _PC walks

	//Element variables
	public GameObject[]		elementPrefabs;//the element_sphere prefabs
	public float			elementRotDist = .5f;//radius of rotation
	public float			elementRotSpeed = .5f;//period of rotation
	public int				maxNumSelectedElements = 1;
	public Color[]			elementColors;

	//these set the min and max distance between 2 line points
	public float			lineMinDelta = .1f;
	public float			lineMaxDelta = .5f;
	public float			lineMaxLength = 8f;

	public AudioClip		playerHitSound;

	//Health and damage variables
	public float			health = 10;//total mage health
	public float			maxHealth = 10;
	public float			damageTime = -100;
	//time that damage occurred. It's set to -100 so that the mage doesn't
	//act damaged immediately when the scene starts
	public float			knockbackDist = 1;//distance to move backward
	public float			knockbackDur = .5f;//seconds to move backward
	public float			invincibleDur = .5f;//seconds to be invincible
	public int 				invTimesToBlink = 4;//# blinks while invincible

	public bool _______________;

	private bool			invincibleBool = false;//is mage invincible?
	private bool			knockbackBool = false;//is mage being knocked back?
	private Vector3			knockbackDir;//direction of knockback
	private Transform		viewCharacterTrans;


	/*protected variables are between public and private.
	 * public variables can be seen by everyone
	 * private variables can only be seen by this class
	 * protected variables can be seen by this class or any subclasses
	 * only public variables appear in the inspector
	 * or those with serializefield in the preceeding line
	 */
	public string			actionStartTag;//["PC","Ground","Enemy"]

	public bool 			walking = false;
	public Vector3			walkTarget;
	public Transform		characterTrans;

	public List<Element>	selectedElements = new List<Element>();

	void Awake(){
		S = this;//Set the PC Singleton

		//Find the characterTrans to rotate with Face()
		characterTrans = transform.Find ("CharacterTrans");
		viewCharacterTrans = characterTrans.Find ("View_Character");



	}

	void Update(){

		//--------------------- WASD  + MouseLook movement----------------//
		Face (Utils.mouseLoc);
		if (Input.GetAxis ("Vertical") < 0)
			transform.Translate (0, -speed * Time.deltaTime, 0);
		if (Input.GetAxis ("Vertical") > 0)
			transform.Translate (0, speed * Time.deltaTime, 0);
		if (Input.GetAxis ("Horizontal") < 0)
			transform.Translate (-speed * Time.deltaTime, 0, 0);
		if (Input.GetAxis ("Horizontal") > 0)
			transform.Translate (speed * Time.deltaTime, 0, 0);

		//--------------------------- it works! --------------------------//

	}


	public void Face(Vector3 poi){//Face toward a point of interest
		Vector3 delta = poi - pos;//Find vector to the point of interest
		//Use Atan2 to get the rotation around Z that points the x-axis of
		//_mage:CharacterTrans toward poi
		float rZ = Mathf.Rad2Deg * Mathf.Atan2 (delta.y, delta.x);
		//Set the rotation of characterTrans (doesn't actually rotate _PC)
		characterTrans.rotation = Quaternion.Euler (0, 0, rZ);
	}

	public void StopWalking(){//Stops the _PC from walking
		walking = false;
		rigidbody.velocity = Vector3.zero;
	}

	void FixedUpdate(){//Happens every physics step
		if (invincibleBool) {
			//get number[0,1]
			float blinkU = (Time.time - damageTime) / invincibleDur;
			blinkU *= invTimesToBlink;//multiply by times to blink
			blinkU %= 1.0f;
			//mod 1 gives the decimal component
			bool visible = (blinkU > .5f);
			if (Time.time - damageTime > invincibleDur) {
				invincibleBool = false;
				visible = true;//just to be sure
			}
			//making the gameobject inactive makes it invisible
			viewCharacterTrans.gameObject.SetActive (visible);
		}

		if (knockbackBool) {
			if (Time.time - damageTime > knockbackDur) {
				knockbackBool = false;
			}
			float knockbackSpeed = knockbackDist / knockbackDur;
			vel = knockbackDir * knockbackSpeed;
			return;//avoids walking code
		}
		if (walking) {
			if ((walkTarget - pos).magnitude < speed * Time.fixedDeltaTime) {
				//If the _PC is very close to walkTarget, just stop there
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

	//--------------------Damage and death code----------------------------//
	void OnCollisionEnter(Collision coll){
		GameObject otherGO = coll.gameObject;
		//Colliding with a wall can also stop walking
		Tile ti = otherGO.GetComponent<Tile> ();
		if (ti != null) {
			if (ti.height > 0) {//if ti.height is > 0
				//then this ti is a wall, and mage should stop
				StopWalking ();
			}
		}
		//see if it's an enemybug
		EnemyBug bug = coll.gameObject.GetComponent<EnemyBug> ();
		//if otherGO is an enemyBug, pass otherGo to collisionDamage()
		//which will interpret it as an enemy
		if (bug != null)
			CollisionDamage (bug);
	}

	void CollisionDamage(Enemy enemy){
		//don't take damage if you're already invincible
		if (invincibleBool)
			return;

		//the mage has been hit by an enemy
		StopWalking ();
		AudioSource.PlayClipAtPoint (playerHitSound, pos);
		health -= enemy.touchDamage;//take damage based on Enemy
		if (health <= 0) {
			Die ();
			return;
		}

		damageTime = Time.time;
		knockbackBool = true;
		knockbackDir = (pos - enemy.pos).normalized;
		invincibleBool = true;
	}

	void Die(){
		Application.LoadLevel ("GameOverScene");//reload the level

	}

	void OnTriggerEnter(Collider other){
		EnemySpiker spiker = other.GetComponent<EnemySpiker> ();
		if (spiker != null) {
			//collisiondamage will see spiker as an enemy
			CollisionDamage (spiker);
			AudioSource.PlayClipAtPoint(playerHitSound, pos);
		}
	}
	//---------------------END OF DAMAGE AND DEATH CODE----------------//

}