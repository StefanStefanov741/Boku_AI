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

        //Weights
        private int minValue = -999999999;
        private int maxValue = 999999999;
        private int winValue = 10000000;
        private int capValue = 1000;

        public BokuBot(bool isPl1, int move_time = 5)
        {
            this.moveTime = move_time;
            this.isPlayer1 = isPl1;
            this.timeIsUp = DateTime.Now.AddDays(10);
        }

        struct MoveStruct
        {
            public int score;
            public string move;
        }

        public async Task<string> MakeMove(GameState state)
        {
            MoveStruct bestMove = new MoveStruct();
            bestMove.move = state.freeHexes.ElementAt(0);
            if (bestMove.move == state.takenLastRound)
            {
                bestMove.move = state.freeHexes.ElementAt(1);
            }
            timeIsUp = DateTime.Now.AddSeconds(moveTime);

            //Start NegaMax Search
            bestMove.score = minValue;
            int alpha = minValue;
            int beta = maxValue;

            //Start tree search
            for (int maxDepth = 1; maxDepth <= 81; maxDepth++)
            {
                if (timeIsUp > DateTime.Now)
                {
                    MoveStruct currentDepthMove = NegaMaxScore(isPlayer1, new GameState(state), maxDepth, alpha, beta);
                    if (maxDepth == 2)
                    {
                        //At 1 deep the score could be really good for a bad position => always replace
                        bestMove.score = currentDepthMove.score;
                        bestMove.move = currentDepthMove.move;
                    }
                    else
                    {
                        if (currentDepthMove.score > bestMove.score)
                        {
                            bestMove.score = currentDepthMove.score;
                            bestMove.move = currentDepthMove.move;
                        }
                    }
                }
            }
            return bestMove.move;
        }

        private MoveStruct NegaMaxScore(bool isWhitePlayer, GameState entryState, int depth, int alpha, int beta)
        {
            MoveStruct bestMove = new MoveStruct();
            bestMove.score = minValue;
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
            else
            {
                iterationHexes = new List<string>(entryState.canBeTakenTags);
                bestMove.move = entryState.canBeTakenTags.ElementAt(1);
            }
            foreach (string move in iterationHexes)
            {
                GameState currentState = new GameState(entryState);
                if (currentState.takenLastRound != move)
                {
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
                                bestMove.score = winValue + depth * 1000;
                                return bestMove;
                            }
                            else
                            {
                                bestMove.move = move;
                                bestMove.score = -(winValue + depth * 1000);
                                return bestMove;
                            }
                        case 2:
                            //Black won
                            if (!isWhitePlayer)
                            {
                                bestMove.move = move;
                                bestMove.score = winValue + depth * 1000;
                                return bestMove;
                            }
                            else
                            {
                                bestMove.move = move;
                                bestMove.score = -(winValue + depth * 1000);
                                return bestMove;
                            }
                        default:
                            break;
                    }

                    //Check if it captured during this move
                    bool didCapture = currentState.CheckCapture(move, isWhitePlayer, true);
                    if (didCapture)
                    {
                        //Choose which one to capture
                        MoveStruct bestCapMove = new MoveStruct();
                        bestCapMove.move = currentState.canBeTakenTags.ElementAt(1);
                        bestCapMove.score = minValue;
                        int capAlpha = minValue;
                        int capBeta = maxValue;
                        foreach (string possibleCap in currentState.canBeTakenTags) {
                            GameState capState = new GameState(currentState);
                            capState.placeMarble(possibleCap, true);
                            int capScore = -NegaMaxScore(!isWhitePlayer, capState, 1, -capBeta, -capAlpha).score;
                            if (capScore > bestCapMove.score) {
                                bestCapMove.score = capScore;
                                bestCapMove.move = possibleCap;
                            }
                            if (bestCapMove.score > alpha)
                            {
                                alpha = bestCapMove.score;
                            }
                            if (bestCapMove.score >= beta)
                            {
                                break;
                            }
                        }

                        currentState.placeMarble(bestCapMove.move, true);

                    }

                    if (depth > 1)
                    {
                        if (timeIsUp < DateTime.Now) {
                            return bestMove;
                        }
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
                    else
                    {
                        //Evaluate so far
                        int currentBoardScore = currentState.EvaluateBoard(isWhitePlayer);
                        if ((currentBoardScore - depth * 300) > bestMove.score)
                        {
                            bestMove.score = currentBoardScore - depth * 300;
                            if (bestMove.score < 0)
                            {
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
