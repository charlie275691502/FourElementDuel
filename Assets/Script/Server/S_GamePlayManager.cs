using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_GamePlayManager : MonoBehaviour {
    public ServerController serverController;
    public S_GameController s_GameController;

    private int receiveSerials;
    private bool onReceive;
    private Packet receivePacket;
    private string receiveEndPoint;

    public int[] board;

    void OnEnable()
    {
        onReceive = false;
        receiveSerials = serverController.AddSubscriptor(new ServerSubscriptor(OnReceive, new Command[1] { Command.C2M_PLACE }));
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

    void Win_Check()
    {
        int[] Opoint = new int[8];
        int[] Xpoint = new int[8];

        int point = 0;
        for (int i = 0; i < 9; i++)
        {
            int b = board[i];
            int x = i % 3;
            int y = i / 3;
            if (b == 1)
            {
                point++;
                Opoint[x]++;
                Opoint[y + 3]++;
                if (x == y) Opoint[6]++;
                if (x + y == 2) Opoint[7]++;
            }
            if (b == 2)
            {
                point++;
                Xpoint[x]++;
                Xpoint[y + 3]++;
                if (x == y) Xpoint[6]++;
                if (x + y == 2) Xpoint[7]++;
            }
        }

        foreach (int O in Opoint) {
            if (O >= 3){
                serverController.SendToAllClient(new Packet(Command.M2C_GAME_OVER, new int[1] { 1 }, new string[1] { serverController.playerList.FindPlayer(s_GameController.Oserial).nick }));
                return;
            }
        }
        foreach (int X in Xpoint) {
            if (X >= 3){
                serverController.SendToAllClient(new Packet(Command.M2C_GAME_OVER, new int[1] { 1 }, new string[1] { serverController.playerList.FindPlayer(s_GameController.Xserial).nick }));
                return;
            }
        }
        if(point == 9){
            serverController.SendToAllClient(new Packet(Command.M2C_GAME_OVER, new int[1] { 0 }, new string[1] { "" }));
            return;
        }
    }
    /* -- Receiving Packet -- */

    public void OnReceive(Packet packet, string endPoint){
        while (onReceive == true) ;
        onReceive = true;
        receivePacket = packet;
        receiveEndPoint = endPoint;
    }

    public void AnalysisReceive(Packet packet, string endPoint)
    {
        switch (packet.command)
        {
            case Command.C2M_PLACE:
                C2M_PLACE(packet, endPoint);
                break;
            default:
                break;
        }
    }

    /* -- Processing Packet -- */


    void C2M_PLACE(Packet packet, string endPoint){
        int index = packet.datas[0] - 1;
        if(board[index] == 0){
            int player_team = serverController.playerList.FindPlayer(endPoint).team;
            if(s_GameController.turn_priority == player_team){
                board[index] = serverController.playerList.FindPlayer(endPoint).team;
                serverController.SendToAllClient(new Packet(Command.M2C_UPDATE_BOARD, board));
                s_GameController.turn_priority = 3 - s_GameController.turn_priority;

                Win_Check();
            }
        }
    }
}
