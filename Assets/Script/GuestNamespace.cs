using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

namespace Guest{
    [System.Serializable]
    public class Player
    {
        public int serial;
        public string nick;
        public bool ready;
        public bool game_ready;
        public float ping_time;
        public float pong_time
        {
            set
            {
                PING = value - ping_time;
            }
        }
        public float PING; //ping value
        public bool disconnected = false;
        public Socket clientSocket;


        public Player(string n, Socket s, int se)
        {
            nick = n;
            ready = false;
            game_ready = false;
            ping_time = 0;
            clientSocket = s;
            serial = se;
        }
    }

    [System.Serializable]
    public class PlayerList{
        public List<Player> players = new List<Player>();

        int serial = 0;
        public int AddPlayer(Socket clientSocket){
            players.Add(new Player("Unknown", clientSocket, ++serial));
            return serial;
        }

        public void RemovePlayer(Player visitor)
        {
            players.Remove(visitor);
            Debug.Log("removed");
        }

        public Player FindPlayer(int serial){
            foreach (Player v in players){
                if (v.serial == serial)
                    return v;
            }

            return null;
        }

        public Player FindPlayer(string endPoint_string){
            foreach (Player v in players){
                if (v.clientSocket.RemoteEndPoint.ToString() == endPoint_string)
                    return v;
            }

            return null;
        }

        public bool HasPlayerNick(string nick)
        {
            foreach (Player v in players)
            {
                if (v.nick == nick) return true;
            }
            return false;
        }
    }
}
