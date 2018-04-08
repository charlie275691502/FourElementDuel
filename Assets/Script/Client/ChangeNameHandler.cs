using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeNameHandler : MonoBehaviour {
    public HubManager hubManager;

    public InputField inputField;

    public void Change_Nick(){
        hubManager.Change_Nick(inputField.text);
        inputField.text = "";
    }
}
