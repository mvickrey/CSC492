using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PowerUp : MonoBehaviour {

	public Vector2 rotMinMax = new Vector2(15,90);
	public Vector2 driftMinMax = new Vector2(.25f,2);
	public float lifeTime = 10f;
	public float fadeTime = 5f;
	public bool _________________;
	public int type;
	public GameObject cube;
	public Vector3 rotPerSecond;
	public float birthTime;
	
	void Awake () {
		cube = transform.FindChild ("Cube").gameObject;	

		//Set a random velocity
		Vector3 vel = Random.onUnitSphere;
		vel.Normalize ();
		vel.z = 0;
		vel *= Random.Range (driftMinMax.x, driftMinMax.y);
		//rigidbody.velocity = vel;

		//Stuff for rotation
		transform.rotation = Quaternion.identity;
		rotPerSecond = new Vector3 (Random.Range (rotMinMax.x, rotMinMax.y),
		                           Random.Range (rotMinMax.x, rotMinMax.y),
		                           Random.Range (rotMinMax.x, rotMinMax.y));
		birthTime = Time.time;
	}

	void Update () {
		//Rotate the cube
		cube.transform.rotation = Quaternion.Euler (rotPerSecond * Time.time);

		float u = (Time.time - (birthTime + lifeTime)) / fadeTime;
		if (u >= 1) {
			Destroy(this.gameObject);
			Debug.Log("Cube destroyed!"	);
			return;
		}else if (u > 0) {
			//Fade the cube
			Color c = cube.renderer.material.color;
			c.a = 1f-u;
			cube.renderer.material.color = c;
		}
	}

	public void SetType(int newType){
		Color c = cube.renderer.material.color;
		switch (newType) {
		case 0: //Fire
			c = Color.red;
			break;
		case 1: //Water
			c = Color.blue;
			break;
		case 2: //Air
			c = Color.white;
			break;
		case 3: // Earth
			c = new Color(.647f,.164f,.164f);
			break;
		default: //Health
			c = Color.green;
			break;
		}
		cube.renderer.material.color = c;
		type = newType;
	}

	public void AbsorbedBy(GameObject target){
		Destroy (this.gameObject);
	}

	void OnTriggerEnter(Collider other){
		//Upgrade spells will go here
		Debug.Log("PowerUp picked up!");
		Destroy (this.gameObject);
	}
}
