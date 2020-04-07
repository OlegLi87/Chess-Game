using System;
using System.Threading;

namespace chess_game_oleg
{
    class Program
    {
        static ChessPiece[,] board;
        static ChessPiece currentlyPlayingPiece;
        static Player[] players;
        static King whiteKing, blackKing;
        static string boardStatesLog;
        static char splitChar;
        static int fiftyMoveCounter;

        static Program()
        {
            board = new ChessPiece[8, 8];
            players = new Player[2];
            splitChar = ';';
        }

        static void Main(string[] args)
        {
            bool endGame = false, gameOver = false;

            while (!endGame)
            {
                placeChessPieces();
                createPlayers();
                drawTheBoard();

                int moveCounter = 0;
                fiftyMoveCounter = 0;
                boardStatesLog = String.Empty;

                while (!gameOver)
                {
                    Player playingNow = moveCounter++ % 2 == 0 ? players[0] : players[1];
                    string playingNowColor = playingNow.GetColor();
                    string input;
                    do
                    {
                        Console.Write("\n{0} make your move with {1}'s : ", playingNow, playingNowColor);
                        input = Console.ReadLine();
                    }
                    while (!makeAMove(inputConverter(input), playingNow));
                    registerBoardState();
                    if (isADrawByThreefold_repetition()) break;// must be declared before actual move is made,in my case before a board re-draw itself
                    drawTheBoard();
                    King opponentsKing = playingNowColor == "white" ? blackKing : whiteKing;
                    // checking for a check position
                    if (opponentsKing.IsACheck(board))
                    {
                        opponentsKing.SetCheckedBy(currentlyPlayingPiece);
                        // checking for a mate position
                        if (opponentsKing.IsAMate(board))
                        {
                            Console.WriteLine("\t\t!!! Mate !!!");
                            Thread.Sleep(5000);
                            break;
                        }
                        Console.WriteLine("\t\t!!! Check !!!");
                        Thread.Sleep(2000);
                        drawTheBoard();
                    }
                    else if (isADraw(playingNow)) gameOver = true;
                }
                Console.WriteLine("To continue type \"y\" ");
                endGame = Console.ReadLine().ToLower() != "y" ? true : false;
                //clear the board
                if (!endGame)
                {
                    for (int row = 0; row < board.GetLength(0); row++)
                    {
                        for (int column = 0; column < board.GetLength(1); column++)
                        {
                            board[row, column] = null;
                        }
                    }
                }
            }
        }
        static void placeChessPieces()
        {
            //placing pawns        
            for (int i = 0; i < 16; i++)
            {
                if (i < 8) board[1, i] = new Pawn("black", "pawn");
                else board[6, i - 8] = new Pawn("white", "pawn");
            }
            //placing Rooks
            board[0, 0] = new Rook("black", "rook");
            board[0, 7] = new Rook("black", "rook");
            board[7, 0] = new Rook("white", "rook");
            board[7, 7] = new Rook("white", "rook");
            //placing knights
            board[0, 1] = new Knight("black", "nkight");
            board[0, 6] = new Knight("black", "nkight");
            board[7, 1] = new Knight("white", "nkight");
            board[7, 6] = new Knight("white", "nkight");
            //placing bishops
            board[0, 2] = new Bishop("black", "bishop");
            board[0, 5] = new Bishop("black", "bishop");
            board[7, 2] = new Bishop("white", "bishop");
            board[7, 5] = new Bishop("white", "bishop");
            //placing queens
            board[0, 3] = new Queen("black", "queen");
            board[7, 3] = new Queen("white", "queen");
            //placing kings
            whiteKing = new King("white", "king");
            blackKing = new King("black", "king");
            board[0, 4] = blackKing;
            board[7, 4] = whiteKing;
            registerBoardState();
        }

