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

        private static byte enPassant = 65;

        public static void GiocaMossa(Mossa mv)
        {
            penultimaPosizione = CopiaPosizione(posizione);
            (posizione[mv.inizio], posizione[mv.fine]) = (0, posizione[mv.inizio]);
        }

        public static void AnnullaUltimaMossa()
        {
            posizione = CopiaPosizione(penultimaPosizione);
        }

        public static bool CasellaContieneStessoColore(byte pezzo, byte turno)
        {
            return pezzo > 6 * turno && pezzo < 7 + 7 * turno;
        }

        public static bool PosizioneValida(Mossa mv, byte turno)
        {
            // Uso l'array "penultimaPosizione" perché quando muovo la casella di inizio mossa diventa 0
            byte pezzoInizio = penultimaPosizione[mv.inizio];
            byte pezzoFine = penultimaPosizione[mv.fine];

            // Il giocatore non può muovere i pezzi dell'altro o selezionare caselle vuote come primo click
            if (pezzoInizio <= 6 * turno || pezzoInizio > 6 + 6 * turno) return false;

            // Il giocatore non può muovere un pezzo dove è presente un altro dello stesso colore
            if (CasellaContieneStessoColore(pezzoFine, turno)) return false;

            // Controllo le mosse legali per il pezzo che ho mosso
            byte[] mosseLegali = MosseLegali(pezzoInizio, turno, mv.inizio);

            Console.WriteLine("-----");
            foreach (byte x in mosseLegali) Console.WriteLine(x);
            Console.WriteLine("-----");

            //if (!mosseLegali.Contains(mv.fine)) return false;

            return true;
        }

        public static byte[] MosseLegali(byte pezzo, byte turno, byte posizione)
        {
            switch (pezzo)
            {
                case byte x when x == Pezzo.WBishop || x == Pezzo.BBishop: return GeneraMosseDiagonali(posizione, turno);
                case byte x when x == Pezzo.WRook || x == Pezzo.BRook: return GeneraMosseLaterali(posizione, turno);
                case byte x when x == Pezzo.WKnight || x == Pezzo.BKnight: return GeneraMosseCavallo(posizione, turno);
                case byte x when x == Pezzo.WPawn || x == Pezzo.BPawn: return GeneraMossePedone(posizione, turno);
                case byte x when x == Pezzo.WKing || x == Pezzo.BKing: return GeneraMosseRe(posizione, turno);
                case byte x when x == Pezzo.WQueen || x == Pezzo.BQueen:
                    return GeneraMosseLaterali(posizione, turno).Concat(GeneraMosseDiagonali(posizione, turno)).ToArray();
                default: return new byte[0];
            }
        }

        public static byte[] GeneraMosseRe(byte posizione, byte turno)
        {
            int[] offsets = new int[8] { -9, -8, -7, -1, 1, 7, 8, 9 };
            HashSet<byte> mosse = new HashSet<byte>();

            int rigaInizio = posizione / 8;
            int colonnaInizio = posizione % 8;

            foreach (int offset in offsets)
            {
                if (rigaInizio == 0 && (offset <= -7)) continue;
                if (rigaInizio == 7 && (offset >= 7)) continue;
                if (colonnaInizio == 0 && (offset == -9 || offset == -1 || offset == 7)) continue;
                if (colonnaInizio == 7 && (offset == 9 || offset == 1 || offset == -7)) continue;

                byte casella = (byte)(posizione + offset);

                if (CasellaContieneStessoColore(penultimaPosizione[casella], turno)) continue;

                mosse.Add(casella);
            }

            return mosse.ToArray();
        }

        public static byte[] GeneraMossePedone(byte posizione, byte turno)
        {
            int[] offsets = new int[8] { -17, -10, 6, 15, -6, 10, -15, 17 };
            HashSet<byte> mosse = new HashSet<byte>();

            int rigaInizio = posizione / 8;
            int colonnaInizio = posizione % 8;

            foreach (int offset in offsets)
            {

            }

            return mosse.ToArray();
        }

        public static byte[] GeneraMosseCavallo(byte posizione, byte turno)
        {
            int[] offsets = new int[8] { -17, -10, 6, 15, -6, 10, -15, 17  };
            HashSet<byte> mosse = new HashSet<byte>();

            int rigaInizio = posizione / 8;
            int colonnaInizio = posizione % 8;

            foreach (int offset in offsets)
            {
                if (rigaInizio == 0 && offset <= -6) continue;
                if (rigaInizio == 1 && offset <= -15) continue;
                if (rigaInizio == 6 && offset >= 15) continue;
                if (rigaInizio == 7 && offset >= 6) continue;
                if (colonnaInizio == 0 && (offset == -17 || offset == -10 || offset == 6 || offset == 15)) continue;
                if (colonnaInizio == 1 && (offset == -10 || offset == 6)) continue;
                if (colonnaInizio == 6 && (offset == 10 || offset == -6)) continue;
                if (colonnaInizio == 7 && (offset == 17 || offset == 10 || offset == -6 || offset == -15)) continue;

                byte casella = (byte)(posizione + offset);

                if (CasellaContieneStessoColore(penultimaPosizione[casella], turno)) continue;

                mosse.Add(casella);
            }

            return mosse.ToArray();
        }

        public static byte[] GeneraMosseDiagonali(byte posizione, byte turno)
        {
            int[] offsets = new int[4] { -9, -7, 7, 9 };
            HashSet<byte> mosse = new HashSet<byte>();

            int rigaInizio = posizione / 8;
            int colonnaInizio = posizione % 8;

            foreach (int offset in offsets)
            {
                // Controllo se la posizione iniziale è ai bordi della scacchiera ed escludo a priori alcune direzioni
                if (rigaInizio == 0 && offset < 0) continue;
                if (rigaInizio == 7 && offset > 0) continue;
                if (colonnaInizio == 0 && (offset == -9 || offset == 7)) continue;
                if (colonnaInizio == 7 && (offset == 9 || offset == -7)) continue;

                for (int i = posizione + offset; i >= 0 && i < 64; i += offset)
                {
                    byte casella = (byte)i;

                    int riga = casella / 8;
                    int colonna = casella % 8;

                    // Controllo se la diagonale interseca con un pezzo dello stesso colore
                    if (CasellaContieneStessoColore(penultimaPosizione[casella], turno)) break;

                    // Controllo che la diagonale non esca dalla scacchiera o che si espanda oltre una casella non vuota
                    bool casellaAlBordo = riga == 0 || colonna == 0 || riga == 7 || colonna == 7;
                    if (casellaAlBordo || penultimaPosizione[casella] != 0)
                    {

                        // Aggiungo la mossa e cambio diagonale
                        mosse.Add(casella);
                        break;
                    }

                    // Altrimenti aggiungo la mossa
                    mosse.Add(casella);
                }
            }

            return mosse.ToArray();
        }        
        
        public static byte[] GeneraMosseLaterali(byte posizione, byte turno)
        {
            int[] offsets = new int[4] { -8, -1, 1, 8 };

            HashSet<byte> mosse = new HashSet<byte>();

            int rigaInizio = posizione / 8;
            int colonnaInizio = posizione % 8;

            foreach (int offset in offsets)
            {
                // Controllo se la posizione iniziale è ai bordi della scacchiera ed escludo a priori alcune direzioni
                if (rigaInizio == 0 && offset == -8) continue;
                if (rigaInizio == 7 && offset == 8) continue;
                if (colonnaInizio == 0 && offset == -1) continue;
                if (colonnaInizio == 7 && offset == 1) continue;

                for (int i = posizione + offset; i >= 0 && i < 64; i += offset)
                {
                    byte casella = (byte)i;

                    int riga = casella / 8;
                    int colonna = casella % 8;

                    // Controllo se la riga interseca con un pezzo dello stesso colore
                    if (CasellaContieneStessoColore(penultimaPosizione[casella], turno)) break;

                    // Controllo che la riga non esca dalla scacchiera o che si espanda oltre una casella non vuota.
                    // Il controllo viene effettuato solo sulle righe in cui non parte il pezzo.
                    bool casellaAlBordo =
                        (riga == 0 && rigaInizio != 0) ||
                        (colonna == 0 && colonnaInizio != 0) ||
                        (riga == 7 && rigaInizio != 7) ||
                        (colonna == 7 && colonnaInizio != 7);

                    if (casellaAlBordo || penultimaPosizione[casella] != 0)
                    {
                        // Aggiungo la mossa e cambio riga
                        mosse.Add(casella);
                        break;
                    }

                    // Altrimenti aggiungo la mossa
                    mosse.Add(casella);
                }
            }
            
            return mosse.ToArray();
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
