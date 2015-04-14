using UnityEngine;
using System.Collections;

public class ChangeScene : MonoBehaviour {

	// Update is called once per frame
	public void ChangeToScene (string nextScene) {
		Application.LoadLevel (nextScene);
	
	}
	public void EndGame(){
		Application.Quit ();
	}
}
