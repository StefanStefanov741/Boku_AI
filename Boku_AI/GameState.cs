using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boku_AI
{
    internal class GameState
    {
        public List<HexagonalButton> grid;
        private bool isPlayer1Turn;
        private List<List<HexagonalButton>> boardHistory = new List<List<HexagonalButton>>();
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

        public bool placeMarble() {
            isPlayer1Turn = !isPlayer1Turn;
            boardHistory.Add(grid);
            if (boardHistory.Count > 10) {
                boardHistory.RemoveAt(0);
            }
            return !isPlayer1Turn;
        }

        public void UndoState() {
            grid = boardHistory.ElementAt(boardHistory.Count - 1);
            boardHistory.RemoveAt(boardHistory.Count - 1);
            isPlayer1Turn = !isPlayer1Turn;
        }

    }
}
