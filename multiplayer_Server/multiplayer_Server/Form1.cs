using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using moveData;
using packet;
using ChineseDarkChess;

namespace multiplayer_Server {
    public partial class serverGUI : Form {

        private static byte[] result = new byte[2048];
        private static int myProt = 8885;   //埠
        static Socket serverSocket;
        private delegate void DelUpdateMessage(string sMessage);
        private delegate void DelUpdateSocket(Socket s, int c);
        private delegate void DelPacketAnalyze(Packet pkt);
        private delegate void DelGUIUpdate(int c, string s);
        

        Socket player1 = null;
        Socket player2 = null;
        int connectionNumber = 0;

        bool gameStart;
        Packet gameStartHold = null;
        DarkChessModel darkChessModel = null;



        public serverGUI() {
            InitializeComponent();
            GUIUpdate(0);

            this.FormClosing += new FormClosingEventHandler(MainForm_Closing);
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, myProt));  //繫結IP地址：埠
            serverSocket.Listen(2);    //設定最多2個排隊連線請求
            //通過Clientsoket傳送資料
            Thread listening = new Thread(ListenClientConnect);
            listening.Start();
            updateMessage("Server Initialize Complete");
            GUIUpdate(1);
            gameStart = false;

        }

        private void ListenClientConnect() {
            while (true) {
                Socket clientSocket = serverSocket.Accept();
                if (connectionNumber >= 2) {
                    clientSocket.Send(Encoding.ASCII.GetBytes("This room is full"));
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    continue;
                }
                connectionNumber++;

                //clientSocket.Send(Encoding.ASCII.GetBytes("Server Say Hello"));
                updateMessage(IPAddress.Parse(((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString()) + "joined the server ");
                setPlayer(clientSocket, 1);
                //broadcast(new Packet(Command.FLIP, new MoveData(0, 0, 0, 0)));
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);

                if (player1 != null && player2 != null) {
                    if (!gameStart) {
                        gameStart = true;
                        darkChessModel = new DarkChessModel();
                        darkChessModel.initBoard();
                        darkChessModel.isPlayer1Turn = true;
                        broadcast(null, 2);
                    }
                }
            }
        }

        private void ReceiveMessage(object clientSocket) {
            Socket myClientSocket = (Socket)clientSocket;
            while (true) {
                try {
                    //通過clientSocket接收資料
                    int receiveNumber = myClientSocket.Receive(result);
                    if (receiveNumber == 0) {
                        throw new Exception(String.Format("{0}' has disconnected", myClientSocket.RemoteEndPoint.ToString()));
                    }
                    object obj = Packet.Deserialize(result);
                    if (obj is Packet) {
                        packetAnalyze((Packet)obj);
                    } else {
                        updateMessage(String.Format("接收客戶端 {0} 訊息 {1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.ASCII.GetString(result, 0, receiveNumber)));
                    }
                } catch (Exception ex) {
                    updateMessage(ex.Message);
                    if (myClientSocket.Connected) {
                        myClientSocket.Shutdown(SocketShutdown.Both);
                        myClientSocket.Close();
                    }
                    updateMessage(String.Format("Disconnected both two Player"));
                    setPlayer(null, 2);
                    break;
                }
            }
        }

        private void broadcast(Packet packet, int n) {
            switch (n) {
                case 1:
                    Packet pkt = Packet.Serialize(packet);
                    if (player1 != null)
                        player1.Send(pkt.Data);
                    if (player2 != null)
                        player2.Send(pkt.Data);
                    break;
                case 2:
                    updateMessage(String.Format("All Player Standby GAMESTART!!"));

                    GUIUpdate(4);
                    Packet pkt2 = new Packet(Command.GAME_START, darkChessModel.getBoard(), darkChessModel.redPiecesTaken, darkChessModel.blackPiecesTaken);
                    pkt2.playerStatusChange = true;
                    player1.Send(Packet.Serialize(pkt2).Data);

                    pkt2.playerStatusChange = false;
                    player2.Send(Packet.Serialize(pkt2).Data);
                    break;
            }

        }

        private void packetAnalyze(Packet pkt) {
            if (this.InvokeRequired) // 若非同執行緒
{
                DelPacketAnalyze del = new DelPacketAnalyze(packetAnalyze); //利用委派執行
                this.Invoke(del, pkt);
                //同執行緒
            } else {
                string Player;
                if (darkChessModel.isPlayer1Turn)
                    Player = "Player1";
                else
                    Player = "Player2";

                switch (pkt.command) {
                    case Command.FLIP:
                        MoveData flipPos = pkt.moveData;
                        darkChessModel.flip(flipPos.fromX, flipPos.fromY);

                        updateMessage(String.Format("{0} FLIP a Chess at {1} , {2} ", Player, flipPos.fromY, flipPos.fromY));

                        darkChessModel.isPlayer1Turn = !darkChessModel.isPlayer1Turn;
                        Packet sent = new Packet(Command.UPDATE_BOARD, darkChessModel.getBoard(), darkChessModel.redPiecesTaken, darkChessModel.blackPiecesTaken);

                        if (!darkChessModel.isGameStart) {
                            gameStartHold = sent;
                            bool isPlayer1Black = false;
                            if (darkChessModel.getBoard()[flipPos.fromX, flipPos.fromY] > 0) {
                                isPlayer1Black = true;
                                updateMessage(String.Format("ASSIGN Player1 is Black"));
                                updateMessage(String.Format("ASSIGN Player2 is Red"));
                            } else {
                                isPlayer1Black = false;
                                updateMessage(String.Format("ASSIGN Player1 is Red"));
                                updateMessage(String.Format("ASSIGN Player2 is Black"));
                            }

                            darkChessModel.isGameStart = true;
                            Packet setPlayerColor = new Packet(Command.COLOR_ASSIGN, isPlayer1Black);
                            player1.Send(Packet.Serialize(setPlayerColor).Data);
                            setPlayerColor.playerStatusChange = !setPlayerColor.playerStatusChange;
                            player2.Send(Packet.Serialize(setPlayerColor).Data);
                        }
                        broadcast(sent, 1);
                        break;
                    case Command.MOVE://MOVE
                        bool isMoveValid = darkChessModel.sumbitMove(pkt.moveData);
                        if (isMoveValid) {
                            updateMessage(String.Format("{0} MOVE a Chess from {1} , {2} to {3} , {4} ", Player, pkt.moveData.fromX, pkt.moveData.fromY, pkt.moveData.toX, pkt.moveData.toY));
                            darkChessModel.isPlayer1Turn = !darkChessModel.isPlayer1Turn;
                            broadcast(new Packet(Command.UPDATE_BOARD, darkChessModel.getBoard(), darkChessModel.redPiecesTaken, darkChessModel.blackPiecesTaken), 1);
                        } else {
                            updateMessage(String.Format("{0} MOVE FAIL ", Player));
                            if (darkChessModel.isPlayer1Turn) {
                                player1.Send(Packet.Serialize(new Packet(Command.MOVEFAIL, null)).Data);
                            } else {
                                player2.Send(Packet.Serialize(new Packet(Command.MOVEFAIL, null)).Data);
                            }
                        }
                        if (darkChessModel.isBlackWin()) {
                            updateMessage(String.Format("BLACK is WIN"));
                            GUIUpdate(5, String.Format("BLACK is WIN"));
                            //Broadcast isBlackWin
                            broadcast(new Packet(Command.GAME_RESULT, true), 1);
                        } else if (darkChessModel.isRedWin()) {
                            updateMessage(String.Format("RED is WIN"));
                            GUIUpdate(5, String.Format("RED is WIN"));
                            //Broadcast !isBlackWin
                            broadcast(new Packet(Command.GAME_RESULT, false), 1);
                        }
                        break;
                    case Command.SURRENDER:
                        //Black Surrender
                        if (pkt.playerStatusChange) {
                            updateMessage(String.Format("BLACK is SURRENDER"));
                            GUIUpdate(5, String.Format("BLACK is SURRENDER"));
                            //Broadcast !isBlackWin
                            broadcast(new Packet(Command.GAME_RESULT, false), 1);

                            //Red Surrender
                        } else if (pkt.playerStatusChange) {
                            updateMessage(String.Format("RED is SURRENDER"));
                            GUIUpdate(5, String.Format("RED is SURRENDER"));
                            //Broadcast isBlackWin
                            broadcast(new Packet(Command.GAME_RESULT, true), 1);
                        }
                        break;

                }
            }
        }

        private void updateMessage(string msg) {
            if (this.InvokeRequired) // 若非同執行緒
{
                DelUpdateMessage del = new DelUpdateMessage(updateMessage); //利用委派執行
                this.Invoke(del, msg);
                //同執行緒
            } else {
                ConsoleMessage.AppendText(msg + Environment.NewLine);
            }
        }

        private void setPlayer(Socket player, int c) {
            if (this.InvokeRequired) // 若非同執行緒
{
                DelUpdateSocket del = new DelUpdateSocket(setPlayer); //利用委派執行
                this.Invoke(del, player, c);
                //同執行緒
            } else {
                string playerip = "";
                if (player != null)
                    playerip = IPAddress.Parse(((IPEndPoint)player.RemoteEndPoint).Address.ToString()).ToString();
                switch (c) {
                    case 1:
                        if (player1 == null) {
                            GUIUpdate(2, playerip);
                            player1 = player;
                        } else {
                            GUIUpdate(3, playerip);
                            player2 = player;
                        }
                        break;
                    case 2:
                        if (player1 != null)
                            player1.Close();
                        if (player2 != null)
                            player2.Close();

                        player1 = null;
                        player2 = null;
                        connectionNumber = 0;
                        gameStart = false;
                        darkChessModel = null;
                        GUIUpdate(1);
                        break;

                }

            }
        }
        private void MainForm_Closing(object sender, CancelEventArgs e) {
            setPlayer(null, 2);
            System.Environment.Exit(0);

        }

        private void GUIUpdate(int c, string s = "") {
            if (this.InvokeRequired) { // 若非同執行緒
                DelGUIUpdate del = new DelGUIUpdate(GUIUpdate); //利用委派執行
                this.Invoke(del, c, s);
                //同執行緒
            } else {
                switch (c) {
                    case 0://init GUI
                        GameStatusLabel.Text = "Status : Initializing";
                        PLAYER1IP.Text = "Player1 :";
                        PLAYER2IP.Text = "Player2 :";
                        break;
                    case 1://reset GUI
                        GameStatusLabel.Text = "Status : Waiting For Player";
                        PLAYER1IP.Text = "Player1 :";
                        PLAYER2IP.Text = "Player2 :";
                        break;
                    case 2://set Player1 IP
                        PLAYER1IP.Text = String.Format("PLAYER1 : {0}", s);
                        break;
                    case 3://set Player2 IP
                        PLAYER2IP.Text = String.Format("PLAYER2 : {0}", s);
                        break;
                    case 4://GameStart
                        GameStatusLabel.Text = "Status : Gaming";
                        break;
                    case 5://GameOver
                        GameStatusLabel.Text = "Status : {0}, s";
                        break;

                }
            }
        }
    }
}
