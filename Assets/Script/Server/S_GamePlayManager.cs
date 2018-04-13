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
        receiveSerials = serverController.AddSubscriptor(new ServerSubscriptor(OnReceive, new Command[4] { Command.C2M_GAME_READY, Command.C2M_PUT_SKILLPOINT, Command.C2M_CARDING, Command.C2M_TARGETING }));
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
        playersGameStatus = new PlayersGameStatus();
        for (int i = 0; i < Constants.maxPlayer; i++){
            Guest.Player player = serverController.playerList.players[i];
            playersGameStatus.AddPlayer(new Player(player.nick, player.clientSocket, i));
        }

        for (int i = 0; i < Constants.maxPlayer; i++){
            serverController.SendToClient(true, new Packet(Command.M2C_START_GAME, new int[1] { i }), playersGameStatus.players[i].clientSocket);
        }
    }

    bool gameStart = false;
    public void StartGame(){
        Debug.Log("Start game");
        performer_serial = Constants.maxPlayer - 1;
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
            case Command.C2M_CARDING:
                C2M_CARDING(packet, endPoint);
                break;
            case Command.C2M_TARGETING:
                C2M_TARGETING(packet, endPoint);
                break;
            default:
                break;
        }
    }

    void C2M_GAME_READY(Packet packet, string endPoint){
        serverController.playerList.FindPlayer(endPoint).game_ready = true;
        if (AllGameReady() && !gameStart){
            gameStart = true;
            for (int i = 0; i < Constants.maxPlayer; i++) serverController.playerList.players[i].game_ready = false;
            StartGame();
        }
    }

    void C2M_PUT_SKILLPOINT(Packet packet, string endPoint){
        Player player = playersGameStatus.FindPlayer(endPoint);

        if (player.gameSerial != performer_serial) return;
        player.attributes.ap--;
        if (packet.datas[0] == 1) player.attributes.fireAp++;
        if (packet.datas[0] == 2) player.attributes.waterAp++;
        if (packet.datas[0] == 3) player.attributes.airAp++;
        if (packet.datas[0] == 4) player.attributes.earthAp++;
        if (packet.datas[0] == 5) player.attributes.poisonAp++;
        if (packet.datas[0] == 6) player.attributes.thunderAp++;

        UpdateBoard();

        //點完所有屬性點才能繼續
        if(playersGameStatus.FindPlayer(endPoint).attributes.ap == 0){
            serverController.SendToClient(true, new Packet(Command.M2C_START_CARDING), player.clientSocket);
        }
    }

    void C2M_CARDING(Packet packet, string endPoint){
        playersGameStatus.placedCard = packet.datas[0];
        playersGameStatus.placedDir  = packet.datas[1];

        UpdateBoard();
        serverController.SendToClient(true, new Packet(Command.M2C_SPELL_TARGETING, new int[1]{1}, new List<int>[1]{ 
            new List<int>(new int[3]{ 1, 2 ,3})
        }), playersGameStatus.players[performer_serial].clientSocket);
    }

    void C2M_TARGETING(Packet packet, string endPoint){

    }

    /* -- Processing Packet -- */

    void UpdateBoard(){
        for (int i = 0; i < Constants.maxPlayer; i++){
            serverController.SendToClient(true, new Packet(Command.M2C_UPDATE_BOARD, new int[2]{playersGameStatus.placedCard, playersGameStatus.placedDir} ,
                                                           playersGameStatus.nicks, new List<int>[5]{
                playersGameStatus.players[0].attributes.list,
                playersGameStatus.players[1].attributes.list,
                playersGameStatus.players[2].attributes.list,
                playersGameStatus.players[3].attributes.list,
                playersGameStatus.players[i].cards
            }), playersGameStatus.players[i].clientSocket);
        }
    }

    bool AllGameReady(){
        for (int i = 0; i < Constants.maxPlayer; i++){
            if (!serverController.playerList.players[i].game_ready) return false;
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
                playersGameStatus.DrawCard(i, 1);
                playersGameStatus.players[i].attributes.ap += 1;
            }
        }
        UpdateBoard();
    }
}
