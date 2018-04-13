using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

// for server
public class Attributes{
    public List<int> list = new List<int>(new int[10]{40, 0, 0, 0, 0, 0, 0, 0, 0, 0});
    public int _hp;
    public int _shield;
    public int _fireAp;
    public int _waterAp;
    public int _airAp;
    public int _earthAp;
    public int _poisonAp;
    public int _thunderAp;
    public int _ap;
    public int _cardsCount;
    public virtual int hp           { get { return _hp; }           set { _hp = value;          list[0] = value; } }
    public virtual int shield       { get { return _shield; }       set { _shield = value;      list[1] = value; } }
    public virtual int fireAp       { get { return _fireAp; }       set { _fireAp = value;      list[2] = value; } }
    public virtual int waterAp      { get { return _waterAp; }      set { _waterAp = value;     list[3] = value; } }
    public virtual int airAp        { get { return _airAp; }        set { _airAp = value;       list[4] = value; } }
    public virtual int earthAp      { get { return _earthAp; }      set { _earthAp = value;     list[5] = value; } }
    public virtual int poisonAp     { get { return _poisonAp; }     set { _poisonAp = value;    list[6] = value; } }
    public virtual int thunderAp    { get { return _thunderAp; }    set { _thunderAp = value;   list[7] = value; } }
    public virtual int ap           { get { return _ap; }           set { _ap = value;          list[8] = value; } }
    public virtual int cardsCount   { get { return _cardsCount; }   set { _ap = value;          list[9] = value; } }
}

public class Player{
    public string nick;
    public Socket clientSocket;
    public int gameSerial;
    public Attributes attributes = new Attributes();
    public List<int> cards = new List<int>();

    public Player(string n, Socket cs, int g){
        nick = n;
        clientSocket = cs;
        gameSerial = g;
        //attributes = new Attributes();
    }
}

public class PlayersGameStatus{
    public List<Player> players = new List<Player>();
    public string[] nicks = new string[0];
    public List<int> cards;
    public int placedCard = 0;
    public int placedDir = 0;

    public PlayersGameStatus(){
        cards = new List<int>(new int[Constants.maxCards]);
        for (int i = 0; i < Constants.maxCards; i++) cards[i] = i + 1;
    }

    public void DrawCard(int index, int amount){
        while((--amount) >= 0){
            int ran = Random.Range(0, cards.Count);
            int result = cards[ran];
            cards.RemoveAt(ran);

            players[index].cards.Add(result);
            players[index].attributes.cardsCount++;
        }
    }

    public void AddPlayer(Player playerGameStatus){
        players.Add(playerGameStatus);
        string[] tmp = nicks;
        nicks = new string[tmp.Length + 1];
        for (int i = 0; i < tmp.Length; i++) nicks[i] = tmp[i];
        nicks[tmp.Length] = playerGameStatus.nick;
    }

    public Player FindPlayer(string endPoint_string){
        foreach(Player player in players){
            if(player.clientSocket.RemoteEndPoint.ToString() == endPoint_string){
                return player;
            }
        }
        Debug.Log("Can't find player");
        return null;
    }
}