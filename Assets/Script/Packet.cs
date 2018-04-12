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

static class Constants{
	public const int maxPlayer = 4;
    public const int maxCards = 40;
}

public enum Data_Type{
	Byte,
	Short,
	Unsigned_Short,
	SP_Float, /* single_precision_float */
	String,
	Short_List
}

public enum NUM{
	Integer,
	Decimal,
	String,
	Short_List
}

public class Pair{
	public int first;
	public int second;
	public int third;
	public int forth;
}

public enum Command
{
    Unsigned = 0x00,

    C2M_CHANGE_NICK = 0x10,
    C2M_PING = 0x01,
    C2M_SERIAL_PONG = 0x02,
    C2M_PLAY = 0x11,
    C2M_HUB_READY = 0x12,
    C2M_GAME_READY = 0x13,
    C2M_PUT_SKILLPOINT = 0x20,

    M2C_PONG = 0x81,
    M2C_WELCOME = 0x83,
	M2C_HUB_LIST = 0x90,
    M2C_START_GAME = 0x91,
    M2C_UPDATE_BOARD = 0xA0,
    M2C_TURN_START = 0xA1,
    M2C_GAIN_SKILLPOINT = 0xA2
}

public class Packet{
	public int serial;
	public Command command;
	public int[] datas;
	public float[] f_datas;
	public string[] s_datas;
	public List<int>[] l_datas;
	public byte[] b_datas;

	public Packet(){

	}

	// 輸入數值，並自動建立bytes[]
	public Packet(Command c, int[] d, float[] f_d, string[] s_d, List<int>[] l_d){	new_Packet (c, d			, f_d			, s_d			, l_d				);}
	public Packet(Command c, int[] d, float[] f_d              , List<int>[] l_d){	new_Packet (c, d			, f_d			, new string[0]	, l_d				);}
	public Packet(Command c, int[] d,              string[] s_d, List<int>[] l_d){	new_Packet (c, d			, new float[0]	, s_d			, l_d				);}
	public Packet(Command c, int[] d                           , List<int>[] l_d){	new_Packet (c, d			, new float[0]	, new string[0]	, l_d				);}
	public Packet(Command c,          float[] f_d, string[] s_d, List<int>[] l_d){	new_Packet (c, new int[0]	, f_d			, s_d			, l_d				);}
	public Packet(Command c,          float[] f_d              , List<int>[] l_d){	new_Packet (c, new int[0]	, f_d			, new string[0]	, l_d				);}
	public Packet(Command c,					   string[] s_d, List<int>[] l_d){	new_Packet (c, new int[0]	, new float[0]	, s_d			, l_d				);}
	public Packet(Command c                      	    	   , List<int>[] l_d){	new_Packet (c, new int[0]	, new float[0]	, new string[0]	, l_d				);}
	public Packet(Command c, int[] d, float[] f_d, string[] s_d                 ){	new_Packet (c, d			, f_d			, s_d			, new List<int>[0]	);}
	public Packet(Command c, int[] d, float[] f_d                               ){	new_Packet (c, d			, f_d			, new string[0]	, new List<int>[0]	);}
	public Packet(Command c, int[] d,              string[] s_d                 ){	new_Packet (c, d			, new float[0]	, s_d			, new List<int>[0]	);}
	public Packet(Command c, int[] d                                            ){	new_Packet (c, d			, new float[0]	, new string[0]	, new List<int>[0]	);}
	public Packet(Command c,          float[] f_d, string[] s_d                 ){	new_Packet (c, new int[0]	, f_d			, s_d			, new List<int>[0]	);}
	public Packet(Command c,          float[] f_d                               ){	new_Packet (c, new int[0]	, f_d			, new string[0]	, new List<int>[0]	);}
	public Packet(Command c,					   string[] s_d                 ){	new_Packet (c, new int[0]	, new float[0]	, s_d			, new List<int>[0]	);}
	public Packet(Command c                      	    	                    ){	new_Packet (c, new int[0]	, new float[0]	, new string[0]	, new List<int>[0]	);}
    public Packet(int s){
        serial = s;
        command = Command.C2M_SERIAL_PONG;
        datas = new int[0];
        f_datas = new float[0];
        s_datas = new string[0];
        l_datas = new List<int>[0];
        b_datas = Generate_b_datas();
    }

