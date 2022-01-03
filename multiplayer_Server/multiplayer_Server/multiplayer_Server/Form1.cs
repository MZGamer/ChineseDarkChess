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

namespace multiplayer_Server {
    public partial class serverGUI : Form {

        private static byte[] result = new byte[2048];
        private static int myProt = 8885;   //埠
        static Socket serverSocket;
        private delegate void DelUpdateMessage(string sMessage);

        private delegate void DelUpdateSocket(Socket s, int c);

        Socket player1 = null;
        Socket player2 = null;
        int connectionNumber = 0;
        


        public serverGUI() {
            InitializeComponent();
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, myProt));  //繫結IP地址：埠
            serverSocket.Listen(2);    //設定最多2個排隊連線請求
            //通過Clientsoket傳送資料
            Thread listening = new Thread(ListenClientConnect);
            listening.Start();
            updateMessage("Server Initialize Complete");
            

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
                broadcast(new Packet(Command.FLIP, new MoveData(0, 0, 0, 0)));
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
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

                    } else {
                        updateMessage(String.Format("接收客戶端 {0} 訊息 {1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.ASCII.GetString(result, 0, receiveNumber)));
                    }
                } catch (Exception ex) {
                    updateMessage(ex.Message);
                    connectionNumber--;
                    if(myClientSocket.Connected) {
                        myClientSocket.Shutdown(SocketShutdown.Both);
                        myClientSocket.Close();
                    }
                    setPlayer(null, 2);
                    break;
                }
            }
        }

        private void broadcast(Packet packet) {
            Packet pkt = Packet.Serialize(packet);
            if (player1 != null)
                player1.Send(pkt.Data);
            object test = Packet.Deserialize(pkt.Data);
            if (player2 != null)
                player2.Send(pkt.Data);
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
                        break;

                }

            }
        }
    }
}
