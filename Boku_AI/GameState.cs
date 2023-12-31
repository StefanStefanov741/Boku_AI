﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using static System.Formats.Asn1.AsnWriter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace Boku_AI
{
    internal class GameState
    {
        private bool isPlayer1Turn;
        public bool lastWasCapture;

        public List<HexagonalButton> grid;
        public List<string> freeHexes;
        private List<string> whiteMarbles;
        private List<string> blackMarbles;

        private List<List<string>> freeHexesHistory;
        private List<List<string>> whiteMarblesHistory;
        private List<List<string>> blackMarblesHistory;
        private List<string> takenLastRoundHistory;
        private List<string> taken2RoundsAgoHistory;

        private char[] boardLetters;

        List<HexagonalButton> canBeTaken;
        public List<string> canBeTakenTags;
        public string takenLastRound;
        public string taken2RoundsAgo;
        Random randomizer;
        ZobristKeys zk;

        //Weights
        static int line2;
        static int line4;
        static int line3;
        static int stopLine1;
        static int stopLine2;
        static int stopLine3;
        static int stopLine4;
        static int blockBonus;
        static int free4lineBonus;

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
            line2 = 1800;
            line4 = 6000;
            line3 = 5500;
            stopLine1 = 800;
            stopLine2 = 2000;
            stopLine3 = 5000;
            stopLine4 = 10000;
            blockBonus = 1200;
            free4lineBonus = 3000;
            isPlayer1Turn = player1turn;
            zk = new ZobristKeys();
            randomizer = new Random();
            boardLetters = new []{ 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' };
            freeHexes = new List<string>(AllHexes.hexes);
            lastWasCapture = false;
            whiteMarbles = new List<string>();
            blackMarbles = new List<string>();
            freeHexesHistory = new List<List<string>>();
            whiteMarblesHistory = new List<List<string>>();
            blackMarblesHistory = new List<List<string>>();
            takenLastRoundHistory = new List<string>();
            taken2RoundsAgoHistory = new List<string>();
            canBeTaken = new List<HexagonalButton>();
            canBeTakenTags = new List<string>();
            takenLastRound = "";
            taken2RoundsAgo = "";
        }

        public GameState(GameState gsToCopy)
        {
            line2 = 1800;
            line4 = 6000;
            line3 = 5500;
            stopLine1 = 800;
            stopLine2 = 2000;
            stopLine3 = 5000;
            stopLine4 = 6500;
            blockBonus = 1200;
            free4lineBonus = 4000;
            this.zk = gsToCopy.zk;
            this.randomizer = gsToCopy.randomizer;
            this.boardLetters = gsToCopy.boardLetters;
            this.isPlayer1Turn = gsToCopy.isPlayer1Turn;
            this.lastWasCapture = gsToCopy.lastWasCapture;
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
            this.taken2RoundsAgo = gsToCopy.taken2RoundsAgo;
            this.taken2RoundsAgoHistory = new List<string>(gsToCopy.taken2RoundsAgoHistory);
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
                        taken2RoundsAgoHistory.Add(taken2RoundsAgo);
                        btnToPlace.PlaceMarble(!isPlayer1Turn);
                    }
                    taken2RoundsAgo = takenLastRound;
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
                        taken2RoundsAgoHistory.RemoveAt(0);
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
                    taken2RoundsAgo = takenLastRound;
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
                if (taken2RoundsAgoHistory.Count > 0)
                {
                    taken2RoundsAgo = taken2RoundsAgoHistory.ElementAt(taken2RoundsAgoHistory.Count - 1);
                    taken2RoundsAgoHistory.RemoveAt(taken2RoundsAgoHistory.Count - 1);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private int EvaluateLine1(List<string> evaluatedLine1, List<string> myMarbles, List<string> enemyMarbles, int number, char letter)
        {
            //Same letter, different number
            int score = 0;
            int numberCopy = number;
            int count = 1;
            int startPos = number;
            int endPos = number;

            bool side1Free = false;
            bool side2Free = false;

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
                                    //If it is a more ergent 3 in a line block add more points
                                    if ((numberCopy + 4 <= 10) && !myMarbles.Contains(letter.ToString() + (numberCopy + 4).ToString()))
                                    {
                                        //If on the other side there is also an empty space
                                        score += blockBonus * 3;
                                    }
                                    else
                                    {
                                        if (letter == 'A' || letter == 'J')
                                        {
                                            score -= stopLine3;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (letter != 'A' && letter != 'J')
                                {
                                    score += stopLine2;
                                }
                            }
                        }
                        else
                        {
                            if (letter != 'A' && letter != 'J')
                            {
                                score += stopLine1;
                            }
                        }
                    }
                    else {
                        side1Free = true;
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
                                    //If it is a more ergent 3 in a line block add more points
                                    if ((numberCopy - 4 >= 1) && !myMarbles.Contains(letter.ToString() + (numberCopy - 4).ToString()))
                                    {
                                        //If on the other side there is also an empty space
                                        score += blockBonus * 3;
                                    }
                                    else
                                    {
                                        if (letter == 'A' || letter == 'J')
                                        {
                                            score -= stopLine3;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (letter != 'A' && letter != 'J')
                                {
                                    score += stopLine2;
                                }
                            }
                        }
                        else
                        {
                            if (letter != 'A' && letter != 'J')
                            {
                                score += stopLine1;
                            }
                        }
                    }
                    else {
                        side2Free = true;
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
                if (side1Free && side2Free) {
                    score += free4lineBonus;
                }
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
            //Same number, different letter
            int score = 0;
            int indexOfLetterCopy = indexOfLetter;
            int count = 1;
            int startPos = indexOfLetterCopy;
            int endPos = indexOfLetterCopy;
            bool startedLine = false;
            bool side1Free = false;
            bool side2Free = false;

            while (indexOfLetterCopy < 8)
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
                                    //If it is a more ergent 3 in a line block add more points
                                    if ((indexOfLetterCopy + 4 <= 9) && !myMarbles.Contains(boardLetters[indexOfLetterCopy + 4].ToString() + number.ToString()))
                                    {
                                        //If on the other side there is also an empty space
                                        score += blockBonus * 3;
                                    }
                                    else
                                    {
                                        if (number == 1 || number == 10)
                                        {
                                            score -= stopLine3;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (number != 1 && number != 10)
                                {
                                    score += stopLine2;
                                }
                            }
                        }
                        else
                        {
                            if (number != 1 && number != 10)
                            {
                                score += stopLine1;
                            }
                        }
                    }
                    else {
                        side1Free = true;
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
                                    //If it is a more ergent 3 in a line block add more points
                                    if ((indexOfLetterCopy - 4 >= 0) && !myMarbles.Contains(boardLetters[indexOfLetterCopy - 4].ToString() + number.ToString()))
                                    {
                                        //If on the other side there is also an empty space
                                        score += blockBonus * 3;
                                    }
                                    else
                                    {
                                        if (number == 1 || number == 10)
                                        {
                                            score -= stopLine3;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (number != 1 && number != 10)
                                {
                                    score += stopLine2;
                                }
                            }
                        }
                        else
                        {
                            if (number != 1 && number != 10)
                            {
                                score += stopLine1;
                            }
                        }
                    }
                    else {
                        side2Free = true;
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
                if (side1Free && side2Free)
                {
                    score += free4lineBonus;
                }
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
            //Incrementing letters and numbers
            int score = 0;
            int numberCopy = number;
            int indexOfLetterCopy = indexOfLetter;
            int count = 1;
            int startPosLetter = indexOfLetterCopy;
            int startPosNumber = numberCopy;
            int endPosLetter = indexOfLetterCopy;
            int endPosNumber = numberCopy;
            bool side1Free = false;
            bool side2Free = false;
            while (numberCopy < 10 && indexOfLetterCopy < 8)
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
                                    //If it is a more ergent 3 in a line block add more points
                                    if ((indexOfLetterCopy + 4 <= 9) && numberCopy + 4 <= 10 && !myMarbles.Contains(boardLetters[indexOfLetterCopy + 4].ToString() + (numberCopy + 4).ToString()))
                                    {
                                        //If on the other side there is also an empty space
                                        score += blockBonus * 3;
                                    }
                                    else
                                    {
                                        if ((number == 10 && boardLetters.ElementAt(indexOfLetter) == 'E') || (number == 9 && boardLetters.ElementAt(indexOfLetter) == 'D') || (number == 8 && boardLetters.ElementAt(indexOfLetter) == 'C') ||
                                            (number == 7 && boardLetters.ElementAt(indexOfLetter) == 'B') || (number == 6 && boardLetters.ElementAt(indexOfLetter) == 'A') || (number == 5 && boardLetters.ElementAt(indexOfLetter) == 'J') ||
                                            (number == 4 && boardLetters.ElementAt(indexOfLetter) == 'I') || (number == 3 && boardLetters.ElementAt(indexOfLetter) == 'H') || (number == 2 && boardLetters.ElementAt(indexOfLetter) == 'G') ||
                                            (number == 1 && boardLetters.ElementAt(indexOfLetter) == 'F'))
                                        {
                                            score -= stopLine3;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if ((number != 10 && boardLetters.ElementAt(indexOfLetter) != 'E') && (number != 9 && boardLetters.ElementAt(indexOfLetter) != 'D') && (number != 8 && boardLetters.ElementAt(indexOfLetter) != 'C') &&
                                    (number != 7 && boardLetters.ElementAt(indexOfLetter) != 'B') && (number != 6 && boardLetters.ElementAt(indexOfLetter) != 'A') && (number != 5 && boardLetters.ElementAt(indexOfLetter) != 'J') &&
                                    (number != 4 && boardLetters.ElementAt(indexOfLetter) != 'I') && (number != 3 && boardLetters.ElementAt(indexOfLetter) != 'H') && (number != 2 && boardLetters.ElementAt(indexOfLetter) != 'G') &&
                                    (number != 1 && boardLetters.ElementAt(indexOfLetter) != 'F'))
                                {
                                    score += stopLine2;
                                }
                            }
                        }
                        else
                        {
                            if ((number != 10 && boardLetters.ElementAt(indexOfLetter) != 'E') && (number != 9 && boardLetters.ElementAt(indexOfLetter) != 'D') && (number != 8 && boardLetters.ElementAt(indexOfLetter) != 'C') &&
                                (number != 7 && boardLetters.ElementAt(indexOfLetter) != 'B') && (number != 6 && boardLetters.ElementAt(indexOfLetter) != 'A') && (number != 5 && boardLetters.ElementAt(indexOfLetter) != 'J') &&
                                (number != 4 && boardLetters.ElementAt(indexOfLetter) != 'I') && (number != 3 && boardLetters.ElementAt(indexOfLetter) != 'H') && (number != 2 && boardLetters.ElementAt(indexOfLetter) != 'G') &&
                                (number != 1 && boardLetters.ElementAt(indexOfLetter) != 'F'))
                            {
                                score += stopLine1;
                            }
                        }
                    }
                    else {
                        side1Free = true;
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
                                    //If it is a more ergent 3 in a line block add more points
                                    if ((indexOfLetterCopy - 4 >= 0) && numberCopy - 4 >= 1 && !myMarbles.Contains(boardLetters[indexOfLetterCopy - 4].ToString() + (numberCopy - 4).ToString()))
                                    {
                                        //If on the other side there is also an empty space
                                        score += blockBonus * 3;
                                    }
                                    else
                                    {
                                        if ((number == 10 && boardLetters.ElementAt(indexOfLetter) == 'E') || (number == 9 && boardLetters.ElementAt(indexOfLetter) == 'D') || (number == 8 && boardLetters.ElementAt(indexOfLetter) == 'C') ||
                                            (number == 7 && boardLetters.ElementAt(indexOfLetter) == 'B') || (number == 6 && boardLetters.ElementAt(indexOfLetter) == 'A') || (number == 5 && boardLetters.ElementAt(indexOfLetter) == 'J') ||
                                            (number == 4 && boardLetters.ElementAt(indexOfLetter) == 'I') || (number == 3 && boardLetters.ElementAt(indexOfLetter) == 'H') || (number == 2 && boardLetters.ElementAt(indexOfLetter) == 'G') ||
                                            (number == 1 && boardLetters.ElementAt(indexOfLetter) == 'F'))
                                        {
                                            score -= stopLine3;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if ((number != 10 && boardLetters.ElementAt(indexOfLetter) != 'E') && (number != 9 && boardLetters.ElementAt(indexOfLetter) != 'D') && (number != 8 && boardLetters.ElementAt(indexOfLetter) != 'C') &&
                                    (number != 7 && boardLetters.ElementAt(indexOfLetter) != 'B') && (number != 6 && boardLetters.ElementAt(indexOfLetter) != 'A') && (number != 5 && boardLetters.ElementAt(indexOfLetter) != 'J') &&
                                    (number != 4 && boardLetters.ElementAt(indexOfLetter) != 'I') && (number != 3 && boardLetters.ElementAt(indexOfLetter) != 'H') && (number != 2 && boardLetters.ElementAt(indexOfLetter) != 'G') &&
                                    (number != 1 && boardLetters.ElementAt(indexOfLetter) != 'F'))
                                {
                                    score += stopLine2;
                                }
                            }
                        }
                        else
                        {
                            if ((number != 10 && boardLetters.ElementAt(indexOfLetter) != 'E') && (number != 9 && boardLetters.ElementAt(indexOfLetter) != 'D') && (number != 8 && boardLetters.ElementAt(indexOfLetter) != 'C') &&
                                (number != 7 && boardLetters.ElementAt(indexOfLetter) != 'B') && (number != 6 && boardLetters.ElementAt(indexOfLetter) != 'A') && (number != 5 && boardLetters.ElementAt(indexOfLetter) != 'J') &&
                                (number != 4 && boardLetters.ElementAt(indexOfLetter) != 'I') && (number != 3 && boardLetters.ElementAt(indexOfLetter) != 'H') && (number != 2 && boardLetters.ElementAt(indexOfLetter) != 'G') &&
                                (number != 1 && boardLetters.ElementAt(indexOfLetter) != 'F'))
                            {
                                score += stopLine1;
                            }
                        }
                    }
                    else {
                        side2Free = true;
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
                if (side1Free && side2Free)
                {
                    score += free4lineBonus;
                }
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
            //Task[] evaluationTasks = new Task[3];

            if (isWhitePlayer)
            {
                myMarbles = new List<string>(whiteMarbles);
                enemyMarbles = new List<string>(blackMarbles);
                score += (whiteMarbles.Count() - blackMarbles.Count()) * 20;
            }
            else
            {
                myMarbles = new List<string>(blackMarbles);
                enemyMarbles = new List<string>(whiteMarbles);
                score += ((blackMarbles.Count() + 1) - whiteMarbles.Count()) * 20;
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
                    /*
                    evaluationTasks[0] = Task.Run(() =>
                    {
                        evaluatedLine1.Add(mrbl);
                        score1 += EvaluateLine1(evaluatedLine1, myMarbles, enemyMarbles, number, letter);
                    }
                    );
                    */
                    evaluatedLine1.Add(mrbl);
                    score1 += EvaluateLine1(evaluatedLine1, myMarbles, enemyMarbles, number, letter);
                }
                //Same number, different letter
                if (!evaluatedLine2.Contains(mrbl))
                {
                    /*
                    evaluationTasks[1] = Task.Run(() =>
                    {
                        evaluatedLine2.Add(mrbl);
                        score2 += EvaluateLine2(evaluatedLine2, myMarbles, enemyMarbles, number, indexOfLetter);
                    }
                    );*/
                    evaluatedLine2.Add(mrbl);
                    score2 += EvaluateLine2(evaluatedLine2, myMarbles, enemyMarbles, number, indexOfLetter);
                }
                //Incrementing number and letter
                if (!evaluatedLine3.Contains(mrbl))
                {
                    /*
                    evaluationTasks[2] = Task.Run(() =>
                    {
                        evaluatedLine3.Add(mrbl);
                        score3 += EvaluateLine3(evaluatedLine3, myMarbles, enemyMarbles, number, indexOfLetter);
                    }
                    );*/
                    evaluatedLine3.Add(mrbl);
                    score3 += EvaluateLine3(evaluatedLine3, myMarbles, enemyMarbles, number, indexOfLetter);
                }
                if ((mrbl.Contains('6') || mrbl.Contains('5') || mrbl.Contains('4') || mrbl.Contains('7') || mrbl.Contains('8')) && (mrbl.Contains('E') || mrbl.Contains('F') || mrbl.Contains('D') || mrbl.Contains('C') || mrbl.Contains('G') || mrbl.Contains('H')))
                {
                    score += 1000;
                }
                //Task.WaitAll(evaluationTasks);
                score = score + score1 + score2 + score3;
            }

            //Get player's current board score
            score += randomizer.Next(-100, 100);
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
                    while (letterIndexCopy < 8)
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
                    while (numberCopy < 10 && letterIndexCopy < 8)
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
                        while (numberCopy > 1 && letterIndexCopy > 0)
                        {
                            if (whiteMarbles.Contains(boardLetters[letterIndexCopy - 1].ToString() + (numberCopy - 1).ToString()))
                            {
                                matchCount++;
                            }
                            else
                            {
                                break;
                            }
                            numberCopy--;
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
                    while (letterIndexCopy < 8)
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
                    while (numberCopy < 10 && letterIndexCopy < 8)
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
                        while (numberCopy > 1 && letterIndexCopy > 0)
                        {
                            if (blackMarbles.Contains(boardLetters[letterIndexCopy - 1].ToString() + (numberCopy - 1).ToString()))
                            {
                                matchCount++;
                            }
                            else
                            {
                                break;
                            }
                            numberCopy--;
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
            string capturedPool = "";
            char letter = lastPlaced.ElementAt(0);
            int number = int.Parse(lastPlaced.Substring(1));
            int indexOfLetter = Array.IndexOf(boardLetters, letter);
            if (isWhite)
            {
                //Check for capture with the same letter (Both ways)
                if (blackMarbles.Contains(letter + (number - 1).ToString()) && blackMarbles.Contains(letter + (number - 2).ToString()) && whiteMarbles.Contains(letter + (number - 3).ToString()))
                {
                    capturedPool += letter + (number - 1).ToString() + "|" + letter + (number - 2).ToString();
                }
                if (blackMarbles.Contains(letter + (number + 1).ToString()) && blackMarbles.Contains(letter + (number + 2).ToString()) && whiteMarbles.Contains(letter + (number + 3).ToString()))
                {
                    capturedPool += letter + (number + 1).ToString() + "|" + letter + (number + 2).ToString();
                }
                //Check for capture with the same number (Both ways)
                if (indexOfLetter - 3 >= 0 && blackMarbles.Contains(boardLetters[indexOfLetter - 1].ToString() + number.ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter - 2].ToString() + number.ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter - 3].ToString() + number.ToString()))
                {
                    capturedPool += boardLetters[indexOfLetter - 1].ToString() + number.ToString() + "|" + boardLetters[indexOfLetter - 2].ToString() + number.ToString();
                }
                if (indexOfLetter + 3 <= boardLetters.Length - 1 && blackMarbles.Contains(boardLetters[indexOfLetter + 1].ToString() + number.ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter + 2].ToString() + number.ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter + 3].ToString() + number.ToString()))
                {
                    capturedPool += boardLetters[indexOfLetter + 1].ToString() + number.ToString() + "|" + boardLetters[indexOfLetter + 2].ToString() + number.ToString();
                }
                //Check for capture vertically (Both ways)
                if (indexOfLetter - 3 >= 0 && blackMarbles.Contains(boardLetters[indexOfLetter - 1].ToString() + (number - 1).ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter - 2].ToString() + (number - 2).ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter - 3].ToString() + (number - 3).ToString()))
                {
                    capturedPool += boardLetters[indexOfLetter - 1].ToString() + (number - 1).ToString() + "|" + boardLetters[indexOfLetter - 2].ToString() + (number - 2).ToString();
                }
                if (indexOfLetter + 3 <= boardLetters.Length - 1 && blackMarbles.Contains(boardLetters[indexOfLetter + 1].ToString() + (number + 1).ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter + 2].ToString() + (number + 2).ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter + 3].ToString() + (number + 3).ToString()))
                {
                    capturedPool += boardLetters[indexOfLetter + 1].ToString() + (number + 1).ToString() + "|" + boardLetters[indexOfLetter + 2].ToString() + (number + 2).ToString();
                }
            }
            else
            {
                //Check for capture with the same letter (Both ways)
                if (whiteMarbles.Contains(letter + (number - 1).ToString()) && whiteMarbles.Contains(letter + (number - 2).ToString()) && blackMarbles.Contains(letter + (number - 3).ToString()))
                {
                    capturedPool += letter + (number - 1).ToString() + "|" + letter + (number - 2).ToString();
                }
                if (whiteMarbles.Contains(letter + (number + 1).ToString()) && whiteMarbles.Contains(letter + (number + 2).ToString()) && blackMarbles.Contains(letter + (number + 3).ToString()))
                {
                    capturedPool += letter + (number + 1).ToString() + "|" + letter + (number + 2).ToString();
                }
                //Check for capture with the same number (Both ways)
                if (indexOfLetter - 3 >= 0 && whiteMarbles.Contains(boardLetters[indexOfLetter - 1].ToString() + number.ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter - 2].ToString() + number.ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter - 3].ToString() + number.ToString()))
                {
                    capturedPool += boardLetters[indexOfLetter - 1].ToString() + number.ToString() + "|" + boardLetters[indexOfLetter - 2].ToString() + number.ToString();
                }
                if (indexOfLetter + 3 <= boardLetters.Length - 1 && whiteMarbles.Contains(boardLetters[indexOfLetter + 1].ToString() + number.ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter + 2].ToString() + number.ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter + 3].ToString() + number.ToString()))
                {
                    capturedPool += boardLetters[indexOfLetter + 1].ToString() + number.ToString() + "|" + boardLetters[indexOfLetter + 2].ToString() + number.ToString();
                }
                //Check for capture vertically (Both ways)
                if (indexOfLetter - 3 >= 0 && whiteMarbles.Contains(boardLetters[indexOfLetter - 1].ToString() + (number - 1).ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter - 2].ToString() + (number - 2).ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter - 3].ToString() + (number - 3).ToString()))
                {
                    capturedPool += boardLetters[indexOfLetter - 1].ToString() + (number - 1).ToString() + "|" + boardLetters[indexOfLetter - 2].ToString() + (number - 2).ToString();
                }
                if (indexOfLetter + 3 <= boardLetters.Length - 1 && whiteMarbles.Contains(boardLetters[indexOfLetter + 1].ToString() + (number + 1).ToString()) && whiteMarbles.Contains(boardLetters[indexOfLetter + 2].ToString() + (number + 2).ToString()) && blackMarbles.Contains(boardLetters[indexOfLetter + 3].ToString() + (number + 3).ToString()))
                {
                    capturedPool += boardLetters[indexOfLetter + 1].ToString() + (number + 1).ToString() + "|" + boardLetters[indexOfLetter + 2].ToString() + (number + 2).ToString();
                }
            }

            if (capturedPool.Length > 0)
            {
                foreach (string mrbl in capturedPool.Split("|"))
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
                //Switch the turn back to previous player
                isPlayer1Turn = !isPlayer1Turn;
            }

            return capturedPool.Length > 0;
        }

        public string chooseCapture(string mrbl1,string mrbl2, bool isWhitePOV) {
            string bestMrbl = mrbl2;
            int mrbl1Score = 0;
            int mrbl2Score = 0;

            char letter1;
            int number1;
            int indexOfLetter1;
            List<string> currentCheckMarbles;
            //Swap between the 2 marbles
            for (int j = 0; j < 2; j++)
            {
                if (j == 0)
                {
                    letter1 = mrbl1.ElementAt(0);
                    number1 = int.Parse(mrbl1.Substring(1));
                }
                else {
                    letter1 = mrbl2.ElementAt(0);
                    number1 = int.Parse(mrbl2.Substring(1));
                }
                indexOfLetter1 = Array.IndexOf(boardLetters, letter1);
                //Evaluate score for this marble position for black and for white
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0)
                    {
                        currentCheckMarbles = whiteMarbles;
                    }
                    else
                    {
                        currentCheckMarbles = blackMarbles;
                    }
                    //Same letter
                    int numberCopy1 = number1;
                    int myMatchCount1 = 0;
                    int allowedGap = 1;
                    while (numberCopy1 < 10)
                    {
                        if (currentCheckMarbles.Contains(letter1.ToString() + (numberCopy1 + 1).ToString()))
                        {
                            myMatchCount1++;
                        }
                        else
                        {
                            if (allowedGap > 0 && freeHexes.Contains(letter1.ToString() + (numberCopy1 + 1).ToString()) && currentCheckMarbles.Contains(letter1.ToString() + (numberCopy1 + 2).ToString()))
                            {
                                myMatchCount1++;
                                allowedGap--;
                            }
                            else {
                                break;
                            }
                        }
                        numberCopy1++;
                    }
                    numberCopy1 = number1;
                    while (numberCopy1 > 1)
                    {
                        if (currentCheckMarbles.Contains(letter1.ToString() + (numberCopy1 - 1).ToString()))
                        {
                            myMatchCount1++;
                        }
                        else
                        {
                            if (allowedGap > 0 && freeHexes.Contains(letter1.ToString() + (numberCopy1 - 1).ToString()) && (numberCopy1 - 2)>=1 && currentCheckMarbles.Contains(letter1.ToString() + (numberCopy1 - 2).ToString()))
                            {
                                myMatchCount1++;
                                allowedGap--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        numberCopy1--;
                    }
                    //Same number
                    int letterIndexCopy1 = indexOfLetter1;
                    int myMatchCount2 = 0;
                    allowedGap = 1;
                    while (letterIndexCopy1 < 8)
                    {
                        if (currentCheckMarbles.Contains(boardLetters[letterIndexCopy1 + 1].ToString() + number1.ToString()))
                        {
                            myMatchCount2++;
                        }
                        else
                        {
                            if (allowedGap > 0 && freeHexes.Contains(boardLetters[letterIndexCopy1 + 1].ToString() + number1.ToString()) && (letterIndexCopy1 + 2)<=9 && currentCheckMarbles.Contains(boardLetters[letterIndexCopy1 + 2].ToString() + number1.ToString()))
                            {
                                myMatchCount2++;
                                allowedGap--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        letterIndexCopy1++;
                    }
                    letterIndexCopy1 = indexOfLetter1;
                    while (letterIndexCopy1 > 0)
                    {
                        if (currentCheckMarbles.Contains(boardLetters[letterIndexCopy1 - 1].ToString() + number1.ToString()))
                        {
                            myMatchCount2++;
                        }
                        else
                        {
                            if (allowedGap > 0 && freeHexes.Contains(boardLetters[letterIndexCopy1 - 1].ToString() + number1.ToString()) && (letterIndexCopy1-2)>=0 && currentCheckMarbles.Contains(boardLetters[letterIndexCopy1 - 2].ToString() + number1.ToString()))
                            {
                                myMatchCount2++;
                                allowedGap--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        letterIndexCopy1--;
                    }
                    //Incrementing both letter and number
                    numberCopy1 = number1;
                    letterIndexCopy1 = indexOfLetter1;
                    int myMatchCount3 = 0;
                    allowedGap = 1;
                    while (numberCopy1 < 10 && letterIndexCopy1 < 8)
                    {
                        if (currentCheckMarbles.Contains(boardLetters[letterIndexCopy1 + 1].ToString() + (numberCopy1 + 1).ToString()))
                        {
                            myMatchCount3++;
                        }
                        else
                        {
                            if (allowedGap > 0 && freeHexes.Contains(boardLetters[letterIndexCopy1 + 1].ToString() + (numberCopy1 + 1).ToString()) && (letterIndexCopy1+2)<=9 && currentCheckMarbles.Contains(boardLetters[letterIndexCopy1 + 2].ToString() + (numberCopy1 + 2).ToString()))
                            {
                                myMatchCount3++;
                                allowedGap--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        numberCopy1++;
                        letterIndexCopy1++;
                    }
                    numberCopy1 = number1;
                    letterIndexCopy1 = indexOfLetter1;
                    while (numberCopy1 > 1 && letterIndexCopy1 > 0)
                    {
                        if (currentCheckMarbles.Contains(boardLetters[letterIndexCopy1 - 1].ToString() + (numberCopy1 - 1).ToString()))
                        {
                            myMatchCount3++;
                        }
                        else
                        {
                            if (allowedGap > 0 && freeHexes.Contains(boardLetters[letterIndexCopy1 - 1].ToString() + (numberCopy1 - 1).ToString()) && (letterIndexCopy1-2)>=0 && (numberCopy1-2)>=1 && currentCheckMarbles.Contains(boardLetters[letterIndexCopy1 - 2].ToString() + (numberCopy1 - 2).ToString()))
                            {
                                myMatchCount3++;
                                allowedGap--;
                            }
                            else
                            {
                                break;
                            }
                        }
                        numberCopy1--;
                        letterIndexCopy1--;
                    }
                    //Calculate Score For The Lines
                    if (j == 0)
                    {
                        mrbl1Score = mrbl1Score + myMatchCount1 * myMatchCount1 + myMatchCount2 * myMatchCount2 + myMatchCount3 * myMatchCount3;
                    }
                    else {
                        mrbl2Score = mrbl2Score + myMatchCount1 * myMatchCount1 + myMatchCount2 * myMatchCount2 + myMatchCount3 * myMatchCount3;
                    }
                }
            }

            if (mrbl1Score > mrbl2Score) {
                bestMrbl = mrbl1;
            }

            return bestMrbl;
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


        public ulong GetZobristHash()
        {
            ulong hash = 0;

            //XOR the Zobrist keys for black pieces
            foreach (string position in blackMarbles)
            {
                hash ^= zk.BlackKeys[position];
            }

            //XOR the Zobrist keys for white pieces
            foreach (string position in whiteMarbles)
            {
                hash ^= zk.WhiteKeys[position];
            }

            //XOR the Zobrist key for the forbidden position
            hash ^= zk.TakenLastRoundKeys[takenLastRound];

            //XOR with the player turn
            if (isPlayer1Turn)
            {
                hash ^= zk.isWhiteTurnKey;
            }
            else
            {
                hash ^= zk.isBlackTurnKey;
            }

            return hash;
        }
    }
}