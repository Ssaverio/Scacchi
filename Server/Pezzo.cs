using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal static class Pezzo
    {
        public const byte WKing = 12;
        public const byte WQueen = 11;
        public const byte WRook = 10;
        public const byte WBishop = 9;
        public const byte WKnight = 8;
        public const byte WPawn = 7;
               
        public const byte BKing = 6;
        public const byte BQueen = 5;
        public const byte BRook = 4;
        public const byte BBishop = 3;
        public const byte BKnight = 2;
        public const byte BPawn = 1;

        public static bool Bianco(byte pezzo)
        {
            return pezzo >= 7 && pezzo <= 12;
        }

        public static bool Nero(byte pezzo)
        {
            return pezzo >= 1 && pezzo <= 6;
        }
    }
}