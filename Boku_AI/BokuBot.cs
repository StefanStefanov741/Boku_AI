using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Boku_AI
{
    internal class BokuBot
    {
        int moveTime;
        bool haveMoreTime;
        CancellationTokenSource cancellationTokenSource;
        string lastBestMove = "";
        bool isPlayer1;

        public BokuBot(bool isPl1, int move_time = 5) {
            this.moveTime = move_time;
            this.haveMoreTime = true;
            this.isPlayer1 = isPl1;
        }

        public async Task<string> MakeMove(GameState state)
        {
            haveMoreTime = true;
            lastBestMove = state.freeHexes.ElementAt(0);
            //Start the countdown timer
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Task.Run(() => StartCountdown(cancellationToken));

            //Start MiniMax Search
            int timeoutMilliseconds = moveTime * 1000;


            int topScore = int.MinValue;

            //Start tree search
            foreach (var move in state.freeHexes)
            {
                Task<int> scoreTask = Task.Run(() => MiniMaxScore(isPlayer1, new GameState(state), "", 0), cancellationToken);

                //Check if the time ran out
                if (Task.WaitAny(scoreTask, Task.Delay(timeoutMilliseconds)) == 1)
                {
                    //Time is up, stop the search
                    cancellationTokenSource.Cancel();
                    haveMoreTime = false;
                }
                int score = await scoreTask;

                if (score > topScore)
                {
                    topScore = score;
                    lastBestMove = move;
                }
            }

            return lastBestMove;
        }

        private int MiniMaxScore(bool isWhitePlayer, GameState currentState, string movPos, int depth) {
            if (!haveMoreTime)
            {
                //Stop Now!
                return 0;
            }

            //Keep searching

            return 0;
        }

        private void StartCountdown(CancellationToken cancellationToken)
        {
            Thread.Sleep(moveTime * 1000);
            //Stop the search
            cancellationTokenSource.Cancel();
        }

    }
}
