using UnityEngine;
using System.Collections;

public class BulletShooter : MonoBehaviour {
	public GameObject	bulletPrefab;
	public float		bulletSpeed = 10f;
	public int period = 4;
	public int physicsFrames;

	public void Start(){
		physicsFrames = 0;
	}
	
	void FixedUpdate(){
		physicsFrames ++;

	}
	public void ShootBullet(){

		GameObject bullet = Instantiate (bulletPrefab, transform.position, transform.rotation) as GameObject;
		bullet.GetComponent<Rigidbody> ().velocity = transform.TransformDirection (Vector3.forward * bulletSpeed);

	}
	
	void Update () {
		if (Input.GetButton ("Fire1")&& (physicsFrames % period == 1)) {
			ShootBullet ();
		}
		
	}
}
