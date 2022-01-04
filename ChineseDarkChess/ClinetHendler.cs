using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using packet;
using moveData;
using System.ComponentModel;

namespace ChineseDarkChess {
    public partial class Form1 : Form {

        Socket clientSocket;
        private static byte[] result = new byte[2048];
        private delegate void DelPacketAnalyze(Packet pkt);
        private delegate void DelChangeToLocal();
        private bool waitForRespond = false;

        private bool isPlayerBlack;
        private bool isPlayerTurn;
        private bool isPlayer1;
        private int[,] board;

        private void ClinetHendler() {
            Thread receiveThread = new Thread(ReceiveMessage);
            resetButton.Hide();
            receiveThread.Start(clientSocket);
            waitForRespond = true;
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
                    byte[] des = (byte[])result.Clone();
                    Array.Resize(ref des, receiveNumber);
                    object obj = Packet.Deserialize(des);
                    if (obj is Packet) {
                        Packet pkt = (Packet)obj;
                        PacketAnalyze(pkt);
                    } else {
                        Console.WriteLine(String.Format("接收伺服端 {0} 訊息 {1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.ASCII.GetString(result, 0, receiveNumber)));
                    }
                } catch (Exception ex) {
                    if (!myClientSocket.Connected) {
                        Console.WriteLine("連線伺服器失敗，返回單人模式！");
                        changeToLocal();
                        myClientSocket.Shutdown(SocketShutdown.Both);
                        myClientSocket.Close();
                    }
                    break;
                }
            }
        }

        private void changeToLocal() {
            if (this.InvokeRequired) // 若非同執行緒
{
                DelChangeToLocal del = new DelChangeToLocal(changeToLocal); //利用委派執行
                this.Invoke(del);
                //同執行緒
            } else {
                clientSocket = null;
                resetButton.Show();
                isLocalPlayer = true;
                initButtons();
            }
        }

        private void PacketAnalyze(Packet pkt) {
            if (this.InvokeRequired) // 若非同執行緒
{
                DelPacketAnalyze del = new DelPacketAnalyze(PacketAnalyze); //利用委派執行
                this.Invoke(del, pkt);
                //同執行緒
            } else {
                switch (pkt.command) {
                    case Command.GAME_START:
                        MTPinitButtons(pkt.playerStatusChange);
                        if (pkt.playerStatusChange)
                            isPlayer1 = true;
                        board = pkt.board;
                        waitForRespond = false;
                        break;
                    case Command.COLOR_ASSIGN:
                        isPlayerBlack = pkt.playerStatusChange;
                        if(isPlayer1)
                            isPlayer1Black = isPlayerBlack;
                        else
                            isPlayer1Black = !isPlayerBlack;
                        player1Picture.BackgroundImage = isPlayer1Black ? getPieceImage((int)PieceEnum.BlackKing) : getPieceImage((int)PieceEnum.RedKing);
                        player2Picture.BackgroundImage = !isPlayer1Black ? getPieceImage((int)PieceEnum.BlackKing) : getPieceImage((int)PieceEnum.RedKing);
                        isGameStart = true;
                        waitForRespond = false;
                        break;
                    case Command.UPDATE_BOARD:
                        MTPupdateBoard(pkt);
                        MTPTurnChange();
                        if(isGameStart)
                            waitForRespond = false;
                        break;
                    case Command.MOVEFAIL:
                        waitForRespond = false;
                        break;

                }

            }
        }

        private void MTPTurnChange(bool setup = false) {
            if (setup) {
                player1ColorLabel.BackColor = Color.RosyBrown;
                player2ColorLabel.BackColor = Color.Transparent;
                return;
            }

            isPlayerTurn = !isPlayerTurn;
            Color temp;
            temp = player1ColorLabel.BackColor;
            player1ColorLabel.BackColor = player2ColorLabel.BackColor;
            player2ColorLabel.BackColor = temp;
        }

        private void MTPinitButtons(bool isPlayerTurn) {

            darkChessModel = new DarkChessModel();
            isGameStart = false;
            this.isPlayerTurn = isPlayerTurn;
            selectedButton = null;
            attackButton = null;
            MTPTurnChange(true);

            foreach (PictureBox pictureBox in redPiecesTakenPictures) {
                Controls.Remove(pictureBox);
                pictureBox.Dispose();
            }

            foreach (PictureBox pictureBox in blackPiecesTakenPictures) {
                Controls.Remove(pictureBox);
                pictureBox.Dispose();
            }

            redPiecesTakenPictures = new List<PictureBox>();
            blackPiecesTakenPictures = new List<PictureBox>();

            for (int i = 0; i < Rule.BOARD_WIDTH; ++i) {
                for (int j = 0; j < Rule.BOARD_HEIGHT; ++j) {

                    if (pieceButtons[i, j] != null) {
                        Controls.Remove(pieceButtons[i, j]);
                        pieceButtons[i, j].Dispose();
                    }

                    pieceButtons[i, j] = new Button();
                    Controls.Add(pieceButtons[i, j]);
                    pieceButtons[i, j].Size = new Size(65, 65);
                    pieceButtons[i, j].BackgroundImage = Properties.Resources.unflip;
                    pieceButtons[i, j].FlatStyle = FlatStyle.Flat;
                    pieceButtons[i, j].TabStop = false;
                    pieceButtons[i, j].Tag = new Pair<int, int>(i, j);
                    pieceButtons[i, j].Click += MTPbuttonClick;
                    pieceButtons[i, j].BackgroundImageLayout = ImageLayout.Stretch;
                    pieceButtons[i, j].Location = new Point(i * BUTTON_PADDING_X + BUTTON_START_POSITION_X, j * BUTTON_PADDING_Y + BUTTON_START_POSITION_Y);
                    pieceButtons[i, j].BringToFront();
                    pieceButtons[i, j].Parent = background;
                    pieceButtons[i, j].FlatAppearance.BorderSize = 0;

                }
            }
        }
        private bool MTPisPlayerMoveInCorrectTurn(Pair<int, int> clickedButtonPair) {
            int[,] board = this.board;
            int x = clickedButtonPair.First;
            int y = clickedButtonPair.Second;


            if (isPlayerTurn) {
                if (board[x, y] == (int)PieceEnum.Unflip) {
                    return true;
                }

                if (!(selectedButton is null)) {
                    return true;
                }

                if (isPlayerBlack) {
                    return board[x, y] > 0;
                } else {
                    return board[x, y] < 0;
                }
            } else {
                return false;
            }
        }

