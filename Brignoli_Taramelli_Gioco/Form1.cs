using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

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

        // Metodo per ricevere i messaggi dal server
        private void ReceiveMessages()
        {
            byte[] bytesToRead = new byte[4096];

            while (true)
            {
                try
                {
                    int bytesRead = stream.Read(bytesToRead, 0, bytesToRead.Length);

                    if (bytesRead > 0)
                    {
                        
                        string message = Encoding.UTF8.GetString(bytesToRead, 0, bytesRead);
                        if (ChessBoardPanel.IsHandleCreated) {
                            ChessBoardPanel.Invoke(new LoadPos(GeneraPosizione), bytesToRead);
                        }
                    }
                    Thread.Sleep(100);
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
            int i = 0;
            foreach (Control control in ChessBoardPanel.Controls)
            {
                Label casella = control as Label;
                TableLayoutPanelCellPosition index = ChessBoardPanel.GetPositionFromControl(control);

                if (casella != null)
                {
                    casella.Text = pos[i].ToString();
                }
                i++;
            }
        }
    }
}
