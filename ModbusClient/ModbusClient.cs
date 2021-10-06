using System;
using System.Net.Sockets;

namespace ModbusClient
{
    public class ModbusClient
    {
        private TcpClient tcpClient;
        private string ipAddress = "127.0.0.1";
        private int port = 502;
        private int connectTimeout = 1000;
        private NetworkStream stream;
        private uint transactionIdentifierInternal = 0;
        private byte[] transactionIdentifier = new byte[2];
        private byte[] protocolIdentifier = new byte[2];
        private byte[] startingAddress = new byte[2];
        private byte[] length = new byte[2];
        private byte functionCode;
        private byte[] quantity = new byte[2];
        private byte[] crc = new byte[2];
        private byte unitIdentifier = 1;
        public byte[] receiveData;
        public byte[] sendData;
        public bool connected = false;
        public ModbusClient(string ipAddress, int port)
        {
            AddLog("IPAddress: " + ipAddress + ", Port: " + port.ToString());
            this.ipAddress = ipAddress;
            this.port = port;
        }
        public void AddLog(string text)
        {
            Console.WriteLine(String.Format("[{0}] {1}", DateTime.Now, text));
        }
        public void Connect()
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(ipAddress, port);
            stream = this.tcpClient.GetStream();
            stream.ReadTimeout = this.connectTimeout;
            connected = true;
            AddLog("Connected");
        }
        public void WriteSingleRegister(int startingAddress, int value)
        {
            checked { ++this.transactionIdentifierInternal; }

            if (this.tcpClient == null)
            {
                throw new Exception("connection error");
            }
            byte[] numArray1 = new byte[2];
            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);
            this.length = BitConverter.GetBytes(6);
            this.functionCode = (byte)6;
            this.startingAddress = BitConverter.GetBytes(startingAddress);
            byte[] bytes = BitConverter.GetBytes(value);
            byte[] buffArray = new byte[14]
            {
        this.transactionIdentifier[1],
        this.transactionIdentifier[0],
        this.protocolIdentifier[1],
        this.protocolIdentifier[0],
        this.length[1],
        this.length[0],
        this.unitIdentifier,
        this.functionCode,
        this.startingAddress[1],
        this.startingAddress[0],
        bytes[1],
        bytes[0],
        this.crc[0],
        this.crc[1]
            };
            this.crc = BitConverter.GetBytes(calculateCRC(buffArray, (ushort)6, 6));
            buffArray[12] = this.crc[0];
            buffArray[13] = this.crc[1];
            if (this.tcpClient.Client.Connected)
            {


                this.stream.Write(buffArray, 0, checked(buffArray.Length - 2));

                {
                    byte[] numArray4 = new byte[checked(buffArray.Length - 2)];
                    Array.Copy((Array)buffArray, 0, (Array)numArray4, 0, checked(buffArray.Length - 2));
                    AddLog("Send ModbusTCP-Data: " + BitConverter.ToString(numArray4));

                    buffArray = new byte[2100];
                    int length = this.stream.Read(buffArray, 0, buffArray.Length);

                    this.receiveData = new byte[length];
                    Array.Copy((Array)buffArray, 0, (Array)this.receiveData, 0, length);
                    AddLog("Receive ModbusTCP-Data: " + BitConverter.ToString(this.receiveData));



                }
                if (buffArray[7] == (byte)134 & buffArray[8] == (byte)1)
                {
                    throw new Exception("Function code not supported by master");
                }
                if (buffArray[7] == (byte)134 & buffArray[8] == (byte)2)
                {
                    throw new Exception("Starting address invalid or starting address + quantity invalid");
                }
                if (buffArray[7] == (byte)134 & buffArray[8] == (byte)3)
                {
                    throw new Exception("quantity invalid");
                }
                if (buffArray[7] == (byte)134 & buffArray[8] == (byte)4)
                {
                    throw new Exception("error reading");
                }
            }
        }
        public int[] ReadHoldingRegisters(int startingAddress, int quantity)
        {
            AddLog("FC3 (Read Holding Registers from Master device), StartingAddress: " + startingAddress.ToString() + ", Quantity: " + quantity.ToString());
            checked { ++this.transactionIdentifierInternal; }

            if (this.tcpClient == null)
            {
                throw new Exception("connection error");
            }
            if (startingAddress > (int)ushort.MaxValue | quantity > 125)
            {
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 125");
            }
            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);
            this.length = BitConverter.GetBytes(6);
            this.functionCode = (byte)3;
            this.startingAddress = BitConverter.GetBytes(startingAddress);
            this.quantity = BitConverter.GetBytes(quantity);
            byte[] numArray1 = new byte[14]
            {
                this.transactionIdentifier[1],
                this.transactionIdentifier[0],
                this.protocolIdentifier[1],
                this.protocolIdentifier[0],
                this.length[1],
                this.length[0],
                this.unitIdentifier,
                this.functionCode,
                this.startingAddress[1],
                this.startingAddress[0],
                this.quantity[1],
                this.quantity[0],
                this.crc[0],
                this.crc[1]
            };
            this.crc = BitConverter.GetBytes(ModbusClient.calculateCRC(numArray1, (ushort)6, 6));
            numArray1[12] = this.crc[0];
            numArray1[13] = this.crc[1];
            if (this.tcpClient.Client.Connected)
            {
                stream.Write(numArray1, 0, checked(numArray1.Length - 2));
                numArray1 = new byte[256];
                int length = this.stream.Read(numArray1, 0, numArray1.Length);
                this.receiveData = new byte[length];
                Array.Copy((Array)numArray1, 0, (Array)this.receiveData, 0, length);
                AddLog("Receive ModbusTCP-Data: " + BitConverter.ToString(this.receiveData));
            }
            if (numArray1[7] == (byte)131 & numArray1[8] == (byte)1)
            {
                throw new Exception("Function code not supported by master");
            }
            if (numArray1[7] == (byte)131 & numArray1[8] == (byte)2)
            {
                throw new Exception("Starting address invalid or starting address + quantity invalid");
            }
            if (numArray1[7] == (byte)131 & numArray1[8] == (byte)3)
            {
                throw new Exception("quantity invalid");
            }
            if (numArray1[7] == (byte)131 & numArray1[8] == (byte)4)
            {
                throw new Exception("error reading");
            }

            int[] numArray4 = new int[quantity];
            int index = 0;
            while (index < quantity)
            {
                byte num1 = numArray1[checked(9 + index * 2)];
                byte num2 = numArray1[checked(9 + index * 2 + 1)];
                numArray1[checked(9 + index * 2)] = num2;
                numArray1[checked(9 + index * 2 + 1)] = num1;
                numArray4[index] = (int)BitConverter.ToInt16(numArray1, checked(9 + index * 2));
                checked { ++index; }
            }
            return numArray4;
        }
        public void WriteCoil(int startingAddress, bool value)
        {
            checked { ++this.transactionIdentifierInternal; }

            if (this.tcpClient == null)
            {
                throw new Exception("connection error");
            }
            byte[] numArray1 = new byte[2];
            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);
            this.length = BitConverter.GetBytes(6);
            this.functionCode = (byte)5;
            this.startingAddress = BitConverter.GetBytes(startingAddress);
            byte[] numArray2 = !value ? BitConverter.GetBytes(0) : BitConverter.GetBytes(65280);
            byte[] numArray3 = new byte[14]
            {
        this.transactionIdentifier[1],
        this.transactionIdentifier[0],
        this.protocolIdentifier[1],
        this.protocolIdentifier[0],
        this.length[1],
        this.length[0],
        this.unitIdentifier,
        this.functionCode,
        this.startingAddress[1],
        this.startingAddress[0],
        numArray2[1],
        numArray2[0],
        this.crc[0],
        this.crc[1]
            };
            this.crc = BitConverter.GetBytes(ModbusClient.calculateCRC(numArray3, (ushort)6, 6));
            numArray3[12] = this.crc[0];
            numArray3[13] = this.crc[1];

            if (this.tcpClient.Client.Connected)
            {
                {
                    this.stream.Write(numArray3, 0, checked(numArray3.Length - 2));
                    //if (this.debug)
                    //{
                    //    byte[] numArray5 = new byte[checked(numArray3.Length - 2)];
                    //    Array.Copy((Array)numArray3, 0, (Array)numArray5, 0, checked(numArray3.Length - 2));
                    //    if (this.debug)
                    //        StoreLogData.Instance.Store("Send ModbusTCP-Data: " + BitConverter.ToString(numArray5), DateTime.Now);
                    //}
                    //if (this.SendDataChanged != null)
                    //{
                    //    this.sendData = new byte[checked(numArray3.Length - 2)];
                    //    Array.Copy((Array)numArray3, 0, (Array)this.sendData, 0, checked(numArray3.Length - 2));
                    //    this.SendDataChanged((object)this);
                    //}
                    numArray3 = new byte[2100];
                    int length = this.stream.Read(numArray3, 0, numArray3.Length);

                    this.receiveData = new byte[length];
                    Array.Copy((Array)numArray3, 0, (Array)this.receiveData, 0, length);
                    AddLog("Receive ModbusTCP-Data: " + BitConverter.ToString(this.receiveData));


                }
            }
            if (numArray3[7] == (byte)133 & numArray3[8] == (byte)1)
            {
                throw new Exception("Function code not supported by master");
            }
            if (numArray3[7] == (byte)133 & numArray3[8] == (byte)2)
            {

                throw new Exception("Starting address invalid or starting address + quantity invalid");
            }
            if (numArray3[7] == (byte)133 & numArray3[8] == (byte)3)
            {
                throw new Exception("quantity invalid");
            }
            if (numArray3[7] == (byte)133 & numArray3[8] == (byte)4)
            {
                throw new Exception("error reading");
            }

        }
        public bool[] ReadCoils(int address, int value)
        {
            AddLog("FC1 (Read Coils), StartingAddress: " + address.ToString() + ", Quantity: " + value.ToString());
            checked { ++transactionIdentifierInternal; }
            if (address > ushort.MaxValue | value > 2000)
            {
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
            }
            transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            protocolIdentifier = BitConverter.GetBytes(0);
            length = BitConverter.GetBytes(6);
            functionCode = (byte)1;
            startingAddress = BitConverter.GetBytes(address);
            quantity = BitConverter.GetBytes(value);
            byte[] buffArray = new byte[14]
            {
                transactionIdentifier[1],
                transactionIdentifier[0],
                protocolIdentifier[1],
                protocolIdentifier[0],
                length[1],
                length[0],
                unitIdentifier,
                functionCode,
                startingAddress[1],
                startingAddress[0],
                quantity[1],
                quantity[0],
                crc[0],
                crc[1]
            };
            crc = BitConverter.GetBytes(ModbusClient.calculateCRC(buffArray, (ushort)6, 6));
            buffArray[12] = this.crc[0];
            buffArray[13] = this.crc[1];


            if (tcpClient.Client.Connected)
            {
                {
                    stream.Write(buffArray, 0, checked(buffArray.Length - 2));
                    buffArray = new byte[2100];
                    int length = this.stream.Read(buffArray, 0, buffArray.Length);
                    receiveData = new byte[length];
                    Array.Copy((Array)buffArray, 0, (Array)this.receiveData, 0, length);
                    AddLog("Receive ModbusTCP-Data: " + BitConverter.ToString(this.receiveData));
                }
            }
            if (buffArray[7] == (byte)129 & buffArray[8] == (byte)1)
            {

                throw new Exception("Function code not supported by master");
            }
            if (buffArray[7] == (byte)129 & buffArray[8] == (byte)2)
            {

                throw new Exception("Starting address invalid or starting address + quantity invalid");
            }
            if (buffArray[7] == (byte)129 & buffArray[8] == (byte)3)
            {

                throw new Exception("quantity invalid");
            }
            if (buffArray[7] == (byte)129 & buffArray[8] == (byte)4)
            {

                throw new Exception("error reading");
            }

            bool[] flagArray = new bool[value];
            int index = 0;
            while (index < value)
            {
                int num = (int)buffArray[checked(9 + unchecked(index / 8))];
                int int32 = Convert.ToInt32(Math.Pow(2.0, (double)(index % 8)));
                flagArray[index] = Convert.ToBoolean((num & int32) / int32);
                checked { ++index; }
            }
            return flagArray;
        }
        public static ushort calculateCRC(byte[] data, ushort numberOfBytes, int startByte)
        {
            byte[] numArray1 = new byte[256]
            {
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 1,
        (byte) 192,
        (byte) 128,
        (byte) 65,
        (byte) 0,
        (byte) 193,
        (byte) 129,
        (byte) 64
            };
            byte[] numArray2 = new byte[256]
            {
        (byte) 0,
        (byte) 192,
        (byte) 193,
        (byte) 1,
        (byte) 195,
        (byte) 3,
        (byte) 2,
        (byte) 194,
        (byte) 198,
        (byte) 6,
        (byte) 7,
        (byte) 199,
        (byte) 5,
        (byte) 197,
        (byte) 196,
        (byte) 4,
        (byte) 204,
        (byte) 12,
        (byte) 13,
        (byte) 205,
        (byte) 15,
        (byte) 207,
        (byte) 206,
        (byte) 14,
        (byte) 10,
        (byte) 202,
        (byte) 203,
        (byte) 11,
        (byte) 201,
        (byte) 9,
        (byte) 8,
        (byte) 200,
        (byte) 216,
        (byte) 24,
        (byte) 25,
        (byte) 217,
        (byte) 27,
        (byte) 219,
        (byte) 218,
        (byte) 26,
        (byte) 30,
        (byte) 222,
        (byte) 223,
        (byte) 31,
        (byte) 221,
        (byte) 29,
        (byte) 28,
        (byte) 220,
        (byte) 20,
        (byte) 212,
        (byte) 213,
        (byte) 21,
        (byte) 215,
        (byte) 23,
        (byte) 22,
        (byte) 214,
        (byte) 210,
        (byte) 18,
        (byte) 19,
        (byte) 211,
        (byte) 17,
        (byte) 209,
        (byte) 208,
        (byte) 16,
        (byte) 240,
        (byte) 48,
        (byte) 49,
        (byte) 241,
        (byte) 51,
        (byte) 243,
        (byte) 242,
        (byte) 50,
        (byte) 54,
        (byte) 246,
        (byte) 247,
        (byte) 55,
        (byte) 245,
        (byte) 53,
        (byte) 52,
        (byte) 244,
        (byte) 60,
        (byte) 252,
        (byte) 253,
        (byte) 61,
        byte.MaxValue,
        (byte) 63,
        (byte) 62,
        (byte) 254,
        (byte) 250,
        (byte) 58,
        (byte) 59,
        (byte) 251,
        (byte) 57,
        (byte) 249,
        (byte) 248,
        (byte) 56,
        (byte) 40,
        (byte) 232,
        (byte) 233,
        (byte) 41,
        (byte) 235,
        (byte) 43,
        (byte) 42,
        (byte) 234,
        (byte) 238,
        (byte) 46,
        (byte) 47,
        (byte) 239,
        (byte) 45,
        (byte) 237,
        (byte) 236,
        (byte) 44,
        (byte) 228,
        (byte) 36,
        (byte) 37,
        (byte) 229,
        (byte) 39,
        (byte) 231,
        (byte) 230,
        (byte) 38,
        (byte) 34,
        (byte) 226,
        (byte) 227,
        (byte) 35,
        (byte) 225,
        (byte) 33,
        (byte) 32,
        (byte) 224,
        (byte) 160,
        (byte) 96,
        (byte) 97,
        (byte) 161,
        (byte) 99,
        (byte) 163,
        (byte) 162,
        (byte) 98,
        (byte) 102,
        (byte) 166,
        (byte) 167,
        (byte) 103,
        (byte) 165,
        (byte) 101,
        (byte) 100,
        (byte) 164,
        (byte) 108,
        (byte) 172,
        (byte) 173,
        (byte) 109,
        (byte) 175,
        (byte) 111,
        (byte) 110,
        (byte) 174,
        (byte) 170,
        (byte) 106,
        (byte) 107,
        (byte) 171,
        (byte) 105,
        (byte) 169,
        (byte) 168,
        (byte) 104,
        (byte) 120,
        (byte) 184,
        (byte) 185,
        (byte) 121,
        (byte) 187,
        (byte) 123,
        (byte) 122,
        (byte) 186,
        (byte) 190,
        (byte) 126,
        (byte) 127,
        (byte) 191,
        (byte) 125,
        (byte) 189,
        (byte) 188,
        (byte) 124,
        (byte) 180,
        (byte) 116,
        (byte) 117,
        (byte) 181,
        (byte) 119,
        (byte) 183,
        (byte) 182,
        (byte) 118,
        (byte) 114,
        (byte) 178,
        (byte) 179,
        (byte) 115,
        (byte) 177,
        (byte) 113,
        (byte) 112,
        (byte) 176,
        (byte) 80,
        (byte) 144,
        (byte) 145,
        (byte) 81,
        (byte) 147,
        (byte) 83,
        (byte) 82,
        (byte) 146,
        (byte) 150,
        (byte) 86,
        (byte) 87,
        (byte) 151,
        (byte) 85,
        (byte) 149,
        (byte) 148,
        (byte) 84,
        (byte) 156,
        (byte) 92,
        (byte) 93,
        (byte) 157,
        (byte) 95,
        (byte) 159,
        (byte) 158,
        (byte) 94,
        (byte) 90,
        (byte) 154,
        (byte) 155,
        (byte) 91,
        (byte) 153,
        (byte) 89,
        (byte) 88,
        (byte) 152,
        (byte) 136,
        (byte) 72,
        (byte) 73,
        (byte) 137,
        (byte) 75,
        (byte) 139,
        (byte) 138,
        (byte) 74,
        (byte) 78,
        (byte) 142,
        (byte) 143,
        (byte) 79,
        (byte) 141,
        (byte) 77,
        (byte) 76,
        (byte) 140,
        (byte) 68,
        (byte) 132,
        (byte) 133,
        (byte) 69,
        (byte) 135,
        (byte) 71,
        (byte) 70,
        (byte) 134,
        (byte) 130,
        (byte) 66,
        (byte) 67,
        (byte) 131,
        (byte) 65,
        (byte) 129,
        (byte) 128,
        (byte) 64
            };
            ushort num1 = numberOfBytes;
            byte maxValue = byte.MaxValue;
            byte num2 = byte.MaxValue;
            int num3 = 0;
            while (num1 > (ushort)0)
            {
                checked { --num1; }
                if (checked(num3 + startByte) < data.Length)
                {
                    int index = (int)num2 ^ (int)data[checked(num3 + startByte)];
                    num2 = checked((byte)((int)maxValue ^ (int)numArray1[index]));
                    maxValue = numArray2[index];
                }
                checked { ++num3; }
            }
            return checked((ushort)((int)maxValue << 8 | (int)num2));
        }
        public void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (tcpClient != null)
                tcpClient.Close();
            connected = false;
        }
    }
}