        private void MTPbuttonClick(object sender, EventArgs e) {
            Button clickedButton = (Button)sender;
            Pair<int, int> clickedButtonPair = (Pair<int, int>)clickedButton.Tag;

            if (waitForRespond || !MTPisPlayerMoveInCorrectTurn(clickedButtonPair)) {
                return;
            }

            if (!(selectedButton is null)) {
                attackButton = clickedButton;
                Pair<int, int> fromPos = (Pair<int, int>)selectedButton.Tag;
                Pair<int, int> toPos = (Pair<int, int>)attackButton.Tag;
                MoveData moveData = new MoveData(fromPos.First, fromPos.Second, toPos.First, toPos.Second);
                //Upload to Server
                Packet pkt = new Packet(Command.MOVE, moveData);
                clientSocket.Send(Packet.Serialize(pkt).Data);
                waitForRespond = true;

                selectedButton.BackColor = Color.Transparent;
                selectedButton = null;
                attackButton = null;
            } else if (this.board[clickedButtonPair.First, clickedButtonPair.Second] == (int)PieceEnum.Unflip) {
                Pair<int, int> p = (Pair<int, int>)clickedButton.Tag;
                //upload to server
                MoveData moveData = new MoveData(p.First, p.Second, 0, 0);
                Packet pkt = new Packet(Command.FLIP,moveData);
                clientSocket.Send(Packet.Serialize(pkt).Data);
                waitForRespond = true;

            } else {
                selectedButton = clickedButton;
                selectedButton.BackColor = Color.Red;
            }

            /*if (!isGameStart) {// only player1 can trigger
                isGameStart = true;
                isPlayerBlack = board[clickedButtonPair.First, clickedButtonPair.Second] > 0;
                isPlayer1Black = isPlayerBlack;
                //UploadToServer
                Packet pkt = new Packet(Command.COLOR_ASSIGN, isPlayerBlack);
                clientSocket.Send(Packet.Serialize(pkt).Data);

                player1Picture.BackgroundImage = isPlayer1Black ? getPieceImage((int)PieceEnum.BlackKing) : getPieceImage((int)PieceEnum.RedKing);
                player2Picture.BackgroundImage = !isPlayer1Black ? getPieceImage((int)PieceEnum.BlackKing) : getPieceImage((int)PieceEnum.RedKing);
            }*/


            if (darkChessModel.isBlackWin()) {
                victoryLabel.Text = "黑方獲勝";
            } else if (darkChessModel.isRedWin()) {
                victoryLabel.Text = "紅方獲勝";
            }

        }

        private void MTPupdatePiecesTaken(Packet pkt) {
            while (blackPiecesTakenPictures.Count < pkt.blackPiecesTaken.Count) {
                Bitmap piecePicture = getPieceImage(pkt.blackPiecesTaken[blackPiecesTakenPictures.Count]);
                PictureBox newPicture = new PictureBox();
                newPicture.BackgroundImage = piecePicture;
                newPicture.Size = new Size(30, 30);
                newPicture.BackgroundImageLayout = ImageLayout.Stretch;
                newPicture.Location = new Point(BLACK_PIECE_TAKEN_START_POSTION_X + blackPiecesTakenPictures.Count * PIECE_TAKEN_PADDING, BLACK_PIECE_TAKEN_START_POSTION_Y);
                Controls.Add(newPicture);
                blackPiecesTakenPictures.Add(newPicture);
                newPicture.BringToFront();
            }

            while (redPiecesTakenPictures.Count < pkt.redPiecesTaken.Count) {
                Bitmap piecePicture = getPieceImage(pkt.redPiecesTaken[redPiecesTakenPictures.Count]);
                PictureBox newPicture = new PictureBox();
                newPicture.BackgroundImage = piecePicture;
                newPicture.Size = new Size(30, 30);
                newPicture.BackgroundImageLayout = ImageLayout.Stretch;
                newPicture.Location = new Point(RED_PIECE_TAKEN_START_POSTION_X + redPiecesTakenPictures.Count * PIECE_TAKEN_PADDING, RED_PIECE_TAKEN_START_POSTION_Y);
                Controls.Add(newPicture);
                redPiecesTakenPictures.Add(newPicture);
                newPicture.BringToFront();
            }
        }

        private void MTPupdateBoard(Packet pkt) {
            board = pkt.board;
            MTPupdatePiecesTaken(pkt);
            for(int y=0; y < 4; y++) {
                for(int x = 0; x < 8; x++) {
                    pieceButtons[x, y].BackgroundImage = getPieceImage(pkt.board[x, y]);
                }
            }


        }
        private void MainForm_Closing(object sender, CancelEventArgs e) {
            if(clientSocket != null) {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
                
        }

    }
}
