using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
    internal class Program
    {
        const int PORT_NO = 50000;
        const string SERVER_IP = "127.0.0.1";
        const int MAX_CLIENTS = 2;
        static int currentClients = 0;
        static List<TcpClient> connectedClients = new List<TcpClient>();

        const string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        static byte turn = 1;

        static void Main(string[] args)
        {
            Scacchiera.LoadFEN(startFen);
            Scacchiera.Dump();

            StartServer();
        }

        static void StartServer()
        {
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);
            Console.WriteLine($"Listening on port {PORT_NO}");
            listener.Start();

            while (true)
            {
                try
                {
                    if (currentClients < MAX_CLIENTS)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        Interlocked.Increment(ref currentClients);
                        Console.WriteLine($"New client connected. Current clients: {currentClients}");

                        lock (connectedClients)
                        {
                            connectedClients.Add(client);
                        }
                        Thread clientThread = new Thread(HandleClient);
                        clientThread.Start(client);
                    }
                    else
                    {
                        Console.WriteLine("Max clients reached. Rejecting new connection...");
                        TcpClient rejectedClient = listener.AcceptTcpClient();
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
            NetworkStream stream = client.GetStream();

            if (currentClients == 1)
                stream.Write(new byte[1] {1}, 0, 1);
            else if (currentClients == 2)
                stream.Write(new byte[1] {0}, 0, 1);

            stream.Read(new byte[1], 0, 1);

            try
            {
                while (true)
                {
                    if (currentClients == 2 && client.Connected)
                    {
                        byte[] send = new byte[1] { turn }.Concat(Scacchiera.posizione).ToArray();
                        Console.WriteLine("Invio dei dati...");
                        stream.Write (send, 0, send.Length);

                        byte[] buffer = new byte[64];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        Console.WriteLine("Risposta arrivata");

                    } else Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                lock (connectedClients)
                {
                    connectedClients.Remove(client);
                }

                client.Close();
                Interlocked.Decrement(ref currentClients);
                Console.WriteLine($"Client disconnected. Current clients: {currentClients}");
            }
        }

        static void SendToAllClients(byte[] buffer)
        {
            lock (connectedClients)
            {
                foreach (TcpClient client in connectedClients)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        Console.WriteLine($"Invio dei dati...");
                        stream.Write(buffer, 0, buffer.Length);
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