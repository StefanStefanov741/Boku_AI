﻿namespace Boku_AI
{
    public class MoveStruct
    {
        public int score;
        public string move;
        public bool ignoreMove;
        public bool nullMoveCutOff;

        public MoveStruct(int sc, string mv) {
            score = sc;
            move = mv;
            ignoreMove = false;
            nullMoveCutOff = false;
        }
    }
}
