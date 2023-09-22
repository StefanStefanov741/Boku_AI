using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static System.Windows.Forms.AxHost;

namespace Boku_AI
{
    internal class BokuBot
    {
        int moveTime;
        CancellationTokenSource cancellationTokenSource;
        bool isPlayer1;
        DateTime timeIsUp = DateTime.Now.AddDays(10);

        public BokuBot(bool isPl1, int move_time = 5) {
            this.moveTime = move_time;
            this.isPlayer1 = isPl1;
            this.timeIsUp = DateTime.Now.AddDays(10);
        }

        struct MoveStruct{
            public int score;
            public string move;
        }

        public async Task<string> MakeMove(GameState state)
        {
            MoveStruct bestMove = new MoveStruct();
            bestMove.move = state.freeHexes.ElementAt(0);
            if (bestMove.move == state.takenLastRound) {
                bestMove.move = state.freeHexes.ElementAt(1);
            }
            timeIsUp = DateTime.Now.AddSeconds(moveTime);

            //Start NegaMax Search
            bestMove.score = -999999999;
            int alpha = -999999999;
            int beta = 999999999;

            //Start tree search
            for (int maxDepth = 1; maxDepth <= 81; maxDepth++)
            {
                if (timeIsUp > DateTime.Now) {
                    MoveStruct currentDepthMove = NegaMaxScore(isPlayer1, new GameState(state), maxDepth, alpha, beta);
                    bestMove.score = currentDepthMove.score;
                    bestMove.move = currentDepthMove.move;
                } 
            }
            return bestMove.move;
        }

        private MoveStruct NegaMaxScore(bool isWhitePlayer, GameState entryState, int depth,int alpha, int beta) {
            MoveStruct bestMove = new MoveStruct();
            bestMove.score = -999999999;
            List<string> iterationHexes;
            if (entryState.canBeTakenTags.Count == 0)
            {
                iterationHexes = new List<string>(entryState.freeHexes);
                bestMove.move = iterationHexes.ElementAt(0);
                if (bestMove.move == entryState.takenLastRound)
                {
                    bestMove.move = iterationHexes.ElementAt(1);
                }
            }
            else {
                iterationHexes = new List<string>(entryState.canBeTakenTags);
                bestMove.move = entryState.canBeTakenTags.ElementAt(1);
            }
            foreach (string move in iterationHexes)
            {
                GameState currentState = new GameState(entryState);
                int currentBoardScore = 0;
                if (currentState.takenLastRound != move) {
                    //Apply move
                    currentState.placeMarble(move, true);
                    //Check if game is over
                    int gameEnded = currentState.CheckGameEnded(move, isWhitePlayer);
                    switch (gameEnded)
                    {
                        case 0:
                            //Draw
                            bestMove.move = move;
                            bestMove.score = 0;
                            return bestMove;
                        case 1:
                            //White won
                            if (isWhitePlayer)
                            {
                                bestMove.move = move;
                                bestMove.score = 10000000 + depth*1000;
                                return bestMove;
                            }
                            else
                            {
                                bestMove.move = move;
                                bestMove.score = -(10000000 + depth*1000);
                                return bestMove;
                            }
                        case 2:
                            //Black won
                            if (!isWhitePlayer)
                            {
                                bestMove.move = move;
                                bestMove.score = 10000000 + depth*1000;
                                return bestMove;
                            }
                            else
                            {
                                bestMove.move = move;
                                bestMove.score = -(10000000 + depth*1000);
                                return bestMove;
                            }
                        default:
                            break;
                    }

                    //Get intermediary score
                    //Check if it captured
                    bool didCapture = currentState.CheckCapture(move, isWhitePlayer, true);
                    if (didCapture)
                    {
                        //Choose which one to capture
                        currentBoardScore += 800;
                        int maxCapMoveValueScore = -999999999;
                        int captureAlpha = -999999999;
                        int captureBeta = 999999999;
                        string bestCapMove = currentState.canBeTakenTags.ElementAt(1); //Replace with logic for deciding on the best capture move

                        currentState.placeMarble(bestCapMove, true);
                    }

                    if (depth > 1)
                    {
                        //Go Deeper
                        MoveStruct nextValue = NegaMaxScore(!isWhitePlayer, new GameState(currentState), depth - 1, -beta, -alpha);
                        int nextScore = -nextValue.score;
                        if (nextScore > bestMove.score)
                        {
                            bestMove.score = nextScore;
                            bestMove.move = move;
                        }
                        if (bestMove.score > alpha)
                        {
                            alpha = bestMove.score;
                        }
                        if (bestMove.score >= beta)
                        {
                            break;
                        }
                    }
                    else {
                        //Evaluate so far
                        currentBoardScore += currentState.EvaluateBoard(isWhitePlayer);
                        if ((currentBoardScore - depth * 300) > bestMove.score)
                        {
                            bestMove.score = currentBoardScore-depth*300;
                            if (bestMove.score < 0) {
                                bestMove.score = 0;
                            }
                            bestMove.move = move;
                        }
                    }
                }
            }

            return bestMove;
        }

    }
}
