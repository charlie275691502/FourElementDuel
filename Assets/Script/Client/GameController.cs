using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum Phases{
	StartMenu,
    Hub,
	GamePlay,
    TestSend
}


public class GameController : MonoBehaviour {
    public Camera mainCamera;
	public Phases now_Phase;
	private bool phase_has_change;
    public GameObject startMenu_gmo;
	public GameObject hub_gmo;
    public GameObject gamePlay_gmo;
    public GameObject testSend_gmo;
    public bool has_dialog;
    public GameObject dialog_gmo;

    [HideInInspector] public int serial;
    [HideInInspector] public int gameSerial;

    [HideInInspector] public string nick;

	void Start(){
        nick = PlayerPrefs.GetString("NICK");
        if (nick == "") nick = "Unknown";
		SwitchPhases(Phases.StartMenu);
        //Change_Camera_Size();
	}


    void Update(){
        if (phase_has_change)
        {
            phase_has_change = false;
            startMenu_gmo.SetActive(now_Phase == Phases.StartMenu);
            hub_gmo.SetActive(now_Phase == Phases.Hub);
            gamePlay_gmo.SetActive(now_Phase == Phases.GamePlay);
            testSend_gmo.SetActive(now_Phase == Phases.TestSend);
        }
    }

    void Change_Camera_Size(){
        float dpi = Screen.dpi;
        if (Mathf.Abs(dpi - 0.0f) < 0.01f){
            dpi = 400.0f;
        }
        float height_length = Screen.height / dpi * 4;
        float width_length = Screen.width / dpi * 4;
        mainCamera.orthographicSize = width_length * height_length / 20;
    }

	public void SwitchPhases(Phases phase){
		now_Phase = phase;
		phase_has_change = true;
	}

    public void Start_Dialog(Dialog_Delegate d, string title, string content, int options_amount){
        if (has_dialog) return;
        has_dialog = true;
        GameObject dialog = Instantiate(dialog_gmo, Vector2.zero, Quaternion.identity);
        dialog.GetComponent<Dialog_manager>().dialog_Delegate = d;
        dialog.GetComponent<Dialog_manager>().options_amount = options_amount;
        dialog.transform.Find("canvas").Find("Title").GetComponent<Text>().text = title;
        dialog.transform.Find("canvas").Find("Content").GetComponent<Text>().text = content;
    }
}
