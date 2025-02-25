using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToe
{
    public class TicTacToeGame
    {
        public Client Player1 { get; set; }
        public Client Player2 { get; set; }
        public bool IsGameOver { get; private set; }
        public bool IsDraw { get; private set; }

        private readonly int[] field = new int[9];
        private int numbersOfMoves = 9;

        public TicTacToeGame()
        {
            //Resetowanie gry
            for (var i = 0; i < field.Length; i++)
            {
                field[i] = -1;
            }
        }
        public bool Play(int player, int position)
        {
            if (IsGameOver)
            {
                return false;
            }
            // Przekazanie numeru gracza i pozycji na planszy
            PlaceMarker(player, position);

            return CheckWinner();
        }

        private bool PlaceMarker(int player, int position)
        {
            numbersOfMoves -= 1;

            if (numbersOfMoves <= 0)
            {
                IsGameOver = true;
                IsDraw = true;
                return false;
            }

            if (position > field.Length)
            {
                return false;
            }
            if (field[position] != -1)
            {
                return false;
            }

            field[position] = player;

            return true;
        }
        private bool CheckWinner()
        {
            //sprawdzenie pion i poziom
            if ((field[0] != -1 && field[0] == field[1] && field[0] == field[2]) || 
                (field[3] != -1 && field[3] == field[4] && field[3] == field[5]) || 
                (field[6] != -1 && field[6] == field[7] && field[6] == field[8]) ||
                (field[0] != -1 && field[0] == field[3] && field[0] == field[6]) ||
                (field[1] != -1 && field[1] == field[4] && field[1] == field[7]) ||
                (field[2] != -1 && field[2] == field[5] && field[2] == field[8]))
            {
                IsGameOver = true;
                return true;
            }
            // Sprawdzenie skosne
            if ((field[0] != -1 && field[0] == field[4] && field[0] == field[8]) || 
                (field[2] != -1 && field[2] == field[4] && field[2] == field[6]))
            {
                IsGameOver = true;
                return true;
            }

            return false;
        }
    }
}
