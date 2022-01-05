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

        Socket player1 = null;
        Socket player2 = null;
        int connectionNumber = 0;

        bool gameStart;
        Packet gameStartHold = null;
        DarkChessModel darkChessModel = null;
        


        public serverGUI() {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(MainForm_Closing);
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, myProt));  //繫結IP地址：埠
            serverSocket.Listen(2);    //設定最多2個排隊連線請求
            //通過Clientsoket傳送資料
            Thread listening = new Thread(ListenClientConnect);
            listening.Start();
            updateMessage("Server Initialize Complete");

            gameStart = false;

        }

        private void server_Load(object sender, EventArgs e) {

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
                    if(obj is Packet) {
                        packetAnalyze((Packet)obj);
                    } else {
                        updateMessage(String.Format("接收客戶端 {0} 訊息 {1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.ASCII.GetString(result, 0, receiveNumber)));
                    }
                } catch (Exception ex) {
                    updateMessage(ex.Message);
                    if(myClientSocket.Connected) {
                        myClientSocket.Shutdown(SocketShutdown.Both);
                        myClientSocket.Close();
                    }
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
                switch (pkt.command) {
                    case Command.FLIP:
                        MoveData flipPos = pkt.moveData;
                        darkChessModel.flip(flipPos.fromX, flipPos.fromY);
                        darkChessModel.isPlayer1Turn = !darkChessModel.isPlayer1Turn;
                        Packet sent = new Packet(Command.UPDATE_BOARD, darkChessModel.getBoard(), darkChessModel.redPiecesTaken, darkChessModel.blackPiecesTaken);
                        
                        if (!darkChessModel.isGameStart) {
                            gameStartHold = sent;
                            bool isPlayer1Black = false;
                            if (darkChessModel.getBoard()[flipPos.fromX, flipPos.fromY] > 0) {
                                isPlayer1Black = true;
                            } else {
                                isPlayer1Black = false;
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
                            darkChessModel.isPlayer1Turn = !darkChessModel.isPlayer1Turn;
                            broadcast(new Packet(Command.UPDATE_BOARD, darkChessModel.getBoard(), darkChessModel.redPiecesTaken, darkChessModel.blackPiecesTaken), 1);
                        } else {
                            if (darkChessModel.isPlayer1Turn) {
                                player1.Send(Packet.Serialize(new Packet(Command.MOVEFAIL, null)).Data);
                            } else {
                                player2.Send(Packet.Serialize(new Packet(Command.MOVEFAIL, null)).Data);
                            }
                        }
                        if(darkChessModel.isBlackWin()){
                            //Broadcast isBlackWin
                            broadcast(new Packet(Command.GAME_RESULT, true), 1);
                        } else if (darkChessModel.isRedWin()) {
                            //Broadcast !isBlackWin
                            broadcast(new Packet(Command.GAME_RESULT, false), 1);
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
                switch(c){
                    case 1:
                        if (player1 == null)
                            player1 = player;
                        else
                            player2 = player;
                        break;
                    case 2:
                        if(player1 != null)
                            player1.Close();
                        if (player2 != null)
                            player2.Close();

                        player1 = null;
                        player2 = null;
                        connectionNumber = 0;
                        gameStart = false;
                        darkChessModel = null;
                        break;

                }

            }
        }
        private void MainForm_Closing(object sender, CancelEventArgs e) {
            setPlayer(null, 2);
            System.Environment.Exit(0);

        }
    }
}
