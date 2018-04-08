using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour {
    public ClientController clientController;
    public GameController gameController;

    private int receiveSerials;
    private bool onReceive;
    private Packet receivePacket;

    public GameObject serverController_gmo;
    public InputField inputField;

    void OnEnable()
    {
        onReceive = false;
        receiveSerials = clientController.AddSubscriptor(new ClientSubscriptor(OnReceive, new Command[1] { Command.M2C_WELCOME }));
    }
    void OnDisable()
    {
        clientController.RemoveSubscriptor(receiveSerials);
    }

    void Update()
    {
        if (onReceive)
        {
            AnalysisReceive(receivePacket);
            onReceive = false;
        }
    }

    public void ConnectToServer()
    {
        clientController.StartConnection(inputField.text, 6805);
    }

    public void CreateServer()
    {
        serverController_gmo.SetActive(true);
    }

    /* -- Sending Packet -- */



    /* -- Receiving Packet -- */

    public void OnReceive(Packet packet)
    {
        while (onReceive == true) ;
        onReceive = true;
        receivePacket = packet;
    }

    public void AnalysisReceive(Packet packet)
    {
        switch (packet.command)
        {
            case Command.M2C_WELCOME:
                M2C_WELCOME(packet);
                break;
            default:
                break;
        }
    }

    /* -- Processing data -- */

    void M2C_WELCOME(Packet packet) {
        clientController.StartCoroutine(clientController.Start_Ping());
        gameController.serial = packet.datas[0];
        gameController.SwitchPhases(Phases.Hub);
    }

}
