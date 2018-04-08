using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void Dialog_Delegate(bool option);

public class Dialog_manager : MonoBehaviour {
	public Dialog_Delegate dialog_Delegate;
	public int options_amount;
	public GameController gameController;
	public GameObject option1;
	public GameObject option2;

	void Start(){
		gameController = GameObject.Find ("GameController").GetComponent<GameController> ();
		if (options_amount == 1) {
			Destroy (option2);
			option1.transform.localPosition = new Vector2(0, option1.transform.localPosition.y);
		}
	}

	public void Click_Dialog(bool option){
		gameController.has_dialog = false;
		GetComponent<Animator> ().Play("out");
		if(dialog_Delegate != null)dialog_Delegate (option);
	}

	public void destroy(){
		Destroy (gameObject);
	}
}
