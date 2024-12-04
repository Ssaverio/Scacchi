using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Mossa
    {
        public byte inizio;
        public byte fine;

        public Mossa(byte inizio, byte fine)
        {
            this.inizio = inizio;
            this.fine = fine;
        }

        public Mossa(byte[] move)
        {
            inizio = move[0];
            fine = move[1];
        }
    }
}
