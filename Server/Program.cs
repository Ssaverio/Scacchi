using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace Server
{
    internal class Program
    {
        const int PORT_NO = 50000;
        const string SERVER_IP = "127.0.0.1";
        const int MAX_CLIENTS = 2;  // Limite massimo di client
        static int currentClients = 0;  // Contatore dei client connessi
        static List<TcpClient> connectedClients = new List<TcpClient>();  // Lista dei client connessi

        static void Main(string[] args)
        {
            //---listen at the specified IP and port no.---
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);
            Console.WriteLine($"Listening on port {PORT_NO}");
            listener.Start();

            while (true)
            {
                try
                {
                    //---accept new clients only if the limit is not reached---
                    if (currentClients < MAX_CLIENTS)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        Interlocked.Increment(ref currentClients);  // Incrementa il contatore dei client
                        Console.WriteLine($"New client connected. Current clients: {currentClients}");

                        // Aggiungi il client alla lista dei connessi
                        lock (connectedClients)
                        {
                            connectedClients.Add(client);
                        }

                        //---handle the client in a new thread---
                        Thread clientThread = new Thread(HandleClient);
                        clientThread.Start(client);
                    }
                    else
                    {
                        // Max clients connected, reject the third client
                        Console.WriteLine("Max clients reached. Rejecting new connection...");
                        TcpClient rejectedClient = listener.AcceptTcpClient(); // Accept but immediately close
                        rejectedClient.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream nwStream = client.GetStream();
            byte[] buffer = new byte[4096];

            try
            {
                while (true)
                {
                    int bytesRead = nwStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Client disconnected.");
                        break; // Client disconnected
                    }

                    // Directly use the buffer data
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received: " + message);

                    if (message == "exit")
                    {
                        // Send exit message to the client
                        byte[] exitMessage = Encoding.UTF8.GetBytes("Uscita...");
                        Console.WriteLine("Uscita...");
                        nwStream.Write(exitMessage, 0, exitMessage.Length);
                        break; // End communication with client
                    }

                    // Invia il messaggio a tutti i client connessi, incluso il mittente
                    SendToAllClients(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // Rimuovi il client dalla lista quando si disconnette
                lock (connectedClients)
                {
                    connectedClients.Remove(client);
                }

                client.Close();
                Interlocked.Decrement(ref currentClients);  // Decrementa il contatore quando il client si disconnette
                Console.WriteLine($"Client disconnected. Current clients: {currentClients}");
            }
        }

        // Funzione che invia un messaggio a tutti i client connessi
        static void SendToAllClients(string message)
        {
            // Converti il messaggio in un array di byte
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Invia il messaggio a tutti i client connessi, incluso il mittente
            lock (connectedClients)
            {
                foreach (TcpClient client in connectedClients)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(messageBytes, 0, messageBytes.Length);
                        Console.WriteLine("Sent to client: " + message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending message to client: {ex.Message}");
                    }
                }
            }
        }
    }
}