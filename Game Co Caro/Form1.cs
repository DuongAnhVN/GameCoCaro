using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game_Co_Caro
{
    public partial class Form1 : Form
    {
        private Label[,] Map;
        private static int columns, rows;
        private int player;
        private bool TimeOfPlayer;
        private bool gameover;
        private bool vsComputer;
        private int[,] vtMap;
        private Stack<Chess> chesses;
        private Chess chess;
        private TCPManager TCPGame;
        private bool ContinueGame = true;
        private delegate void SafeCallDelegate(string text, Control obj);

        public static string PlayerLAN = null;
        public Form1()
        {
            
            InitializeComponent();
            int a = pnTableChess.Width;
            int b = pnTableChess.Height;

            columns = a/28;
            rows = b/28;

            vsComputer = false;
            gameover = false;
            player = 1;
            Map = new Label[rows+1, columns+1];
            vtMap = new int[rows+1, columns+1];
            chesses = new Stack<Chess>();

            BuildTable();
            Gameover();

            checkBox2.Checked = true;
        }

 
        private void BuildTable()
        {
            for (int i = 0; i <= rows; i++)
                for (int j = 0; j <= columns; j++)
                {
                    if (InvokeRequired)
                    {
                        this.Invoke((MethodInvoker)(() =>
                        {
                            Map[i, j] = new Label();
                            Map[i, j].Parent = pnTableChess;
                            Map[i, j].Top = i * Contain.edgeChess;
                            Map[i, j].Left = j * Contain.edgeChess;
                            Map[i, j].Size = new Size(Contain.edgeChess - 1, Contain.edgeChess - 1);
                            Map[i, j].BackColor = Color.Snow;

                            Map[i, j].MouseLeave += Form1_MouseLeave;
                            Map[i, j].MouseEnter += Form1_MouseEnter;
                            Map[i, j].Click += Form1_Click;
                        }));
                    }
                    else
                    {
                        Map[i, j] = new Label();
                        Map[i, j].Parent = pnTableChess;
                        Map[i, j].Top = i * Contain.edgeChess;
                        Map[i, j].Left = j * Contain.edgeChess;
                        Map[i, j].Size = new Size(Contain.edgeChess - 1, Contain.edgeChess - 1);
                        Map[i, j].BackColor = Color.Snow;

                        Map[i, j].MouseLeave += Form1_MouseLeave;
                        Map[i, j].MouseEnter += Form1_MouseEnter;
                        Map[i, j].Click += Form1_Click;

                    }
                }
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            if (gameover)
                return;
            Label lb = (Label)sender;
            int x = lb.Top / Contain.edgeChess, y = lb.Left / Contain.edgeChess;

            if (vtMap[x, y] != 0)
                return;
            if (vsComputer)
            {
                player = 1;
                psbCooldownTime.Value = 0;
                tmCooldown.Start();
                lb.Image = Properties.Resources.o;             
                vtMap[x, y] = 1;
                Check(x, y);                        
                CptFindChess();
            }
            else
            {
                if (TimeOfPlayer)
                {
                    PlayerCheck(x, y);
                }
            }
            chess = new Chess(lb, x, y);
            chesses.Push(chess);          
        }

        private void PlayerCheck(int x, int y)
        {
            if (TimeOfPlayer)
            {
                if (TCPGame.isServer)
                {
                    TCPGame.Server.Send(PlayerLAN, TCPComand.Command($"{TCPComand.SEND_POINT} {x.ToString()},{y.ToString()}"));

                }
                else
                {
                    TCPGame.Client.Send(TCPComand.Command($"{TCPComand.SEND_POINT} {x.ToString()},{y.ToString()}"));
                }
                psbCooldownTime.Value = 0;
                tmCooldown.Start();
                Map[x, y].Image = Properties.Resources.o;
                vtMap[x, y] = player;
                Check(x, y);

                TimeOfPlayer = false;
                ptbPayer.Image = Properties.Resources.x_copy;
                txtNamePlayer.Text = "Guest";
            }
            else
            {
                if (InvokeRequired)
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        psbCooldownTime.Value = 0;
                        Map[x, y].Image = Properties.Resources.x;
                        ptbPayer.Image = Properties.Resources.onnnn;
                        txtNamePlayer.Text = "Me";
                    }));
                }
                else
                {
                    psbCooldownTime.Value = 0;
                    Map[x, y].Image = Properties.Resources.x;
                    ptbPayer.Image = Properties.Resources.onnnn;
                    txtNamePlayer.Text = "Me";
                }


                vtMap[x, y] = 2;
                Check(x, y);
                TimeOfPlayer = true;
                
            }
        }


        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            if (gameover)
                return;
            Label lb = (Label)sender;
            lb.BackColor = Color.AliceBlue;
        }

        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            if (gameover)
                return;
            Label lb = (Label)sender;
            lb.BackColor = Color.Snow;
        }

        
        private void tmCooldown_Tick(object sender, EventArgs e)
        {
            psbCooldownTime.PerformStep();
            if (psbCooldownTime.Value >= psbCooldownTime.Maximum)
            {
                Gameover();
                if (!vsComputer)
                {
                    string textresult = "You lost!!";
                    if (!TimeOfPlayer)
                        textresult = "You win!!";
                    ResultKetQuaGame(textresult);
                }
                
            }
        }
      
        private void menuUndo_Click(object sender, EventArgs e)
        {
            if (!vsComputer)
            {
                Chess template = new Chess();
                template = chesses.Pop();
                template.lb.Image = null;
                vtMap[template.X, template.Y] = 0;
                psbCooldownTime.Value = 0;
                ChangePlayer();
            }
            else
            {
                Chess template = new Chess();
                template = chesses.Pop();
                template.lb.Image = null;
                vtMap[template.X, template.Y] = 0;

                template = chesses.Pop();
                template.lb.Image = null;
                vtMap[template.X, template.Y] = 0;

                psbCooldownTime.Value = 0;
                player = 1;
            }
        }

        private void menuQuit_Click_1(object sender, EventArgs e)
        {
            DialogResult dialog;
            dialog = MessageBox.Show("Bạn có chắc muốn thoát không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.Yes)
            {
                this.Dispose();
                this.Close();
            }
        }


        private void LoadGameLAN()
        {
            vsComputer = false;
            gameover = false;
            this.Invoke((MethodInvoker)(() =>
            {
                tmCooldown.Stop();
                txtNamePlayer.Text = "";
                psbCooldownTime.Value = 0;
                pnTableChess.Controls.Clear();
                ptbPayer.Image = null;
            }));
            
            
            
            //menuStrip1.Parent = pnTableChess;
            Map = new Label[rows + 2, columns + 2];
            vtMap = new int[rows + 2, columns + 2];
            chesses = new Stack<Chess>();
            BuildTable();
            psbCooldownTime.Invoke((MethodInvoker)(() =>
            {
                if(TimeOfPlayer)
                {
                    txtNamePlayer.Text = "Me";;
                    ptbPayer.Image = Properties.Resources.onnnn;
                }
                else
                {
                    txtNamePlayer.Text = "Guest";
                    ptbPayer.Image = Properties.Resources.x_copy;
                }
                psbCooldownTime.Value = 0;
                tmCooldown.Start();
            }));

        }


        private void PlayVsComputer(object sender, EventArgs e)
        {
            vsComputer = true;
            gameover = false;
            psbCooldownTime.Value = 0;
            tmCooldown.Stop();
            pnTableChess.Controls.Clear();

            ptbPayer.Image = Properties.Resources.onnnn;
            txtNamePlayer.Text = "Player";
            //menuStrip1.Parent = pnTableChess;
            player = 1;
            Map = new Label[rows + 2, columns + 2];
            vtMap = new int[rows + 2, columns + 2];
            chesses = new Stack<Chess>();

            BuildTable();
        }
        private void Gameover()
        {
            tmCooldown.Stop();
            gameover = true;
            backgroundgameover();
        }
        private void backgroundgameover()
        {
            for (int i = 0; i <= rows; i++)
                for (int j = 0; j <= columns; j++)
                {
                    Map[i, j].BackColor = Color.Gray;
                }
        }
        private void ChangePlayer()
        {
            if (player == 1)
            {
                player = 2;
                txtNamePlayer.Text = "Player2";
                ptbPayer.Image = Properties.Resources.x_copy;
            }
            else
            {
                player = 1;
                txtNamePlayer.Text = "Player1";
                ptbPayer.Image = Properties.Resources.onnnn;
            }
        }
        private void Check(int x, int y)
        {
            int column = 1, row = 1, mdiagonal = 1, ediagonal = 1;

            int i = x - 1, j = y;
            if(i >= 0)
            {
                while (i >= 0 && vtMap[x, y] == vtMap[i, j] )
                {
                    column++;
                    i--;
                }
            }
           
            i = x + 1;
            while (vtMap[x, y] == vtMap[i, j] && i <= rows)
            {
                column++;
                i++;
            }

            i = x; j = y - 1;
            if(j >= 0)
            {
                while (j >= 0 && vtMap[x, y] == vtMap[i, j])
                {
                    row++;
                    j--;
                }
            }
            
            j = y + 1;
            while (vtMap[x, y] == vtMap[i, j] && j <= columns)
            {
                row++;
                j++;
            }


            i = x - 1; j = y - 1;
            if (i >= 0 && j >= 0)
            {
                while (i >= 0 && j >= 0 && vtMap[x, y] == vtMap[i, j])
                {
                    mdiagonal++;
                    i--;
                    j--;
                }
            }
            
            i = x + 1; j = y + 1;
            while (vtMap[x, y] == vtMap[i, j] && i <= rows && j <= columns)
            {
                mdiagonal++;
                i++;
                j++;
            }
            
            i = x - 1; j = y + 1;
            if (i >= 0)
            {
                while (i >= 0 && vtMap[x, y] == vtMap[i, j] && j <= columns)
                {
                    ediagonal++;
                    i--;
                    j++;
                }
            }
            
            i = x + 1; j = y - 1;
            if(j >= 0)
            {
                while (j >= 0 && vtMap[x, y] == vtMap[i, j] && i <= rows)
                {
                    ediagonal++;
                    i++;
                    j--;
                }
            }
           
            if (row >= 5 || column >= 5 || mdiagonal >= 5 || ediagonal >= 5)
            {
                Gameover();
                if (vsComputer)
                {
                    if (player == 1)
                        MessageBox.Show("You win!!");
                    else
                        MessageBox.Show("You lost!!");
                }
                else
                {
                    string textresult = "You lost!!";
                    if (TimeOfPlayer)
                        textresult = "You win!!";
                    ResultKetQuaGame(textresult);
                }
            }

        }

        private void ResultKetQuaGame(string textresult)
        {
           
            DialogResult dialog;
            dialog = MessageBox.Show($"{textresult}\r\n  Tiếp tục chơi ván tiếp theo?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.Yes)
            {
                if (TCPGame.isServer)
                {
                    ContinueGame = true;
                }
                else
                {
                    TCPGame.Client.Send(TCPComand.Command($"{TCPComand.CONTINUE_GAME}"));
                }
            }
            else
            {
                if (TCPGame.isServer)
                {
                    TCPGame.Server.DisconnectClient(PlayerLAN);
                    PlayerLAN = null;
                }
                else
                {
                    TCPGame.Client.Dispose();
                    PlayerLAN = null;
                }
            }
        }


        #region AI

        private int[] Attack = new int[7] { 0, 9, 54, 162, 1458, 13112, 118008 };
        private int[] Defense = new int[7] { 0, 3, 27, 99, 729, 6561, 59049 };

        private void PutChess(int x, int y)
        {
            player = 0;
            psbCooldownTime.Value = 0;          
            Map[x+1, y].Image = Properties.Resources.x;

            vtMap[x, y] = 2;
            Check(x, y);

            chess = new Chess(Map[x+1, y], x, y);
            chesses.Push(chess);
        }

        private void CptFindChess()
        {
            if (gameover) return;
            long max = 0;
            int imax = 1, jmax = 1;
            for (int i = 1; i < rows; i++)
            {
                for (int j = 1; j < columns; j++)
                    if (vtMap[i, j] == 0)
                    {
                        long temp = Caculate(i, j);
                        if (temp > max)
                        {
                            max = temp;
                            imax = i; jmax = j;
                        }
                    }
            }
            PutChess(imax, jmax);
        }
        private long Caculate(int x, int y)
        {
            return EnemyChesses(x, y) + ComputerChesses(x, y);
        }
        private long ComputerChesses(int x, int y)
        {
            int i = x - 1, j = y;
            int column = 0, row = 0, mdiagonal = 0, ediagonal = 0;
            int sc_ = 0, sc = 0, sr_ = 0, sr = 0, sm_ = 0, sm = 0, se_ = 0, se = 0;
            while (vtMap[i, j] == 2 && i >= 0)
            {
                column++;
                i--;
            }
            if (vtMap[i, j] == 0) sc_ = 1;
            i = x + 1;
            while (vtMap[i, j] == 2 && i <= rows)
            {
                column++;
                i++;
            }
            if (vtMap[i, j] == 0) sc = 1;
            i = x; j = y - 1;
            while (vtMap[i, j] == 2 && j >= 0)
            {
                row++;
                j--;
            }
            if (vtMap[i, j] == 0) sr_ = 1;
            j = y + 1;
            while (vtMap[i, j] == 2 && j <= columns)
            {
                row++;
                j++;
            }
            if (vtMap[i, j] == 0) sr = 1;
            i = x - 1; j = y - 1;
            while (vtMap[i, j] == 2 && i >= 0 && j >= 0)
            {
                mdiagonal++;
                i--;
                j--;
            }
            if (vtMap[i, j] == 0) sm_ = 1;
            i = x + 1; j = y + 1;
            while (vtMap[i, j] == 2 && i <= rows && j <= columns)
            {
                mdiagonal++;
                i++;
                j++;
            }
            if (vtMap[i, j] == 0) sm = 1;
            i = x - 1; j = y + 1;
            while (vtMap[i, j] == 2 && i >= 0 && j <= columns)
            {
                ediagonal++;
                i--;
                j++;
            }
            if (vtMap[i, j] == 0) se_ = 1;
            i = x + 1; j = y - 1;
            while (vtMap[i, j] == 2 && i <= rows && j >= 0)
            {
                ediagonal++;
                i++;
                j--;
            }
            if (vtMap[i, j] == 0) se = 1;

            if (column == 4) column = 5;
            if (row == 4) row = 5;
            if (mdiagonal == 4) mdiagonal = 5;
            if (ediagonal == 4) ediagonal = 5;

            if (column == 3 && sc == 1 && sc_ == 1) column = 4;
            if (row == 3 && sr == 1 && sr_ == 1) row = 4;
            if (mdiagonal == 3 && sm == 1 && sm_ == 1) mdiagonal = 4;
            if (ediagonal == 3 && se == 1 && se_ == 1) ediagonal = 4;

            if (column == 2 && row == 2 && sc == 1 && sc_ == 1 && sr == 1 && sr_ == 1) column = 3;
            if (column == 2 && mdiagonal == 2 && sc == 1 && sc_ == 1 && sm == 1 && sm_ == 1) column = 3;
            if (column == 2 && ediagonal == 2 && sc == 1 && sc_ == 1 && se == 1 && se_ == 1) column = 3;
            if (row == 2 && mdiagonal == 2 && sm == 1 && sm_ == 1 && sr == 1 && sr_ == 1) column = 3;
            if (row == 2 && ediagonal == 2 && se == 1 && se_ == 1 && sr == 1 && sr_ == 1) column = 3;
            if (ediagonal == 2 && mdiagonal == 2 && sm == 1 && sm_ == 1 && se == 1 && se_ == 1) column = 3;

            long Sum = Attack[row] + Attack[column] + Attack[mdiagonal] + Attack[ediagonal];

            return Sum;
        }
        private long EnemyChesses(int x, int y)
        {
            int i = x - 1, j = y;
            int sc_ = 0, sc = 0, sr_ = 0, sr = 0, sm_ = 0, sm = 0, se_ = 0, se = 0;
            int column = 0, row = 0, mdiagonal = 0, ediagonal = 0;
            while (vtMap[i, j] == 1 && i >= 0)
            {
                column++;
                i--;
            }
            if (vtMap[i, j] == 0) sc_ = 1;
            i = x + 1;
            while (vtMap[i, j] == 1 && i <= rows)
            {
                column++;
                i++;
            }
            if (vtMap[i, j] == 0) sc = 1;
            i = x; j = y - 1;
            while (vtMap[i, j] == 1 && j >= 0)
            {
                row++;
                j--;
            }
            if (vtMap[i, j] == 0) sr_ = 1;
            j = y + 1;
            while (vtMap[i, j] == 1 && j <= columns)
            {
                row++;
                j++;
            }
            if (vtMap[i, j] == 0) sr = 1;
            i = x - 1; j = y - 1;
            while (vtMap[i, j] == 1 && i >= 0 && j >= 0)
            {
                mdiagonal++;
                i--;
                j--;
            }
            if (vtMap[i, j] == 0) sm_ = 1;
            i = x + 1; j = y + 1;
            while (vtMap[i, j] == 1 && i <= rows && j <= columns)
            {
                mdiagonal++;
                i++;
                j++;
            }
            if (vtMap[i, j] == 0) sm = 1;
            i = x - 1; j = y + 1;
            while (vtMap[i, j] == 1 && i >= 0 && j <= columns)
            {
                ediagonal++;
                i--;
                j++;
            }
            if (vtMap[i, j] == 0) se_ = 1;
            i = x + 1; j = y - 1;
            while (vtMap[i, j] == 1 && i <= rows && j >= 0)
            {
                ediagonal++;
                i++;
                j--;
            }
            if (vtMap[i, j] == 0) se = 1;

            if (column == 4) column = 5;
            if (row == 4) row = 5;
            if (mdiagonal == 4) mdiagonal = 5;
            if (ediagonal == 4) ediagonal = 5;

            if (column == 3 && sc == 1 && sc_ == 1) column = 4;
            if (row == 3 && sr == 1 && sr_ == 1) row = 4;
            if (mdiagonal == 3 && sm == 1 && sm_ == 1) mdiagonal = 4;
            if (ediagonal == 3 && se == 1 && se_ == 1) ediagonal = 4;

            if (column == 2 && row == 2 && sc == 1 && sc_ == 1 && sr == 1 && sr_ == 1) column = 3;
            if (column == 2 && mdiagonal == 2 && sc == 1 && sc_ == 1 && sm == 1 && sm_ == 1) column = 3;
            if (column == 2 && ediagonal == 2 && sc == 1 && sc_ == 1 && se == 1 && se_ == 1) column = 3;
            if (row == 2 && mdiagonal == 2 && sm == 1 && sm_ == 1 && sr == 1 && sr_ == 1) column = 3;
            if (row == 2 && ediagonal == 2 && se == 1 && se_ == 1 && sr == 1 && sr_ == 1) column = 3;
            if (ediagonal == 2 && mdiagonal == 2 && sm == 1 && sm_ == 1 && se == 1 && se_ == 1) column = 3;
            long Sum = Defense[row] + Defense[column] + Defense[mdiagonal] + Defense[ediagonal];

            return Sum;
        }
        #endregion






        private void BtnCreateLAN_Click(object sender, EventArgs e)
        {
            if(TCPGame == null || TCPGame.Server.IsListening == false)
            {
                if (!string.IsNullOrEmpty(TbAdressLAN.Text) && TbAdressLAN.Text.Split(':').Length == 2)
                {
                    TCPGame = new TCPManager();
                    TCPGame.IP = TbAdressLAN.Text.Split(':')[0];
                    TCPGame.PORT = Int32.Parse(TbAdressLAN.Text.Split(':')[1]);

                }
                if (!TCPGame.CreateServer())
                {
                    MessageBox.Show("Error");
                    return;
                }

                TCPGame.Server.Events.ClientConnected += Server_ClientConnected;
                TCPGame.Server.Events.ClientDisconnected += Server_ClientDisconnected;
                TCPGame.Server.Events.DataReceived += Server_DataReceived;

                BtnCreateLAN.Text = "Hủy phòng";
                BtnConnectLAN.Enabled = false;
                BtnFindGame.Enabled = false;
            }
            else
            {
                TCPGame.Server.Dispose();
                BtnCreateLAN.Text = "Tạo phòng";
                BtnConnectLAN.Enabled = true;
                BtnFindGame.Enabled = true;
                Gameover();
            }
            
        }

        private void Server_ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            if(PlayerLAN == e.IpPort || TCPGame.Server.IsConnected(e.IpPort))
            {
                richTextBox1.Invoke((MethodInvoker)(() =>
                {
                    richTextBox1.Text = string.Empty;
                }));
                PlayerLAN = null;
                Gameover();
                MessageBox.Show($"{e.IpPort} Đã thoát khỏi trò chơi !!!");
            }
        }

        private void Server_ClientConnected(object sender, ConnectionEventArgs e)
        {
            if (PlayerLAN == null)
            {
                DialogResult dialog;
                dialog = MessageBox.Show($"Chấp nhận chơi Game cùng {e.IpPort} ?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialog == DialogResult.Yes)
                {
                    newgameserver(e.IpPort);
                }
                else
                {
                    TCPGame.Server.Send(PlayerLAN, TCPComand.Command(TCPComand.CANCEL_GAME));
                }
            }
        }

        private void newgameserver(string IpPort)
        {
            PlayerLAN = IpPort;
            player = new Random().Next(1, 3);
            bool timeparam = true;
            if (player == 1) { TimeOfPlayer = false; timeparam = true; } else { TimeOfPlayer = true; timeparam = false; }
            TCPGame.Server.Send(PlayerLAN, TCPComand.Command($"{TCPComand.NEW_GAME} {timeparam}"));
            TCPGame.isServer = true;
            LoadGameLAN();
        }

        private void Server_DataReceived(object sender, DataReceivedEventArgs e)
        {
            HandleData(e);
        }

        private void BtnConnectLAN_Click(object sender, EventArgs e)
        {
            if (TCPGame == null || TCPGame.Client == null || !TCPGame.Client.IsConnected)
            {
                if (!string.IsNullOrEmpty(TbAdressLAN.Text) && TbAdressLAN.Text.Split(':').Length == 2)
                {
                    TCPGame = new TCPManager();
                    TCPGame.IP = TbAdressLAN.Text.Split(':')[0];
                    TCPGame.PORT = Int32.Parse(TbAdressLAN.Text.Split(':')[1]);
                    if (!TCPGame.ConnectServer())
                    {
                        MessageBox.Show("Error");
                        return;
                    }
                    TCPGame.Client.Events.DataReceived += client_DataReceived;
                    TCPGame.Client.Events.Disconnected += client_Disconnected;
                    BtnCreateLAN.Enabled = false;
                    BtnConnectLAN.Text = "Rời phòng";
                    BtnFindGame.Enabled = false;
                }
                else
                {
                    MessageBox.Show("Vui lòng nhập đúng định dạng địa chỉ !!!");
                }
            }
            else
            {
                TCPGame.Client.Dispose();
                BtnCreateLAN.Enabled = true;
                BtnConnectLAN.Text = "Vào phòng";
                BtnFindGame.Enabled = true;
            }
            
        }

        private void client_Disconnected(object sender, ConnectionEventArgs e)
        {
            PlayerLAN = null;
            Gameover();
            BtnCreateLAN.Enabled = true;
            BtnConnectLAN.Text = "Vào phòng";
            BtnFindGame.Enabled = true;

            richTextBox1.Invoke((MethodInvoker)(() =>
            {
                richTextBox1.Text = string.Empty;
            }));

            MessageBox.Show($"{e.IpPort} Đã thoát khỏi trò chơi !!!");
        }

        private void client_DataReceived(object sender, DataReceivedEventArgs e)
        {
            HandleData(e);
        }

        private void HandleData(DataReceivedEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Data);
            string[] Agr = data.Split(' ');
            if (Agr[0] == "/PHONGGAME" && Agr.Length >= 2)
            {
                switch (Agr[1])
                {
                    case "NEWGAME":
                        if (Agr.Length == 3)
                        {
                            PlayerLAN = e.IpPort;
                            TimeOfPlayer = bool.Parse(Agr[2]);
                            LoadGameLAN();
                            TCPGame.isServer = false;
                        }
                        break;
                    case "CANCELGAME":
                        MessageBox.Show("Người chơi từ chối bắt đầu game !!!");
                        BtnCreateLAN.Enabled = true;
                        BtnConnectLAN.Enabled = true;
                        BtnFindGame.Enabled = true;
                        break;
                    case "SENDPOINT":
                        if (Agr.Length == 3)
                        {
                            int x = Int32.Parse(Agr[2].Split(',')[0]);
                            int y = Int32.Parse(Agr[2].Split(',')[1]);
                            PlayerCheck(x, y);
                        }
                        break;
                    case "CONTINUEGAME":
                        newgameserver(e.IpPort);
                        break;
                    case "XINTHUA":
                        tmCooldown.Stop();
                        ResultKetQuaGame("Đối Thủ Xin Thua , You win!!");
                        break;

                }
            }else if(Agr[0] == "/CHAT" && Agr.Length >= 2)
            {
                UpdateTextThreadSafe($"Guest : {Agr[1]}{Environment.NewLine}", richTextBox1);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                PlayerPC.Enabled = true;
                checkBox2.Checked = false;
                BtnConnectLAN.Enabled = false;
                BtnCreateLAN.Enabled = false;
                BtnFindGame.Enabled = false;
            }
        }

        private void BtnFindGame_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Comming Soon !!!");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tmCooldown.Stop();
            Gameover();
            if (TCPGame.isServer)
            {
                TCPGame.Server.Send(PlayerLAN, TCPComand.Command(TCPComand.XIN_THUA));
            }
            else
            {
                TCPGame.Client.Send(TCPComand.Command(TCPComand.XIN_THUA));
            }
            ResultKetQuaGame("You lost!!");
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Checked = false;
                BtnConnectLAN.Enabled = true;
                BtnCreateLAN.Enabled = true;
                BtnFindGame.Enabled = true;
                PlayerPC.Enabled = false;
            }
        }

        private void BtnSendChat_Click(object sender, EventArgs e)
        {
            try
            {
                if(TCPGame != null && TCPGame.Client != null && TCPGame.Client.IsConnected)
                {
                    TCPGame.Client.Send(TCPComand.Chat(richTextBox2.Text));
                }
                else if(TCPGame != null && TCPGame.Server != null && TCPGame.Server.IsListening)
                {
                    TCPGame.Server.Send(PlayerLAN, TCPComand.Chat(richTextBox2.Text));
                }

                UpdateTextThreadSafe($"Me : {richTextBox2.Text}{Environment.NewLine}", richTextBox1);
                richTextBox2.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateTextThreadSafe(string text, Control control)
        {
            if (control.InvokeRequired)
            {
                var d = new SafeCallDelegate(UpdateTextThreadSafe);
                control.Invoke(d, new object[] { text, control });
            }
            else
            {
                if (control is RichTextBox)
                {
                    ((RichTextBox)control).AppendText("\r\n" + text);
                    ((RichTextBox)control).ScrollToCaret();
                }
                else
                {
                    control.Text = text;

                }
            }
        }
    }



    public class Chess
    {
        public Label lb;
        public int X;
        public int Y;
        public Chess()
        {
            lb = new Label();
        }
        public Chess(Label _lb, int x, int y)
        {
            lb = new Label();
            lb = _lb;
            X = x;
            Y = y;
        }
    }
}
