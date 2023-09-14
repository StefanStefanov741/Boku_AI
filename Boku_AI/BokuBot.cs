using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace Boku_AI
{
    internal class BokuBot
    {
        int moveTime;
        CancellationTokenSource cancellationTokenSource;
        bool isPlayer1;
        bool hasMoreTime = true;

        public BokuBot(bool isPl1, int move_time = 5) {
            this.moveTime = move_time;
            this.isPlayer1 = isPl1;
            this.hasMoreTime = true;
        }

        public async Task<string> MakeMove(GameState state)
        {
            string bestMove = state.freeHexes.ElementAt(0);
            //Start the countdown timer
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Task.Run(() => StartCountdown(cancellationToken));

            //Start MiniMax Search
            int timeoutMilliseconds = moveTime * 1000;


            int topScore = int.MinValue;

            //Start tree search
            foreach (string move in state.freeHexes)
            {
                Task<int> scoreTask = Task.Run(() => MiniMaxScore(isPlayer1, new GameState(state), move, 1), cancellationToken);

                //Check if the time ran out
                if (Task.WaitAny(scoreTask, Task.Delay(timeoutMilliseconds)) == 1)
                {
                    //Time is up, stop the search
                    //cancellationTokenSource.Cancel();
                    hasMoreTime = false;
                }
                int score = await scoreTask;

                if (score > topScore)
                {
                    topScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private int MiniMaxScore(bool isWhitePlayer, GameState currentState, string movPos, int depth) {
            //Apply move
            currentState.placeMarble(movPos);

            //Check if game is over
            int gameEnded = currentState.CheckGameEnded(movPos, isWhitePlayer);
            switch (gameEnded) {
                case 0:
                    //Draw
                    return -100;
                case 1:
                    //White won
                    if (isWhitePlayer) {
                        return (10000-depth);
                    }
                    else {
                        return -(11000 + depth);
                    }
                case 2:
                    //Black won
                    if (!isWhitePlayer)
                    {
                        return (10000-depth);
                    }
                    else
                    {
                        return - (11000 + depth);
                    }
                default:
                    break;
            }
            //Get intermediary score
            int bestScoreSoFar = 0;
            int worstScoreSoFar = 0;

            //Check if has time to go deeper
            if (!hasMoreTime) {
                return isWhitePlayer == isPlayer1 ? bestScoreSoFar : worstScoreSoFar;
            }
            //Go deeper
            depth++;
            int maxScore = int.MinValue;
            int minScore = int.MaxValue;
            foreach (string move in currentState.freeHexes)
            {
                int moveValue = MiniMaxScore(!isWhitePlayer, new GameState(currentState), move, depth);
                if (moveValue > maxScore)
                {
                    maxScore = moveValue;
                }
                if (moveValue < minScore)
                {
                    minScore = moveValue;
                }
            }

            //Return appropriate score depending on wether or not this is my turn
            return isWhitePlayer==isPlayer1 ? maxScore : minScore;
        }

        private void StartCountdown(CancellationToken cancellationToken)
        {
            Thread.Sleep(moveTime * 1000);
            //Stop the search
            cancellationTokenSource.Cancel();
        }

    }
}
