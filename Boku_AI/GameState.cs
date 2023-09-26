using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Formats.Asn1.AsnWriter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace Boku_AI
{
    internal class GameState
    {
        private bool isPlayer1Turn;
        public bool lastWasCapture = false;

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

        //Weights
        static int line2 = 400;
        static int line4 = 600;
        static int line3 = 800;
        static int stopLine1 = 100;
        static int stopLine2 = 1000;
        static int stopLine3 = 5000;
        static int stopLine4 = 8000;

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
                    lastWasCapture = false;
                    isPlayer1Turn = !isPlayer1Turn;
                    if (!logical)
                    {
                        freeHexesHistory.Add(new List<string>(freeHexes));
                        whiteMarblesHistory.Add(new List<string>(whiteMarbles));
                        blackMarblesHistory.Add(new List<string>(blackMarbles));
                        takenLastRoundHistory.Add(takenLastRound);
                        btnToPlace.PlaceMarble(!isPlayer1Turn);
                    }
                    takenLastRound = "";
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

        private int EvaluateLine1(List<string> evaluatedLine1, List<string>myMarbles, List<string>enemyMarbles,int number, char letter) {
            int score = 0;
            int numberCopy = number;
            int count = 1;
            int startPos = number;
            int endPos = number;

            while (numberCopy < 10)
            {
                if (myMarbles.Contains(letter.ToString() + (numberCopy + 1).ToString()))
                {
                    evaluatedLine1.Add(letter.ToString() + (numberCopy + 1).ToString());
                    count++;
                    endPos = (numberCopy + 1);
                }
                else
                {
                    //Check if it messes with the enemy line
                    if (enemyMarbles.Contains(letter.ToString() + (numberCopy + 1).ToString()))
                    {
                        //Check for block 2
                        if ((numberCopy + 2 <= 10) && enemyMarbles.Contains(letter.ToString() + (numberCopy + 2).ToString()))
                        {
                            //Check for block 3
                            if ((numberCopy + 3 <= 10) && enemyMarbles.Contains(letter.ToString() + (numberCopy + 3).ToString()))
                            {
                                //Check for block 4
                                if ((numberCopy + 4 <= 10) && enemyMarbles.Contains(letter.ToString() + (numberCopy + 4).ToString()))
                                {
                                    score += stopLine4;
                                }
                                else
                                {
                                    score += stopLine3;
                                }
                            }
                            else
                            {
                                score += stopLine2;
                            }
                        }
                        else
                        {
                            score += stopLine1;
                        }
                    }
                    break;
                }
                numberCopy++;
            }
            numberCopy = number;
            while (numberCopy > 1)
            {
                if (myMarbles.Contains(letter.ToString() + (numberCopy - 1).ToString()))
                {
                    evaluatedLine1.Add(letter.ToString() + (numberCopy - 1).ToString());
                    startPos = (numberCopy - 1);
                    count++;
                }
                else
                {
                    //Check if it messes with the enemy line
                    if (enemyMarbles.Contains(letter.ToString() + (numberCopy - 1).ToString()))
                    {
                        //Check for block 2
                        if ((numberCopy - 2 >= 1) && enemyMarbles.Contains(letter.ToString() + (numberCopy - 2).ToString()))
                        {
                            //Check for block 3
                            if ((numberCopy - 3 >= 1) && enemyMarbles.Contains(letter.ToString() + (numberCopy - 3).ToString()))
                            {
                                //Check for block 4
                                if ((numberCopy - 4 >= 1) && enemyMarbles.Contains(letter.ToString() + (numberCopy - 4).ToString()))
                                {
                                    score += stopLine4;
                                }
                                else
                                {
                                    score += stopLine3;
                                }
                            }
                            else
                            {
                                score += stopLine2;
                            }
                        }
                        else
                        {
                            score += stopLine1;
                        }
                    }
                    break;
                }
                numberCopy--;
            }
            if (count == 3)
            {
                score += line3;
            }
            else if (count == 4)
            {
                score += line4;
            }
            else if (count == 2)
            {
                if (enemyMarbles.Contains(letter + (startPos - 1).ToString()) || enemyMarbles.Contains(letter + (endPos + 1).ToString()))
                {
                    score -= line2;
                }
                else
                {
                    score += line2;
                }
            }
            return score;
        }
        private int EvaluateLine2(List<string> evaluatedLine2, List<string> myMarbles, List<string> enemyMarbles, int number, int indexOfLetter)
        {
            int score = 0;
            int indexOfLetterCopy = indexOfLetter;
            int count = 1;
            int startPos = indexOfLetterCopy;
            int endPos = indexOfLetterCopy;
            bool startedLine = false;

            while (indexOfLetterCopy < 9)
            {
                if (myMarbles.Contains(boardLetters[indexOfLetterCopy + 1].ToString() + number.ToString()))
                {
                    evaluatedLine2.Add(boardLetters[indexOfLetterCopy + 1].ToString() + number.ToString());
                    count++;
                    endPos = (indexOfLetterCopy + 1);
                    startedLine = true;
                }
                else
                {
                    if (startedLine)
                    {
                        break;
                    }
                    if (enemyMarbles.Contains(boardLetters[indexOfLetterCopy + 1].ToString() + number.ToString()))
                    {
                        //Check for block 2
                        if ((indexOfLetterCopy + 2 <= 9) && enemyMarbles.Contains(boardLetters[indexOfLetterCopy + 2].ToString() + number.ToString()))
                        {
                            //Check for block 3
                            if ((indexOfLetterCopy + 3 <= 9) && enemyMarbles.Contains(boardLetters[indexOfLetterCopy + 3].ToString() + number.ToString()))
                            {
                                //Check for block 4
                                if ((indexOfLetterCopy + 4 <= 9) && enemyMarbles.Contains(boardLetters[indexOfLetterCopy + 4].ToString() + number.ToString()))
                                {
                                    score += stopLine4;
                                }
                                else
                                {
                                    score += stopLine3;
                                }
                            }
                            else
                            {
                                score += stopLine2;
                            }
                        }
                        else
                        {
                            score += stopLine1;
                        }
                    }
                    break;
                }
                indexOfLetterCopy++;
            }
            indexOfLetterCopy = indexOfLetter;
            while (indexOfLetterCopy > 0)
            {
                if (myMarbles.Contains(boardLetters[indexOfLetterCopy - 1].ToString() + number.ToString()))
                {
                    evaluatedLine2.Add(boardLetters[indexOfLetterCopy - 1].ToString() + number.ToString());
                    startPos = (indexOfLetterCopy - 1);
                    count++;
                }
                else
                {
                    if (enemyMarbles.Contains(boardLetters[indexOfLetterCopy - 1].ToString() + number.ToString()))
                    {
                        //Check for block 2
                        if ((indexOfLetterCopy - 2 >= 0) && enemyMarbles.Contains(boardLetters[indexOfLetterCopy - 2].ToString() + number.ToString()))
                        {
                            //Check for block 3
                            if ((indexOfLetterCopy - 3 >= 0) && enemyMarbles.Contains(boardLetters[indexOfLetterCopy - 3].ToString() + number.ToString()))
                            {
                                //Check for block 4
                                if ((indexOfLetterCopy - 4 >= 0) && enemyMarbles.Contains(boardLetters[indexOfLetterCopy - 4].ToString() + number.ToString()))
                                {
                                    score += stopLine4;
                                }
                                else
                                {
                                    score += stopLine3;
                                }
                            }
                            else
                            {
                                score += stopLine2;
                            }
                        }
                        else
                        {
                            score += stopLine1;
                        }
                    }
                    break;
                }
                indexOfLetterCopy--;
            }
            if (count == 3)
            {
                score += line3;
            }
            else if (count == 4)
            {
                score += line4;
            }
            else if (count == 2)
            {
                if ((startPos - 1 >= 0 && enemyMarbles.Contains(boardLetters[startPos - 1].ToString() + number.ToString())) || (endPos + 1 <= 9 && enemyMarbles.Contains(boardLetters[endPos + 1].ToString() + number.ToString())))
                {
                    score -= line2;
                }
                else
                {
                    score += line2;
                }
            }
            return score;
        }
        private int EvaluateLine3(List<string> evaluatedLine3, List<string> myMarbles, List<string> enemyMarbles, int number, int indexOfLetter)
        {
            int score = 0;
            int numberCopy = number;
            int indexOfLetterCopy = indexOfLetter;
            int count = 1;
            int startPosLetter = indexOfLetterCopy;
            int startPosNumber = numberCopy;
            int endPosLetter = indexOfLetterCopy;
            int endPosNumber = numberCopy;
            while (numberCopy < 10 && indexOfLetterCopy < 9)
            {
                if (myMarbles.Contains(boardLetters[indexOfLetterCopy + 1].ToString() + (numberCopy + 1).ToString()))
                {
                    evaluatedLine3.Add(boardLetters[indexOfLetterCopy + 1].ToString() + (numberCopy + 1).ToString());
                    count++;
                    endPosNumber = numberCopy;
                    endPosLetter = indexOfLetterCopy;
                }
                else
                {
                    if (enemyMarbles.Contains(boardLetters[indexOfLetterCopy + 1].ToString() + (numberCopy + 1).ToString()))
                    {
                        //Check for block 2
                        if ((indexOfLetterCopy + 2 <= 9) && numberCopy + 2 <= 10 && enemyMarbles.Contains(boardLetters[indexOfLetterCopy + 2].ToString() + (numberCopy + 2).ToString()))
                        {
                            //Check for block 3
                            if ((indexOfLetterCopy + 3 <= 9) && numberCopy + 3 <= 10 && enemyMarbles.Contains(boardLetters[indexOfLetterCopy + 3].ToString() + (numberCopy + 3).ToString()))
                            {
                                //Check for block 4
                                if ((indexOfLetterCopy + 4 <= 9) && numberCopy + 4 <= 10 && enemyMarbles.Contains(boardLetters[indexOfLetterCopy + 4].ToString() + (numberCopy + 4).ToString()))
                                {
                                    score += stopLine4;
                                }
                                else
                                {
                                    score += stopLine3;
                                }
                            }
                            else
                            {
                                score += stopLine2;
                            }
                        }
                        else
                        {
                            score += stopLine1;
                        }
                    }
                    break;
                }
                numberCopy++;
                indexOfLetterCopy++;
            }
            numberCopy = number;
            indexOfLetterCopy = indexOfLetter;
            while (numberCopy > 0 && indexOfLetterCopy > 0)
            {
                if (myMarbles.Contains(boardLetters[indexOfLetterCopy - 1].ToString() + (numberCopy - 1).ToString()))
                {
                    evaluatedLine3.Add(boardLetters[indexOfLetterCopy - 1].ToString() + (numberCopy - 1).ToString());
                    count++;
                    startPosNumber = numberCopy;
                    startPosLetter = indexOfLetterCopy;
                }
                else
                {
                    if (enemyMarbles.Contains(boardLetters[indexOfLetterCopy - 1].ToString() + (numberCopy - 1).ToString()))
                    {
                        //Check for block 2
                        if ((indexOfLetterCopy - 2 >= 0) && numberCopy - 2 >= 1 && enemyMarbles.Contains(boardLetters[indexOfLetterCopy - 2].ToString() + (numberCopy - 2).ToString()))
                        {
                            //Check for block 3
                            if ((indexOfLetterCopy - 3 >= 0) && numberCopy - 3 >= 1 && enemyMarbles.Contains(boardLetters[indexOfLetterCopy - 3].ToString() + (numberCopy - 3).ToString()))
                            {
                                //Check for block 4
                                if ((indexOfLetterCopy - 4 >= 0) && numberCopy - 4 >= 1 && enemyMarbles.Contains(boardLetters[indexOfLetterCopy - 4].ToString() + (numberCopy - 4).ToString()))
                                {
                                    score += stopLine4;
                                }
                                else
                                {
                                    score += stopLine3;
                                }
                            }
                            else
                            {
                                score += stopLine2;
                            }
                        }
                        else
                        {
                            score += stopLine1;
                        }
                    }
                    break;
                }
                numberCopy--;
                indexOfLetterCopy--;
            }
            if (count == 3)
            {
                score += line3;
            }
            else if (count == 4)
            {
                score += line4;
            }
            else if (count == 2)
            {
                if ((startPosLetter - 1 >= 0 && enemyMarbles.Contains(boardLetters[startPosLetter - 1].ToString() + startPosNumber.ToString())) || (endPosLetter + 1 <= 9 && enemyMarbles.Contains(boardLetters[endPosLetter + 1].ToString() + endPosNumber.ToString())))
                {
                    score -= line2;
                }
                else
                {
                    score += line2;
                }
            }
            return score;
        }
        public int EvaluateBoard(bool isWhitePlayer)
        {
            //Evaluate board score
            int score = 0;
            
            List<string> myMarbles;
            List<string> enemyMarbles;
            List<string> evaluatedLine1 = new List<string>();
            List<string> evaluatedLine2 = new List<string>();
            List<string> evaluatedLine3 = new List<string>();
            Task[] evaluationTasks = new Task[3];

            if (isWhitePlayer)
            {
                myMarbles = new List<string>(whiteMarbles);
                enemyMarbles = new List<string>(blackMarbles);
                score += (whiteMarbles.Count() - blackMarbles.Count())*20;
            }
            else
            {
                myMarbles = new List<string>(blackMarbles);
                enemyMarbles = new List<string>(whiteMarbles);
                score += ((blackMarbles.Count()+1) - whiteMarbles.Count()) * 20;
            }

            foreach (string mrbl in myMarbles)
            {
                char letter = mrbl.ElementAt(0);
                int number = int.Parse(mrbl.Substring(1));
                int indexOfLetter = Array.IndexOf(boardLetters, letter);
                int score1 = 0;
                int score2 = 0;
                int score3 = 0;
                //Same letter, different number
                if (!evaluatedLine1.Contains(mrbl))
                {
                    evaluationTasks[0] = Task.Run(() =>
                    {
                        evaluatedLine1.Add(mrbl);
                        score1 += EvaluateLine1(evaluatedLine1,myMarbles,enemyMarbles,number,letter);
                    }
                    );
                }
                //Same number, different letter
                if (!evaluatedLine2.Contains(mrbl))
                {
                    evaluationTasks[1] = Task.Run(() =>
                    {
                        evaluatedLine2.Add(mrbl);
                        score2 += EvaluateLine2(evaluatedLine2, myMarbles, enemyMarbles, number, indexOfLetter);
                    }
                    );
                }
                //Incrementing number and letter
                if (!evaluatedLine3.Contains(mrbl))
                {
                    evaluationTasks[2] = Task.Run(() =>
                    {
                        evaluatedLine3.Add(mrbl);
                        score3 += EvaluateLine3(evaluatedLine3, myMarbles, enemyMarbles, number, indexOfLetter);
                    }
                    );
                }
                Task.WaitAll(evaluationTasks);
                score = score + score1 + score2 + score3;
            }
            
            //Get player's current board score
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