	public void new_Packet(Command c, int[] d, float[] f_d, string[] s_d, List<int>[] l_d){
		serial = 0;
		command = c;
		datas = d;
		f_datas = f_d;
		s_datas = s_d;
		l_datas = l_d;
		b_datas = Generate_b_datas();
	}

	public Packet(byte[] b_d){
		b_datas = b_d;
		serial = b_d [0];
		command = (Command)b_d[1];
		Data_Type[] slices = Get_Packet_Slices ();
		Pair pair = Get_Slices_Length (slices);
		datas = new int[pair.first];
		f_datas = new float[pair.second];
		s_datas = new string[pair.third];
		l_datas = new List<int>[pair.forth];

		int d_pointer = 0;
		int f_d_pointer = 0;
		int s_d_pointer = 0;
		int l_d_pointer = 0;
		int b_d_pointer = 2;
		for(int i=0;i<slices.Length;i++){
			int num = 0; 
			byte[] bytes = new byte[0];
			switch (slices [i]) {
			case Data_Type.Byte:
				num = b_d [b_d_pointer++];
				datas [d_pointer++] = (num >= 128) ? num - 256 : num;
				break;
			case Data_Type.Short:
				num =  b_d [b_d_pointer++];
				num += b_d [b_d_pointer++] * 256;
				datas [d_pointer++] = (num >= 32768) ? num - 65536 : num;
				break;
			case Data_Type.Unsigned_Short:
				num =  b_d [b_d_pointer++];
				num += b_d [b_d_pointer++] * 256;
				datas [d_pointer++] = num;
				break;
			case Data_Type.SP_Float:
				bytes = new byte[4];
				bytes [0] = b_d [b_d_pointer++];
				bytes [1] = b_d [b_d_pointer++];
				bytes [2] = b_d [b_d_pointer++];
				bytes [3] = b_d [b_d_pointer++];
				f_datas [f_d_pointer++] = Bytes_To_SP_Float (bytes);
				break;
			case Data_Type.String:
				num =  b_d [b_d_pointer++];
				num += b_d [b_d_pointer++] * 256;
				bytes = new byte[num];
				for (int j = 0; j < num; j++) bytes [j] = b_d [b_d_pointer++];
				s_datas [s_d_pointer++] = System.Text.Encoding.ASCII.GetString (bytes);
				break;
			case Data_Type.Short_List:
				num = b_d [b_d_pointer++];
				List<int> l = new List<int> ();
				for (int j = 0; j < num; j++) {
					int temp =  b_d [b_d_pointer++];
					temp += b_d [b_d_pointer++] * 256;
					l.Add (temp);
				}
				l_datas [l_d_pointer++] = l;
				break;
			default:
				break;
			}
		}

	}

