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
            byte[] bytesToRead = new byte[64];

            while (true)
            {
                try
                {
                    int bytesRead = stream.Read(bytesToRead, 0, bytesToRead.Length);

                    if (bytesRead > 0)
                    {
                        if (ChessBoardPanel.IsHandleCreated) ChessBoardPanel.Invoke(new LoadPos(GeneraPosizione), bytesToRead);
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

            int i = 63;
            foreach (Control control in ChessBoardPanel.Controls)
            {
                if (control is Label casella)
                {
                    if (pos[i] > 0)
                    {
                        byte numericPiece = (byte)(pos[i] - 1);
                        casella.Image = imgPezzi[numericPiece];
                    }
                    i--;
                }
            }
        }
    }
}
