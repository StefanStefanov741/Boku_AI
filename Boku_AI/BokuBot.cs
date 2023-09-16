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

            //Start MiniMax Search
            int topScore = int.MinValue;

            //Start tree search
            for (int maxDepth = 1; maxDepth < 3/*change to 81*/; maxDepth++)
            {
                int currentDepthTopScore = int.MinValue;
                string currentDepthTopMove = state.freeHexes.ElementAt(0);
                if (state.canBeTakenTags.Count==0) {
                    foreach (string move in state.freeHexes)
                    {
                        if (state.takenLastRound!=move) {
                            if (timeIsUp > DateTime.Now)
                            {
                                int score = MiniMaxScore(isPlayer1, new GameState(state), move, 1, maxDepth);
                                if (score > currentDepthTopScore) {
                                    currentDepthTopScore = score;
                                    currentDepthTopMove = move;
                                }
                                if (score > topScore)
                                {
                                    topScore = score;
                                    bestMove = move;
                                }
                                //Check if on the next depth we didnt found out that the previous depth best move is actually really bad
                                if (move == bestMove && topScore > currentDepthTopScore) {
                                    bestMove = currentDepthTopMove;
                                    topScore = currentDepthTopScore;
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
                            int score = MiniMaxScore(isPlayer1, new GameState(state), move, 1, maxDepth);

                            if (score > topScore)
                            {
                                topScore = score;
                                bestMove = move;
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

        private int MiniMaxScore(bool isWhitePlayer, GameState currentState, string movPos, int depth, int maxDepth) {
            //Apply move
            currentState.placeMarble(movPos,true);

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
                    if (isWhitePlayer)
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
            int currentBoardScore = 0;
            //Check if it captured
            bool didCapture = currentState.CheckCapture(movPos,isWhitePlayer,true);
            if (didCapture) {
                //Choose which one to capture
                if (isWhitePlayer==isPlayer1)
                {
                    currentBoardScore += 2000;
                }
                else
                {
                    currentBoardScore -= 2000;
                }
                int maxCapMoveValueScore = int.MinValue;
                string bestCapMove = currentState.canBeTakenTags.ElementAt(0);
                foreach (string capMove in currentState.canBeTakenTags)
                {
                    GameState capState = new GameState(currentState);
                    capState.placeMarble(capMove, true);
                    foreach (string move in capState.freeHexes)
                    {
                        if (timeIsUp > DateTime.Now)
                        {
                            int capMoveValue = MiniMaxScore(!isWhitePlayer, new GameState(capState), move, depth+1, depth + 2);
                            if (capMoveValue > maxCapMoveValueScore)
                            {
                                maxCapMoveValueScore = capMoveValue;
                                bestCapMove = capMove;
                            }
                        }
                        else {
                            break;
                        }
                    }

                }
                currentState.placeMarble(bestCapMove,true);
            }
            if (isWhitePlayer==isPlayer1)
            {
                currentBoardScore += currentState.ScoreBoard();
            }
            else
            {
                currentBoardScore -= currentState.ScoreBoard();
            }
            //Check if has time to go deeper
            if (timeIsUp <= DateTime.Now || depth>=maxDepth) {       
               return isWhitePlayer == isPlayer1 ? currentBoardScore : -currentBoardScore;
            }

            //Go deeper
            int maxScore = int.MinValue;

            depth++;
            foreach (string move in currentState.freeHexes)
            {
                if (currentState.canBeTakenTags.Count > 0) {
                    int wtf = 1;
                }
                if (timeIsUp > DateTime.Now)
                {
                    int moveValue = MiniMaxScore(!isWhitePlayer, new GameState(currentState), move, depth, maxDepth);
                    if (moveValue > maxScore)
                    {
                        maxScore = moveValue;
                    }
                }
                else {
                    break;
                }
            }

            maxScore += currentBoardScore;

            return -maxScore;
        }

    }
}
