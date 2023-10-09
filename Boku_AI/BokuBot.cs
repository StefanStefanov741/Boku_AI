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
        TranspositionTable tt = new TranspositionTable();

        //Weights
        private static int winValue = 10000000;
        private static int minValue = -winValue;
        private static int maxValue = winValue;
        private static int captureBonus = 10000;

        public BokuBot(bool isPl1, int move_time = 5)
        {
            this.moveTime = move_time;
            this.isPlayer1 = isPl1;
            this.timeIsUp = DateTime.Now.AddDays(10);
        }

        public async Task<string> MakeMove(GameState state)
        {
            MoveStruct bestMove = new MoveStruct(minValue, state.freeHexes.ElementAt(0));
            if (bestMove.move == state.takenLastRound)
            {
                bestMove.move = state.freeHexes.ElementAt(1);
            }
            timeIsUp = DateTime.Now.AddSeconds(moveTime);

            //Start NegaMax Search
            int alpha = minValue;
            int beta = maxValue;

            List<MoveStruct> possibleMoves;
            if (state.canBeTakenTags.Count == 0)
            {
                possibleMoves = new List<MoveStruct>();
                foreach (string m in state.freeHexes)
                {
                    possibleMoves.Add(new MoveStruct(minValue, m));
                }
            }
            else {
                possibleMoves = null;
            }

            //Start tree search
            for (int maxDepth = 1; maxDepth <= 81; maxDepth++)
            {
                if (timeIsUp > DateTime.Now && bestMove.score<winValue-500)
                {
                    if (possibleMoves!=null && maxDepth > 1) {
                        //Order moves if possible
                        possibleMoves = possibleMoves.OrderByDescending(move => move.score).ToList();
                    }
                    //Go deeper
                    MoveStruct currentDepthMove = NegaMaxScore(isPlayer1, new GameState(state), maxDepth, alpha, beta,maxDepth, possibleMoves);
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
                            //Calculate the Zobrist hash for the entry state
                            ulong zobristHash = state.GetZobristHash();
                            bestMove.score = currentDepthMove.score;
                            bestMove.move = currentDepthMove.move;
                            if (maxDepth > 1) {
                                //Store best move in the TT
                                tt.Store(zobristHash, bestMove.score, bestMove.move, maxDepth, maxDepth, bestMove.score <= alpha ? NodeType.UpperBound : bestMove.score >= beta ? NodeType.LowerBound : NodeType.Exact);
                            }
                        }
                    }
                }
                else {
                    break;
                }
            }

            if (state.taken2RoundsAgo != "" && state.taken2RoundsAgo != bestMove.move && bestMove.score < winValue / 2) {
                bestMove.move = state.taken2RoundsAgo;
            }
            return bestMove.move;
        }

        private MoveStruct NegaMaxScore(bool isWhitePlayer, GameState entryState, int depth, int alpha, int beta, int initialMaxDepth,List<MoveStruct>movesList=null)
        {
            //Calculate the Zobrist hash for the entry state
            ulong zobristHash = entryState.GetZobristHash();

            TranspositionTableEntry ttEntry;

            //Check if a TT entry exists for this state
            if (tt.TryRetrieve(zobristHash, out ttEntry) && ttEntry.Depth >= depth)
            {
                //Use the entry information
                if (ttEntry.NodeType == NodeType.Exact)
                {
                    //Return if it was the exact value
                    return new MoveStruct(ttEntry.Score, ttEntry.Move);
                }
                else if (ttEntry.NodeType == NodeType.LowerBound)
                {
                    //Change alpha if it was a lower bound and its bigger
                    alpha = Math.Max(alpha, ttEntry.Score);
                }
                else
                {
                    //Change beta if it was a upper bound and its lower
                    beta = Math.Min(beta, ttEntry.Score);
                }
                //Prune if possible
                if (alpha >= beta)
                {
                    return new MoveStruct(ttEntry.Score, ttEntry.Move);
                }
            }

            //Continue searching
            MoveStruct bestMove = new MoveStruct(minValue, "");
            List<string> iterationHexes;
            if (movesList != null)
            {
                iterationHexes = new List<string>();
                foreach (MoveStruct m in movesList)
                {
                    iterationHexes.Add(m.move);
                }
                bestMove.move = iterationHexes.ElementAt(0);
                if (bestMove.move == entryState.takenLastRound)
                {
                    bestMove.move = iterationHexes.ElementAt(1);
                }
            }
            else {
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
            }

            for (int iterationIndex = 0; iterationIndex < iterationHexes.Count; iterationIndex++)
            {
                int currentBoardScore = 0;
                GameState currentState = new GameState(entryState);
                if (currentState.takenLastRound != iterationHexes[iterationIndex])
                {
                    //Apply move
                    currentState.placeMarble(iterationHexes[iterationIndex], true);
                    //Check if game is over
                    int gameEnded = currentState.CheckGameEnded(iterationHexes[iterationIndex], isWhitePlayer);
                    switch (gameEnded)
                    {
                        case 0:
                            //Draw
                            bestMove.move = iterationHexes[iterationIndex];
                            bestMove.score = 0;
                            return bestMove;
                        case 1:
                            //White won
                            if (isWhitePlayer)
                            {
                                bestMove.move = iterationHexes[iterationIndex];
                                bestMove.score = winValue + depth * 1000;
                                //Store best move in the TT
                                tt.Store(zobristHash, bestMove.score, bestMove.move, depth, initialMaxDepth, bestMove.score <= alpha ? NodeType.UpperBound : bestMove.score >= beta ? NodeType.LowerBound : NodeType.Exact);
                                return bestMove;
                            }
                            else
                            {
                                bestMove.move = iterationHexes[iterationIndex];
                                bestMove.score = -(winValue + depth * 1000);
                                return bestMove;
                            }
                        case 2:
                            //Black won
                            if (!isWhitePlayer)
                            {
                                bestMove.move = iterationHexes[iterationIndex];
                                bestMove.score = winValue + depth * 1000;
                                //Store best move in the TT
                                tt.Store(zobristHash, bestMove.score, bestMove.move, depth, initialMaxDepth, bestMove.score <= alpha ? NodeType.UpperBound : bestMove.score >= beta ? NodeType.LowerBound : NodeType.Exact);
                                return bestMove;
                            }
                            else
                            {
                                bestMove.move = iterationHexes[iterationIndex];
                                bestMove.score = -(winValue + depth * 1000);
                                return bestMove;
                            }
                        default:
                            break;
                    }

                    //Check if it captured during this move
                    bool didCapture = currentState.CheckCapture(iterationHexes[iterationIndex], isWhitePlayer, true);
                    if (didCapture)
                    {
                        //Choose which one to capture
                        MoveStruct bestCapMove = new MoveStruct(minValue, currentState.canBeTakenTags.ElementAt(1));
                        int capAlpha = minValue;
                        int capBeta = maxValue;
                        foreach (string possibleCap in currentState.canBeTakenTags) {
                            GameState capState = new GameState(currentState);
                            capState.placeMarble(possibleCap, true);
                            int capPoints = capState.EvaluateBoard(isWhitePlayer);
                            int nextScore = -NegaMaxScore(!isWhitePlayer, new GameState(currentState), 2, -beta, -alpha, 2).score;
                            capPoints += nextScore;
                            if (capPoints > bestCapMove.score) {
                                bestCapMove.score = capPoints;
                                bestCapMove.move = possibleCap;
                            }
                        }
                        currentBoardScore += captureBonus;
                        currentState.placeMarble(bestCapMove.move, true);
                        
                    }

                    if (depth > 1)
                    {
                        if (timeIsUp < DateTime.Now) {
                            if (isWhitePlayer == isPlayer1)
                            {
                                if (bestMove.score < winValue - 10000) {
                                    bestMove.score = minValue;
                                }
                            }
                            else {
                                if (bestMove.score < winValue - 10000) {
                                    bestMove.score = winValue;
                                }
                            }
                            return bestMove;
                        }
                        //Go Deeper
                        MoveStruct nextValue = NegaMaxScore(!isWhitePlayer, new GameState(currentState), depth - 1, -beta, -alpha, initialMaxDepth);
                        int nextScore = -nextValue.score;
                        int currentEvaluation = currentState.EvaluateBoard(isWhitePlayer);
                        if (nextScore + currentBoardScore + currentEvaluation> bestMove.score)
                        {
                            bestMove.score = nextScore + currentBoardScore + currentEvaluation;
                            bestMove.move = iterationHexes[iterationIndex];
                        }
                        if (movesList != null)
                        {
                            movesList.ElementAt(iterationIndex).score = nextScore +currentBoardScore + currentEvaluation;
                        }
                        if (nextScore > alpha)
                        {
                            alpha = nextScore;
                        }
                        if (nextScore >= beta)
                        {
                            //Store best move in the TT
                            tt.Store(zobristHash, bestMove.score, bestMove.move, depth, initialMaxDepth, bestMove.score <= alpha ? NodeType.UpperBound : bestMove.score >= beta ? NodeType.LowerBound : NodeType.Exact);
                            break;
                        }
                    }
                    else
                    {
                        //Evaluate so far
                        currentBoardScore += currentState.EvaluateBoard(isWhitePlayer);
                        if ((currentBoardScore + depth * 300) > bestMove.score)
                        {
                            bestMove.score = currentBoardScore + depth * 300;
                            if (bestMove.score < 0)
                            {
                                bestMove.score = 0;
                            }
                            bestMove.move = iterationHexes[iterationIndex];
                        }
                        if (movesList != null)
                        {
                            movesList.ElementAt(iterationIndex).score = currentBoardScore + depth * 300;
                        }
                    }
                }
            }
            return bestMove;
        }

    }
}
