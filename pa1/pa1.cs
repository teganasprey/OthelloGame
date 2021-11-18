#nullable enable
using System;
using System.Collections.Generic;
using static System.Console;

namespace Othello
{
    record Player(string Colour, string Symbol, string Name);
   
    static partial class Program
    {
        // A read-only Dictionary of directions to check around a grid square when a move is being made
        private static readonly Dictionary<string, int[]> DirectionChecks = new Dictionary<string, int[]>
        {
            { "topCenter",    new int[] { -1, 0 } },
            { "bottomCenter", new int[] {  1, 0 } },
            { "rightCenter",  new int[] {  0, 1 } },
            { "leftCenter",   new int[] {  0, -1 } },
            { "topLeft",      new int[] { -1, -1 } },
            { "topRight",     new int[] { -1, 1 } },
            { "bottomLeft",   new int[] {  1, -1 } },
            { "bottomRight",  new int[] {  1, 1 } }
        };

        // Display common text for the top of the screen.
        static void Welcome()
        {
            WriteLine("Welcome to Othello!! Game on!!" + Environment.NewLine);
        }

        // Display a message for the game
        static void DisplayMessage(string message)
        {
            WriteLine(message + Environment.NewLine);
        }
        
        // Collect a player name or default to form the player record.
        static Player NewPlayer(string colour, string symbol, string name)
        {
            if (colour.Length < 1) colour = "red";
            if (symbol.Length < 1) symbol = "R";
            if (name.Length < 1) name = "FakePlayer";
            return new Player(colour, symbol, name);
        }
        
        // Determine which player goes first or default.
        static int GetFirstTurn(Player[] players, int firstPlayer)
        {
            int numPlayers = players.Length;
            // Generate random number for the first player to move
            if (firstPlayer < 0)
            {
                System.Random random = new System.Random();
                return random.Next(0, numPlayers);
            }
            else
                return firstPlayer;
        }
        
        // Get a board size (between 4 and 26 and even) or default, for one direction (row or col). 
        // Default to size 8 if not within parameters
        static int GetBoardSize(string direction, int size)
        {
            if (size < 4 || size > 26 || size % 2 == 1)
                return 8;
            else
                return size;
        }
        
        // Get a move from a player.
        static string GetMove(Player player)
        {
            Write("Player {0} ({1}) enter your move: ", player.Name, player.Symbol);
            string? move = ReadLine();
            return move;
        }
        
        static int[] ConvertMoveStringToRowCol(string move)
        {
            int row = -1;
            int col = -1;

            if (move.Length == 2)
            {
                row = IndexAtLetter(move.Substring(0, 1));
                col = IndexAtLetter(move.Substring(1, 1));
            }
            return new int[] { row, col };
        }

        // Try to make a move. Return true if it worked.
        static bool TryMove(string[,] board, Player player, string move)
        {
            bool validMove = false;

            // Convert the move string into row, col coordinates
            int[] rowCol = ConvertMoveStringToRowCol(move);
            int row = rowCol[0];
            int col = rowCol[1];

            // Check that we have a valid row and col, then check pieces on the board
            if (row >= 0 && row < board.GetLength(0) && col >= 0 && col < board.GetLength(1))
            {
                validMove = IsValidMove(board, player, row, col);
            }
            return validMove;
         }

        static bool IsValidMove(string[,] board, Player player, int row, int col)
        {
            bool validMove = false;
            string symbol = player.Symbol;
            string blank = " ";

            // We can exit immediately if the spot is already taken
            if (!board[row, col].Equals(blank))
            {
                return validMove;
            }

            // Eight directions to check that an opposing piece is flanked by the player's piece to the desired move spot
            foreach (string key in DirectionChecks.Keys)
            {
                int deltaRow = DirectionChecks[key][0];
                int deltaCol = DirectionChecks[key][1];
                validMove = TryDirection(board, player, row + deltaRow, deltaRow, col + deltaCol, deltaCol);
                if (validMove)
                {
                    // At least one valid move has been found, so stop
                    break;
                }
            }
            return validMove;
        }
        
        // Determine whether there are any valid moves for the given player on the board
        static bool AnyValidMoveLeft(string[,] board, Player player)
        {
            bool validMoveLeft = false;
            for (int i = 0; i < board.GetLength(0); i++)            // loop over rows
            {
                for (int j = 0; j < board.GetLength(1); j++)        // loop over cols
                {
                    if (IsValidMove(board, player, i, j))
                    {
                        return true;
                    }
                }
            }
            return validMoveLeft;
        }

        static string[,] UpdateBoard(string[,] board, Player player, string move)
        {
            string symbol = player.Symbol;
            string blank = " ";

            // Convert the move string into row, col coordinates
            int[] rowCol = ConvertMoveStringToRowCol(move);
            int row = rowCol[0];
            int col = rowCol[1];

            // First place the new piece
            board[row, col] = symbol;

            // Eight directions to check that opposing pieces can be flipped
            foreach (string key in DirectionChecks.Keys)
            {
                int deltaRow = DirectionChecks[key][0];
                int deltaCol = DirectionChecks[key][1];
                if (TryDirection(board, player, row + deltaRow, deltaRow, col + deltaCol, deltaCol))
                {
                    // There are pieces to flip in this direction
                    int rowCheck = row + deltaRow;
                    int colCheck = col + deltaCol;
                    while (rowCheck >= 0 && rowCheck < board.GetLength(0) && colCheck >= 0 && colCheck < board.GetLength(1))
                    {
                        if (!board[rowCheck, colCheck].Equals(symbol) && !board[rowCheck, colCheck].Equals(blank))
                        {
                            // Opponent's piece found, so flip it
                            board[rowCheck, colCheck] = symbol;
                        }
                        rowCheck += deltaRow;
                        colCheck += deltaCol;
                    }
                }
            }

            return board;
        }
        
