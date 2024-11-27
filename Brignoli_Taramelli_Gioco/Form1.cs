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
        delegate void ChangeLabel(string text);  // Delegate per aggiornare la label in modo thread-safe

        const int PORT = 50000;
        const string ADDRESS = "127.0.0.1";
        TcpClient client;
        NetworkStream stream;


        
        public Form1()
        {
            InitializeComponent();
            client = new TcpClient(ADDRESS, PORT);
            stream = client.GetStream();

            // Avvia il thread per leggere continuamente i messaggi dal server
            new Thread(ReceiveMessages).Start();

            GeneraPezzi();
        }

        private void GeneraPezzi()
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

        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                try
                {
                    // Invia il messaggio del client al server
                    byte[] bytesToSend = Encoding.UTF8.GetBytes(textBox1.Text);
                    stream.Write(bytesToSend, 0, bytesToSend.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore nella connessione: " + ex.Message);
                }
            }).Start();
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
                        // Aggiorna la label con il messaggio ricevuto
                        string message = Encoding.UTF8.GetString(bytesToRead, 0, bytesRead);
                        label1.Invoke(new ChangeLabel(UpdateLabel), message);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore durante la lettura dei messaggi: " + ex.Message);
                    break;
                }
            }
        }

        // Metodo per aggiornare la label (chiamato dal thread principale)
        private void UpdateLabel(string message)
        {
            label1.Text = message;
        }
    }
}
