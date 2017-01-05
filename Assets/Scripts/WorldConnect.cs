using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace EQBrowser {

	public partial class WorldConnect : MonoBehaviour
	{  
		public static WorldConnect instance;
		public GameObject WorldConnectObject;
		public GameObject CharSelect;
		public GameObject CharSelectCamera;
		public GameObject UIScriptsObject;
		public GameObject GameCamera;
		public GameObject ChatText;
		public GameObject EqemuConnectObject;
		public UIScripts UIScript;
		public CharacterSelect CSel;
//		public GoogleAnalyticsV4 googleAnalytics;
//		public LootScript LootS;
		public Text ChatText2;
		public GameObject NullGameObject;
		public int isAttacking;

        //No ping yet, but we can keep track of packets per second to see if connection is moving.
        public static int packetsLatestSecond;
        public static long packetsUnhandledType;
        public static long packetsTotal;

		public void Awake() 
		{
			if(instance)
			DestroyImmediate(WorldConnectObject);
			else
			{
				DontDestroyOnLoad(WorldConnectObject);
				instance = this;
			}
		}

		
		public bool Connected;
		public bool AttemptingZoneConnect;
		public bool isTyping;
		public bool isLooting;
		public bool isDead;
		public bool initXP = false;
		public bool playerLock = true;
		public string ourPlayerName;
		public Int32 curZoneId = -1;
		public Int32 curInstanceId = -1;
		public Int32 OurEntityID = 0;
		public Int32 OurTargetID;
		public Int32 OurTargetLootID;
		public string cursorItemName;
		public Int32 cursorIconId = 0;
		public Int32 cursorSlotId = 0;
		public WebSocket ws_;
		public byte[] userNamePass;
        public string ourUsername;
        public string ourPassword;
		
		public int char0zone;
		public int char1zone;
		public int char2zone;
		public int char3zone;
		public int char4zone;
		public int char5zone;
		public int char6zone;
		public int char7zone;

		delegate void OpcodeFunc (byte[] data, int datasize, bool fromWorld);
		//Use this for initialization
		
		public Byte ReadInt8(byte[] data, ref Int32 position)
		{
			byte retval = data[position];
			position += 1;
			return retval;
		}

		public Int16 ReadInt16(byte[] data, ref Int32 position)
		{
			Int16 retval = BitConverter.ToInt16(data, position);
			position += 2;
			return retval;
		}

		public Int32 ReadInt32(byte[] data, ref Int32 position)
		{
			Int32 retval = BitConverter.ToInt32 (data, position);
			position += 4;
			return retval;
		}

		public string ReadFixedLengthString(byte[] data, ref Int32 position, Int32 count)
		{
			string retval = System.Text.Encoding.Default.GetString(data, position, count);
			position += count;
			return retval;
		}

		public byte[] ReadFixedLengthByteArray(byte[] data, ref Int32 position, Int32 count)
		{
			byte[] retval = new byte[count];
			Buffer.BlockCopy (data, position, retval, 0, count);
			position += count;
			return retval;
		}
		
		
		public void WriteInt8(byte value, ref byte[] data, ref Int32 position)
		{
			Buffer.BlockCopy (BitConverter.GetBytes(value), 0, data, position, 1);
			position += 1;
			return;
		}
		
		public void WriteInt16(Int16 value, ref byte[] data, ref Int32 position)
		{
			Buffer.BlockCopy (BitConverter.GetBytes(value), 0, data, position, 2);
			position += 2;
			return;
		}
		
		public void WriteInt32(Int32 value, ref byte[] data, ref Int32 position)
		{
			Buffer.BlockCopy (BitConverter.GetBytes(value), 0, data, position, 4);
			position += 4;
			return;
		}
		
		public void WriteFloat(float value, ref byte[] data, ref Int32 position)
		{
			Buffer.BlockCopy (BitConverter.GetBytes(value), 0, data, position, 4);
			position += 4;
			return;
		}
		
		public void WriteFixedLengthString(string inString, ref byte[] data, ref Int32 position, Int32 count)
		{
			Buffer.BlockCopy (Encoding.UTF8.GetBytes(inString), 0, data, position, Encoding.UTF8.GetBytes(inString).Length);
			position += count;
			return;
		}
		
		public void WriteFixedLengthByteArray(byte[] inArray, ref byte[] data, ref Int32 position, Int32 count)
		{
			Buffer.BlockCopy (inArray, 0, data, position, inArray.Length);
			position += count;
			return;
		}

		Dictionary<string, OpcodeFunc> opcodeDict;
        Dictionary<OpCode, OpcodeFunc> opCodes;

				
		void Start()
	    {
//			googleAnalytics.StartSession();
			instance = this;

			opcodeDict = new Dictionary<string, OpcodeFunc>();//for testing
			opcodeDict.Add ("423", HandleWorldMessage_SendCharInfo);
			opcodeDict.Add ("36", HandleWorldMessage_ApproveName);
			opcodeDict.Add ("548", HandleWorldMessage_ZoneUnavailable);
			opcodeDict.Add ("545", HandleWorldMessage_ZoneServerInfo);
			opcodeDict.Add ("365", HandleWorldMessage_PlayerProfile);
			opcodeDict.Add ("338", HandleWorldMessage_NewZone);
			opcodeDict.Add ("546", HandleWorldMessage_ZoneServerReady);
			opcodeDict.Add ("549", HandleWorldMessage_EmuKeepAlive);
			opcodeDict.Add ("550", HandleWorldMessage_EmuKeepAlive);
			opcodeDict.Add ("551", HandleWorldMessage_EmuRequestClose);
			opcodeDict.Add ("296", HandleWorldMessage_LogOutReply);
			opcodeDict.Add ("424", HandleWorldMessage_SendExpZonein);
			opcodeDict.Add ("547", HandleWorldMessage_ZoneSpawns);
			opcodeDict.Add ("336", HandleWorldMessage_ZoneSpawns);
			opcodeDict.Add ("539", HandleWorldMessage_ZoneChange);
			opcodeDict.Add ("116", HandleWorldMessage_DeleteSpawn);
			opcodeDict.Add ("69", HandleWorldMessage_ChannelMessage);
			opcodeDict.Add ("87", HandleWorldMessage_ClientUpdate);
			opcodeDict.Add ("458", HandleWorldMessage_SimpleMessage);
			opcodeDict.Add ("168", HandleWorldMessage_FormattedMessage);
			opcodeDict.Add ("109", HandleWorldMessage_Damage);
			opcodeDict.Add ("242", HandleWorldMessage_HPUpdate);
			opcodeDict.Add ("541", HandleWorldMessage_ZoneEntryInfo);
			opcodeDict.Add ("366", HandleWorldMessage_PlayerStateAdd);
			opcodeDict.Add ("367", HandleWorldMessage_PlayerStateRemove);
			opcodeDict.Add ("323", HandleWorldMessage_MobHealth);
			opcodeDict.Add ("154", HandleWorldMessage_ExpUpdate);
			opcodeDict.Add ("110", HandleWorldMessage_Death);
			opcodeDict.Add ("465", HandleWorldMessage_SpawnAppearance);
			opcodeDict.Add ("52", HandleWorldMessage_BecomeCorpse);
			opcodeDict.Add ("544", HandleWorldMessage_ZonePlayerToBind);
			opcodeDict.Add ("257", HandleWorldMessage_ItemPacket);
			opcodeDict.Add ("327", HandleWorldMessage_MoneyOnCorpse);
			opcodeDict.Add ("329", HandleWorldMessage_WorldMOTD);
			opcodeDict.Add ("72", HandleWorldMessage_ItemPacket);
            opcodeDict.Add("151", HandleWorldMessage_EnterWorld);

            opCodes = new Dictionary<OpCode, OpcodeFunc>();
            opCodes.Add(OpCode.OP_SendCharInfo, HandleWorldMessage_SendCharInfo);
            opCodes.Add(OpCode.OP_ApproveName, HandleWorldMessage_ApproveName);
            opCodes.Add(OpCode.OP_ZoneUnavail, HandleWorldMessage_ZoneUnavailable);
            opCodes.Add(OpCode.OP_ZoneServerInfo, HandleWorldMessage_ZoneServerInfo);
            opCodes.Add(OpCode.OP_PlayerProfile, HandleWorldMessage_PlayerProfile);
            opCodes.Add(OpCode.OP_NewZone, HandleWorldMessage_NewZone);
            opCodes.Add(OpCode.OP_ZoneServerReady, HandleWorldMessage_ZoneServerReady);
            opCodes.Add(OpCode.OP_ResetAA, HandleWorldMessage_EmuKeepAlive);
            opCodes.Add(OpCode.OP_EmuKeepAlive, HandleWorldMessage_EmuKeepAlive);
            opCodes.Add(OpCode.OP_EmuRequestClose, HandleWorldMessage_EmuRequestClose);
            opCodes.Add(OpCode.OP_LogoutReply, HandleWorldMessage_LogOutReply);
            opCodes.Add(OpCode.OP_SendExpZonein, HandleWorldMessage_SendExpZonein);
            opCodes.Add(OpCode.OP_ZoneSpawns, HandleWorldMessage_ZoneSpawns);
            opCodes.Add(OpCode.OP_NewSpawn, HandleWorldMessage_ZoneSpawns);
            opCodes.Add(OpCode.OP_ZoneChange, HandleWorldMessage_ZoneChange);
            opCodes.Add(OpCode.OP_DeleteSpawn, HandleWorldMessage_DeleteSpawn);
            opCodes.Add(OpCode.OP_ChannelMessage, HandleWorldMessage_ChannelMessage);
            opCodes.Add(OpCode.OP_ClientUpdate, HandleWorldMessage_ClientUpdate);
            opCodes.Add(OpCode.OP_SimpleMessage, HandleWorldMessage_SimpleMessage);
            opCodes.Add(OpCode.OP_FormattedMessage, HandleWorldMessage_FormattedMessage);
            opCodes.Add(OpCode.OP_Damage, HandleWorldMessage_Damage);
            opCodes.Add(OpCode.OP_HPUpdate, HandleWorldMessage_HPUpdate);
            opCodes.Add(OpCode.OP_ZoneEntry, HandleWorldMessage_ZoneEntryInfo);
            opCodes.Add(OpCode.OP_PlayerStateAdd, HandleWorldMessage_PlayerStateAdd);
            opCodes.Add(OpCode.OP_PlayerStateRemove, HandleWorldMessage_PlayerStateRemove);
            opCodes.Add(OpCode.OP_MobHealth, HandleWorldMessage_MobHealth);
            opCodes.Add(OpCode.OP_ExpUpdate, HandleWorldMessage_ExpUpdate);
            opCodes.Add(OpCode.OP_Death, HandleWorldMessage_Death);
            opCodes.Add(OpCode.OP_SpawnAppearance, HandleWorldMessage_SpawnAppearance);
            opCodes.Add(OpCode.OP_BecomeCorpse, HandleWorldMessage_BecomeCorpse);
            opCodes.Add(OpCode.OP_ZonePlayerToBind, HandleWorldMessage_ZonePlayerToBind);
            opCodes.Add(OpCode.OP_ItemPacket, HandleWorldMessage_ItemPacket);
            opCodes.Add(OpCode.OP_MoneyOnCorpse, HandleWorldMessage_MoneyOnCorpse);
            opCodes.Add(OpCode.OP_MOTD, HandleWorldMessage_WorldMOTD);
            opCodes.Add(OpCode.OP_CharInventory, HandleWorldMessage_ItemPacket);
            opCodes.Add(OpCode.OP_EnterWorld, HandleWorldMessage_EnterWorld);



            //Auto-Connect to Salty Server
            //			StartCoroutine(ConnectToWebSocketServer("158.69.221.200", "aksdjlka23ij3l1j23lk1j23j123jkjql", "XLOGINX", "XPASSWORDX"));

        }

	    public class Auth
	    {
	        public string id;
	        public string method;
	        public string[] @params;
		}

		[Serializable]
		public class OpcodeFromServerClass
		{
			public string id;
			public string method;
			public string zoneid;
			public string instanceid;
			public string opcode;
			public string datasize;
			public string data;
		}

		[Serializable]
		public class OpcodeToServerClass
		{
			public string id;
			public string method;
			public string[] @params;
			
		}

		[Serializable]		
	    public class CheckIdClass
	    {
	        public static string id;
	        public static string method;
	        public string[] @params;
		}
	
		public IEnumerator ConnectToWebSocketServer(string hostname, string auth, string username, string password)
		{

			if(ws_ != null)
			{
				ws_.Close();
				ws_ = null;
			}

//			string realhost = "ws://" + "158.69.221.200"  + ":80";
//			string realhost = "ws://" + hostname+ ":9080";
			string realhost = "ws://" + hostname;			
			ws_ = new WebSocket(new Uri(realhost));
			yield return StartCoroutine(ws_.Connect());

			Auth auth1 = new Auth();
			auth1.id = "token_auth_id";
			auth1.method = "WebInterface.Authorize";
			auth1.@params = new string[1] { auth };

			string authoutput =  JsonUtility.ToJson(auth1);
			ws_.SendString(authoutput);
			Debug.Log("SendAuth: " + authoutput);

            ourUsername = username;
            ourPassword = password;

            DoSendLoginInfo(ourUsername, ourPassword, 0);
			
			Connected = true;
			yield return 0;
		} 
		
		//This is called assuming we have a connection.
		public string GenerateWorldPacket(int pktsize, short opcode, Int32 zoneid, Int32 instanceid, params byte[][] list)
		{

			byte[] OutPkt = new byte[0];

			if(pktsize > 0)
				OutPkt = new byte[pktsize];

			int cur_pos = 0;

			if (OutPkt.Length > 0) {
				for (int i = 0; i < list.Length; i++) {
					Buffer.BlockCopy (list [i], 0, OutPkt, cur_pos, list [i].Length);
					cur_pos += list [i].Length;
				}
			}
			string base64Packet = "";
			base64Packet = System.Convert.ToBase64String (OutPkt);
			OpcodeToServerClass SerializedPacket = new OpcodeToServerClass();
			SerializedPacket.id = "1337";
			SerializedPacket.method = "World.OpcodeToClient";
			SerializedPacket.@params = new string[5] { zoneid.ToString(), instanceid.ToString (), opcode.ToString(), cur_pos.ToString(), base64Packet };

			string SerializedPacketString = JsonUtility.ToJson(SerializedPacket);
			return SerializedPacketString;


			//Request Entity Positions from server
		}

        public void GenerateAndSendWorldPacket(int pktsize, OpCode opcode, Int32 zoneid, Int32 instanceid, params byte[][] list)
        {
            short opCodeShort = (short)opcode;
            GenerateAndSendWorldPacket(pktsize, opCodeShort, zoneid, instanceid, list);
        }

		public void GenerateAndSendWorldPacket(int pktsize, short opcode, Int32 zoneid, Int32 instanceid, params byte[][] list)
		{
			string serialized = GenerateWorldPacket(pktsize, opcode, zoneid, instanceid, list);
			ws_.SendString(serialized);
//			Debug.Log("SendPacket" + serialized);
		}

		// Update is called once per frame
		void Update ()
		{

			//Establish a world connection.
			if (ws_ != null) {
				
				string reply = ws_.RecvString ();
				if (reply != null) {
					
//					Debug.Log("reply" + reply);
					OpcodeFromServerClass IdChecker1 = JsonUtility.FromJson<OpcodeFromServerClass> (reply);

					if (IdChecker1.id != null && IdChecker1.id == "token_auth_id") {
						if(CSel != null)
							CSel.LoginStatus.text = "Auth successful. Proceeding...";
					}
					if (IdChecker1.method == "Client.Opcode") {
						byte[] RawData = null;
						if (Convert.ToInt32(IdChecker1.datasize) > 0) {
							RawData = System.Convert.FromBase64CharArray (IdChecker1.data.ToCharArray(), 0, IdChecker1.data.Length);
						}

                        OpCode opcodeConverted = (OpCode)short.Parse(IdChecker1.opcode);

                        if (opCodes.ContainsKey(opcodeConverted))
                        {
                            int len = 0;
                            if (RawData != null)
                            {
                                len = RawData.Length;
                            }

                            opCodes[opcodeConverted](RawData, len, IdChecker1.zoneid == "-1" ? true : false);
                            packetsLatestSecond++;
                            packetsTotal++;
                        }
                        else
                        {
                            Debug.LogWarningFormat("Unhandled OpCode: {0}", opcodeConverted);
                            packetsUnhandledType++;
                            packetsTotal++;
                        }

                        //if (opcodeDict.ContainsKey(IdChecker1.opcode))
                        //{
                        //    string RawOp = IdChecker1.opcode;

                        //    int length = 0;
                        //    if (RawData != null)
                        //    {
                        //        length = RawData.Length;
                        //    }

                        //    opcodeDict[RawOp](RawData, length, IdChecker1.zoneid == "-1" ? true : false);
                        //    packetsLatestSecond++;
                        //    packetsTotal++;
                        //}
                        //else
                        //{
                        //    packetsUnhandledType++;
                        //    packetsTotal++;
                        //}
					}
				}
				if (ws_.Error != null) {
					LogError ("Error: " + ws_.Error);
					ws_ = null;
					SceneManager.LoadScene("1 Character creation");
					Destroy (WorldConnectObject);
				}
			}
		}

		public void Log(string message)
		{
			Debug.Log(message);
			Application.ExternalCall("console.log", message);
		}
		
		public void LogError(string error)
		{
			Debug.Log(error);
			Application.ExternalCall("console.log", error);
		}
	}

}