	public byte[] Generate_b_datas(){
		Data_Type[] slices = Get_Packet_Slices ();

		if (slices.Length != (datas.Length + f_datas.Length + s_datas.Length + l_datas.Length)) {
			N_Print ("封包長度或參數數量錯誤\n" + command.ToString() + "\n" + datas.Length.ToString() + " "  + f_datas.Length.ToString() + " "  + s_datas.Length.ToString() + " "  + l_datas.Length.ToString() + " " );
			return null;
		}

		int data_length = Get_Bytes_Amount (slices, s_datas, l_datas);
		byte[] ret = new byte[2 + data_length];
		ret[0] = System.Convert.ToByte(serial);
		ret[1] = System.Convert.ToByte((int)command);

		int d_pointer = 0;
		int f_d_pointer = 0;
		int s_d_pointer = 0;
		int l_d_pointer = 0;
		int b_d_pointer = 2;
		for(int i=0;i<slices.Length;i++){
			int num = 0;
			float f = 0.0f;
			string s = "";
			Byte[] temp_bytes;
			List<int> l;
			switch (slices [i]) {
			case Data_Type.Byte:
				num = datas [d_pointer] + ((datas [d_pointer] >= 0) ? 0 : 256);
				d_pointer++;
				ret [b_d_pointer++] = System.Convert.ToByte (num);
				break;
			case Data_Type.Short:
				num = datas [d_pointer] + ((datas [d_pointer] >= 0) ? 0 : 65536);
				d_pointer++;
				ret [b_d_pointer++] = System.Convert.ToByte (num % 256);
				ret [b_d_pointer++] = System.Convert.ToByte (num / 256);
				break;
			case Data_Type.Unsigned_Short:
				num = datas [d_pointer++];
				ret [b_d_pointer++] = System.Convert.ToByte (num % 256);
				ret [b_d_pointer++] = System.Convert.ToByte (num / 256);
				break;
			case Data_Type.SP_Float:
				f = f_datas [f_d_pointer++];
				temp_bytes = SP_Float_To_Bytes (f);
				ret [b_d_pointer++] = temp_bytes [0];
				ret [b_d_pointer++] = temp_bytes [1];
				ret [b_d_pointer++] = temp_bytes [2];
				ret [b_d_pointer++] = temp_bytes [3];
				break;
			case Data_Type.String:
				s = s_datas [s_d_pointer++];
				temp_bytes = System.Text.Encoding.ASCII.GetBytes (s);
				num = s.Length;
				ret [b_d_pointer++] = System.Convert.ToByte (num % 256);
				ret [b_d_pointer++] = System.Convert.ToByte (num / 256);
				for (int j = 0; j < s.Length; j++) ret [b_d_pointer++] = temp_bytes [j];
				break;
			case Data_Type.Short_List:
				l = l_datas [l_d_pointer++];
				ret [b_d_pointer++] = System.Convert.ToByte (l.Count);
				for (int j = 0; j < l.Count; j++) {
					num = l [j];
					ret [b_d_pointer++] = System.Convert.ToByte (num % 256);
					ret [b_d_pointer++] = System.Convert.ToByte (num / 256);
				}
				break;
			default:
				break;
			}
		}
		return ret;
	}

	public Pair Get_Slices_Length(Data_Type[] slices){
		Pair ret = new Pair ();
		foreach (Data_Type slice in slices) {
			switch (Decide_NUM (slice)) {
			case NUM.Integer:		ret.first++;	break;
			case NUM.Decimal:		ret.second++;	break;
			case NUM.String:		ret.third++;	break;
			case NUM.Short_List:	ret.forth++;	break;
			}
		}
		return ret;
	}

	public int Get_Bytes_Amount(Data_Type[] slices, string[] s_s, List<int>[] l_s){
		int ret = 0;
		foreach (Data_Type slice in slices) {
			switch (slice) {
			case Data_Type.Byte:			ret += 1;	break;
			case Data_Type.Short:			ret += 2;	break;
			case Data_Type.Unsigned_Short:	ret += 2;	break;
			case Data_Type.SP_Float:		ret += 4;	break;
			case Data_Type.String:			ret += 2;	break;
			case Data_Type.Short_List:		ret += 1;	break;
			}
		}
		foreach (string s in s_s) ret += s.Length;
		foreach (List<int> s in l_s) ret += s.Count * 2;
		return ret;
	}

	NUM Decide_NUM(Data_Type data_Type){
		switch (data_Type) {
		case Data_Type.Byte:			return NUM.Integer;
		case Data_Type.Short:			return NUM.Integer;
		case Data_Type.Unsigned_Short:	return NUM.Integer;
		case Data_Type.SP_Float:		return NUM.Decimal;
		case Data_Type.String:			return NUM.String;
		case Data_Type.Short_List:		return NUM.Short_List;
		}
		return NUM.Integer;
	}

