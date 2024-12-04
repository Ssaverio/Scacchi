using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal static class Scacchiera
    {
        private static byte[] penultimaPosizione = new byte[64];
        public static byte[] posizione = new byte[64];

        public static void GiocaMossa(Mossa mv)
        {
            penultimaPosizione = CopiaPosizione(posizione);
            (posizione[mv.inizio], posizione[mv.fine]) = (0, posizione[mv.inizio]);
        }

        public static void AnnullaUltimaMossa()
        {
            posizione = CopiaPosizione(penultimaPosizione);
        }

        public static bool PosizioneValida(byte turn)
        {
            return false;
        }
        public static byte[] CopiaPosizione(byte[] pos)
        {
            return pos.Select(x => x).ToArray();
        }

        public static void Dump()
        {
            Console.WriteLine("#######################");
            int i = 0;
            foreach (byte pezzo in posizione) {
                string s = pezzo > 9 ? " " : "  ";
                if (i == 7)
                {
                    Console.WriteLine(pezzo.ToString());
                    i = 0;
                } else
                {
                    Console.Write(pezzo.ToString() + s);
                    i++;
                }
            }
            Console.WriteLine("#######################");
        }

        public static void LoadFEN(string fen)
        {
            Dictionary<char, byte> pieceMap = new Dictionary<char, byte>()
            {
                ['k'] = Pezzo.BKing,
                ['p'] = Pezzo.BPawn,
                ['b'] = Pezzo.BBishop,
                ['n'] = Pezzo.BKnight,
                ['r'] = Pezzo.BRook,
                ['q'] = Pezzo.BQueen,

                ['K'] = Pezzo.WKing,
                ['P'] = Pezzo.WPawn,
                ['B'] = Pezzo.WBishop,
                ['N'] = Pezzo.WKnight,
                ['R'] = Pezzo.WRook,
                ['Q'] = Pezzo.WQueen,
            };
            int casella = 0;

            foreach (char c in fen.Split(' ')[0]) {
                if (c == '/') continue; 
                if (char.IsDigit(c)) 
                    casella += (byte)char.GetNumericValue(c);
                else
                {
                    byte pezzo = pieceMap[c];
                    posizione[casella] = pezzo;
                    casella++;
                }
            }
        }
    }
}
