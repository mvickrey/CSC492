using UnityEngine;
using System.Collections;

public class CameraFollow : PT_MonoBehaviour {
	public static CameraFollow S;
	public GameObject prefabPowerUp;

	public Transform	targetTransform;
	public float 		camEasing = 0.1f;
	public Vector3		followOffset = new Vector3 (0,0,-2);

	void Awake(){
		S = this;
	}

	void FixedUpdate(){
		Vector3 pos1 = targetTransform.position + followOffset;
		pos = Vector3.Lerp (pos, pos1, camEasing);
	}

	public void SpawnPowerUp(Enemy e){
		GameObject go = Instantiate(prefabPowerUp) as GameObject;
		PowerUp pu = go.GetComponent<PowerUp>();
		int rnd = Random.Range(0,4);
		pu.SetType(rnd);
		Vector3 pos = e.transform.position;
		pos.z -= .5f;
		pu.transform.position = pos;
		Debug.Log (pu.transform.position);
		Debug.Log("Powerup created!");
	}

}
