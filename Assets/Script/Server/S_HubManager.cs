using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Guest;

public class S_HubManager : MonoBehaviour {
    public ServerController serverController;
    public S_GameController s_GameController;

    private int receiveSerials;
    private bool onReceive;
    private Packet receivePacket;
    private string receiveEndPoint;

    void OnEnable(){
        onReceive = false;
        receiveSerials = serverController.AddSubscriptor(new ServerSubscriptor(OnReceive, new Command[3] { Command.C2M_CHANGE_NICK, Command.C2M_PLAY, Command.C2M_HUB_READY}));
    }
    void OnDisable(){
        serverController.RemoveSubscriptor(receiveSerials);
    }

    void Update()
    {
        if (onReceive){
            AnalysisReceive(receivePacket, receiveEndPoint);
            onReceive = false;
        }
    }

    /* -- Sending Packet -- */



    /* -- Receiving Packet -- */

    public void OnReceive(Packet packet, string endPoint)
    {
        while (onReceive == true) ;
        onReceive = true;
        receivePacket = packet;
        receiveEndPoint = endPoint;
    }

    public void AnalysisReceive(Packet packet, string endPoint){
        switch (packet.command){
            case Command.C2M_CHANGE_NICK:
                C2M_CHANGE_NICK(packet, endPoint);
                break;
            case Command.C2M_PLAY:
                C2M_PLAY(packet, endPoint);
                break;
            case Command.C2M_HUB_READY:
                C2M_READY(packet, endPoint);
                break;
            default:
                break;
        }
    }

    void C2M_CHANGE_NICK(Packet packet, string endPoint){
		Guest.Player player = serverController.playerList.FindPlayer(endPoint);
        player.nick = packet.s_datas[0];
        serverController.SendHubList();
    }

    void C2M_PLAY(Packet packet, string endPoint){
        if (!AllReady()) return;
        serverController.playerList.players[0].ready = false;
        serverController.playerList.players[1].ready = false;
        serverController.playerList.players[2].ready = false;
        serverController.playerList.players[3].ready = false;
        s_GameController.s_GamePlayManager.InitGame();
    }

    void C2M_READY(Packet packet, string endPoint){
        serverController.playerList.FindPlayer(endPoint).ready = true;
        serverController.SendHubList();
    }

    /* -- Processing Packet -- */

    bool AllReady(){
        for (int i = 0; i < 4; i++) {
            if (!serverController.playerList.players[0].ready) return false;
        }
        return true;
    }
}
