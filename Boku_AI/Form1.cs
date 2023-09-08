using System.Drawing.Text;

namespace Boku_AI
{
    public partial class Form1 : Form
    {
        GameState gameState = new GameState(null);
        int gameEnded = -1;
        public Form1()
        {
            InitializeComponent();
            BuildGrid();
        }

        private void BuildGrid() {
            int hexSize = 80;
            //Column positions
            float col1X = 110;
            float col2X = col1X+58;
            float col3X = col2X + 58;
            float col4X = col3X + 58;
            float col5X = col4X + 58;
            float col6X = col5X + 58;
            float col7X = col6X + 58;
            float col8X = col7X + 58;
            float col9X = col8X + 58;
            float col10X = col9X + 58;
            float col11X = col10X + 58;

            //Row positions
            float row1Y = 50;
            float row2Y = row1Y+32.5f;
            float row3Y = row2Y+32.5f;
            float row4Y = row3Y+32.5f;
            float row5Y = row4Y+32.5f;
            float row6Y = row5Y+32.5f;
            float row7Y = row6Y+32.5f;
            float row8Y = row7Y + 32.5f;
            float row9Y = row8Y + 32.5f;
            float row10Y = row9Y + 32.5f;
            float row11Y = row10Y + 32.5f;
            float row12Y = row11Y + 32.5f;
            float row13Y = row12Y + 32.5f;
            float row14Y = row13Y + 32.5f;
            float row15Y = row14Y + 32.5f;
            float row16Y = row15Y + 32.5f;
            float row17Y = row16Y + 32.5f;
            float row18Y = row17Y + 32.5f;
            float row19Y = row18Y + 32.5f;

            //Create the board

            //Col1
            gameState.grid.Add(HexFactory(hexSize, col1X, row6Y, "E10"));
            gameState.grid.Add(HexFactory(hexSize, col1X, row8Y, "D9"));
            gameState.grid.Add(HexFactory(hexSize, col1X, row10Y, "C8"));
            gameState.grid.Add(HexFactory(hexSize, col1X, row12Y, "B7"));
            gameState.grid.Add(HexFactory(hexSize, col1X, row14Y, "A6"));
            //Col2
            gameState.grid.Add(HexFactory(hexSize, col2X, row5Y, "F10"));
            gameState.grid.Add(HexFactory(hexSize, col2X, row7Y, "E9"));
            gameState.grid.Add(HexFactory(hexSize, col2X, row9Y, "D8"));
            gameState.grid.Add(HexFactory(hexSize, col2X, row11Y, "C7"));
            gameState.grid.Add(HexFactory(hexSize, col2X, row13Y, "B6"));
            gameState.grid.Add(HexFactory(hexSize, col2X, row15Y, "A5"));
            //Col3
            gameState.grid.Add(HexFactory(hexSize, col3X, row4Y, "G10"));
            gameState.grid.Add(HexFactory(hexSize, col3X, row6Y, "F9"));
            gameState.grid.Add(HexFactory(hexSize, col3X, row8Y, "E8"));
            gameState.grid.Add(HexFactory(hexSize, col3X, row10Y, "D7"));
            gameState.grid.Add(HexFactory(hexSize, col3X, row12Y, "C6"));
            gameState.grid.Add(HexFactory(hexSize, col3X, row14Y, "B5"));
            gameState.grid.Add(HexFactory(hexSize, col3X, row16Y, "A4"));
            //Col 4
            gameState.grid.Add(HexFactory(hexSize, col4X, row3Y, "H10"));
            gameState.grid.Add(HexFactory(hexSize, col4X, row5Y, "G9"));    
            gameState.grid.Add(HexFactory(hexSize, col4X, row7Y, "F8"));
            gameState.grid.Add(HexFactory(hexSize, col4X, row9Y, "E7"));
            gameState.grid.Add(HexFactory(hexSize, col4X, row11Y, "D6"));
            gameState.grid.Add(HexFactory(hexSize, col4X, row13Y, "C5"));
            gameState.grid.Add(HexFactory(hexSize, col4X, row15Y, "B4"));
            gameState.grid.Add(HexFactory(hexSize, col4X, row17Y, "A3"));
            //Col 5
            gameState.grid.Add(HexFactory(hexSize, col5X, row2Y, "I10"));
            gameState.grid.Add(HexFactory(hexSize, col5X, row4Y, "H9"));
            gameState.grid.Add(HexFactory(hexSize, col5X, row6Y, "G8"));
            gameState.grid.Add(HexFactory(hexSize, col5X, row8Y, "F7"));
            gameState.grid.Add(HexFactory(hexSize, col5X, row10Y, "E6"));
            gameState.grid.Add(HexFactory(hexSize, col5X, row12Y, "D5"));
            gameState.grid.Add(HexFactory(hexSize, col5X, row14Y, "C4"));
            gameState.grid.Add(HexFactory(hexSize, col5X, row16Y, "B3"));
            gameState.grid.Add(HexFactory(hexSize, col5X, row18Y, "A2"));
            //Col 6 (Middle)
            gameState.grid.Add(HexFactory(hexSize, col6X, row1Y, "J10"));
            gameState.grid.Add(HexFactory(hexSize, col6X, row3Y, "I9"));
            gameState.grid.Add(HexFactory(hexSize, col6X, row5Y, "H8"));
            gameState.grid.Add(HexFactory(hexSize, col6X, row7Y, "G7"));
            gameState.grid.Add(HexFactory(hexSize, col6X, row9Y, "F6"));
            gameState.grid.Add(HexFactory(hexSize, col6X, row11Y, "E5"));
            gameState.grid.Add(HexFactory(hexSize, col6X, row13Y, "D4"));
            gameState.grid.Add(HexFactory(hexSize, col6X, row15Y, "C3"));
            gameState.grid.Add(HexFactory(hexSize, col6X, row17Y, "B2"));
            gameState.grid.Add(HexFactory(hexSize, col6X, row19Y, "A1"));
            //Col 7
            gameState.grid.Add(HexFactory(hexSize, col7X, row2Y, "J9"));
            gameState.grid.Add(HexFactory(hexSize, col7X, row4Y, "I8"));
            gameState.grid.Add(HexFactory(hexSize, col7X, row6Y, "H7"));
            gameState.grid.Add(HexFactory(hexSize, col7X, row8Y, "G6"));
            gameState.grid.Add(HexFactory(hexSize, col7X, row10Y, "F5"));
            gameState.grid.Add(HexFactory(hexSize, col7X, row12Y, "E4"));
            gameState.grid.Add(HexFactory(hexSize, col7X, row14Y, "D3"));
            gameState.grid.Add(HexFactory(hexSize, col7X, row16Y, "C2"));
            gameState.grid.Add(HexFactory(hexSize, col7X, row18Y, "B1"));
            //Col 8
            gameState.grid.Add(HexFactory(hexSize, col8X, row3Y, "J8"));
            gameState.grid.Add(HexFactory(hexSize, col8X, row5Y, "I7"));
            gameState.grid.Add(HexFactory(hexSize, col8X, row7Y, "H6"));
            gameState.grid.Add(HexFactory(hexSize, col8X, row9Y, "G5"));
            gameState.grid.Add(HexFactory(hexSize, col8X, row11Y, "F4"));
            gameState.grid.Add(HexFactory(hexSize, col8X, row13Y, "E3"));
            gameState.grid.Add(HexFactory(hexSize, col8X, row15Y, "D2"));
            gameState.grid.Add(HexFactory(hexSize, col8X, row17Y, "C1"));
            //Col 9
            gameState.grid.Add(HexFactory(hexSize, col9X, row4Y, "J7"));
            gameState.grid.Add(HexFactory(hexSize, col9X, row6Y, "I6"));
            gameState.grid.Add(HexFactory(hexSize, col9X, row8Y, "H5"));
            gameState.grid.Add(HexFactory(hexSize, col9X, row10Y, "G4"));
            gameState.grid.Add(HexFactory(hexSize, col9X, row12Y, "F3"));
            gameState.grid.Add(HexFactory(hexSize, col9X, row14Y, "E2"));
            gameState.grid.Add(HexFactory(hexSize, col9X, row16Y, "D1"));
            //Col 10
            gameState.grid.Add(HexFactory(hexSize, col10X, row5Y, "J6"));
            gameState.grid.Add(HexFactory(hexSize, col10X, row7Y, "I5"));
            gameState.grid.Add(HexFactory(hexSize, col10X, row9Y, "H4"));
            gameState.grid.Add(HexFactory(hexSize, col10X, row11Y, "G3"));
            gameState.grid.Add(HexFactory(hexSize, col10X, row13Y, "F2"));
            gameState.grid.Add(HexFactory(hexSize, col10X, row15Y, "E1"));
            //Col 11
            gameState.grid.Add(HexFactory(hexSize, col11X, row6Y, "J5"));
            gameState.grid.Add(HexFactory(hexSize, col11X, row8Y, "I4"));
            gameState.grid.Add(HexFactory(hexSize, col11X, row10Y, "H3"));
            gameState.grid.Add(HexFactory(hexSize, col11X, row12Y, "G2"));
            gameState.grid.Add(HexFactory(hexSize, col11X, row14Y, "F1"));
        }

