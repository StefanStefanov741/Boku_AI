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
        bool isPlayer1;
        DateTime timeIsUp;
        TranspositionTable tt;
        int overthinkingCounter;

        //Weights
        private static int winValue = 77777777;
        private static int minValue = -winValue;
        private static int maxValue = winValue;
        private static int captureBonus = 5000;

        public BokuBot(bool isPl1, int move_time = 5)
        {
            this.moveTime = move_time;
            this.isPlayer1 = isPl1;
            this.timeIsUp = DateTime.Now.AddDays(10);
            this.tt = new TranspositionTable();
            this.overthinkingCounter = 0;
        }

        public string MakeMove(GameState state)
        {
            overthinkingCounter = 0;
            MoveStruct bestMove = new MoveStruct(minValue, state.freeHexes.ElementAt(0));
            if (bestMove.move == state.takenLastRound)
            {
                bestMove.move = state.freeHexes.ElementAt(1);
            }
            timeIsUp = DateTime.Now.AddSeconds(moveTime);

            //Start NegaMax Search
            int alpha = minValue+5000;
            int beta = maxValue-5000;

            List<MoveStruct> possibleMoves;
            if (state.canBeTakenTags.Count == 0)
            {
                possibleMoves = new List<MoveStruct>();
                foreach (string m in state.freeHexes)
                {
                    possibleMoves.Add(new MoveStruct(minValue, m));
                }
            }
            else
            {
                possibleMoves = null;
            }
            int maxDepthLimit = 81;
            if (state.canBeTakenTags.Count > 0)
            {
                maxDepthLimit = 2;
            }
            //Start tree search
            for (int maxDepth = 1; maxDepth <= maxDepthLimit; maxDepth++)
            {
                if (timeIsUp > DateTime.Now || maxDepth < 4)
                {
                    if (possibleMoves != null && maxDepth > 1)
                    {
                        //Order moves if possible
                        possibleMoves = possibleMoves.OrderByDescending(move => move.score).ToList();
                    }

                    //Go deeper
                    MoveStruct currentDepthMove = NegaMaxScore(isPlayer1, new GameState(state), maxDepth, alpha, beta, maxDepth, possibleMoves);

                    //Windowing
                    if (currentDepthMove.score >= beta && currentDepthMove.score<winValue)
                    {
                        alpha = currentDepthMove.score;
                        beta = maxValue;
                        currentDepthMove = NegaMaxScore(isPlayer1, new GameState(state), maxDepth, alpha, beta, maxDepth, possibleMoves);
                    }
                    else if (currentDepthMove.score > minValue && currentDepthMove.score <= alpha)
                    {
                        alpha = minValue;
                        beta = currentDepthMove.score;
                        currentDepthMove = NegaMaxScore(isPlayer1, new GameState(state), maxDepth, alpha, beta, maxDepth, possibleMoves);
                    }

                    //Keep the deepest move
                    if (!currentDepthMove.ignoreMove && !currentDepthMove.nullMoveCutOff) {
                        bestMove.score = currentDepthMove.score;
                        bestMove.move = currentDepthMove.move;
                    }

                }
                else
                {
                    break;
                }
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
                    if (entryState.takenLastRound != "") {
                        iterationHexes.Remove(entryState.takenLastRound);
                    }
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
                    int gameEnded = -1;
                    if (entryState.canBeTakenTags.Count == 0) {
                        gameEnded = currentState.CheckGameEnded(iterationHexes[iterationIndex], isWhitePlayer);
                    }
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
                                bestMove.score = winValue - (initialMaxDepth-depth) * 2000;
                                //Store best move in the TT
                                tt.Store(zobristHash, bestMove.score, bestMove.move, depth, initialMaxDepth, bestMove.score <= alpha ? NodeType.UpperBound : bestMove.score >= beta ? NodeType.LowerBound : NodeType.Exact);

                                return bestMove;
                            }
                            else
                            {
                                bestMove.move = iterationHexes[iterationIndex];
                                bestMove.score = -(winValue - (initialMaxDepth - depth) * 2000);

                                return bestMove;
                            }
                        case 2:
                            //Black won
                            if (!isWhitePlayer)
                            {
                                bestMove.move = iterationHexes[iterationIndex];
                                bestMove.score = winValue - (initialMaxDepth - depth) * 2000;
                                //Store best move in the TT
                                tt.Store(zobristHash, bestMove.score, bestMove.move, depth, initialMaxDepth, bestMove.score <= alpha ? NodeType.UpperBound : bestMove.score >= beta ? NodeType.LowerBound : NodeType.Exact);

                                return bestMove;
                            }
                            else
                            {
                                bestMove.move = iterationHexes[iterationIndex];
                                bestMove.score = -(winValue - (initialMaxDepth - depth) * 2000);

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
                        string bestCapMove = currentState.chooseCapture(currentState.canBeTakenTags.ElementAt(0), currentState.canBeTakenTags.ElementAt(1), isWhitePlayer);
                        currentState.placeMarble(bestCapMove, true);
                        currentBoardScore += captureBonus;
                    }

                    if (depth > 1)
                    {
                        if (timeIsUp < DateTime.Now && initialMaxDepth>3) {
                            //Tag the move for ignoring if the time ran out before a complete search at the current depth
                            bestMove.ignoreMove = true;
                            return bestMove;
                        }
                        //Perform a null move
                        MoveStruct nullMoveResult = NegaMaxScore(!isPlayer1, new GameState(currentState), 1, -beta, -alpha, 1);

                        //Go deeper if the null move did not cause a cut off
                        if (-nullMoveResult.score < beta)
                        {
                            MoveStruct nextValue = NegaMaxScore(!isWhitePlayer, new GameState(currentState), depth - 1, -beta, -alpha, initialMaxDepth);
                            if (nextValue.ignoreMove)
                            {
                                bestMove.ignoreMove = true;
                                return bestMove;
                            }
                            if (nextValue.nullMoveCutOff) {
                                bestMove.nullMoveCutOff = true;
                                return bestMove;
                            }
                            int nextScore = -nextValue.score;
                            int currentEvaluation = currentState.EvaluateBoard(isWhitePlayer) - (initialMaxDepth - depth) * 200;
                            if (nextScore + currentBoardScore + currentEvaluation > bestMove.score)
                            {
                                bestMove.score = nextScore + currentBoardScore + currentEvaluation;
                                bestMove.move = iterationHexes[iterationIndex];
                            }
                            if (movesList != null)
                            {
                                movesList.ElementAt(iterationIndex).score = nextScore + currentBoardScore + currentEvaluation;
                            }
                            if (nextScore > alpha)
                            {
                                alpha = nextScore;
                            }
                            if (nextScore >= beta)
                            {

                                if (currentState.canBeTakenTags.Count == 0)
                                {
                                    //Store best move in the TT
                                    tt.Store(zobristHash, bestMove.score, bestMove.move, depth, initialMaxDepth, bestMove.score <= alpha ? NodeType.UpperBound : bestMove.score >= beta ? NodeType.LowerBound : NodeType.Exact);
                                }
                                break;
                            }
                        }
                        else {
                            bestMove.nullMoveCutOff = true;
                            return bestMove;
                        }
                    }
                    else
                    {
                        //Evaluate so far
                        currentBoardScore += currentState.EvaluateBoard(isWhitePlayer) - (initialMaxDepth - depth) * 200;
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
