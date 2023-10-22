using System;

namespace proje3_son
{
    class Program
    {
        static char[,] board = new char[8, 8];
        static int turn = 0;//0 for player, 1 for bot
        static int round = 1;
        static ConsoleKey key;
        static int cursorPosX = 0, cursorPosY = 0;
        static int selectedPieceX = 0, selectedPieceY = 0;
        static int botPieceX = 0, botPieceY = 0;
        static bool isPieceSelected = false;
        static int jumpCount = 0, direction = 0;
        static Random rand = new Random();

        const char EmptyItem = '.';
        const char Player1Piece = 'x';
        const char Player2Piece = 'o';

        static void Main(string[] args)
        {
            BoardDefault();

            while (true)
            {
                PrintBoard();
                PrintInfo();

                if (turn == 0)
                {
                    PlayerTurn();

                }
                else
                {
                    BotTurn();
                }

                PrintBoard();
                PrintInfo();

                if (WinConditionCheck())
                {
                    WinScreen();
                    Console.SetCursorPosition(0, 11);
                    return;//ends the main func
                }

                if (turn == 0 && isPieceSelected == false)
                {
                    turn = 1;

                }
                else if (turn == 1)
                {
                    turn = 0;
                    round++;

                }
            }
        }

        static void PlayerTurn()
        {
            while (true)
            {
                Console.SetCursorPosition(cursorPosX + 2, cursorPosY + 2);

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    if (cursorPosY > 0)
                        cursorPosY--;
                    else
                        cursorPosY = board.GetLength(0) - 1;

                }
                else if (key == ConsoleKey.DownArrow)
                {
                    if (cursorPosY < board.GetLength(0) - 1)
                        cursorPosY++;
                    else
                        cursorPosY = 0;

                }
                else if (key == ConsoleKey.LeftArrow)
                {
                    if (cursorPosX > 0)
                        cursorPosX--;
                    else
                        cursorPosX = board.GetLength(0) - 1;

                }
                else if (key == ConsoleKey.RightArrow)
                {
                    if (cursorPosX < board.GetLength(0) - 1)
                        cursorPosX++;
                    else
                        cursorPosX = 0;

                }
                else if (key == ConsoleKey.Z && !isPieceSelected)
                {
                    //Made this for testing but probably will stay
                    if (!CanPieceMove(cursorPosX, cursorPosY))
                    {
                        ErrorNotification("Selected piece can't move");
                        continue;
                    }
                    else if (board[cursorPosY, cursorPosX] == Player1Piece)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(Player1Piece);
                        Console.SetCursorPosition(cursorPosX + 2, cursorPosY + 2);
                        selectedPieceX = cursorPosX;
                        selectedPieceY = cursorPosY;
                        Console.ResetColor();
                        isPieceSelected = true;
                    }


                }