        // Check that flips can be made in the specified direction
        static bool TryDirection(string[,] board, Player player, int row, int deltaRow, int col, int deltaCol)
        {
            string blank = " ";
            string symbol = player.Symbol;

            if (!(row >=  0 && row < board.GetLength(0) && col >= 0 & col < board.GetLength(1)))
            {
                // We are already checking off the board
                return false;
            }

            if (board[row, col] != blank && board[row, col] != symbol)
            {
                // We have an opponent's piece
                while (row >= 0 && row < board.GetLength(0) && col >= 0 && col < board.GetLength(1))
                {
                    row += deltaRow;
                    col += deltaCol;
                    if (board[row, col].Equals(blank))
                    {
                        // Not consecutive, we have a blank so no flank
                        return false;
                    }
                    if (board[row, col].Equals(symbol))
                    {
                        // We have flanked at least one opponent piece and found another player piece
                        return true;
                    }
                }
            }

            // If we get here, no flank found
            return false;
        }
        
        // Count the discs to find the score for a player.
        static int GetScore(string[,] board, Player player)
        {
            string symbol = player.Symbol;
            int playerScore = 0;

            // count the number of matching symbols
            for (int i=0; i<board.GetLength(0); i++)            // loop over rows
            {
                for (int j=0; j<board.GetLength(1); j++)        // loop over cols
                {
                    if (board[i, j].Equals(symbol))
                        playerScore++;
                }
            }
            return playerScore;
        }
        
        // Display a line of scores for all players.
        static void DisplayScores(string[,] board, Player[] players)
        {
            for (int i=0; i<players.Length; i++)
                WriteLine("Player {0} score = {1}", players[i].Name, GetScore(board, players[i]));
            DisplayMessage("");
        }
        
        // Display winner(s) and categorize their win over the defeated player(s).
        static void DisplayWinners(string[,] board, Player[] players)
        {
            int[] scores = new int[players.Length];
            int maxScore = -1;
            int winningPlayer = -1;
            bool tie = false;
            for (int i=0; i<players.Length; i++)
            {
                scores[i] = GetScore(board, players[i]);
                if (scores[i] > maxScore)
                {
                    // there is a clear winner
                    maxScore = scores[i];
                    winningPlayer = i;
                }
                else if (scores[i] == maxScore)
                {
                    // we have a tie
                    tie = true;
                }
            }
            if (tie)
            {
                WriteLine("Tie game!");
            }
            else
                WriteLine("Player {0} wins!", players[winningPlayer].Name);

        }
        
        static void Main()
        {
            // Set up the players and game.
            // Note: I used an array of 'Player' objects to hold information about the players.
            // This allowed me to just pass one 'Player' object to methods needing to use
            // the player name, colour, or symbol in 'WriteLine' messages or board operation.
            // The array aspect allowed me to use the index to keep track of whose turn it is.


            string? player1Name = "White";
            string? player1Colour = "white";
            string? player1Symbol = "O";
            string? player2Name = "Black";
            string? player2Colour = "black";
            string? player2Symbol = "X";

            WriteLine("Would you like to choose your names and colours?");
            string? answer = ReadLine();

            if (answer.ToLower() == "yes")
            {
                Write("Player 1 name: ");
                player1Name = ReadLine();
                Write("Player 1 colour: ");
                player1Colour = ReadLine();
                Write("Player 2 name: ");
                player2Name = ReadLine();
                Write("Player 2 colour: ");
                player2Colour = ReadLine();
            }

            Player[] players = new Player[]
            {
                NewPlayer(colour: player1Colour, symbol: player1Symbol, name: player1Name),
                NewPlayer(colour: player2Colour, symbol: player2Symbol, name: player2Name)
            };

            int turn = GetFirstTurn(players, firstPlayer: 0);

            WriteLine($"Player 1 is called {players[0].Name}, represented by colour {players[0].Colour} and symbol {players[0].Symbol}");
            WriteLine($"Player 2 is called {players[1].Name}, represented by colour {players[1].Colour} and symbol {players[1].Symbol}");

            WriteLine($"{players[turn].Name} is first!");


            Write("Input board size: ");
            int boardSize = int.Parse(ReadLine());
            int rows = GetBoardSize(direction: "rows", size: boardSize);
            int cols = GetBoardSize(direction: "columns", size: boardSize);

            string[,] game = NewBoard(rows, cols);
            string message = "Let's start the game!";

            // Play the game.
            bool gameOver = false;
            while (!gameOver)
            {
                Clear();
                Welcome();
                DisplayMessage(message);
                DisplayScores(game, players);
                DisplayBoard(game);

                if (AnyValidMoveLeft(game, players[turn]))
                {
                    string move = GetMove(players[turn]);
                    if (move == "quit")
                    {
                        // The player has given up, so end the game
                        gameOver = true;
                    }
                    else if (move == "skip")
                    {
                        // The player wishes to skip a turn, do not update the board and switch players
                        turn = (turn + 1) % players.Length;
                    }
                    else
                    {
                        bool madeValidMove = TryMove(game, players[turn], move);
                        if (madeValidMove)
                        {
                            game = UpdateBoard(game, players[turn], move);
                            turn = (turn + 1) % players.Length;
                            message="Next player's turn!";
                        }
                        else
                        {
                            message = "Last move was invalid!";
                            WriteLine("Your choice didn't work!");
                            WriteLine("Press <Enter> to try again.");
                            ReadLine();
                        }
                    }
                }
                else
                {
                    gameOver = true;
                }
            }
            
            // Show fhe final results            
            DisplayWinners(game, players);
            WriteLine("Congratulations!");
        }
    }
}
