using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickBoard : MonoBehaviour {
    public GamePlayManager gamePlayManager;
    public int index;

	private void OnMouseDown(){
        gamePlayManager.Place(index);
	}
}
