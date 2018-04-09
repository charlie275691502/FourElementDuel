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



    /* -- Receiving Packet -- */

    public void OnReceive(Packet packet, string endPoint){
        while (onReceive == true) {}
        onReceive = true;
        receivePacket = packet;
        receiveEndPoint = endPoint;
    }

    public void AnalysisReceive(Packet packet, string endPoint)
    {
        switch (packet.command)
        {
            case Command.C2M_PLACE:
                //C2M_PLACE(packet, endPoint);
                break;
            default:
                break;
        }
    }

    /* -- Processing Packet -- */


}
