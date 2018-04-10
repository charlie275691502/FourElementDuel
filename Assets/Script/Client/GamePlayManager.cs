using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ClientAttributes : Attributes{
    public override int hp          { get { return _hp; }           set { _hp = value;          uI.hp.text = value.ToString(); } } 
    public override int shield      { get { return _shield; }       set { _shield = value;      uI.shield.text = value.ToString(); } }
    public override int fireAp      { get { return _fireAp; }       set { _fireAp = value;      uI.fireAp.text = value.ToString(); } }
    public override int waterAp     { get { return _waterAp; }      set { _waterAp = value;     uI.waterAp.text = value.ToString(); } }
    public override int airAp       { get { return _airAp; }        set { _airAp = value;       uI.airAp.text = value.ToString(); } }
    public override int earthAp     { get { return _earthAp; }      set { _earthAp = value;     uI.earthAp.text = value.ToString(); } }
    public override int poisonAp    { get { return _poisonAp; }     set { _poisonAp = value;    uI.poisonAp.text = value.ToString(); } }
    public override int thunderAp   { get { return _thunderAp; }    set { _thunderAp = value;   uI.thunderAp.text = value.ToString(); } }
    public override int ap          { get { return _ap; }           set { _ap = value;          uI.ap.text = value.ToString(); } }
    public AttributesUI uI;

    public void UpdateList(List<int> l){
        hp = l[0];
        shield = l[1];
        fireAp = l[2];
        waterAp = l[3];
        airAp = l[4];
        earthAp = l[5];
        poisonAp = l[6];
        thunderAp = l[7];
        ap = l[8];
    }
}

[System.Serializable]
public class AttributesUI{
    public GameObject lines;
    public Text nick;
    public Text hp;
    public Text shield;
    public Text fireAp;
    public Text waterAp;
    public Text airAp;
    public Text earthAp;
    public Text poisonAp;
    public Text thunderAp;
    public Text ap;

    public void UpdateUI(string n, List<int> list){
        nick.text = n;
        hp.text = list[0].ToString();
        shield.text = list[1].ToString();
        fireAp.text = list[2].ToString();
        waterAp.text = list[3].ToString();
        airAp.text = list[4].ToString();
        earthAp.text = list[5].ToString();
        poisonAp.text = list[6].ToString();
        thunderAp.text = list[7].ToString();
        ap.text = list[8].ToString();
    }

    public void UpdateFrameColor(Color color){
        foreach(Transform child in lines.transform){
            child.GetComponent<Image>().color = color;
        }
    }
}

public class GamePlayManager : MonoBehaviour {

    public ClientController clientController;
    public GameController gameController;

    private int receiveSerials;
    private bool onReceive;
    private Packet receivePacket;

    public ClientAttributes[] clientAttributes;

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

    // 1~6
    public void PutSkillPoint(int index){
        clientController.SendToServer(new Packet(Command.C2M_PUT_SKILLPOINT, new int[1]{index}));
    }

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

    void M2C_UPDATE_BOARD(Packet packet){
        for (int i = 0; i < Constants.maxPlayer; i++){
            clientAttributes[i].uI.UpdateUI(packet.s_datas[ClientSerialToServerSerial(i)], packet.l_datas[ClientSerialToServerSerial(i)]);
        }
    }

    bool myTurn = false;
    void M2C_TURN_START(Packet packet){
        for (int i = 0; i < Constants.maxPlayer; i++) clientAttributes[i].uI.UpdateFrameColor(Color.white);
        int x = ServerSerialToClientSerial(packet.datas[0]);
        clientAttributes[x].uI.UpdateFrameColor(Color.yellow);
        myTurn = (x == 0);
    }

    void M2C_DRAW(Packet packet){
        
    }

    void M2C_GAIN_SKILLPOINT(Packet packet){
        clientAttributes[0].ap += packet.datas[0];
    }

    /* -- Processing data -- */

    public int ClientSerialToServerSerial(int x){
        x += gameController.gameSerial;
        if (x >= Constants.maxPlayer) x -= Constants.maxPlayer;
        return x;
    }

    public int ServerSerialToClientSerial(int x){
        x += (Constants.maxPlayer - gameController.gameSerial);
        if (x >= Constants.maxPlayer) x -= Constants.maxPlayer;
        return x;
    }
}
