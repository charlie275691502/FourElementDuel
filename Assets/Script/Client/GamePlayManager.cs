using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AttributesUI{
    public Text nick;
    public Text hp;
    public Text shield;
    public Text fire_ap;
    public Text water_ap;
    public Text air_ap;
    public Text earth_ap;
    public Text poison_ap;
    public Text thunder_ap;
    public Text ap;

    public void UpdateUI(string n, List<int> list){
        nick.text = n;
        hp.text = list[0].ToString();
        shield.text = list[1].ToString();
        fire_ap.text = list[2].ToString();
        water_ap.text = list[3].ToString();
        air_ap.text = list[4].ToString();
        earth_ap.text = list[5].ToString();
        poison_ap.text = list[6].ToString();
        thunder_ap.text = list[7].ToString();
        ap.text = list[8].ToString();
    }
}

public class GamePlayManager : MonoBehaviour {

    public ClientController clientController;
    public GameController gameController;

    private int receiveSerials;
    private bool onReceive;
    private Packet receivePacket;

    public AttributesUI[] attributesUIs;

    void OnEnable(){
        onReceive = false;
        receiveSerials = clientController.AddSubscriptor(new ClientSubscriptor(OnReceive, new Command[4] { Command.M2C_UPDATE_BOARD, Command.M2C_TURN_START, Command.M2C_DRAW, Command.M2C_GAIN_SKILLPOINT }));
        clientController.SendToServer(new Packet(Command.C2M_GAME_READY));
    }
    void OnDisable()
    {
        clientController.RemoveSubscriptor(receiveSerials);
    }

    void Update()
    {
        if (onReceive){
            AnalysisReceive(receivePacket);
            onReceive = false;
        }
    }

    /* -- Sending Packet -- */

    /* -- Receiving Packet -- */

    public void OnReceive(Packet packet){
        while (onReceive == true) { }
        onReceive = true;
        receivePacket = packet;
    }

    public void AnalysisReceive(Packet packet){
        switch (packet.command){
            case Command.M2C_UPDATE_BOARD:
                M2C_UPDATE_BOARD(packet);
                break;
            case Command.M2C_TURN_START:
                M2C_TURN_START(packet);
                break;
            case Command.M2C_DRAW:
                M2C_DRAW(packet);
                break;
            case Command.M2C_GAIN_SKILLPOINT:
                M2C_GAIN_SKILLPOINT(packet);
                break;
            default:
                break;
        }
    }

    /* -- Processing data -- */

    void M2C_UPDATE_BOARD(Packet packet){
        for (int i = 0; i < Constants.maxPlayer; i++){
            attributesUIs[i].UpdateUI(packet.s_datas[i], packet.l_datas[i]);
        }
    }

    void M2C_TURN_START(Packet packet){

    }

    void M2C_DRAW(Packet packet){

    }

    void M2C_GAIN_SKILLPOINT(Packet packet){

    }
}
