using System;

namespace Boku_AI
{
    public class ZobristKeys
    {
        private List<string> hexes;
        public Dictionary<string, ulong> BlackKeys { get; private set; }
        public Dictionary<string, ulong> WhiteKeys { get; private set; }
        public Dictionary<string, ulong> TakenLastRoundKeys { get; private set; }

        public ulong isWhiteTurnKey;
        public ulong isBlackTurnKey;

        Random rnd;
        byte[] buffer = new byte[8];

        public ZobristKeys()
        {
            InitializeKeys();
        }

        private void InitializeKeys()
        {
            rnd = new Random();
            hexes = new List<string>(AllHexes.hexes);
            BlackKeys = new Dictionary<string, ulong>();
            WhiteKeys = new Dictionary<string, ulong>();
            TakenLastRoundKeys = new Dictionary<string, ulong>();
            isWhiteTurnKey = GenerateRandom64BitKey();
            isBlackTurnKey = GenerateRandom64BitKey();
            TakenLastRoundKeys[""] = GenerateRandom64BitKey();
            foreach (string pos in hexes) {
                BlackKeys[pos] = GenerateRandom64BitKey();
                WhiteKeys[pos] = GenerateRandom64BitKey();
                TakenLastRoundKeys[pos] = GenerateRandom64BitKey();
            }
        }

        private ulong GenerateRandom64BitKey()
        {
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

    }

}