	Data_Type[] Get_Packet_Slices(){
		switch (command) {
            case Command.C2M_PING:              return new Data_Type[1] { Data_Type.Byte };
            case Command.C2M_SERIAL_PONG:	    return new Data_Type[0];
            case Command.C2M_CHANGE_NICK:       return new Data_Type[1] { Data_Type.String };
            case Command.C2M_PLAY:              return new Data_Type[0];
            case Command.C2M_HUB_READY:         return new Data_Type[0];
            case Command.C2M_GAME_READY:        return new Data_Type[0];
            case Command.C2M_PUT_SKILLPOINT:    return new Data_Type[1]{ Data_Type.Byte };
                
            case Command.M2C_PONG:              return new Data_Type[1] { Data_Type.Byte };
            case Command.M2C_WELCOME:           return new Data_Type[1] { Data_Type.Byte };
            case Command.M2C_HUB_LIST:          return new Data_Type[1] { Data_Type.String };
            case Command.M2C_START_GAME:        return new Data_Type[1] { Data_Type.Byte };
            case Command.M2C_UPDATE_BOARD:      return new Data_Type[9] { Data_Type.String, Data_Type.String, Data_Type.String, Data_Type.String, Data_Type.Short_List, Data_Type.Short_List, Data_Type.Short_List, Data_Type.Short_List, Data_Type.Short_List };
            case Command.M2C_TURN_START:        return new Data_Type[1] { Data_Type.Byte };
            case Command.M2C_GAIN_SKILLPOINT:   return new Data_Type[1] { Data_Type.Byte };
		    default:
			    break;
		}
		N_Print ("Command type not found");
		return null;
	}

	public void Print(string s){
		if (datas == null || b_datas == null || f_datas == null || s_datas == null || l_datas == null) {
			N_Print ("datas == null || b_datas == null || f_datas == null || s_datas == null || l_datas == null");
			return;
		}

		string d_string = "";
		Data_Type[] data_Types = Get_Packet_Slices ();
		int d_pointer = 0;
		int f_d_pointer = 0;
		int s_d_pointer = 0;
		int l_d_pointer = 0;
		foreach (Data_Type data_Type in data_Types) {
            if (d_pointer + f_d_pointer + s_d_pointer + l_d_pointer > 0) d_string += ",";

			switch (Decide_NUM (data_Type)) {
			case NUM.Integer:	
				d_string += datas [d_pointer++].ToString ();
				break;
			case NUM.Decimal:
				d_string += f_datas [f_d_pointer++].ToString ();
				break;
			case NUM.String:
				d_string += s_datas [s_d_pointer++];
				break;
			case NUM.Short_List:
				d_string += "[";
				List<int> l = l_datas [l_d_pointer++];
				for (int j = 0; j < l.Count; j++) {
					if (j > 0) d_string += ",";
					d_string += l [j].ToString ();
				}
				d_string += "]";
				break;
			}
		}
		string b_d_string = "";
		foreach (byte b_d in b_datas)b_d_string += String.Format ("{0:X2}", b_d) + " ";
		N_Print(s + "\nserial: " + serial.ToString() + "\nCommand: " + command + "\ndatas: " + d_string + "\nb_datas: " + b_d_string);
	}

	private void N_Print(string s){
		Debug.Log (s);
	}

	float Bytes_To_SP_Float(byte[] bytes){
		MemoryStream stream = new MemoryStream();
		BinaryReader br = new BinaryReader (stream);
		br.Read (bytes, 0, 4);
		return BitConverter.ToSingle (bytes , 0);
	}

	byte[] SP_Float_To_Bytes(float f){
		MemoryStream stream = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(stream);
		bw.Write(f);
		bw.Flush();
		return stream.ToArray();
	}
}