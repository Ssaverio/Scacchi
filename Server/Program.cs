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

        static TcpClient biancoClient, neroClient;
        static NetworkStream bianco, nero;

        const string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        static byte turno = 1;

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
                    biancoClient = listener.AcceptTcpClient();
                    bianco = biancoClient.GetStream();
                    Console.WriteLine("Giocatore bianco connesso");

                    bianco.Write(new byte[1] { 1 }, 0, 1);
                    Console.WriteLine("Mando al giocatore bianco il turno 1");

                    neroClient = listener.AcceptTcpClient();
                    nero = neroClient.GetStream();
                    Console.WriteLine("CLient nero connesso");

                    nero.Write(new byte[1] { 0 }, 0, 1);
                    Console.WriteLine("Mando al giocatore nero il turno 0");

                    byte[] buffer = new byte[65];
                    byte[] bufferMossa = new byte[2];
                    int byteLetti;

                    InviaPosizione();

                    while (true)
                    {
                        byteLetti = 0;

                        if (turno == 1)
                        {
                            Console.WriteLine("Aspettando mossa del bianco...");

                            byteLetti = bianco.Read(bufferMossa, 0, 2);
                            
                            Mossa move = new Mossa(bufferMossa);
                            Console.WriteLine($"Bianco ha mosso: {move.inizio} -> {move.fine}");
                            VerificaMossa(move);
                        } else if (turno == 0)
                        {
                            Console.WriteLine("Aspettando mossa del nero...");

                            byteLetti = nero.Read(bufferMossa, 0, 2);
                            
                            Mossa move = new Mossa(bufferMossa);
                            Console.WriteLine($"Nero ha mosso: {move.inizio} -> {move.fine}");
                            VerificaMossa(move);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    biancoClient.Close();
                    neroClient.Close();
                    listener.Stop();
                    Console.ReadKey();
                }
            }
        }

        private static void VerificaMossa(Mossa mv)
        {
            Scacchiera.GiocaMossa(mv);

            bool mossaValida = Scacchiera.PosizioneValida(turno);
            if (mossaValida)
            {
                Console.WriteLine("Mossa valida!");
                CambiaTurno();
                Scacchiera.Dump();
                InviaPosizione();
            }
            else
            {
                Console.WriteLine("Mossa non valida!");
                Scacchiera.AnnullaUltimaMossa();
                InviaPosizione();
            }
        }

        private static void InviaPosizione()
        {
            byte[] pos = new byte[1] { turno }.Concat(Scacchiera.posizione).ToArray();
            bianco.Write(pos, 0, 65);
            Console.WriteLine("Mando posizione al giocatore bianco");
            nero.Write(pos, 0, 65);
            Console.WriteLine("Mando posizione al giocatore nero");
        }

        private static void CambiaTurno()
        {
            turno = (byte)(turno == 1 ? 0 : 1);
        }
    }
}