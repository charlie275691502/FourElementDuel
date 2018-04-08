using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ServerSubscriptor_Delegate(Packet packet, string endPoint_string);
public class ServerSubscriptor{
    public int serial;
	public ServerSubscriptor_Delegate subscriptor_Delegate;
	public Command[] commands;

	public ServerSubscriptor(ServerSubscriptor_Delegate s, Command[] m){
        subscriptor_Delegate = s;
		commands = m;
	}
}

public delegate void ClientSubscriptor_Delegate(Packet packet);
public class ClientSubscriptor{
    public int serial;
	public ClientSubscriptor_Delegate subscriptor_Delegate;
	public Command[] commands;

	public ClientSubscriptor(ClientSubscriptor_Delegate s, Command[] m){
        subscriptor_Delegate = s;
		commands = m;
	}
}