using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Brignoli_Taramelli_Gioco.Properties;

namespace Brignoli_Taramelli_Gioco
{
    public partial class Form1 : Form
    {
        delegate void LoadPos(byte[] pos);

        const int PORT = 50000;
        const string ADDRESS = "127.0.0.1";
        TcpClient client;
        NetworkStream stream;

        bool caselleSelezionate;
        Control firstClicked = null;
        Control secondClicked = null;
        byte turn = 2;

        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            GeneraScacchiera();

            client = new TcpClient(ADDRESS, PORT);
            stream = client.GetStream();
            new Thread(ReceiveMessages).Start();
        }

        private void GeneraScacchiera()
        {
            int i = 0;

            foreach (Control control in ChessBoardPanel.Controls)
            {
                TableLayoutPanelCellPosition pos = ChessBoardPanel.GetPositionFromControl(control);

                if (control is Label casella)
                {
                    casella.Text = string.Empty;
                    casella.BackColor = (pos.Row + pos.Column) % 2 == 1 ? Color.DarkGray : Color.LightGray;
                }
                i++;
            }
        }

        private void ReceiveMessages()
        {
            int byteLetti;
            byte[] buffer = new byte[65];

            byteLetti = stream.Read(buffer, 0, 65);

            if (byteLetti == 1) turn = buffer[0];
            if (turn == 1) TBColore.Text = "Bianco";
            else if (turn == 0) TBColore.Text = "Nero";
            else Disconnetti();

            while (true)
            {
                byteLetti = stream.Read(buffer, 0, 65);

                if (byteLetti == 0)
                {
                    Disconnetti();
                    return;
                }

                byte turnoDiGioco = buffer[0];
                byte[] pos = buffer.Skip(1).ToArray();

                if (pos.All(v => v == turn))
                {
                    MessageBox.Show("Hai vinto!");
                    Disconnetti();
                    return;
                } else if (pos.All(v => v == (turn == 1 ? 0 : 1)))
                {
                    MessageBox.Show("Hai perso!");
                    Disconnetti();
                    return;
                }

                while (!ChessBoardPanel.IsHandleCreated) Thread.Sleep(100);
                GeneraPosizione(pos);
                TBTurno.Text = turnoDiGioco == 1 ? "Tocca al bianco" : "Tocca al nero";

                if (turn == turnoDiGioco)
                {
                    if (caselleSelezionate) DeselezionaCaselle();

                    while (firstClicked == null || secondClicked == null) Thread.Sleep(100);
                    TableLayoutPanelCellPosition start = ChessBoardPanel.GetPositionFromControl(firstClicked);
                    TableLayoutPanelCellPosition stop = ChessBoardPanel.GetPositionFromControl(secondClicked);

                    byte[] mossa;
                    if (turn == 1)
                        mossa = new byte[2]
                        {
                            (byte)(start.Column + start.Row * 8),
                            (byte)(stop.Column + stop.Row * 8),
                        };
                    else
                        mossa = new byte[2]
                        {
                            (byte)(63 - (start.Column + start.Row * 8)),
                            (byte)(63 - (stop.Column + stop.Row * 8)),
                        };

                    stream.Write(mossa, 0, 2);

                    if (caselleSelezionate) DeselezionaCaselle();
                }
            }
        }

        private void GeneraPosizione(byte[] pos)
        {
            Bitmap[] imgPezzi = new Bitmap[12]
            {
                Resources.BPawn,
                Resources.BKnight,
                Resources.BBishop,
                Resources.BRook,
                Resources.BQueen,
                Resources.BKing,                
                Resources.WPawn,
                Resources.WKnight,
                Resources.WBishop,
                Resources.WRook,
                Resources.WQueen,
                Resources.WKing,
            };

            int i = 0;
            if (turn == 1) i = 63;

            foreach (Control control in ChessBoardPanel.Controls)
            {
                if (control is Label casella)
                {
                    if (pos[i] > 0)
                    {
                        byte numericPiece = (byte)(pos[i] - 1);
                        casella.Image = imgPezzi[numericPiece];
                    } 
                    else casella.Image = null;

                    if (turn == 1) i--;
                    else if (turn == 0) i++;
                }
            }
        }

        private void SelezionaCasella(object sender, EventArgs e)
        {
            if (sender is Control clickedLabel)
            {
                if (firstClicked == null)
                {
                    caselleSelezionate = true;
                    firstClicked = clickedLabel;
                    clickedLabel.BackColor = Color.DarkSeaGreen;
                }
                else if (secondClicked == null)
                {
                    caselleSelezionate = true;
                    secondClicked = clickedLabel;
                    clickedLabel.BackColor = Color.IndianRed;

                }
                else if (caselleSelezionate) DeselezionaCaselle();
            }
        }

        private void DeselezionaCaselle()
        {
            caselleSelezionate = false;
            firstClicked = null;
            secondClicked = null;
            GeneraScacchiera();
        }

        private void Disconnetti()
        {
            stream.Close();
            client.Close();
        }

        private void BtnDeseleziona_Click(object sender, EventArgs e)
        {
            DeselezionaCaselle();
        }

        private void Form1_Closing(object sender, EventArgs e)
        {
            Disconnetti();
        }
    }
}
