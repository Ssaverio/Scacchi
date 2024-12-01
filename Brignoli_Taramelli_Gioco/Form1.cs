using System;
using System.Collections.Generic;
using System.Drawing;
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


        Label firstClicked = null;
        Label secondClicked = null;
        int turn = 2;

        public Form1()
        {
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
                Label casella = control as Label;
                TableLayoutPanelCellPosition pos = ChessBoardPanel.GetPositionFromControl(control);    

                if (casella != null)
                {
                    casella.Text = string.Empty;
                    casella.BackColor = (pos.Row + pos.Column) % 2 == 1 ? Color.DarkGray : Color.LightGray;
                }
                i++;
            }
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[65];

            while (true)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 1 && turn == 2)
                    {
                        TBTurno.Text = buffer[0].ToString();
                        turn = buffer[0];

                        stream.Write(new byte[1] { 0 }, 0, 1);
                    }

                    if (bytesRead > 0)
                    {
                        while (!ChessBoardPanel.IsHandleCreated) Thread.Sleep(100);
                        
                        ChessBoardPanel.Invoke(new LoadPos(GeneraPosizione), buffer);

                        if (buffer[0] == turn)
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore durante la lettura dei messaggi: " + ex.Message);
                    break;
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

            int i = 1;
            if (turn == 1) i = 64;

            foreach (Control control in ChessBoardPanel.Controls)
            {
                if (control is Label casella)
                {
                    if (pos[i] > 0)
                    {
                        byte numericPiece = (byte)(pos[i] - 1);
                        casella.Image = imgPezzi[numericPiece];
                    }

                    if (turn == 1) i--;
                    else if (turn == 0) i++;
                }
            }
        }
    }
}
