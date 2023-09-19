using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Boku_AI
{
    internal class GameState
    {
        private bool isPlayer1Turn;
        bool lastWasCapture = false;

        public List<HexagonalButton> grid;
        public List<string> freeHexes = new List<string>() { "A1","A2","A3","A4","A5","A6","B1","B2","B3","B4","B5","B6","B7","C1","C2","C3","C4","C5","C6","C7","C8",
            "D1","D2","D3","D4","D5","D6","D7","D8","D9","E1","E2","E3","E4","E5","E6","E7","E8","E9","E10",
            "F1","F2","F3","F4","F5","F6","F7","F8","F9","F10","G2","G3","G4","G5","G6","G7","G8","G9","G10",
            "H3","H4","H5","H6","H7","H8","H9","H10","I4","I5","I6","I7","I8","I9","I10","J5","J6","J7","J8","J9","J10"
        };
        private List<string> whiteMarbles = new List<string>();
        private List<string> blackMarbles = new List<string>();

        private List<List<string>> freeHexesHistory = new List<List<string>>();
        private List<List<string>> whiteMarblesHistory = new List<List<string>>();
        private List<List<string>> blackMarblesHistory = new List<List<string>>();
        private List<string> takenLastRoundHistory = new List<string>();

        private char[] boardLetters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' };

        List<HexagonalButton> canBeTaken = new List<HexagonalButton>();
        public List<string> canBeTakenTags = new List<string>();
        public string takenLastRound = "";

        public GameState(List<HexagonalButton> startingGrid, bool player1turn = true)
        {
            if (startingGrid == null)
            {
                grid = new List<HexagonalButton>();
            }
            else
            {
                grid = startingGrid;
            }
            isPlayer1Turn = player1turn;
        }

        public GameState(GameState gsToCopy)
        {
            this.isPlayer1Turn = gsToCopy.isPlayer1Turn;
            this.grid = new List<HexagonalButton>(gsToCopy.grid);
            this.freeHexes = new List<string>(gsToCopy.freeHexes);
            this.whiteMarbles = new List<string>(gsToCopy.whiteMarbles);
            this.blackMarbles = new List<string>(gsToCopy.blackMarbles);
            this.freeHexesHistory = new List<List<string>>(gsToCopy.freeHexesHistory);
            this.whiteMarblesHistory = new List<List<string>>(gsToCopy.whiteMarblesHistory);
            this.blackMarblesHistory = new List<List<string>>(gsToCopy.blackMarblesHistory);
            this.takenLastRoundHistory = new List<string>(gsToCopy.takenLastRoundHistory);
            this.canBeTaken = new List<HexagonalButton>(gsToCopy.canBeTaken);
            this.canBeTakenTags = new List<string>(gsToCopy.canBeTakenTags);
            this.takenLastRound = gsToCopy.takenLastRound;
        }

        private void CorrectBoard()
        {
            foreach (HexagonalButton b in grid)
            {
                b.ClearMarble();
                if (whiteMarbles.Contains(b.tag))
                {
                    b.PlaceMarble(true);
                }
                else if (blackMarbles.Contains(b.tag))
                {
                    b.PlaceMarble(false);
                }
            }
        }

        public bool placeMarble(string hex_pos, bool logical = false)
        {
            HexagonalButton btnToPlace = null;
            foreach (HexagonalButton hex_btn in grid)
            {
                if (hex_btn.tag == hex_pos)
                {
                    btnToPlace = hex_btn;
                    break;
                }
            }
            if (canBeTaken.Count == 0)
            {
                if (!takenLastRound.Contains(hex_pos))
                {
                    //Normal move
                    Debug.WriteLine("Place");
                    lastWasCapture = false;
                    isPlayer1Turn = !isPlayer1Turn;
                    takenLastRound = "";
                    if (!logical)
                    {
                        freeHexesHistory.Add(new List<string>(freeHexes));
                        whiteMarblesHistory.Add(new List<string>(whiteMarbles));
                        blackMarblesHistory.Add(new List<string>(blackMarbles));
                        takenLastRoundHistory.Add(takenLastRound);
                        btnToPlace.PlaceMarble(!isPlayer1Turn);
                    }
                    freeHexes.Remove(hex_pos);
                    if (!isPlayer1Turn)
                    {
                        whiteMarbles.Add(hex_pos);
                    }
                    else
                    {
                        blackMarbles.Add(hex_pos);
                    }
                    if (!logical && freeHexesHistory.Count > 10)
                    {
                        whiteMarblesHistory.RemoveAt(0);
                        blackMarblesHistory.RemoveAt(0);
                        takenLastRoundHistory.RemoveAt(0);
                        freeHexesHistory.RemoveAt(0);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //Capture move
                lastWasCapture = true;
                if (btnToPlace != null && canBeTakenTags.Contains(hex_pos))
                {
                    freeHexes.Add(hex_pos);
                    whiteMarbles.Remove(hex_pos);
                    blackMarbles.Remove(hex_pos);
                    takenLastRound = hex_pos;
                    if (!logical)
                    {
                        btnToPlace.ClearMarble();
                    }
                    foreach (HexagonalButton b in canBeTaken)
                    {
                        if (!logical)
                        {
                            b.canBeTaken = false;
                            b.Invalidate();
                        }
                    }
                    canBeTaken.Clear();
                    canBeTakenTags.Clear();
                }

                if (canBeTaken.Count == 0)
                {
                    isPlayer1Turn = !isPlayer1Turn;
                }
                return true;
            }
        }

        public bool UndoState()
        {
            if (freeHexesHistory.Count > 0)
            {
                whiteMarbles = whiteMarblesHistory.Last();
                blackMarbles = blackMarblesHistory.Last();
                freeHexes = freeHexesHistory.Last();
                whiteMarblesHistory.RemoveAt(whiteMarblesHistory.Count - 1);
                blackMarblesHistory.RemoveAt(blackMarblesHistory.Count - 1);
                freeHexesHistory.RemoveAt(freeHexesHistory.Count - 1);
                CorrectBoard();
                if (canBeTaken.Count > 0)
                {
                    canBeTaken.Clear();
                }
                else
                {
                    isPlayer1Turn = !isPlayer1Turn;
                }
                if (takenLastRoundHistory.Count > 0)
                {
                    takenLastRound = takenLastRoundHistory.ElementAt(takenLastRoundHistory.Count - 1);
                    takenLastRoundHistory.RemoveAt(takenLastRoundHistory.Count - 1);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public int EvaluateBoard()
        {
            //Positive points for white and negative points for black are combined for an overall score of the board
            int score = 0;

            score += new Random().Next(-100,100);
            return score;
        }

        public int CheckGameEnded(string lastPlaced, bool isWhite)
        {
            //-1 Game hasnt ended
            //0 It is a draw
            //1 Player 1 won (White)
            //2 Player 2 won (Black)
            //3 (No clue what has happened...)
            int gameEnded = -1;
            bool whiteWon = false;
            bool blackWon = false;
            char letter = lastPlaced.ElementAt(0);
            int number = int.Parse(lastPlaced.Substring(1));
            int indexOfLetter = Array.IndexOf(boardLetters, letter);

            if (isWhite)
            {
                //Check for white win
                if (whiteMarbles.Count(str => str.Contains(letter)) >= 5)
                {
                    int matchCount = 1;
                    //Possibly 5 in a row with the same letter
                    int numberCopy = number;
                    while (numberCopy < 10)
                    {
                        if (whiteMarbles.Contains(letter.ToString() + (numberCopy + 1).ToString()))
                        {
                            matchCount++;
                        }
                        else
                        {
                            break;
                        }
                        numberCopy++;
                    }
                    if (matchCount < 5)
                    {
                        numberCopy = number;
                        while (numberCopy > 1)
                        {
                            if (whiteMarbles.Contains(letter.ToString() + (numberCopy - 1).ToString()))
                            {
                                matchCount++;
                            }
                            else
                            {
                                break;
                            }
                            numberCopy--;
                        }
                    }
                    if (matchCount >= 5)
                    {
                        whiteWon = true;
                    }
                }
                if (gameEnded == -1 && whiteMarbles.Count(str => str.Contains(number.ToString())) >= 5)
                {
                    //Possibly 5 in a row with the same number
                    int matchCount = 1;
                    int letterIndexCopy = indexOfLetter;
                    while (letterIndexCopy < 9)
                    {
                        if (whiteMarbles.Contains(boardLetters[letterIndexCopy + 1].ToString() + number.ToString()))
                        {
                            matchCount++;
                        }
                        else
                        {
                            break;
                        }
                        letterIndexCopy++;
                    }
                    if (matchCount < 5)
                    {
                        letterIndexCopy = indexOfLetter;
                        while (letterIndexCopy > 0)
                        {
                            if (whiteMarbles.Contains(boardLetters[letterIndexCopy - 1].ToString() + number.ToString()))
                            {
                                matchCount++;
                            }
                            else
                            {
                                break;
                            }
                            letterIndexCopy--;
                        }
                    }
                    if (matchCount >= 5)
                    {
                        whiteWon = true;
                    }
                }
                if (gameEnded == -1)
                {
                    //Possibly 5 in a row with incrementing letters and numbers
                    int matchCount = 1;
                    int numberCopy = number;
                    int letterIndexCopy = indexOfLetter;
                    while (numberCopy < 10 && letterIndexCopy < 9)
                    {
                        if (whiteMarbles.Contains(boardLetters[letterIndexCopy + 1].ToString() + (numberCopy + 1).ToString()))
                        {
                            matchCount++;
                        }
                        else
                        {
                            break;
                        }
                        numberCopy++;
                        letterIndexCopy++;
                    }
                    if (matchCount < 5)
                    {
                        numberCopy = number;
                        letterIndexCopy = indexOfLetter;
                        while (number > 0 && letterIndexCopy > 0)
                        {
                            if (whiteMarbles.Contains(boardLetters[letterIndexCopy - 1].ToString() + (number - 1).ToString()))
                            {
                                matchCount++;
                            }
                            else
                            {
                                break;
                            }
                            number--;
                            letterIndexCopy--;
                        }
                    }
                    if (matchCount >= 5)
                    {
                        whiteWon = true;
                    }
                }
            }
            else
            {
                //Check for black win
                if (blackMarbles.Count(str => str.Contains(letter)) >= 5)
                {
                    int matchCount = 1;
                    //Possibly 5 in a row with the same letter
                    int numberCopy = number;
                    while (numberCopy < 10)
                    {
                        if (blackMarbles.Contains(letter.ToString() + (numberCopy + 1).ToString()))
                        {
                            matchCount++;
                        }
                        else
                        {
                            break;
                        }
                        numberCopy++;
                    }
                    if (matchCount < 5)
                    {
                        numberCopy = number;
                        while (numberCopy > 1)
                        {
                            if (blackMarbles.Contains(letter.ToString() + (numberCopy - 1).ToString()))
                            {
                                matchCount++;
                            }
                            else
                            {
                                break;
                            }
                            numberCopy--;
                        }
                    }
                    if (matchCount >= 5)
                    {
                        blackWon = true;
                    }
                }
                if (gameEnded == -1 && blackMarbles.Count(str => str.Contains(number.ToString())) >= 5)
                {
                    //Possibly 5 in a row with the same number
                    int matchCount = 1;
                    int letterIndexCopy = indexOfLetter;
                    while (letterIndexCopy < 9)
                    {
                        if (blackMarbles.Contains(boardLetters[letterIndexCopy + 1].ToString() + number.ToString()))
                        {
                            matchCount++;
                        }
                        else
                        {
                            break;
                        }
                        letterIndexCopy++;
                    }
                    if (matchCount < 5)
                    {
                        letterIndexCopy = indexOfLetter;
                        while (letterIndexCopy > 0)
                        {
                            if (blackMarbles.Contains(boardLetters[letterIndexCopy - 1].ToString() + number.ToString()))
                            {
                                matchCount++;
                            }
                            else
                            {
                                break;
                            }
                            letterIndexCopy--;
                        }
                    }
                    if (matchCount >= 5)
                    {
                        blackWon = true;
                    }
                }
                if (gameEnded == -1)
                {
                    //Possibly 5 in a row with incrementing letters and numbers
                    int matchCount = 1;
                    int numberCopy = number;
                    int letterIndexCopy = indexOfLetter;
                    while (numberCopy < 10 && letterIndexCopy < 9)
                    {
                        if (blackMarbles.Contains(boardLetters[letterIndexCopy + 1].ToString() + (numberCopy + 1).ToString()))
                        {
                            matchCount++;
                        }
                        else
                        {
                            break;
                        }
                        numberCopy++;
                        letterIndexCopy++;
                    }
                    if (matchCount < 5)
                    {
                        numberCopy = number;
                        letterIndexCopy = indexOfLetter;
                        while (number > 0 && letterIndexCopy > 0)
                        {
                            if (blackMarbles.Contains(boardLetters[letterIndexCopy - 1].ToString() + (number - 1).ToString()))
                            {
                                matchCount++;
                            }
                            else
                            {
                                break;
                            }
                            number--;
                            letterIndexCopy--;
                        }
                    }
                    if (matchCount >= 5)
                    {
                        blackWon = true;
                    }
                }
            }

            if (whiteWon)
            {
                gameEnded = 1;
            }
            else if (blackWon)
            {
                gameEnded = 2;
            }
            else if (whiteMarbles.Count + blackMarbles.Count == 80 && !whiteWon && !blackWon)
            {
                //Game Ended With Draw
                gameEnded = 0;
            }

            return gameEnded;
        }

        public bool CheckCapture(string lastPlaced, bool isWhite, bool logical = false)
        {
            if (lastWasCapture)
            {
                return false;
            }
            List<List<String>> capturedPool = new List<List<String>>();
            char letter = lastPlaced.ElementAt(0);
            int number = int.Parse(lastPlaced.Substring(1));
            int indexOfLetter = Array.IndexOf(boardLetters, letter);
            if (isWhite)
            {
                //Check for capture with the same letter (Both ways)
                if (blackMarbles.Contains(letter + (number - 1).ToString()) && blackMarbles.Contains(letter + (number - 2).ToString()) && whiteMarbles.Contains(letter + (number - 3).ToString()))
                {
                    capturedPool.Add(new List<string>() { letter + (number - 1).ToString(), letter + (number - 2).ToString() });
                }
                if (blackMarbles.Contains(letter + (number + 1).ToString()) && blackMarbles.Contains(letter + (number + 2).ToString()) && whiteMarbles.Contains(letter + (number + 3).ToString()))
                {
                    capturedPool.Add(new List<string>() { letter + (number + 1).ToString(), letter + (number + 2).ToString() });
                }
                //Check for capture with the same number (Both ways)
                if (indexOfLetter - 3 >= 0 && blackMarbles.Contains(boardLetters[indexOfLetter - 1].ToString() + number.ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter - 2].ToString() + number.ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter - 3].ToString() + number.ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter - 1].ToString() + number.ToString(), boardLetters[indexOfLetter - 2].ToString() + number.ToString() });
                }
                if (indexOfLetter + 3 <= boardLetters.Length - 1 && blackMarbles.Contains(boardLetters[indexOfLetter + 1].ToString() + number.ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter + 2].ToString() + number.ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter + 3].ToString() + number.ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter + 1].ToString() + number.ToString(), boardLetters[indexOfLetter + 2].ToString() + number.ToString() });
                }
                //Check for capture vertically (Both ways)
                if (indexOfLetter - 3 >= 0 && blackMarbles.Contains(boardLetters[indexOfLetter - 1].ToString() + (number - 1).ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter - 2].ToString() + (number - 2).ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter - 3].ToString() + (number - 3).ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter - 1].ToString() + (number - 1).ToString(), boardLetters[indexOfLetter - 2].ToString() + (number - 2).ToString() });
                }
                if (indexOfLetter + 3 <= boardLetters.Length - 1 && blackMarbles.Contains(boardLetters[indexOfLetter + 1].ToString() + (number + 1).ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter + 2].ToString() + (number + 2).ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter + 3].ToString() + (number + 3).ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter + 1].ToString() + (number + 1).ToString(), boardLetters[indexOfLetter + 2].ToString() + (number + 2).ToString() });
                }
            }
            else
            {
                //Check for capture with the same letter (Both ways)
                if (whiteMarbles.Contains(letter + (number - 1).ToString()) && whiteMarbles.Contains(letter + (number - 2).ToString()) && blackMarbles.Contains(letter + (number - 3).ToString()))
                {
                    capturedPool.Add(new List<string>() { letter + (number - 1).ToString(), letter + (number - 2).ToString() });
                }
                if (whiteMarbles.Contains(letter + (number + 1).ToString()) && whiteMarbles.Contains(letter + (number + 2).ToString()) && blackMarbles.Contains(letter + (number + 3).ToString()))
                {
                    capturedPool.Add(new List<string>() { letter + (number + 1).ToString(), letter + (number + 2).ToString() });
                }
                //Check for capture with the same number (Both ways)
                if (indexOfLetter - 3 >= 0 && whiteMarbles.Contains(boardLetters[indexOfLetter - 1].ToString() + number.ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter - 2].ToString() + number.ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter - 3].ToString() + number.ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter - 1].ToString() + number.ToString(), boardLetters[indexOfLetter - 2].ToString() + number.ToString() });
                }
                if (indexOfLetter + 3 <= boardLetters.Length - 1 && whiteMarbles.Contains(boardLetters[indexOfLetter + 1].ToString() + number.ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter + 2].ToString() + number.ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter + 3].ToString() + number.ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter + 1].ToString() + number.ToString(), boardLetters[indexOfLetter + 2].ToString() + number.ToString() });
                }
                //Check for capture vertically (Both ways)
                if (indexOfLetter - 3 >= 0 && whiteMarbles.Contains(boardLetters[indexOfLetter - 1].ToString() + (number - 1).ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter - 2].ToString() + (number - 2).ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter - 3].ToString() + (number - 3).ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter - 1].ToString() + (number - 1).ToString(), boardLetters[indexOfLetter - 2].ToString() + (number - 2).ToString() });
                }
                if (indexOfLetter + 3 <= boardLetters.Length - 1 && whiteMarbles.Contains(boardLetters[indexOfLetter + 1].ToString() + (number + 1).ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter + 2].ToString() + (number + 2).ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter + 3].ToString() + (number + 3).ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter + 1].ToString() + (number + 1).ToString(), boardLetters[indexOfLetter + 2].ToString() + (number + 2).ToString() });
                }
            }

            if (capturedPool.Count > 0)
            {
                foreach (List<string> capList in capturedPool)
                {
                    foreach (string mrbl in capList)
                    {
                        foreach (HexagonalButton btn in grid)
                        {
                            if (btn.tag == mrbl)
                            {
                                if (!logical)
                                {
                                    btn.canBeTaken = true;
                                    btn.Invalidate();
                                }
                                canBeTaken.Add(btn);
                                canBeTakenTags.Add(mrbl);
                                break;
                            }
                        }
                    }
                }
                //Switch the turn back to previous player
                isPlayer1Turn = !isPlayer1Turn;
            }

            return capturedPool.Count > 0;
        }

        public void CheckImpossibleTurn()
        {
            if (blackMarbles.Count + whiteMarbles.Count == 79 && takenLastRound != "" && !whiteMarbles.Contains(takenLastRound) && !blackMarbles.Contains(takenLastRound))
            {
                //Skip player's turn because they can't make a move
                isPlayer1Turn = !isPlayer1Turn;
            }
        }

        public bool GetisPlayer1Turn()
        {
            return isPlayer1Turn;
        }

    }
}