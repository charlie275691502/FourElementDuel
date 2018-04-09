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

    void OnEnable(){
        onReceive = false;
        receiveSerials = clientController.AddSubscriptor(new ClientSubscriptor(OnReceive, new Command[4] { Command.M2C_UPDATE_BOARD, Command.M2C_TURN_START, Command.M2C_DRAW, Command.M2C_GAIN_SKILLPOINT }));
        clientController.SendToServer(new Packet(Command.C2M_GAME_READY));
    }
    void OnDisable()
    {
        clientController.RemoveSubscriptor(receiveSerials);
    }

    void Update()
    {
        if (onReceive){
            AnalysisReceive(receivePacket);
            onReceive = false;
        }
    }

    /* -- Sending Packet -- */

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
            case Command.M2C_TURN_START:
                M2C_TURN_START(packet);
                break;
            case Command.M2C_DRAW:
                M2C_DRAW(packet);
                break;
            case Command.M2C_GAIN_SKILLPOINT:
                M2C_GAIN_SKILLPOINT(packet);
                break;
            default:
                break;
        }
    }

    /* -- Processing data -- */

    void M2C_UPDATE_BOARD(Packet packet){
        
    }

    void M2C_TURN_START(Packet packet){

    }

    void M2C_DRAW(Packet packet){

    }

    void M2C_GAIN_SKILLPOINT(Packet packet){

    }
}
