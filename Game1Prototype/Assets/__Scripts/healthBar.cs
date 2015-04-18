using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class healthBar : MonoBehaviour {
	Image image;
	public float fillPercent;
	public float maxHP;
	public GameObject target;
	void Start(){
		image = GetComponent<Image> ();
		fillPercent = 1f;
		image.fillAmount = 1f;
		if (target.GetComponent ("PC")) {
			maxHP = target.gameObject.GetComponent<PC> ().maxHealth;
		}
	}


	void Update () {
		if (target.GetComponent ("PC")) {
			fillPercent = target.gameObject.GetComponent<PC> ().health / maxHP;
		}
		image.fillAmount = fillPercent;

	}
}
