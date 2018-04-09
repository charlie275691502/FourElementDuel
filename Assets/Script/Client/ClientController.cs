using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

public class ClientController : MonoBehaviour {
    public GameController gameController;

	private Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private byte[] _recieveBuffer = new byte[65536];
    public bool hide_ping_msg;
    public int ping_value = 0;
    public Text ping_text;
    public float client_timer;

	private List<ClientSubscriptor> subscriptors = new List<ClientSubscriptor> {};

	void Update(){
        client_timer = Time.time;
        ping_text.text = ping_value.ToString() + "ms";
	}

	void OnApplicationQuit() {
		CloseConnection();
	}

	public void StartConnection(string IP, int Port) {
		try
		{
			_clientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(IP), Port), new AsyncCallback(OnConnect), _clientSocket);
		}
		catch(SocketException ex)
		{
			Debug.Log(ex.Message);
		}
	}
	public void SendToServer(Packet packet) {
		if (packet.command != Command.C2M_PING || !hide_ping_msg)packet.Print ("SEND");

		byte[] data = packet.b_datas;
		try
		{
			byte[] byteArray = data;
			SendData(byteArray);
		}
		catch(SocketException ex)
		{
			Debug.LogWarning(ex.Message);
		}
	}
	private void SendData(byte[] data)
	{
		SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
		socketAsyncData.SetBuffer(data,0,data.Length);
		_clientSocket.SendAsync(socketAsyncData);
	}
	private void OnConnect(IAsyncResult iar)
	{
		Debug.Log ("On Server Connected");
		_clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length,SocketFlags.None,new AsyncCallback(ReceiveCallback), null);
	}

    int ping_seq = 0;
    float[] ping_time = new float[256];
	public IEnumerator Start_Ping(){
        while(true){
            Packet ping = new Packet(Command.C2M_PING, new int[1] { ping_seq });
            SendToServer(ping);
            ping_time[ping_seq] = client_timer;
            ping_seq++;
            if (ping_seq >= 256) ping_seq -= 256;
			yield return new WaitForSeconds(1);
		}
	}

	/// 接收封包.
	private void ReceiveCallback(IAsyncResult AR)
	{
		int recieved = _clientSocket.EndReceive(AR);
		if(recieved <= 0)
			return;
		byte[] recData = new byte[recieved];
		Buffer.BlockCopy(_recieveBuffer,0,recData,0,recieved);
        _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
		
        Packet packet = new Packet(recData);
        if (packet.command != Command.M2C_PONG || !hide_ping_msg)packet.Print ("client RECEIVED");


        if (packet.serial != 0) {
            SendToServer(new Packet(packet.serial));
        }

        if (packet.command == Command.M2C_PONG) {
            M2C_PONG(packet);
            return;
        }

		// Notify other managers when receiving data from server
		foreach (ClientSubscriptor subscriptor in subscriptors) {
			foreach (Command command in subscriptor.commands) {
				if (command == packet.command) {
					subscriptor.subscriptor_Delegate (packet);
					break;
				}
			}
		}
	}

    void M2C_PONG(Packet packet){
        ping_value = Mathf.FloorToInt((client_timer - ping_time[packet.datas[0]]) * 1000);
    }

    int serial = 0;
    public int AddSubscriptor(ClientSubscriptor subscriptor) {
        subscriptor.serial = ++serial;
        subscriptors.Add(subscriptor);
        return serial;
	}

    public void RemoveSubscriptor(int i) {
        foreach (ClientSubscriptor s in subscriptors)
        {
            if (s.serial == i)
            {
                subscriptors.Remove(s);
                return;
            }
        }

        Debug.Log("Serials number not found. subscriptor fail to remove");
        return;
	}

	/// 關閉 Socket 連線.
	public void CloseConnection() {
		_clientSocket.Shutdown(SocketShutdown.Both);
		_clientSocket.Close();
	}

}