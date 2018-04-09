using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSendManager : MonoBehaviour {
    public ClientController clientController;
    public ServerController serverController;

    public void SendToServer(){
        Debug.Log("SendToServer Clicked ");
        clientController.SendToServer(new Packet(Command.C2M_CHANGE_NICK, new string[1]{"hi"}));
    }

    public void SendToClient()
    {
        Debug.Log("SendToClient Clicked ");
        serverController.SendToAllClient(false, new Packet(Command.C2M_CHANGE_NICK, new string[1] { "hi" }));
    }
}
