namespace Boku_AI
{

    enum NodeType
    {
        Exact,
        LowerBound,
        UpperBound
    }

    struct TranspositionTableEntry
    {
        public int Score { get; set; }
        public string Move { get; set; }
        public int Depth { get; set; }
        public NodeType NodeType { get; set; }
    }

    class TranspositionTable
    {
        private Dictionary<ulong, TranspositionTableEntry> table = new Dictionary<ulong, TranspositionTableEntry>();

        public void Store(ulong key, int score, string mov, int depth, int maxDpeth, NodeType nodeType)
        {
            if (!table.ContainsKey(key) || (maxDpeth-depth) >= table[key].Depth)
            {
                table[key] = new TranspositionTableEntry { Score = score, Depth = (maxDpeth - depth), Move = mov, NodeType = nodeType };
            }
        }

        public bool TryRetrieve(ulong key, out TranspositionTableEntry entry)
        {
            return table.TryGetValue(key, out entry);
        }
    }
}
