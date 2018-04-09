using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Guest;

public class ServerController : MonoBehaviour {
	private Socket serverSocket;//伺服器本身的Socket
	private Thread threadConnect;//連線的Thread
	private Thread threadReceive;//接收資料的Thread
	private byte[] _recieveBuffer = new byte[65536];

	public ClientController clientController;
	[HideInInspector] public bool is_Connect;
    public bool hide_ping_msg;

	private List<ServerSubscriptor> subscriptors = new List<ServerSubscriptor> {};

    public Guest.PlayerList playerList = new Guest.PlayerList();

    public InputField inputField;

    void Start(){
        StartServer (inputField.text, 6805);
	}

    void OnApplicationQuit() {
        Debug.Log("Application ending after " + Time.time + " seconds");
        StopServer();
    }

	public void StartServer(string ip, int port){
        Debug.Log(ip);
        //		PlayerPrefs.SetString ("server_ip", ip_text.text);
        //		PlayerPrefs.SetString ("server_port", port_text.text);
		serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//new server socket object
		serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
		serverSocket.Listen(16);//最多一次接受多少人連線
		threadConnect = new Thread(Accept);
		threadConnect.IsBackground = true;//設定為背景執行續，當程式關閉時會自動結束
		threadConnect.Start();
		Debug.Log ("Server Start");
        clientController.StartConnection (ip, 6805);
	}

    public void StopServer(){
        foreach(Player player in playerList.players){
            try
            {
                player.clientSocket.Close();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }

        serverSocket.Close();
    }

	private void Accept()
	{
		try
		{
			Socket clientSocket = serverSocket.Accept();//等到連線成功後才會往下執行
            int playerSerial = playerList.AddPlayer(clientSocket);
			Debug.Log ("Accept: " + clientSocket.RemoteEndPoint.ToString());
            SendToClient(false, new Packet(Command.M2C_WELCOME, new int[1]{playerSerial}), clientSocket);
			clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallback), clientSocket);

			threadConnect = new Thread(Accept);
			threadConnect.IsBackground = true;
			threadConnect.Start();
			//連線成功後，若是不想再接受其他連線，可以關閉serverSocket
			//serverSocket.Close();
		}
		catch (Exception ex)
		{
			Debug.Log (ex.ToString());
		}
	}

    public void SendToAllClient(bool loop_sending, Packet packet){
        foreach(Player player in playerList.players){
            Debug.Log("I");
            SendToClient(loop_sending, packet, player.clientSocket);
        }
    }

    int packet_serial = 1;
    bool[] send_list = new bool[256];

    public void SendToClient(Packet packet, Socket clientSocket){
        SendToClient(false, packet, clientSocket);
    }
    public void SendToClient(bool loop_sending, Packet packet, Socket clientSocket){

        if (loop_sending){
            while (send_list[packet_serial]) {
                packet_serial++;
                if (packet_serial >= 256) packet_serial = 1;   
            }
            send_list[packet_serial] = true;
            packet.Change_Serial(packet_serial);
            Debug.Log("B");

            packet_serial++;
            if (packet_serial >= 256) packet_serial = 1;   
        } 

        if (packet.command != Command.M2C_PONG || !hide_ping_msg) packet.Print("SEND");
        byte[] data = packet.b_datas;
        try
        {
            byte[] byteArray = data;

            if (loop_sending) {
                StartCoroutine(Loop_SendData(packet.serial, byteArray, clientSocket));
            } else {
                SendData(byteArray, clientSocket);
            }
        }
        catch (SocketException ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    IEnumerator Loop_SendData(int this_serial, byte[] data, Socket clientSocket){
        while(send_list[this_serial]){
            SendData(data, clientSocket);
            yield return new WaitForSeconds(1);
        }
    }

    private void SendData(byte[] data, Socket clientSocket)
    {
        SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
        socketAsyncData.SetBuffer(data, 0, data.Length);
        clientSocket.SendAsync(socketAsyncData);
    }

	private void ReceiveCallback(IAsyncResult AR)
	{
		Socket clientSocket = (Socket)AR.AsyncState;
		int recieved = clientSocket.EndReceive(AR);
		if(recieved <= 0)
			return;
		byte[] recData = new byte[recieved];
		Buffer.BlockCopy(_recieveBuffer,0,recData,0,recieved);
        clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientSocket);

		Packet packet = new Packet (recData);
        if(packet.command != Command.C2M_PING || !hide_ping_msg)packet.Print ("server RECEIVE " + clientSocket.RemoteEndPoint.ToString());

        if (packet.command == Command.C2M_SERIAL_PONG) {
            send_list[packet.serial] = false;
            return;
        }
        if (packet.command == Command.C2M_PING){
            C2M_PING(packet, clientSocket);
            return;
        }

		foreach (ServerSubscriptor subscriptor in subscriptors) {
			foreach (Command command in subscriptor.commands) {
				if (command == packet.command) {
					subscriptor.subscriptor_Delegate (packet, clientSocket.RemoteEndPoint.ToString());
					break;
				}
			}
		}

	}

    void C2M_PING(Packet packet, Socket clientSocket){
        SendToClient(false, new Packet(Command.M2C_PONG, new int[1]{packet.datas[0]}), clientSocket);
    } 

    int serial = 0;
    public int AddSubscriptor(ServerSubscriptor subscriptor)
    {
        subscriptor.serial = ++serial;
        subscriptors.Add(subscriptor);
        return serial;
    }

    public void RemoveSubscriptor(int i)
    {
        foreach(ServerSubscriptor s in subscriptors){
            if(s.serial == i){
                subscriptors.Remove(s);
                return;
            }
        }

        Debug.Log("Serials number not found. subscriptor fail to remove");
        return;
    }

    public void SendHubList(){
        string hub_list = "";
        hub_list += "Players List\n";
        foreach (Player player in playerList.players)
        {
            hub_list += " -" + player.nick + "\n";
        }

        SendToAllClient(true, new Packet(Command.M2C_HUB_LIST, new string[1] { hub_list }));
    }
}
