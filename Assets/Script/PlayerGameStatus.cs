using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

public class Attribute{
    public List<int> list = new List<int>(9);
    public int _hp;
    public int _shield;
    public int _fire_ap;
    public int _water_ap;
    public int _air_ap;
    public int _earth_ap;
    public int _poison_ap;
    public int _thunder_ap;
    public int _ap;
    public int hp           { get { return _hp; }           set { _hp = value;          list[0] = value; } }
    public int shield       { get { return _shield; }       set { _shield = value;      list[1] = value; } }
    public int fire_ap      { get { return _fire_ap; }      set { _fire_ap = value;     list[2] = value; } }
    public int water_ap     { get { return _water_ap; }     set { _water_ap = value;    list[3] = value; } }
    public int air_ap       { get { return _air_ap; }       set { _air_ap = value;      list[4] = value; } }
    public int earth_ap     { get { return _earth_ap; }     set { _earth_ap = value;    list[5] = value; } }
    public int poison_ap    { get { return _poison_ap; }    set { _poison_ap = value;   list[6] = value; } }
    public int thunder_ap   { get { return _thunder_ap; }   set { _thunder_ap = value;  list[7] = value; } }
    public int ap           { get { return _ap; }           set { _ap = value;          list[8] = value; } }
}

public class PlayerGameStatus{
    public string nick;
    public Socket clientSocket;
    public int gameSerial;
    public Attribute attribute;
    public List<int> cards;

    public PlayerGameStatus(string n, Socket cs, int g){
        nick = n;
        clientSocket = cs;
        gameSerial = g;
    }
}

public class PlayersGameStatus{
    public List<PlayerGameStatus> players;
    public string[] nicks = new string[0];

    public void AddPlayer(PlayerGameStatus playerGameStatus){
        players.Add(playerGameStatus);
        string[] tmp = nicks;
        nicks = new string[tmp.Length + 1];
        for (int i = 0; i < tmp.Length; i++) nicks[i] = tmp[i];
        nicks[tmp.Length] = playerGameStatus.nick;
    }

    public PlayerGameStatus FindPlayer(string endPoint_string){
        foreach(PlayerGameStatus player in players){
            if(player.clientSocket.RemoteEndPoint.ToString() == endPoint_string){
                return player;
            }
        }
        Debug.Log("Can't find player");
        return null;
    }
}