        static void registerBoardState()
        {
            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int column = 0; column < board.GetLength(1); column++)
                {
                    boardStatesLog += board[row, column] == null ? "?" : board[row, column].ToString();
                }
            }
            boardStatesLog += splitChar;
        }

        static void createPlayers()
        {
            string name1, name2;
            Console.WriteLine("First player type your name in ...");
            name1 = Console.ReadLine();
            Console.WriteLine("Second player type your name in...");
            name2 = Console.ReadLine();

            if (new Random().Next(1, 11) > 5)
            {
                players[0] = new Player(name1, "white");
                players[1] = new Player(name2, "black");
            }
            else
            {
                players[0] = new Player(name2, "white");
                players[1] = new Player(name1, "black");
            }
        }
        static void drawTheBoard()
        {
            Console.Clear();
            Console.Write("   A  B  C  D  E  F  G  H\n");

            for (int i = 8; i > 0; i--)
            {
                Console.Write(i + "  ");
                for (int k = 0; k < 8; k++)
                {
                    ChessPiece cp = board[8 - i, k];
                    if (cp != null) Console.Write(cp + " ");
                    else Console.Write("-- ");
                }
                Console.WriteLine();
            }
        }
        static int[] inputConverter(string input)
        {
            int[] coordinates = new int[4];//first index represents currentX,second currentY,third nextX...
            int a_char_ASCII_value = 'a';

            if (input.Length != 4)
            {
                Console.WriteLine("Input length is not valid!!!");
                return new int[] { };
            }
            for (int i = 0; i < input.Length; i++)
            {
                if (i % 2 == 0)
                {
                    if (input[i] < 'a' || input[i] > 'h')
                    {
                        Console.WriteLine("Validation process failed at char number {0}", i + 1);
                        return new int[] { };
                    }
                    coordinates[i] = input[i] - a_char_ASCII_value;
                }
                else
                {
                    if (input[i] < '1' || input[i] > '8')
                    {
                        Console.WriteLine("Validation process failed at char number {0}", i + 1);
                        return new int[] { };
                    }
                    coordinates[i] = 8 - Int32.Parse(input[i] + "");
                }
            }
            return coordinates;
        }

        static bool makeAMove(int[] coordinates, Player playingNow)
        {
            if (coordinates.Length != 4) return false;
            fiftyMoveCounter++;
            ChessPiece cp = board[coordinates[1], coordinates[0]]; // picked chess piece
            currentlyPlayingPiece = cp;
            ChessPiece targetCp = board[coordinates[3], coordinates[2]]; // targeted cell

            if (cp == null)
            {
                Console.WriteLine("The cell is empty!!!");
                return false;
            }
            if (cp.GetColor() != playingNow.GetColor())
            {
                Console.WriteLine(playingNow + " dont touch your opponent's chess piece!!!");
                return false;
            }
            /*  ---- Conditions for a successful castling ----
           The king has not previously moved;
           Your chosen rook has not previously moved;
           The king is not currently in check;
           */
            //Checking for castling move for a whites               
            if (coordinates[0] == 4 && coordinates[1] == 7 && coordinates[3] == 7 && (coordinates[2] == 0 || coordinates[2] == 7) && cp is King && cp.GetColor().ToLower() == "white" && ((King)cp).GetPrevioslyMoved() == false && targetCp is Rook && targetCp.GetColor().ToLower() == "white" && ((Rook)targetCp).GetPrevioslyMoved() == false)
            {
                if (((King)cp).IsACheck(board))
                {
                    Console.WriteLine("Castling cannot be completed,your king is under a check!!!");
                    return false;
                }
                return processCastling("white", coordinates[2]);
            }
            //Checking for castling move for a blacks
            if (coordinates[0] == 4 && coordinates[1] == 0 && coordinates[3] == 0 && (coordinates[2] == 0 || coordinates[2] == 7) && cp is King && cp.GetColor().ToLower() == "black" && ((King)cp).GetPrevioslyMoved() == false && targetCp is Rook && targetCp.GetColor().ToLower() == "black" && ((Rook)targetCp).GetPrevioslyMoved() == false)
            {
                if (((King)cp).IsACheck(board))
                {
                    Console.WriteLine("Castling cannot be completed,your king is under a check!!!");
                    return false;
                }
                return processCastling("black", coordinates[2]);
            }
            if (targetCp != null && targetCp.GetColor() == playingNow.GetColor())
            {
                Console.WriteLine("Your target cell is allready taken by your chess piece!!!");
                return false;
            }
            if (!cp.IsValidMove(coordinates, targetCp == null ? false : true, board))
            {
                Console.WriteLine("Illegal move!!!");
                return false;
            }
            // checking for promotion
            if (cp is Pawn && (coordinates[3] == 0 && cp.GetColor() == "white" || coordinates[3] == 7 && cp.GetColor() == "black"))
            {
                bool valid = false;
                while (!valid)
                {
                    valid = true;
                    Console.WriteLine("{0} your pawn is promoted choose which chess piece you would like to use \n" +
                   "for Queen type q\nfor Knight type : n\nfor Bishop type b\nfor Rook type r");
                    string input = Console.ReadLine();
                    switch (input.ToLower())
                    {
                        case "q":
                            cp = new Queen(playingNow.GetColor(), "queen");
                            break;
                        case "n":
                            cp = new Knight(playingNow.GetColor(), "nkight");
                            break;
                        case "b":
                            cp = new Bishop(playingNow.GetColor(), "bishop");
                            break;
                        case "r":
                            cp = new Rook(playingNow.GetColor(), "rook");
                            break;
                        default:
                            Console.WriteLine("Your input is incorrect!!!");
                            valid = false;
                            break;
                    }
                }
            }
            if (targetCp != null) fiftyMoveCounter = 0;
            // ask a king for a check threat
            King playersKing = playingNow.GetColor().ToLower() == "white" ? whiteKing : blackKing;
            board[coordinates[3], coordinates[2]] = cp;
            board[coordinates[1], coordinates[0]] = null;
            if (playersKing.IsACheck(board))
            {
                Console.WriteLine("Illegal move!!!Your king is compromised!!!");
                board[coordinates[3], coordinates[2]] = targetCp;
                board[coordinates[1], coordinates[0]] = cp;
                return false;
            }
            if (cp is Pawn)
            {
                fiftyMoveCounter = 0;
                ((Pawn)cp).ChangeFirstMove();
                if (Math.Abs(coordinates[1] - coordinates[3]) == 2) ((Pawn)cp).SetValidFor_enPassant();
            }
            else if (cp is King) ((King)cp).ChangePrevioslyMoved();
            else if (cp is Rook) ((Rook)cp).ChangePrevioslyMoved();
            return true;
        }
        /*  ---- Conditions for a successful castling ----        
        There must be no pieces between the king and the chosen rook;          
        Your king must not pass through a square that is under attack by enemy pieces;
        The king must not end up in check.
        */
        static bool processCastling(string color, int chosenRook_X_position)
        {
            King currentKing = color == "black" ? blackKing : whiteKing;
            int row = color == "black" ? 0 : 7;
            int step = chosenRook_X_position > 4 ? 1 : -1, position = step;

            while (4 + position != chosenRook_X_position)
            {
                if (board[row, 4 + position] != null)
                {
                    Console.WriteLine("Castling cannot be completed,there is some piece/s in between!!!");
                    return false;
                }
                if (Math.Abs(position) < 3)
                {
                    board[row, 4 + position] = currentKing;
                    if (chosenRook_X_position > 4) board[row, 4 + position - 1] = null;
                    else board[row, 4 + position + 1] = null;

                    if (currentKing.IsACheck(board))
                    {
                        Console.WriteLine("Castling cannot be completed because a check compromission!!!");
                        board[row, 4 + position] = null;
                        board[row, 4] = currentKing;
                        return false;
                    }
                    board[row, 4 + position] = null;
                    board[row, 4] = currentKing;
                }
                position += step;
            }
            if (chosenRook_X_position > 4)
            {
                board[row, 6] = currentKing;
                currentKing.ChangePrevioslyMoved();
                board[row, 4] = null;
                board[row, 5] = board[row, 7];
                board[row, 7] = null;
                ((Rook)board[row, 5]).ChangePrevioslyMoved();
            }
            else
            {
                board[row, 2] = currentKing;
                currentKing.ChangePrevioslyMoved();
                board[row, 4] = null;
                board[row, 3] = board[row, 0];
                board[row, 0] = null;
                ((Rook)board[row, 3]).ChangePrevioslyMoved();
            }
            return true;
        }
        //Threefold Repetition
        static bool isADrawByThreefold_repetition()
        {
            string[] states = boardStatesLog.Split(splitChar);
            int sameState = 0;

            for (int i = 0; i < states.Length - 2; i++)
            {
                if (states[i] == states[states.Length - 2]) sameState++;
                if (sameState == 2)
                {
                    Console.WriteLine("Draw by Threefold Repetition!!!");
                    Thread.Sleep(3000);
                    return true;
                }
            }
            return false;
        }
        static bool isADraw(Player playingNow)
        {
            //The Fifty-Move Rule
            if (fiftyMoveCounter == 50)
            {
                Console.WriteLine("Draw by Fifty-Move Rule");
                Thread.Sleep(5000);
                return true;
            }

            //Insufficient Mating Material
            //Checking which pieces remained on the board
            string remainedPieces = String.Empty;
            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int column = 0; column < board.GetLength(1); column++)
                {
                    ChessPiece cp = board[row, column];
                    if (cp != null) remainedPieces += cp;
                }
            }
            // Only two kings left on the board
            if (remainedPieces.Length / 2 == 2)
            {
                Console.WriteLine("Draw By insuficient material!!!");
                Thread.Sleep(5000);
                return true;
            }
            // Two kings + another piece
            if (remainedPieces.Length / 2 == 3)
            {
                if (remainedPieces.Contains("BN") || remainedPieces.Contains("WN") || remainedPieces.Contains("WB") || remainedPieces.Contains("BB"))
                {
                    Console.WriteLine("Draw By insuficient material!!!");
                    System.Threading.Thread.Sleep(5000);
                    return true;
                }
            }

            //Stalemate
            string color = playingNow.GetColor().ToLower() == "white" ? "black" : "white";
            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int column = 0; column < board.GetLength(1); column++)
                {
                    ChessPiece cp = board[row, column];
                    if (cp != null && cp.GetColor().ToLower() == color)
                    {
                        for (int targetRow = 0; targetRow < board.GetLength(0); targetRow++)
                        {
                            for (int targetColumn = 0; targetColumn < board.GetLength(1); targetColumn++)
                            {
                                if (cp.IsValidMove(new int[] { column, row, targetColumn, targetRow }, true, board))
                                {
                                    ChessPiece targetCp = board[targetRow, targetColumn];
                                    board[targetRow, targetColumn] = cp;
                                    board[row, column] = null;
                                    King king = color == "white" ? whiteKing : blackKing;
                                    if (!king.IsACheck(board))
                                    {
                                        board[row, column] = cp;
                                        board[targetRow, targetColumn] = targetCp;
                                        return false;
                                    }
                                    board[row, column] = cp;
                                    board[targetRow, targetColumn] = targetCp;
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Draw by a StaleMate!!!");
            Thread.Sleep(5000);
            return true;
        }
    }

    class Player
    {
        string name;
        string color;

        public Player(string name, string color)
        {
            this.name = name;
            this.color = color;
        }

        public string GetColor() { return color; }

        public override string ToString()
        {
            return name;
        }
    }

    class ChessPiece
    {
        string color;
        string name;

        public ChessPiece(string color, string name)
        {
            this.color = color;
            this.name = name;
        }
        public virtual bool IsValidMove(int[] coordinates, bool toEat, ChessPiece[,] board) { return false; }

        public string GetColor() { return color; }

        public override string ToString() { return color.ToUpper()[0] + "" + name.ToUpper()[0]; }

        public int[] GetCurrentLocation(ChessPiece[,] board)
        {
            int[] currentLocation = new int[2];
            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int column = 0; column < board.GetLength(1); column++)
                {
                    if (board[row, column] == this)
                    {
                        currentLocation[0] = column;
                        currentLocation[1] = row;
                    }
                }
            }
            return currentLocation;
        }

        public bool CheckDiagonals(int[] coordinates, ChessPiece[,] board)
        {
            int currentX = coordinates[0], nextX = coordinates[2], currentY = coordinates[1], nextY = coordinates[3];

            if (Math.Abs(currentX - nextX) == Math.Abs(currentY - nextY))
            {
                //top right direction
                if (currentX < nextX && currentY > nextY)
                {
                    for (int i = 1; i < nextX - currentX; i++)
                        if (board[currentY - i, currentX + i] != null) return false;
                    return true;
                }
                //bottom rigtht direction
                if (currentX < nextX && currentY < nextY)
                {
                    for (int i = 1; i < nextX - currentX; i++)
                        if (board[currentY + i, currentX + i] != null) return false;
                    return true;
                }
                //top left direction
                if (currentX > nextX && currentY > nextY)
                {
                    for (int i = 1; i < currentX - nextX; i++)
                        if (board[currentY - i, currentX - i] != null) return false;
                    return true;
                }
                //bottom left direction
                if (currentX > nextX && currentY < nextY)
                {
                    for (int i = 1; i < currentX - nextX; i++)
                        if (board[currentY + i, currentX - i] != null) return false;
                    return true;
                }
            }
            return false;
        }

        public bool CheckHorizontalAndVertical(int[] coordinates, ChessPiece[,] board)
        {
            int currentX = coordinates[0], nextX = coordinates[2], currentY = coordinates[1], nextY = coordinates[3];

            if (nextY == currentY)
            {
                if (currentX > nextX)
                {
                    for (int i = nextX + 1; i < currentX; i++)
                    {

                        if (board[currentY, i] != null) return false;
                    }
                    return true;
                }
                for (int i = currentX + 1; i < nextX; i++)
                {
                    if (board[currentY, i] != null) return false;
                }
                return true;
            }
            else if (nextX == currentX)
            {
                if (currentY > nextY)
                {
                    for (int i = nextY + 1; i < currentY; i++)
                    {
                        if (board[i, currentX] != null) return false;
                    }
                    return true;
                }
                for (int i = currentY + 1; i < nextY; i++)
                {
                    if (board[i, currentX] != null) return false;
                }
                return true;
            }
            return false;
        }
    }

    class Pawn : ChessPiece
    {
        bool firstMove;
        bool validFor_enPassant;
        public Pawn(string color, string name) : base(color, name) { firstMove = true; }

        public void ChangeFirstMove() { firstMove = false; }
        public void SetValidFor_enPassant() { validFor_enPassant = true; }
        public bool GetValidFor_enPassant() { return validFor_enPassant; }

        public override bool IsValidMove(int[] coordinates, bool toEat, ChessPiece[,] board)
        {
            int currentX = coordinates[0], nextX = coordinates[2], currentY = coordinates[1], nextY = coordinates[3];
            //en passant validaion
            //The capturing pawn must be on the fifth rank. The captured pawn must be on the fourth rank.
            if (((GetColor().ToLower() == "white" && currentY == 3 && currentY - nextY == 1) || (GetColor().ToLower() == "black" && currentY == 4 && currentY - nextY == -1))
                && Math.Abs(currentX - nextX) == 1)
            {
                int verticalStep = GetColor().ToLower() == "white" ? 1 : -1;
                ChessPiece potentialPawnA = null;
                ChessPiece potentialPawnB = null;

                //The captured pawn MUST move two squares forward.
                if (currentX + 1 <= 7) potentialPawnA = board[currentY, currentX + 1]; // to the right
                if (currentX - 1 >= 0) potentialPawnB = board[currentY, currentX - 1]; // to the left

                if (potentialPawnA is Pawn && (nextX - currentX == 1) && ((Pawn)potentialPawnA).GetValidFor_enPassant())
                {
                    if (board[currentY + verticalStep, currentX + 1] == null)
                    {
                        board[currentY, currentX + 1] = null;
                        return true;
                    }
                }
                if (potentialPawnB is Pawn && (nextX - currentX == -1) && ((Pawn)potentialPawnB).GetValidFor_enPassant())
                {
                    if (board[currentY + verticalStep, currentX - 1] == null)
                    {
                        board[currentY, currentX - 1] = null;
                        return true;
                    }
                }
            }
            // non epassant validation
            if (!toEat && nextX == currentX)
            {
                if (firstMove)
                {
                    if (Math.Abs(nextY - currentY) == 2)
                    {
                        if ((board[nextY + 1, currentX] == null && GetColor().ToLower() == "white") || (board[nextY - 1, currentX] == null && GetColor().ToLower() == "black")) return true;
                        return false;
                    }
                    if ((nextY - currentY == 1 && GetColor().ToLower() == "black") || (nextY - currentY == -1 && GetColor().ToLower() == "white")) return true;
                    return false;
                }
                if ((GetColor().ToLower() == "white" && currentY - nextY == 1) || (GetColor().ToLower() == "black" && currentY - nextY == -1)) return true;
                return false;
            }
            else if (toEat && ((nextY - currentY == 1 && GetColor().ToLower() == "black") || nextY - currentY == -1 && GetColor().ToLower() == "white")) return true;
            return false;
        }
    }
    class Rook : ChessPiece
    {
        bool previoslyMoved;

        public Rook(string color, string name) : base(color, name) { }

        public bool GetPrevioslyMoved() { return previoslyMoved; }
        public void ChangePrevioslyMoved() { previoslyMoved = true; }

        public override bool IsValidMove(int[] coordinates, bool toEat, ChessPiece[,] board)
        {
            return CheckHorizontalAndVertical(coordinates, board);
        }
    }
    class Knight : ChessPiece
    {
        public Knight(string color, string name) : base(color, name) { }

        public override bool IsValidMove(int[] coordinates, bool toEat, ChessPiece[,] board)
        {
            int currentX = coordinates[0], nextX = coordinates[2], currentY = coordinates[1], nextY = coordinates[3];

            if ((Math.Abs(currentX - nextX) == 2 && Math.Abs(currentY - nextY) == 1) ||
               (Math.Abs(currentY - nextY) == 2 && Math.Abs(currentX - nextX) == 1)) return true;
            return false;
        }
    }
    class Bishop : ChessPiece
    {
        public Bishop(string color, string name) : base(color, name) { }

        public override bool IsValidMove(int[] coordinates, bool toEat, ChessPiece[,] board)
        {
            return CheckDiagonals(coordinates, board);
        }
    }
    class Queen : ChessPiece
    {
        public Queen(string color, string name) : base(color, name) { }

        public override bool IsValidMove(int[] coordinates, bool toEat, ChessPiece[,] board)
        {
            return CheckDiagonals(coordinates, board) || CheckHorizontalAndVertical(coordinates, board);
        }
    }
    class King : ChessPiece
    {
        bool previoslyMoved;
        ChessPiece checkedBy;

        public King(string color, string name) : base(color, name) { }

        public bool GetPrevioslyMoved() { return previoslyMoved; }
        public void ChangePrevioslyMoved() { previoslyMoved = true; }

        public void SetCheckedBy(ChessPiece checkedBy) { this.checkedBy = checkedBy; }
        public ChessPiece GetCheckedBy() { return checkedBy; }

        public override bool IsValidMove(int[] coordinates, bool toEat, ChessPiece[,] board)
        {
            int currentX = coordinates[0], nextX = coordinates[2], currentY = coordinates[1], nextY = coordinates[3];
            if ((Math.Abs(currentX - nextX) == 1 && currentY - nextY == 0)
                || (Math.Abs(currentY - nextY) == 1 && currentX - nextX == 0)
                || (Math.Abs(currentX - nextX) == 1 && Math.Abs(currentY - nextY) == 1)) return true;
            return false;
        }

        public bool IsACheck(ChessPiece[,] board)
        {
            string currentColor = this.GetColor();
            int[] currentLocation = GetCurrentLocation(board);
            int currentX = currentLocation[0], currentY = currentLocation[1];
            bool possibleCheck = false;

            //scanning horizontal direction for a check position,looking for rivals Queen,Rook or a King
            for (int i = 0; i < currentX; i++)
            {
                if ((board[currentY, i] is Rook || board[currentY, i] is Queen) && (board[currentY, i].GetColor() != currentColor)) possibleCheck = true;
                else if (currentX - i == 1 && board[currentY, i] is King) return true;
                else if (board[currentY, i] != null) possibleCheck = false;
            }
            if (possibleCheck) return true;

            for (int i = currentX + 1; i < 8; i++)
            {
                if ((board[currentY, i] is Rook || board[currentY, i] is Queen) && (board[currentY, i].GetColor() != currentColor)) return true;
                else if (i - currentX == 1 && board[currentY, i] is King) return true;
                else if (board[currentY, i] != null) break;
            }

            //scanning vertical direction for a check position,looking for rivals Queen or a Rook
            for (int i = 0; i < currentY; i++)
            {
                if ((board[i, currentX] is Rook || board[i, currentX] is Queen) && (board[i, currentX].GetColor() != currentColor)) possibleCheck = true;
                else if (currentY - i == 1 && board[i, currentX] is King) return true;
                else if (board[i, currentX] != null) possibleCheck = false;
            }
            if (possibleCheck) return true;

            for (int i = currentY + 1; i < 8; i++)
            {
                if ((board[i, currentX] is Rook || board[i, currentX] is Queen) && (board[i, currentX].GetColor() != currentColor)) return true;
                else if (i - currentY == 1 && board[i, currentX] is King) return true;
                else if (board[i, currentX] != null) break;
            }
            //scanning diagonals,looking for rivals Queen,Bishop,King or a Pawn
            int counter = 1;
            //scanning bottom left diagonal
            while (currentX - counter >= 0 && currentY + counter <= 7)
            {
                int x = currentY + counter, y = currentX - counter;
                if ((board[x, y] is Bishop || board[x, y] is Queen)
                    && board[x, y].GetColor() != currentColor) return true;
                else if (counter == 1 && board[x, y] is King) return true;
                else if (currentColor == "black" && counter == 1 && board[x, y] is Pawn && board[x, y].GetColor().ToLower() == "white") return true;
                else if (board[x, y] != null) break;
                counter++;
            }
            //scanning upper left diagonal
            counter = 1;
            while (currentX - counter >= 0 && currentY - counter >= 0)
            {
                int x = currentY - counter, y = currentX - counter;
                if ((board[x, y] is Bishop || board[x, y] is Queen)
                    && board[x, y].GetColor() != currentColor) return true;
                else if (counter == 1 && board[x, y] is King) return true;
                else if (currentColor == "white" && counter == 1 && board[x, y] is Pawn && board[x, y].GetColor().ToLower() == "black") return true;
                else if (board[x, y] != null) break;
                counter++;
            }
            //scanning bottom right diagonal
            counter = 1;
            while (currentX + counter <= 7 && currentY + counter <= 7)
            {
                int x = currentY + counter, y = currentX + counter;
                if ((board[x, y] is Bishop || board[x, y] is Queen)
                    && board[x, y].GetColor() != currentColor) return true;
                else if (counter == 1 && board[x, y] is King) return true;
                else if (currentColor == "black" && counter == 1 && board[x, y] is Pawn && board[x, y].GetColor().ToLower() == "white") return true;
                else if (board[x, y] != null) break;
                counter++;
            }
            //scanning upper right diagonal
            counter = 1;
            while (currentX + counter <= 7 && currentY - counter >= 0)
            {
                int x = currentY - counter, y = currentX + counter;
                if ((board[x, y] is Bishop || board[x, y] is Queen)
                    && board[x, y].GetColor() != currentColor) return true;
                else if (counter == 1 && board[x, y] is King) return true;
                else if (currentColor == "white" && counter == 1 && board[x, y] is Pawn && board[x, y].GetColor().ToLower() == "black") return true;
                else if (board[x, y] != null) break;
                counter++;
            }

            //scanning for a Knight check (up -> right)
            if (currentY - 2 >= 0 && currentX + 1 <= 7)
                if (board[currentY - 2, currentX + 1] is Knight && board[currentY - 2, currentX + 1].GetColor() != currentColor) return true;
            //scanning for a Knight check (up -> left)
            if (currentY - 2 >= 0 && currentX - 1 >= 0)
                if (board[currentY - 2, currentX - 1] is Knight && board[currentY - 2, currentX - 1].GetColor() != currentColor) return true;
            //scanning for a Knight check (left -> up)
            if (currentY - 1 >= 0 && currentX - 2 >= 0)
                if (board[currentY - 1, currentX - 2] is Knight && board[currentY - 1, currentX - 2].GetColor() != currentColor) return true;
            //scanning for a Knight check (left -> bottom)
            if (currentY + 1 <= 7 && currentX - 2 >= 0)
                if (board[currentY + 1, currentX - 2] is Knight && board[currentY + 1, currentX - 2].GetColor() != currentColor) return true;
            //scanning for a Knight check (bottom -> left)
            if (currentY + 2 <= 7 && currentX - 1 >= 0)
                if (board[currentY + 2, currentX - 1] is Knight && board[currentY + 2, currentX - 1].GetColor() != currentColor) return true;
            //scanning for a Knight check (bottom -> right)
            if (currentY + 2 <= 7 && currentX + 1 <= 7)
                if (board[currentY + 2, currentX + 1] is Knight && board[currentY + 2, currentX + 1].GetColor() != currentColor) return true;
            //scanning for a Knight check (right -> bottom)
            if (currentY + 1 <= 7 && currentX + 2 <= 7)
                if (board[currentY + 1, currentX + 2] is Knight && board[currentY + 1, currentX + 2].GetColor() != currentColor) return true;
            //scanning for a Knight check (right -> up)
            if (currentY - 1 >= 0 && currentX + 2 <= 7)
                if (board[currentY - 1, currentX + 2] is Knight && board[currentY - 1, currentX + 2].GetColor() != currentColor) return true;
            return false;
        }

        public bool IsAMate(ChessPiece[,] board)
        {
            ChessPiece checkedBy = this.checkedBy;
            int[] kings_currentLocation = GetCurrentLocation(board);
            int[] opponents_currentLocation = checkedBy.GetCurrentLocation(board);
            bool kingCanEscape = false;
            int step = 1;

            //Checking if king can escape a check by making a move             
            //to right
            if (kings_currentLocation[0] + step < 8 && (board[kings_currentLocation[1], kings_currentLocation[0] + step] == null || board[kings_currentLocation[1], kings_currentLocation[0] + step].GetColor() != GetColor()))
            {
                ChessPiece targetedCp = board[kings_currentLocation[1], kings_currentLocation[0] + step];
                board[kings_currentLocation[1], kings_currentLocation[0] + step] = this;
                board[kings_currentLocation[1], kings_currentLocation[0]] = null;
                if (!IsACheck(board)) kingCanEscape = true;
                board[kings_currentLocation[1], kings_currentLocation[0] + step] = targetedCp;
                board[kings_currentLocation[1], kings_currentLocation[0]] = this;
                if (kingCanEscape) return false;
            }
            //to left
            if (kings_currentLocation[0] - step >= 0 && (board[kings_currentLocation[1], kings_currentLocation[0] - step] == null || board[kings_currentLocation[1], kings_currentLocation[0] - step].GetColor() != GetColor()))
            {
                ChessPiece targetedCp = board[kings_currentLocation[1], kings_currentLocation[0] - step];
                board[kings_currentLocation[1], kings_currentLocation[0] - step] = this;
                board[kings_currentLocation[1], kings_currentLocation[0]] = null;
                if (!IsACheck(board)) kingCanEscape = true;
                board[kings_currentLocation[1], kings_currentLocation[0] - step] = targetedCp;
                board[kings_currentLocation[1], kings_currentLocation[0]] = this;
                if (kingCanEscape) return false;
            }
            //to bottom
            if (kings_currentLocation[1] + step < 8 && (board[kings_currentLocation[1] + step, kings_currentLocation[0]] == null || board[kings_currentLocation[1] + step, kings_currentLocation[0]].GetColor() != GetColor()))
            {
                ChessPiece targetedCp = board[kings_currentLocation[1] + step, kings_currentLocation[0]];
                board[kings_currentLocation[1] + step, kings_currentLocation[0]] = this;
                board[kings_currentLocation[1], kings_currentLocation[0]] = null;
                if (!IsACheck(board)) kingCanEscape = true;
                board[kings_currentLocation[1] + step, kings_currentLocation[0]] = targetedCp;
                board[kings_currentLocation[1], kings_currentLocation[0]] = this;
                if (kingCanEscape) return false;
            }
            //to top
            if (kings_currentLocation[1] - step >= 0 && (board[kings_currentLocation[1] - step, kings_currentLocation[0]] == null || board[kings_currentLocation[1] - step, kings_currentLocation[0]].GetColor() != GetColor()))
            {
                ChessPiece targetedCp = board[kings_currentLocation[1] - step, kings_currentLocation[0]];
                board[kings_currentLocation[1] - step, kings_currentLocation[0]] = this;
                board[kings_currentLocation[1], kings_currentLocation[0]] = null;
                if (!IsACheck(board)) kingCanEscape = true;
                board[kings_currentLocation[1] - step, kings_currentLocation[0]] = targetedCp;
                board[kings_currentLocation[1], kings_currentLocation[0]] = this;
                if (kingCanEscape) return false;
            }
            // to top right
            if (kings_currentLocation[0] + step < 8 && kings_currentLocation[1] - step >= 0 &&
            (board[kings_currentLocation[1] - step, kings_currentLocation[0] + step] == null || board[kings_currentLocation[1] - step, kings_currentLocation[0] + step].GetColor() != GetColor()))
            {
                ChessPiece targetedCp = board[kings_currentLocation[1] - step, kings_currentLocation[0] + step];
                board[kings_currentLocation[1] - step, kings_currentLocation[0] + step] = this;
                board[kings_currentLocation[1], kings_currentLocation[0]] = null;
                if (!IsACheck(board)) kingCanEscape = true;
                board[kings_currentLocation[1] - step, kings_currentLocation[0] + step] = targetedCp;
                board[kings_currentLocation[1], kings_currentLocation[0]] = this;
                if (kingCanEscape) return false;
            }
            // to top left
            if (kings_currentLocation[0] - step >= 0 && kings_currentLocation[1] - step >= 0 &&
            (board[kings_currentLocation[1] - step, kings_currentLocation[0] - step] == null || board[kings_currentLocation[1] - step, kings_currentLocation[0] - step].GetColor() != GetColor()))
            {
                ChessPiece targetedCp = board[kings_currentLocation[1] - step, kings_currentLocation[0] - step];
                board[kings_currentLocation[1] - step, kings_currentLocation[0] - step] = this;
                board[kings_currentLocation[1], kings_currentLocation[0]] = null;
                if (!IsACheck(board)) kingCanEscape = true;
                board[kings_currentLocation[1] - step, kings_currentLocation[0] - step] = targetedCp;
                board[kings_currentLocation[1], kings_currentLocation[0]] = this;
                if (kingCanEscape) return false;
            }
            // to bottom left
            if (kings_currentLocation[0] - step >= 0 && kings_currentLocation[1] + step < 8 &&
            (board[kings_currentLocation[1] + step, kings_currentLocation[0] - step] == null || board[kings_currentLocation[1] + step, kings_currentLocation[0] - step].GetColor() != GetColor()))
            {
                ChessPiece targetedCp = board[kings_currentLocation[1] + step, kings_currentLocation[0] - step];
                board[kings_currentLocation[1] + step, kings_currentLocation[0] - step] = this;
                board[kings_currentLocation[1], kings_currentLocation[0]] = null;
                if (!IsACheck(board)) kingCanEscape = true;
                board[kings_currentLocation[1] + step, kings_currentLocation[0] - step] = targetedCp;
                board[kings_currentLocation[1], kings_currentLocation[0]] = this;
                if (kingCanEscape) return false;
            }
            // to bottom right
            if (kings_currentLocation[0] + step < 8 && kings_currentLocation[1] + step < 8 &&
            (board[kings_currentLocation[1] + step, kings_currentLocation[0] + step] == null || board[kings_currentLocation[1] + step, kings_currentLocation[0] + step].GetColor() != GetColor()))
            {
                ChessPiece targetedCp = board[kings_currentLocation[1] + step, kings_currentLocation[0] + step];
                board[kings_currentLocation[1] + step, kings_currentLocation[0] + step] = this;
                board[kings_currentLocation[1], kings_currentLocation[0]] = null;
                if (!IsACheck(board)) kingCanEscape = true;
                board[kings_currentLocation[1] + step, kings_currentLocation[0] + step] = targetedCp;
                board[kings_currentLocation[1], kings_currentLocation[0]] = this;
                if (kingCanEscape) return false;
            }
            //Checking if oppennt's knight can be taken by any king's team pieces
            if (checkedBy is Knight)
            {
                for (int i = 0; i < board.GetLength(0); i++)
                {
                    for (int k = 0; k < board.GetLength(1); k++)
                    {
                        if (board[i, k] != null && board[i, k].GetColor() == GetColor())
                        {
                            ChessPiece cp = board[i, k];
                            if (cp.IsValidMove(new int[] { k, i, opponents_currentLocation[0], opponents_currentLocation[1] }, true, board))
                            {
                                board[opponents_currentLocation[1], opponents_currentLocation[0]] = cp;
                                board[i, k] = null;
                                if (!IsACheck(board)) kingCanEscape = true;
                                board[opponents_currentLocation[1], opponents_currentLocation[0]] = checkedBy;
                                board[i, k] = cp;
                                if (kingCanEscape) return false;
                            }
                        }
                    }
                }
            }
            //Checking if any of kings team pieces can block their king or to take a checking opponets piece               
            int stepX, stepY;

            if (kings_currentLocation[0] > opponents_currentLocation[0]) stepX = -1;
            else if (kings_currentLocation[0] < opponents_currentLocation[0]) stepX = 1;
            else stepX = 0;

            if (kings_currentLocation[1] > opponents_currentLocation[1]) stepY = -1;
            else if (kings_currentLocation[1] < opponents_currentLocation[1]) stepY = 1;
            else stepY = 0;

            while (kings_currentLocation[0] + stepX != opponents_currentLocation[0] || kings_currentLocation[1] + stepY != opponents_currentLocation[1])
            {
                for (int row = 0; row < board.GetLength(0); row++)
                {
                    for (int column = 0; column < board.GetLength(1); column++)
                    {
                        if (board[row, column] != null && board[row, column].GetColor() == GetColor())
                        {
                            ChessPiece cp = board[row, column];
                            if (cp.IsValidMove(new int[] { column, row, kings_currentLocation[0] + stepX, kings_currentLocation[1] + stepY }, true, board))
                            {
                                ChessPiece targetedCp = board[kings_currentLocation[1] + stepY, kings_currentLocation[0] + stepX];
                                board[kings_currentLocation[1] + stepY, kings_currentLocation[0] + stepX] = cp;
                                board[row, column] = null;
                                if (!IsACheck(board)) kingCanEscape = true;
                                board[row, column] = cp;
                                board[kings_currentLocation[1] + stepY, kings_currentLocation[0] + stepX] = targetedCp;
                                if (kingCanEscape) return false;
                            }
                        }
                    }
                }
                stepX += stepX;
                stepY += stepY;
            }
            return true;
        }
    }
}