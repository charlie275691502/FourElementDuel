using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour {

    public ClientController clientController;
    public GameController gameController;

    private int receiveSerials;
    private bool onReceive;
    private Packet receivePacket;

    public Text OName;
    public Text XName;
    public Sprite O;
    public Sprite X;
    public GameObject[] slices;

    void OnEnable()
    {
        OName.text = gameController.OName;
        XName.text = gameController.XName;
        OName.color = (gameController.team == 1) ? Color.red : Color.black;
        XName.color = (gameController.team == 2) ? Color.red : Color.black;
        onReceive = false;
        receiveSerials = clientController.AddSubscriptor(new ClientSubscriptor(OnReceive, new Command[2] { Command.M2C_UPDATE_BOARD, Command.M2C_GAME_OVER }));
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

    public void Place(int index){
        clientController.SendToServer(new Packet(Command.C2M_PLACE, new int[1] { index }));
    }

    /* -- Receiving Packet -- */

    public void OnReceive(Packet packet){
        while (onReceive == true) { }
        onReceive = true;
        receivePacket = packet;
    }

    public void AnalysisReceive(Packet packet)
    {
        switch (packet.command)
        {
            case Command.M2C_UPDATE_BOARD:
                M2C_UPDATE_BOARD(packet);
                break;
            case Command.M2C_GAME_OVER:
                M2C_GAME_OVER(packet);
                break;
            default:
                break;
        }
    }

    /* -- Processing data -- */

    void M2C_UPDATE_BOARD(Packet packet)
    {
        for (int i = 0; i < 9;i++){
            switch(packet.datas[i]){
                case 0:
                    slices[i].GetComponent<SpriteRenderer>().sprite = null;
                    break;
                case 1:
                    slices[i].GetComponent<SpriteRenderer>().sprite = O;
                    break;
                case 2:
                    slices[i].GetComponent<SpriteRenderer>().sprite = X;
                    break;
                default:
                    break;
            }
        }
    }

    void M2C_GAME_OVER(Packet packet){
        if(packet.datas[0] == 1){
            gameController.Start_Dialog(GAME_OVER_delegate, "", packet.s_datas[0] + " wins!", 1);
        } else {
            gameController.Start_Dialog(GAME_OVER_delegate, "", "平手", 1);
        }
    }

    public void GAME_OVER_delegate(bool options){
        foreach (GameObject slice in slices) slice.GetComponent<SpriteRenderer>().sprite = null;
        gameController.SwitchPhases(Phases.Hub);
    }
}
