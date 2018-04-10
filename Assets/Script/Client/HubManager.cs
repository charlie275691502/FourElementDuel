using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HubManager : MonoBehaviour {
    public ClientController clientController;
    public GameController gameController;

    private int receiveSerials;
    private bool onReceive;
    private Packet receivePacket;

    public Text text;

    void OnEnable(){
        onReceive = false;
        receiveSerials = clientController.AddSubscriptor(new ClientSubscriptor(OnReceive, new Command[2] { Command.M2C_HUB_LIST, Command.M2C_START_GAME }));
        clientController.SendToServer(new Packet(Command.C2M_CHANGE_NICK, new string[1] { gameController.nick }));
    }
    void OnDisable()
    {
        clientController.RemoveSubscriptor(receiveSerials);
    }

    void Update()
    {
        if (onReceive)
        {
            AnalysisReceive(receivePacket);
            onReceive = false;
        }
    }

    /* -- Sending Packet -- */

    public void Change_Nick(string nick)
    {
        clientController.SendToServer(new Packet(Command.C2M_CHANGE_NICK, new string[1] { nick }));
        PlayerPrefs.SetString("NICK", nick);
    }

    public void Ready(){
        clientController.SendToServer(new Packet(Command.C2M_HUB_READY));
    }

    public void Play()
    {
        clientController.SendToServer(new Packet(Command.C2M_PLAY));
    }

    /* -- Receiving packet -- */

    public void OnReceive(Packet packet){
        while (onReceive == true) ;
        onReceive = true;
        receivePacket = packet;
    }

    public void AnalysisReceive(Packet packet)
    {
        switch (packet.command)
        {
            case Command.M2C_HUB_LIST:
                M2C_HUB_LIST(packet);
                break;
            case Command.M2C_START_GAME:
                M2C_START_GAME(packet);
                break;
            default:
                break;
        }
    }

    /* -- Processing data -- */

    void M2C_HUB_LIST(Packet packet){
        text.text = packet.s_datas[0];
    }

    void M2C_START_GAME(Packet packet){
        gameController.gameSerial = packet.datas[0];
        gameController.SwitchPhases(Phases.GamePlay);
    }
}
