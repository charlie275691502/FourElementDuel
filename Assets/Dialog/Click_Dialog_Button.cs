using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_Dialog_Button : MonoBehaviour {
	public Dialog_manager dialog_manager;
	public bool option;

	void OnMouseDown(){
		dialog_manager.Click_Dialog (option);
	}
}