        HexagonalButton HexFactory(int size,float posX, float posY, string tag) {
            //Create button with set parameters
            HexagonalButton btn = new HexagonalButton(tag);
            btn.Size = new Size(size, size);
            btn.Location = new Point((int)posX, (int)posY);
            btn.Tag = tag;
            btn.Click += Hex_Click;

            //Add to the form
            this.Controls.Add(btn);

            //Return btn
            return btn;
        }

        private void Hex_Click(object sender, EventArgs e)
        {
            if (gameEnded > -1) {
                return;
            }
            //Check the tag property of the sender to determine which button was clicked
            string buttonTag = (sender as HexagonalButton).tag.ToString();
            bool isWhiteMarble = false;
            bool successfulPlace = false;
            foreach (HexagonalButton hex in gameState.grid) {
                if (hex.tag == buttonTag && !hex.marblePlaced) {
                    isWhiteMarble = gameState.placeMarble(buttonTag);
                    hex.PlaceMarble(isWhiteMarble);
                    successfulPlace = true;
                    break;
                }
            }
            if (successfulPlace) {
                gameEnded = gameState.CheckGameEnded(buttonTag, isWhiteMarble);
                switch (gameEnded)
                {
                    case 0:
                        //Its a draw
                        MessageBox.Show("It's a Draw!");
                        break;
                    case 1:
                        //White won
                        MessageBox.Show("White Wins!");
                        break;
                    case 2:
                        //Black won
                        MessageBox.Show("Black Wins!");
                        break;
                    default:
                        //Game hasn't eneded
                        break;
                }
            }
        }

        private void undo(object sender, EventArgs e)
        {
            if(gameEnded > -1) {
                return;
            }
            gameState.UndoState();
            RedrawBoard();
        }

        private void RedrawBoard() {
            this.Controls.Clear();
            InitSidePanel();
            foreach (HexagonalButton hex in gameState.grid) {
                hex.Click += Hex_Click;    
                this.Controls.Add(hex);
            }
        }

    }
}
