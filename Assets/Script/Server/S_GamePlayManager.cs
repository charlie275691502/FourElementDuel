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


    void OnEnable()
    {
        onReceive = false;
        receiveSerials = serverController.AddSubscriptor(new ServerSubscriptor(OnReceive, new Command[2] { Command.C2M_GAME_READY, Command.C2M_PUT_SKILLPOINT }));
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

    int performer_serial; // 0, 1, 2, 3
    public PlayersGameStatus playersGameStatus;

    public void InitGame(){
        // 決定順序
        for (int i = 0; i < Constants.maxPlayer; i++){
            Guest.Player player = serverController.playerList.players[i];
            playersGameStatus.AddPlayer(new Player(player.nick, player.clientSocket, i));
        }

        for (int i = 0; i < Constants.maxPlayer; i++){
            serverController.SendToClient(true, new Packet(Command.M2C_START_GAME, new int[1] { i }), playersGameStatus.players[i].clientSocket);
        }
    }

    public void StartGame(){
        performer_serial = Constants.maxPlayer - 1;
        UpdateBoard();
        NextTurn();
    }

    /* -- Receiving Packet -- */

    public void OnReceive(Packet packet, string endPoint){
        while (onReceive == true) {}
        onReceive = true;
        receivePacket = packet;
        receiveEndPoint = endPoint;
    }

    public void AnalysisReceive(Packet packet, string endPoint)
    {
        switch (packet.command){
            case Command.C2M_GAME_READY:
                C2M_GAME_READY(packet, endPoint);
                break;
            case Command.C2M_PUT_SKILLPOINT:
                C2M_PUT_SKILLPOINT(packet, endPoint);
                break;
            default:
                break;
        }
    }

    void C2M_GAME_READY(Packet packet, string endPoint){
        serverController.playerList.FindPlayer(endPoint).game_ready = true;
        if (AllGameReady()){
            for (int i = 0; i < 4; i++) serverController.playerList.players[0].game_ready = false;
            StartGame();
        }
    }

    void C2M_PUT_SKILLPOINT(Packet packet, string endPoint){
        
        playersGameStatus.FindPlayer(endPoint).attribute.ap--;
        if (packet.datas[0] == 1) playersGameStatus.FindPlayer(endPoint).attribute.fireAp++;
        if (packet.datas[0] == 2) playersGameStatus.FindPlayer(endPoint).attribute.waterAp++;
        if (packet.datas[0] == 3) playersGameStatus.FindPlayer(endPoint).attribute.airAp++;
        if (packet.datas[0] == 4) playersGameStatus.FindPlayer(endPoint).attribute.earthAp++;
        if (packet.datas[0] == 5) playersGameStatus.FindPlayer(endPoint).attribute.poisonAp++;
        if (packet.datas[0] == 6) playersGameStatus.FindPlayer(endPoint).attribute.thunderAp++;

        UpdateBoard();

        //暫時放這裡
        NextTurn();
    }

    void UpdateBoard(){
        for (int i = 0; i < Constants.maxPlayer; i++){
            serverController.SendToClient(true, new Packet(Command.M2C_UPDATE_BOARD, playersGameStatus.nicks, new List<int>[5]{
                playersGameStatus.players[0].attribute.list,
                playersGameStatus.players[1].attribute.list,
                playersGameStatus.players[2].attribute.list,
                playersGameStatus.players[3].attribute.list,
                playersGameStatus.players[i].cards
            }), playersGameStatus.players[i].clientSocket);
        }
    }

    /* -- Processing Packet -- */

    bool AllGameReady(){
        for (int i = 0; i < 4; i++){
            if (!serverController.playerList.players[0].game_ready) return false;
        }
        return true;
    }

    void NextTurn(){
        // next player;
        performer_serial++;
        if (performer_serial >= Constants.maxPlayer) performer_serial -= Constants.maxPlayer;

        for (int i = 0; i < Constants.maxPlayer; i++){

            serverController.SendToClient(true, new Packet(Command.M2C_TURN_START, new int[1] { performer_serial }), playersGameStatus.players[i].clientSocket);
            if( i == performer_serial ){
                // 缺一段code, 把這張牌夾到伺服器資料中的玩家手排中
                serverController.SendToClient(true, new Packet(Command.M2C_DRAW, new int[1] { 0 }), playersGameStatus.players[i].clientSocket);
                playersGameStatus.players[i].attribute.ap += 1;
                serverController.SendToClient(true, new Packet(Command.M2C_GAIN_SKILLPOINT, new int[1]{1}), playersGameStatus.players[i].clientSocket);
            }
        }
    }
}
