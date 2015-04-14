using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour {
	public float inceptionTime;
	public float lifespan = .1f;
	public AudioClip		fireSound;

	void Start(){
		inceptionTime = Time.time;
		AudioSource.PlayClipAtPoint (fireSound, transform.position);
	}

	void Update(){
		if (Time.time - inceptionTime > lifespan) {
			Destroy (gameObject);
		}
	}

	void OnCollisionEnter(Collision coll){
		if (coll.rigidbody != null) {
			coll.rigidbody.AddForce (transform.forward * 150);
			EnemyBug bug = coll.gameObject.GetComponent<EnemyBug>();
			if(bug != null){
				bug.Damage(2f, ElementType.fire, false);
			}
			Destroy (gameObject);
		}
		if (coll.gameObject.tag == "Ground") {
			Destroy (gameObject);
		}
	}
}
