using System;
using System.Collections.Generic;
using System.Linq;
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

        public BokuBot(int move_time = 5) {
            this.moveTime = move_time;
            this.haveMoreTime = true;
        }

        public string MakeMove(GameState state)
        {
            haveMoreTime = true;
            lastBestMove = "";
            //Start the countdown timer
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Task.Run(() => StartCountdown(cancellationToken));

            //Start MiniMax Search
            int timeoutMilliseconds = moveTime * 1000; // Convert moveTime to milliseconds
            Task<int> miniMaxTask = Task.Run(() => MiniMaxScore(true, state, "", 0), cancellationToken);

            //Check if the time ran out
            if (Task.WaitAny(miniMaxTask, Task.Delay(timeoutMilliseconds)) == 1)
            {
                //Time is up, stop the search
                cancellationTokenSource.Cancel();
                haveMoreTime = false;
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