                else if (key == ConsoleKey.X && isPieceSelected)
                {
                    directionfind(cursorPosX, cursorPosY);

                    if (step_jump_op())
                    {
                        board[cursorPosY, cursorPosX] = Player1Piece;
                        board[selectedPieceY, selectedPieceX] = EmptyItem;
                        Console.SetCursorPosition(selectedPieceX + 2, selectedPieceY + 2);
                        Console.Write(EmptyItem);
                        Console.SetCursorPosition(cursorPosX + 2, cursorPosY + 2);
                        Console.Write(Player1Piece);
                        Console.SetCursorPosition(cursorPosX + 2, cursorPosY + 2);
                        if (jumpCount == 0)
                            isPieceSelected = false;
                        else
                            isPieceSelected = true;
                        if (isPieceSelected)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(Player1Piece);
                            Console.SetCursorPosition(cursorPosX + 2, cursorPosY + 2);
                            selectedPieceX = cursorPosX;
                            selectedPieceY = cursorPosY;
                            Console.ResetColor();

                        }
                        ErrorClear();
                        return;
                    }


                }
                else if (key == ConsoleKey.C && jumpCount != 0)
                {
                    isPieceSelected = false;
                    jumpCount = 0;

                    return;
                }
            }
        }

        static bool BotContinueToMove(int number, string axis)
        {
            bool flag = true;
            int counter = 0;
            if (axis == "x")
            {
                for (int i = 0; i < board.GetLength(0); i++)
                {
                    for (int j = number; j < board.GetLength(1); j++)
                    {
                        if (board[i, j] == 'o') counter += 1;
                    }
                }
            }
            else if (axis == "y")
            {
                for (int i = number; i < board.GetLength(0); i++)
                {
                    for (int j = 0; j < board.GetLength(1); j++)
                    {
                        if (board[i, j] == 'o') counter += 1;
                    }
                }
            }
            if (number == 7 && counter == 3) flag = false;
            else if (number == 6 && counter == 6) flag = false;
            else if (number == 5 && counter == 9) flag = false;
            return flag;
        }
        static void BotTurn()
        {
            //getting x and y coordinates of botpieces
            int[,] coordinates_of_o_pieces = new int[9, 2];
            int no_o = 0;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == 'o')
                    {
                        coordinates_of_o_pieces[no_o, 0] = j; //x
                        coordinates_of_o_pieces[no_o, 1] = i; //y
                        no_o++;
                    }
                }
            }

            //counting how many moves can each botpiece make
            int[] count_moves = new int[9];
            string[] paths_for_moves = new string[9];
            for (int a = 0; a < coordinates_of_o_pieces.GetLength(0); a++)
            {
                botPieceX = coordinates_of_o_pieces[a, 0];
                botPieceY = coordinates_of_o_pieces[a, 1];
                count_moves[a] = 0;

                // if bot is able to jump down or right
                if ((CanPieceJump(botPieceX, botPieceY, 0) && BotContinueToMove(botPieceX + 2, "x")) || (CanPieceJump(botPieceX, botPieceY, 3) && BotContinueToMove(botPieceY + 2, "y")))
                {
                    count_moves[a] += 2;
                    while ((CanPieceJump(botPieceX, botPieceY, 0) && BotContinueToMove(botPieceX + 2, "x")) || (CanPieceJump(botPieceX, botPieceY, 3) && BotContinueToMove(botPieceY + 2, "y")))
                    {
                        if ((CanPieceJump(botPieceX, botPieceY, 0) && BotContinueToMove(botPieceX + 2, "x")) && (CanPieceJump(botPieceX, botPieceY, 3) && BotContinueToMove(botPieceY + 2, "y")))
                        {
                            if (rand.Next(1, 3) == 1 && BotContinueToMove(botPieceX + 2, "x"))
                            {
                                botPieceX += 2;
                                count_moves[a]++;
                                paths_for_moves[a] += "0";
                            }
                            else if (BotContinueToMove(botPieceY + 2, "y"))
                            {
                                botPieceY += 2;
                                count_moves[a]++;
                                paths_for_moves[a] += "3";
                            }
                        }
                        else
                        {
                            if (CanPieceJump(botPieceX, botPieceY, 0) && BotContinueToMove(botPieceX + 2, "x"))
                            {
                                botPieceX += 2;
                                count_moves[a]++;
                                paths_for_moves[a] += "0";
                            }
                            else if (CanPieceJump(botPieceX, botPieceY, 3) && BotContinueToMove(botPieceY + 2, "y"))
                            {
                                botPieceY += 2;
                                count_moves[a]++;
                                paths_for_moves[a] += "3";
                            }
                        }
                    }
                }
                // if bot is able to step down or right
                else if ((CanPieceStep(botPieceX, botPieceY, 0) && BotContinueToMove(botPieceX + 1, "x")) || (CanPieceStep(botPieceX, botPieceY, 3) && BotContinueToMove(botPieceY + 1, "y")))
                {
                    count_moves[a] += 2;
                    if ((CanPieceStep(botPieceX, botPieceY, 0) && BotContinueToMove(botPieceX + 1, "x")) && (CanPieceStep(botPieceX, botPieceY, 3) && BotContinueToMove(botPieceY + 1, "y")))
                    {
                        if (rand.Next(1, 3) == 1 && BotContinueToMove(botPieceY + 1, "y"))
                        {
                            count_moves[a] += 1;
                            paths_for_moves[a] += "3";
                        }
                        else if (BotContinueToMove(botPieceX + 1, "x"))
                        {
                            count_moves[a] += 1;
                            paths_for_moves[a] += "0";
                        }
                    }
                    else if (CanPieceStep(botPieceX, botPieceY, 0) && BotContinueToMove(botPieceX + 1, "x"))
                    {
                        count_moves[a] += 1;
                        paths_for_moves[a] += "0";
                    }
                    else if (CanPieceStep(botPieceX, botPieceY, 3) && BotContinueToMove(botPieceY + 1, "y"))
                    {
                        count_moves[a] += 1;
                        paths_for_moves[a] += "3";
                    }
                }
                // if bot is not able to jump or step down or right, it jumps left or up
                else if (CanPieceJump(botPieceX, botPieceY, 1) || CanPieceJump(botPieceX, botPieceY, 2))
                {
                    if (CanPieceJump(botPieceX, botPieceY, 1) && CanPieceJump(botPieceX, botPieceY, 2))
                    {
                        if (rand.Next(1, 3) == 1)
                        {
                            count_moves[a] += 2;
                            paths_for_moves[a] += "1";
                        }
                        else
                        {
                            count_moves[a] += 2;
                            paths_for_moves[a] += "2";
                        }
                    }
                    else if (CanPieceJump(botPieceX, botPieceY, 1))
                    {
                        count_moves[a] += 2;
                        paths_for_moves[a] += "1";
                    }
                    else if (CanPieceJump(botPieceX, botPieceY, 2))
                    {
                        count_moves[a] += 2;
                        paths_for_moves[a] += "2";
                    }
                }
                // if bot is not able to jump or step down or right, it steps left or up
                else if (CanPieceStep(botPieceX, botPieceY, 1) || CanPieceStep(botPieceX, botPieceY, 2))
                {
                    if (CanPieceStep(botPieceX, botPieceY, 1) && CanPieceStep(botPieceX, botPieceY, 2))
                    {
                        if (rand.Next(1, 3) == 1)
                        {
                            count_moves[a] += 1;
                            paths_for_moves[a] += "1";
                        }
                        else
                        {
                            count_moves[a] += 1;
                            paths_for_moves[a] += "2";
                        }
                    }
                    else if (CanPieceStep(botPieceX, botPieceY, 1))
                    {
                        count_moves[a] += 1;
                        paths_for_moves[a] += "1";
                    }
                    else if (CanPieceStep(botPieceX, botPieceY, 2))
                    {
                        count_moves[a] += 1;
                        paths_for_moves[a] += "2";
                    }
                }
            }

            //getting highest number of moves
            int max_of_count_moves = count_moves[0];
            for (int i = 0; i < count_moves.Length; i++)
            {
                if (count_moves[i] > max_of_count_moves) max_of_count_moves = count_moves[i];
            }

            //index of which botpieces have highest move
            string index_of_max = "";
            for (int i = 0; i < count_moves.Length; i++)
            {
                if (count_moves[i] == max_of_count_moves) index_of_max += Convert.ToString(i);
            }

            //selecting one of the botpieces which of them have highest number of moves
            char index = index_of_max[rand.Next(0, index_of_max.Length)];
            int no_of_o_picece = Convert.ToInt32(index.ToString());
            botPieceX = coordinates_of_o_pieces[no_of_o_picece, 0];
            botPieceY = coordinates_of_o_pieces[no_of_o_picece, 1];

            //moves the botpiece using no_of_o_piece
            if (CanPieceJump(botPieceX, botPieceY, 0) || CanPieceJump(botPieceX, botPieceY, 3))
            {
                for (int a = 0; a < paths_for_moves[no_of_o_picece].Length; a++)
                {
                    if (paths_for_moves[no_of_o_picece][a] == '0')
                    {
                        board[botPieceY, botPieceX] = EmptyItem;
                        board[botPieceY, botPieceX + 2] = Player2Piece;
                        botPieceX = botPieceX + 2;
                    }
                    else if (paths_for_moves[no_of_o_picece][a] == '3')
                    {
                        board[botPieceY, botPieceX] = EmptyItem;
                        board[botPieceY + 2, botPieceX] = Player2Piece;
                        botPieceY = botPieceY + 2;
                    }
                    PrintBoard();
                    System.Threading.Thread.Sleep(500);
                }
            }
            else if (CanPieceStep(botPieceX, botPieceY, 0) || CanPieceStep(botPieceX, botPieceY, 3))
            {
                System.Threading.Thread.Sleep(500);
                if (paths_for_moves[no_of_o_picece] == "0")
                {
                    board[botPieceY, botPieceX] = EmptyItem;
                    board[botPieceY, botPieceX + 1] = Player2Piece;
                }
                else if (paths_for_moves[no_of_o_picece] == "3")
                {
                    board[botPieceY, botPieceX] = EmptyItem;
                    board[botPieceY + 1, botPieceX] = Player2Piece;
                }
            }
            else if (CanPieceJump(botPieceX, botPieceY, 1) || CanPieceJump(botPieceX, botPieceY, 2))
            {
                System.Threading.Thread.Sleep(500);
                if (paths_for_moves[no_of_o_picece] == "1")
                {
                    board[botPieceY, botPieceX] = EmptyItem;
                    board[botPieceY, botPieceX - 2] = Player2Piece;
                }
                else if (paths_for_moves[no_of_o_picece] == "2")
                {
                    board[botPieceY, botPieceX] = EmptyItem;
                    board[botPieceY - 2, botPieceX] = Player2Piece;
                }
            }
            else if (CanPieceStep(botPieceX, botPieceY, 1) || CanPieceStep(botPieceX, botPieceY, 2))
            {
                System.Threading.Thread.Sleep(500);
                if (paths_for_moves[no_of_o_picece] == "1")
                {
                    board[botPieceY, botPieceX] = EmptyItem;
                    board[botPieceY, botPieceX - 1] = Player2Piece;
                }
                else if (paths_for_moves[no_of_o_picece] == "2")
                {
                    board[botPieceY, botPieceX] = EmptyItem;
                    board[botPieceY - 1, botPieceX] = Player2Piece;
                }
            }
        }

        static bool CanPieceMove(int pieceX, int pieceY)
        {
            if (CanPieceStep(pieceX, pieceY, 0))
                return true;

            if (CanPieceStep(pieceX, pieceY, 1))
                return true;

            if (CanPieceStep(pieceX, pieceY, 2))
                return true;

            if (CanPieceStep(pieceX, pieceY, 3))
                return true;

            if (CanPieceJump(pieceX, pieceY, 0))
                return true;

            if (CanPieceJump(pieceX, pieceY, 1))
                return true;

            if (CanPieceJump(pieceX, pieceY, 2))
                return true;

            if (CanPieceJump(pieceX, pieceY, 3))
                return true;

            return false;
        }

        //direction 0 for rigth, 1 for left, 2 for up, 3 for down
        static bool CanPieceStep(int pieceX, int pieceY, int direction)
        {
            if (direction == 0 && pieceX < (board.GetLength(1) - 1))
            {
                return board[pieceY, pieceX + 1] == EmptyItem;
            }

            else if (direction == 1 && pieceX > 0)
            {
                return board[pieceY, pieceX - 1] == EmptyItem;
            }

            else if (direction == 2 && pieceY > 0)
            {
                return board[pieceY - 1, pieceX] == EmptyItem;
            }

            else if (direction == 3 && pieceY < (board.GetLength(0) - 1))
            {
                return board[pieceY + 1, pieceX] == EmptyItem;
            }

            return false;
        }

        //direction 0 for rigth, 1 for left, 2 for up, 3 for down
        static bool CanPieceJump(int pieceX, int pieceY, int direction)
        {
            if (direction == 0 && pieceX < 6)
            {
                if (board[pieceY, pieceX + 1] != EmptyItem)
                    return board[pieceY, pieceX + 2] == EmptyItem;
            }

            else if (direction == 1 && pieceX > 1)
            {
                if (board[pieceY, pieceX - 1] != EmptyItem)
                    return board[pieceY, pieceX - 2] == EmptyItem;
            }

            else if (direction == 2 && pieceY > 1)
            {
                if (board[pieceY - 1, pieceX] != EmptyItem)
                    return board[pieceY - 2, pieceX] == EmptyItem;
            }

            else if (direction == 3 && pieceY < 6)
            {
                if (board[pieceY + 1, pieceX] != EmptyItem)
                    return board[pieceY + 2, pieceX] == EmptyItem;
            }

            return false;
        }

        static void ErrorClear()
        {
            Console.SetCursorPosition(13, 9);
            Console.Write("                                                      ");
        }

        static void ErrorNotification(string error)
        {
            ErrorClear();
            Console.SetCursorPosition(13, 9);
            Console.Write(error);
        }

        static void PrintBoard()
        {
            //Printing the column numbers
            Console.SetCursorPosition(2, 0);
            for (int i = 1; i <= board.GetLength(1); i++)
            {
                Console.Write(i);
            }

            //Top board boundary
            Console.SetCursorPosition(0, 1);
            Console.Write(" +");
            for (int i = 0; i < board.GetLength(1); i++)
            {
                Console.Write("-");
            }
            Console.Write("+");

            //Inside the board
            for (int i = 0; i < board.GetLength(0); i++)
            {
                Console.SetCursorPosition(0, i + 2);
                Console.Write(i + 1);//Row number
                Console.Write("|");
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (isPieceSelected && (selectedPieceX == j) && (selectedPieceY == i))
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(board[i, j]);
                        Console.ResetColor();
                    }
                    else if (board[i, j] == Player1Piece)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(board[i, j]);
                        Console.ResetColor();
                    }
                    else if (board[i, j] == Player2Piece)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write(board[i, j]);
                        Console.ResetColor();
                    }
                    else
                        Console.Write(board[i, j]);
                }
                Console.Write("|");
            }

            //Bottom board boundary
            Console.SetCursorPosition(0, board.GetLength(1) + 2);
            Console.Write(" +");
            for (int i = 0; i < board.GetLength(1); i++)
            {
                Console.Write("-");
            }
            Console.Write("+");
        }

        static void PrintInfo()
        {
            Console.SetCursorPosition(13, 1);
            Console.Write("Round: " + round);
            Console.SetCursorPosition(13, 2);
            if (turn == 0)
                Console.Write("Turn: x");
            else
                Console.Write("Turn: o");
        }

        static bool WinConditionCheck()
        {
            if (turn == 0)
            {
                for (int i = 0; i <= 2; i++)
                {
                    for (int j = 0; j <= 2; j++)
                    {
                        if (board[i, j] != Player1Piece)
                            return false;
                    }
                }
            }
            else
            {
                for (int i = board.GetLength(0) - 3; i < board.GetLength(0); i++)
                {
                    for (int j = board.GetLength(1) - 3; j < board.GetLength(1); j++)
                    {
                        if (board[i, j] != Player2Piece)
                            return false;
                    }
                }
            }

            return true;
        }

        static void WinScreen()
        {
            ErrorClear();
            Console.SetCursorPosition(13, 3);
            if (turn == 0)
                Console.Write("Winner: x");
            else
                Console.Write("Winner: o");

            Console.SetCursorPosition(13, 4);
            Console.Write("Press a button to exit.");
            Console.ReadKey(true);
        }

        static void BoardDefault()
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    board[i, j] = EmptyItem;
                }
            }

            for (int i = 0; i <= 2; i++)
            {
                for (int j = 0; j <= 2; j++)
                {
                    board[i, j] = Player2Piece;
                }
            }

            for (int i = board.GetLength(0) - 3; i < board.GetLength(0); i++)
            {
                for (int j = board.GetLength(1) - 3; j < board.GetLength(1); j++)
                {
                    board[i, j] = Player1Piece;
                }
            }
        }
        static int directionfind(int cursorPosX, int cursorPosY)
        {
            if (cursorPosX > selectedPieceX && cursorPosY == selectedPieceY)
                return direction = 0;

            else if (cursorPosX < selectedPieceX && cursorPosY == selectedPieceY)
                return direction = 1;

            else if (cursorPosX == selectedPieceX && cursorPosY < selectedPieceY)
                return direction = 2;

            else
                return direction = 3;

        }
        static bool step_jump_op()
        {
            //Step operation
            if (CanPieceStep(selectedPieceX, selectedPieceY, direction) && cursorPosX == selectedPieceX + 1 && cursorPosY == selectedPieceY && jumpCount == 0)
            {
                jumpCount = 0;
                return true;
            }
            else if (CanPieceStep(selectedPieceX, selectedPieceY, direction) && cursorPosX == selectedPieceX - 1 && cursorPosY == selectedPieceY && jumpCount == 0)
            {
                jumpCount = 0;
                return true;
            }
            else if (CanPieceStep(selectedPieceX, selectedPieceY, direction) && cursorPosX == selectedPieceX && cursorPosY == selectedPieceY - 1 && jumpCount == 0)
            {
                jumpCount = 0;
                return true;
            }
            else if (CanPieceStep(selectedPieceX, selectedPieceY, direction) && cursorPosX == selectedPieceX && cursorPosY == selectedPieceY + 1 && jumpCount == 0)
            {
                jumpCount = 0;
                return true;
            }
            //Jump operation
            else if (CanPieceJump(selectedPieceX, selectedPieceY, direction) && cursorPosX == selectedPieceX + 2 && cursorPosY == selectedPieceY)
            {
                jumpCount = jumpCount + 1;
                return true;
            }
            else if (CanPieceJump(selectedPieceX, selectedPieceY, direction) && cursorPosX == selectedPieceX - 2 && cursorPosY == selectedPieceY)
            {
                jumpCount = jumpCount + 1;
                return true;
            }
            else if (CanPieceJump(selectedPieceX, selectedPieceY, direction) && cursorPosX == selectedPieceX && cursorPosY == selectedPieceY - 2)
            {
                jumpCount = jumpCount + 1;
                return true;
            }
            else if (CanPieceJump(selectedPieceX, selectedPieceY, direction) && cursorPosX == selectedPieceX && cursorPosY == selectedPieceY + 2)
            {
                jumpCount = jumpCount + 1;
                return true;
            }

            else
            {
                ErrorNotification("Can not move to there"); return false;
            }

        }

    }
}
