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

        public async Task<string> MakeMove(GameState state)
        {
            string bestMove = state.freeHexes.ElementAt(0);
            if (bestMove == state.takenLastRound) {
                bestMove = state.freeHexes.ElementAt(1);
            }
            timeIsUp = DateTime.Now.AddSeconds(moveTime);

            //Start NegaMax Search
            int topScore = int.MinValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            //Start tree search
            for (int maxDepth = 1; maxDepth < 80; maxDepth++)
            {
                int currentDepthTopScore = int.MinValue;
                string currentDepthTopMove = state.freeHexes.ElementAt(0);
                if (state.canBeTakenTags.Count==0) {
                    foreach (string move in state.freeHexes)
                    {
                        if (state.takenLastRound!=move) {
                            if (timeIsUp > DateTime.Now)
                            {
                                int score = -NegaMaxScore(isPlayer1, new GameState(state), move, 1, maxDepth,alpha,beta);
                                if (score > currentDepthTopScore) {
                                    currentDepthTopScore = score;
                                    currentDepthTopMove = move;
                                }
                                if (currentDepthTopScore > alpha) {
                                    alpha = currentDepthTopScore;
                                }
                                if (currentDepthTopScore > topScore)
                                {
                                    topScore = currentDepthTopScore;
                                    bestMove = move;
                                }
                                //Check if on the next depth we didnt found out that the previous depth best move is actually really bad
                                if (move == bestMove && topScore > currentDepthTopScore)
                                {
                                    bestMove = currentDepthTopMove;
                                    topScore = currentDepthTopScore;
                                }
                                if (currentDepthTopScore >= beta) {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                else {
                    bestMove = state.canBeTakenTags.ElementAt(0);
                    foreach (string move in state.canBeTakenTags)
                    {
                        if (timeIsUp > DateTime.Now)
                        {
                            int score = -NegaMaxScore(isPlayer1, new GameState(state), move, 1, maxDepth, alpha, beta);
                            if (score > currentDepthTopScore)
                            {
                                currentDepthTopScore = score;
                                currentDepthTopMove = move;
                            }
                            if (currentDepthTopScore > alpha)
                            {
                                alpha = currentDepthTopScore;
                            }
                            if (currentDepthTopScore > topScore)
                            {
                                topScore = currentDepthTopScore;
                                bestMove = move;
                            }
                            //Check if on the next depth we didnt found out that the previous depth best move is actually really bad
                            if (move == bestMove && topScore > currentDepthTopScore)
                            {
                                bestMove = currentDepthTopMove;
                                topScore = currentDepthTopScore;
                            }
                            if (currentDepthTopScore >= beta)
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                
            }

            return bestMove;
        }

        private int NegaMaxScore(bool isWhitePlayer, GameState currentState, string movPos, int depth, int maxDepth,int alpha, int beta) {
            //Apply move
            currentState.placeMarble(movPos,true);

            //Check if game is over
            int gameEnded = currentState.CheckGameEnded(movPos, isWhitePlayer);
            switch (gameEnded) {
                case 0:
                    //Draw
                    return 0;
                case 1:
                    //White won
                    if (isWhitePlayer)
                    {
                        return (100000 - depth);
                    }
                    else {
                        return -(100000 - depth);
                    }
                case 2:
                    //Black won
                    if (!isWhitePlayer)
                    {
                        return (100000 - depth);
                    }
                    else
                    {
                        return -(100000 - depth);
                    }
                default:
                    break;
            }
            //Get intermediary score
            int currentBoardScore = 0;
            //Check if it captured
            bool didCapture = currentState.CheckCapture(movPos,isWhitePlayer,true);
            if (didCapture) {
                //Choose which one to capture
                currentBoardScore += 800;
                int maxCapMoveValueScore = int.MinValue;
                int captureAlpha = int.MinValue;
                int captureBeta = int.MaxValue;
                string bestCapMove = currentState.canBeTakenTags.ElementAt(0);
                foreach (string capMove in currentState.canBeTakenTags)
                {
                    GameState capState = new GameState(currentState);
                    capState.placeMarble(capMove, true);
                    foreach (string move in capState.freeHexes)
                    {
                        if (timeIsUp > DateTime.Now)
                        {
                            int capMoveValue = NegaMaxScore(!isWhitePlayer, new GameState(capState), move, depth+1, depth + 2, captureAlpha, captureBeta);
                            if (capMoveValue > maxCapMoveValueScore)
                            {
                                maxCapMoveValueScore = capMoveValue;
                                bestCapMove = capMove;
                            }
                            if (maxCapMoveValueScore > captureAlpha) {
                                captureAlpha = maxCapMoveValueScore;
                            }
                            if (maxCapMoveValueScore >= captureBeta) {
                                break;
                            }
                        }
                        else {
                            break;
                        }
                    }

                }
                currentState.placeMarble(bestCapMove,true);
            }
            currentBoardScore += currentState.EvaluateBoard(isWhitePlayer);
            //Check if has time to go deeper
            if (timeIsUp <= DateTime.Now || depth>=maxDepth) {
                return currentBoardScore;
            }

            //Go deeper
            int maxScore = int.MinValue;
            foreach (string move in currentState.freeHexes)
            {
                if (timeIsUp > DateTime.Now)
                {
                    int moveValue = -NegaMaxScore(!isWhitePlayer, new GameState(currentState), move, depth+1, maxDepth,-beta, -alpha);
                    if (moveValue > maxScore)
                    {
                        maxScore = moveValue;
                    }
                    if (maxScore > alpha) {
                        alpha = maxScore;
                    }
                    if (maxScore >= beta) {
                        break;
                    }
                }
                else {
                    break;
                }
            }
            maxScore -= currentBoardScore;

            return maxScore;
        }

    }
}
