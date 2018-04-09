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
        receiveSerials = serverController.AddSubscriptor(new ServerSubscriptor(OnReceive, new Command[3] { Command.C2M_CHANGE_NICK, Command.C2M_PLAY, Command.C2M_READY}));
    }
    void OnDisable()
    {
        serverController.RemoveSubscriptor(receiveSerials);
    }

    void Update()
    {
        if (onReceive)
        {
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

    public void AnalysisReceive(Packet packet, string endPoint)
    {
        switch (packet.command)
        {
            case Command.C2M_CHANGE_NICK:
                C2M_CHANGE_NICK(packet, endPoint);
                break;
            case Command.C2M_PLAY:
                C2M_PLAY(packet, endPoint);
                break;
            case Command.C2M_READY:
                C2M_READY(packet, endPoint);
                break;
            default:
                break;
        }
    }

    void C2M_CHANGE_NICK(Packet packet, string endPoint){
        Player player = serverController.playerList.FindPlayer(endPoint);
        player.nick = packet.s_datas[0];
        serverController.SendHubList();
    }

    void C2M_PLAY(Packet packet, string endPoint){
        if (!serverController.playerList.players[0].ready || !serverController.playerList.players[1].ready) return;
        int team1 = Random.Range(0, 2);
        int team2 = 1 - team1;
        serverController.playerList.players[team1].team = 1;
        serverController.playerList.players[team2].team = 2;
        s_GameController.Oserial = serverController.playerList.players[team1].serial;
        s_GameController.Xserial = serverController.playerList.players[team2].serial;
        s_GameController.turn_priority = 1;
        s_GameController.s_GamePlayManager.board = new int[9];
        serverController.playerList.players[0].ready = false;
        serverController.playerList.players[1].ready = false;
        serverController.SendToAllClient(true, new Packet(Command.M2C_START_GAME, new int[2] { serverController.playerList.players[team1].serial, serverController.playerList.players[team2].serial }, new string[2] { serverController.playerList.players[team1].nick, serverController.playerList.players[team2].nick }));
    }

    void C2M_READY(Packet packet, string endPoint){
        serverController.playerList.FindPlayer(endPoint).ready = true;
        serverController.SendHubList();
    }

    /* -- Processing Packet -- */
}
