using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boku_AI
{
    internal class GameState
    {
        private bool isPlayer1Turn;

        public List<HexagonalButton> grid;
        private List<string> whiteMarbles = new List<string>();
        private List<string> blackMarbles = new List<string>();

        private List<List<HexagonalButton>> boardHistory = new List<List<HexagonalButton>>();
        private List<List<string>> whiteMarblesHistory = new List<List<string>>();
        private List<List<string>> blackMarblesHistory = new List<List<string>>();
        private List<string> takenLastRoundHistory = new List<string>();

        private char[] boardLetters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' };

        List<HexagonalButton> canBeTaken = new List<HexagonalButton>();
        List<string> canBeTakenTags = new List<string>();
        string takenLastRound = "";

        public GameState(List<HexagonalButton>startingGrid,bool player1turn= true) {
            if (startingGrid == null)
            {
                grid = new List<HexagonalButton>();
            }
            else {
                grid = startingGrid;
            }
            isPlayer1Turn = player1turn;
        }

        public bool placeMarble(string hex_pos) {
            HexagonalButton btnToPlace = null;
            foreach (HexagonalButton hex_btn in grid) {
                if (hex_btn.tag == hex_pos) {
                    btnToPlace = hex_btn;
                    break;
                }
            }
            if (canBeTaken.Count == 0)
            {
                if (!takenLastRound.Contains(hex_pos))
                {
                    //Normal move
                    isPlayer1Turn = !isPlayer1Turn;
                    List<HexagonalButton> gridCopy = grid.Select(hex => new HexagonalButton(hex)).ToList();
                    List<string> whiteMarblesCopy = new List<string>(whiteMarbles);
                    List<string> blackMarblesCopy = new List<string>(blackMarbles);
                    boardHistory.Add(gridCopy);
                    whiteMarblesHistory.Add(whiteMarblesCopy);
                    blackMarblesHistory.Add(blackMarblesCopy);
                    btnToPlace.PlaceMarble(!isPlayer1Turn);
                    if (!isPlayer1Turn)
                    {
                        whiteMarbles.Add(hex_pos);
                    }
                    else
                    {
                        blackMarbles.Add(hex_pos);
                    }
                    if (boardHistory.Count > 10)
                    {
                        boardHistory.RemoveAt(0);
                        whiteMarblesHistory.RemoveAt(0);
                        blackMarblesHistory.RemoveAt(0);
                        takenLastRoundHistory.RemoveAt(0);
                    }
                    takenLastRoundHistory.Add(takenLastRound);
                    takenLastRound = "";
                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                //Capture move
                if (btnToPlace!=null && canBeTakenTags.Contains(hex_pos))
                {
                    btnToPlace.ClearMarble();
                    whiteMarbles.Remove(hex_pos);
                    blackMarbles.Remove(hex_pos);
                    takenLastRound=hex_pos;
                    btnToPlace.canBeTaken = false;
                    btnToPlace.Invalidate();
                    foreach (HexagonalButton b in canBeTaken) {
                        b.canBeTaken = false;
                        b.Invalidate();
                    }
                    canBeTaken.Clear();
                }

                if (canBeTaken.Count == 0) {
                    isPlayer1Turn = !isPlayer1Turn;
                }
                return false;
            }
        }

        public bool UndoState() {
            if (boardHistory.Count > 0)
            {
                grid = boardHistory.Last();
                whiteMarbles = whiteMarblesHistory.Last();
                blackMarbles = blackMarblesHistory.Last();
                boardHistory.RemoveAt(boardHistory.Count - 1);
                whiteMarblesHistory.RemoveAt(whiteMarblesHistory.Count - 1);
                blackMarblesHistory.RemoveAt(blackMarblesHistory.Count - 1);
                isPlayer1Turn = !isPlayer1Turn;
                if (takenLastRoundHistory.Count > 0) {
                    takenLastRound = takenLastRoundHistory.ElementAt(takenLastRoundHistory.Count-1);
                    takenLastRoundHistory.RemoveAt(takenLastRoundHistory.Count-1);
                }
                return true;
            }
            else {
                return false;
            }
        }

        public int CheckGameEnded(string lastPlaced,bool isWhite) {
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
                if (whiteMarbles.Count(str => str.Contains(letter))>=5) {
                    int matchCount = 1;
                    //Possibly 5 in a row with the same letter
                    int numberCopy = number;
                    while (numberCopy < 10) {
                        if (whiteMarbles.Contains(letter.ToString() + (numberCopy + 1).ToString()))
                        {
                            matchCount++;
                        }
                        else {
                            break;
                        }
                        numberCopy++;
                    }
                    if (matchCount < 5) {
                        numberCopy = number;
                        while (numberCopy > 1) {
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
                    if (matchCount >= 5) { 
                        whiteWon= true;
                    }
                }
                if (gameEnded == -1 && whiteMarbles.Count(str => str.Contains(number.ToString())) >= 5)
                {
                    //Possibly 5 in a row with the same number
                    int matchCount = 1;
                    int letterIndexCopy = indexOfLetter;
                    while (letterIndexCopy < 9)
                    {
                        if (whiteMarbles.Contains(boardLetters[letterIndexCopy+1].ToString() + number.ToString()))
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
                            if (whiteMarbles.Contains(boardLetters[letterIndexCopy-1].ToString() + number.ToString()))
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
                    while (numberCopy < 10 && letterIndexCopy<9)
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
                        while (number > 0 && letterIndexCopy>0)
                        {
                            if (whiteMarbles.Contains(boardLetters[letterIndexCopy - 1].ToString() + (number-1).ToString()))
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
            else {
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
            else if (blackWon) {
                gameEnded = 2;
            }
            else if (whiteMarbles.Count + blackMarbles.Count == 80 && !whiteWon && !blackWon) {
                //Game Ended With Draw
                gameEnded = 0;
            }

            return gameEnded;
        }

        public bool CheckCapture(string lastPlaced, bool isWhite) {
            List<List<String>> capturedPool = new List<List<String>>();
            char letter = lastPlaced.ElementAt(0);
            int number = int.Parse(lastPlaced.Substring(1));
            int indexOfLetter = Array.IndexOf(boardLetters, letter);
            if (isWhite)
            {
                //Check for capture with the same letter (Both ways)
                if (blackMarbles.Contains(letter + (number - 1).ToString()) && blackMarbles.Contains(letter + (number - 2).ToString()) && whiteMarbles.Contains(letter + (number - 3).ToString())) {
                    capturedPool.Add(new List<string>() { letter + (number - 1).ToString(), letter + (number - 2).ToString() });
                }
                if (blackMarbles.Contains(letter + (number + 1).ToString()) && blackMarbles.Contains(letter + (number + 2).ToString()) && whiteMarbles.Contains(letter + (number + 3).ToString())) {
                    capturedPool.Add(new List<string>() { letter + (number + 1).ToString(), letter + (number + 2).ToString() });
                }
                //Check for capture with the same number (Both ways)
                if (indexOfLetter - 3 >=0 && blackMarbles.Contains(boardLetters[indexOfLetter - 1].ToString() + number.ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter - 2].ToString() + number.ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter - 3].ToString() + number.ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter - 1].ToString() + number.ToString(), boardLetters[indexOfLetter - 2].ToString() + number.ToString() });
                }
                if (indexOfLetter + 3 <= boardLetters.Length-1 && blackMarbles.Contains(boardLetters[indexOfLetter + 1].ToString() + number.ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter + 2].ToString() + number.ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter + 3].ToString() + number.ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter + 1].ToString() + number.ToString(), boardLetters[indexOfLetter + 2].ToString() + number.ToString() });
                }
                //Check for capture vertically (Both ways)
                if (indexOfLetter - 3 >= 0 && blackMarbles.Contains(boardLetters[indexOfLetter - 1].ToString() + (number-1).ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter - 2].ToString() + (number - 2).ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter - 3].ToString() + (number - 3).ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter - 1].ToString() + (number - 1).ToString(), boardLetters[indexOfLetter - 2].ToString() + (number - 2).ToString() });
                }
                if (indexOfLetter + 3 <= boardLetters.Length - 1 && blackMarbles.Contains(boardLetters[indexOfLetter + 1].ToString() + (number + 1).ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter + 2].ToString() + (number + 2).ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter + 3].ToString() + (number + 3).ToString()))
                {
                    capturedPool.Add(new List<string>() { boardLetters[indexOfLetter + 1].ToString() + (number + 1).ToString(), boardLetters[indexOfLetter + 2].ToString() + (number + 2).ToString() });
                }
            }
            else {
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

            if (capturedPool.Count > 0) {
                foreach (List<string> capList in capturedPool) {
                    foreach (string mrbl in capList) {
                        foreach (HexagonalButton btn in grid) {
                            if (btn.tag == mrbl) {
                                btn.canBeTaken = true;
                                btn.Invalidate();
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

        public bool GetisPlayer1Turn() {
            return isPlayer1Turn;
        }

    }
}
