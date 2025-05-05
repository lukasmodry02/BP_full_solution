using Emgu.CV;

namespace ChessNotationsGenerator.CoreLogic;
public class Game() 
{
    private readonly ChessNotation _chessNotation = new();
    private List<ChessBoard> GameStates { get; } = [];
    private List<string> PicPaths { get; } = [];
    // private  List<(List<(int row, int col)> position, bool isCaptured)> Promoted { get; } = []; //not implemented 

    private List<string> GetPicPaths()
    {
        return PicPaths;
    }
    
    private string GetPicPathOnIndex(int index)
    {
        return PicPaths[index];
    }

    public List<ChessBoard> GetGameStates()
    {
        return GameStates;
    }
    
    private ChessBoard GetGameStateOnIndex(int index)
    {
        return GameStates[index];
    }
    
    //Updated to parallel loading kept for testing
    // public void LoadGame(string gameFolder, bool sortByDateTaken = true)
    // {
    //     var imagePaths = Directory.GetFiles(gameFolder, "*.*")
    //         .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
    //                        file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
    //                        file.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
    //
    //     var sortedPaths = sortByDateTaken
    //         ? imagePaths.OrderBy(path => ImageProcessing.GetDateTaken(path) ?? new FileInfo(path).CreationTime).ToList()
    //         : imagePaths.OrderBy(Path.GetFileName).ToList();
    //
    //     GameStates.Clear();
    //     PicPaths.Clear();
    //
    //     if (sortedPaths.Count == 0)
    //     {
    //         Console.WriteLine("No images found in the folder.");
    //         return;
    //     }
    //
    //     // Load and add the first (empty) board
    //     var firstImage = CvInvoke.Imread(sortedPaths[0]);
    //     var emptyBoard = ImageProcessing.InitializeChessBoardFromImage(firstImage);
    //     GameStates.Add(emptyBoard);
    //     PicPaths.Add(sortedPaths[0]);
    //
    //     // Load the rest using the first board as reference
    //     for (var i = 1; i < sortedPaths.Count; i++)
    //     {
    //         var image = CvInvoke.Imread(sortedPaths[i]);
    //         if (image.IsEmpty) continue;
    //
    //         var filledBoard = ImageProcessing.InitializeChessBoardFromImage(image, emptyBoard);
    //         GameStates.Add(filledBoard);
    //         PicPaths.Add(sortedPaths[i]);
    //     }
    //
    //     Console.WriteLine("Pictures count: " + GameStates.Count);
    //     Console.WriteLine();
    // }
    //
    // public bool IsFirstBoardEmpty()
    // {
    //     if (GameStates.Count < 2)
    //         return false;
    //
    //     var counter = 0;
    //     var firstBoard = GameStates[0].Squares; 
    //     var secondBoard = GameStates[1].Squares;
    //     for (int i = 0; i < firstBoard.GetLength(0); i++)
    //     {
    //         for (int j = 0; j < firstBoard.GetLength(1); j++)
    //         {
    //             if (firstBoard[i, j].IsOccupied != secondBoard[i, j].IsOccupied)
    //             {
    //                 counter++;
    //             }
    //         } 
    //     }
    //
    //     Console.WriteLine($"number of different pictures: {counter}");
    //     return counter < 3;
    // }

    public void LoadGameParallel(string gameFolder, bool sortByDateTaken = true)
    {
        var imagePaths = Directory.GetFiles(gameFolder, "*.*")
            .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".png", StringComparison.OrdinalIgnoreCase));

        var sortedPaths = sortByDateTaken
            ? imagePaths.OrderBy(path => ImageProcessing.GetDateTaken(path) ?? new FileInfo(path).CreationTime).ToList()
            : imagePaths.OrderBy(Path.GetFileName).ToList();

        GameStates.Clear();
        PicPaths.Clear();
        _chessNotation.Clear();

        if (sortedPaths.Count == 0)
        {
            Console.WriteLine("No images found in the folder.");
            return;
        }

        // Load the first image and use it as reference
        var firstImage = CvInvoke.Imread(sortedPaths[0]);
        var emptyBoard = ImageProcessing.InitializeChessBoardFromImage(firstImage, 0);
        GameStates.Add(emptyBoard);
        PicPaths.Add(sortedPaths[0]);

        var boardResults = new ChessBoard[sortedPaths.Count - 1];
        var pathResults = new string[sortedPaths.Count - 1];
        
        Parallel.For(1, sortedPaths.Count, i =>
        {
            var image = CvInvoke.Imread(sortedPaths[i]);
            if (image.IsEmpty) return;
            
            var board = ImageProcessing.InitializeChessBoardFromImage(image, i, emptyBoard);
            boardResults[i - 1] = board;
            pathResults[i - 1] = sortedPaths[i];    
        });
        
        GameStates.AddRange(boardResults);
        PicPaths.AddRange(pathResults);

        Console.WriteLine("Pictures count: " + GameStates.Count);
        Console.WriteLine();
    }
    private static FigureType GetPieceTypeForSideRow(int row)
    {
        return row switch
        {
            0 or 7 => FigureType.Rook,   
            1 or 6 => FigureType.Knight, 
            2 or 5 => FigureType.Bishop, 
            3 => FigureType.Queen,     
            4 => FigureType.King,      
            _ => FigureType.Empty // Should never reach here
        };
    }

    private void SetStartingFigureDesignation()
    {
        for (var row = 0; row < 8; row++) 
        {
            for (var col = 0; col < 8; col++) 
            {
                var square = GameStates[1].Squares[row, col];

                switch (col)
                {
                    case 1: // White pawns column second from the left
                        square.FigureType = FigureType.Pawn;
                        square.FigureColor = FigureColor.White;
                        break;

                    case 6: // Black pawns column (second from the right)
                        square.FigureType = FigureType.Pawn;
                        square.FigureColor = FigureColor.Black;
                        break;

                    case 0: // White back-row pieces (leftmost column)
                        square.FigureType = GetPieceTypeForSideRow(row);
                        square.FigureColor = FigureColor.White;
                        break;

                    case 7: // Black back-row pieces (rightmost column)
                        square.FigureType = GetPieceTypeForSideRow(row);
                        square.FigureColor = FigureColor.Black;
                        break;

                    default: // Empty squares for all other columns
                        square.FigureType = FigureType.Empty;
                        square.FigureColor = FigureColor.Empty;
                        break;
                }
            }
        }
    }

    private static bool IsFigureProbablyWhite(Square square)
    {
        return square.SquareIntensity > Constans.WhitePieceIntensityThreshold;
    }

    private static bool IsFigureProbablyBlack(Square square)
    {
        return square.SquareIntensity < Constans.BlackPieceIntensityThreshold;
    }

    private static Dictionary<(FigureType type, FigureColor color), int> GetPiecesDictionary()
    {
        return new Dictionary<(FigureType type, FigureColor color), int>
        {
            // max 1 of each
            { (FigureType.King, FigureColor.White), 1 },
            { (FigureType.King, FigureColor.Black), 1 },
            { (FigureType.Queen, FigureColor.White), 1 },
            { (FigureType.Queen, FigureColor.Black), 1 },

            // max 2 of each
            { (FigureType.Rook, FigureColor.White), 2 },
            { (FigureType.Rook, FigureColor.Black), 2 },
            { (FigureType.Bishop, FigureColor.White), 2 },
            { (FigureType.Bishop, FigureColor.Black), 2 },
            { (FigureType.Knight, FigureColor.White), 2 },
            { (FigureType.Knight, FigureColor.Black), 2 },

            // max 8 of each
            { (FigureType.Pawn, FigureColor.White), 8 },
            { (FigureType.Pawn, FigureColor.Black), 8 },
        };
    }

    private static FigureColor SetPieceColorWithIntensityBounds(Square square, FigureColor predictedColor)
    {
        if (IsFigureProbablyWhite(square))
        {
            square.FigureColor = FigureColor.White;
        }
        else if (IsFigureProbablyBlack(square))
        {
            square.FigureColor = FigureColor.Black;
        }
        else
        {
            square.FigureColor = predictedColor;
        }
        return square.FigureColor;
    }
    private static void AssignEdgeSquaresOnlyMajors(
        Dictionary<(FigureType, FigureColor), int> allowedCounts,
        ChessBoard board,
        string imagePath)
    {
        foreach (var row in Enumerable.Range(0, 8))
        {
            foreach (var col in new[] { 0, 7 })
            {
                var square = board.Squares[row, col];
                if (!square.IsOccupied) continue;
    
                var predictions = PredictTopFivePieces(imagePath, board, (row, col));
                var assigned = false;
    
                foreach (var (predictedType, predictedColor) in predictions)
                {
                    if (predictedType == FigureType.Pawn) continue; 
    
                    var key = (predictedType, predictedColor);
                    if (allowedCounts.TryGetValue(key, out var count) && count > 0)
                    {
                        square.FigureType = predictedType;
                        square.FigureColor = SetPieceColorWithIntensityBounds(square, predictedColor);
                        allowedCounts[key]--;
                        assigned = true;
                        break;
                    }
                }
    
                if (!assigned)
                {
                    square.FigureType = FigureType.Empty;
                    square.FigureColor = FigureColor.Empty;
                    square.IsOccupied = false;
                }
            }
        }
    }
    private void SetFiguresOnBoardImageClassification()
    {
        //TODO fix when promoted there can be more number of major pieces maxum repetition of piece is
        //declared be missing number of pawns from the list
        var allowedCounts = GetPiecesDictionary();
        var board = GetGameStateOnIndex(1);
        var imagePath = GetPicPathOnIndex(1);
    
        // First fill edge squares without pawns
        AssignEdgeSquaresOnlyMajors(allowedCounts, board, imagePath);
    
        // Then fill the rest of the board
        for (var row = 0; row < 8; row++)
        {
            for (var col = 1; col <= 6; col++)
            {
                var square = board.Squares[row, col];
                if (!square.IsOccupied) continue;
    
                var predictions = PredictTopFivePieces(imagePath, board, (row, col));
                var assigned = false;
    
                foreach (var (predictedType, predictedColor) in predictions)
                {
                    if (predictedType == FigureType.Empty || predictedColor == FigureColor.Empty)
                        continue;
    
                    var key = (predictedType, predictedColor);
                    if (allowedCounts.TryGetValue(key, out var count) && count > 0)
                    {
                        square.FigureType = predictedType;
                        square.FigureColor = SetPieceColorWithIntensityBounds(square, predictedColor);
                        allowedCounts[key]--;
                        assigned = true;
                        break;
                    }
                }
    
                if (!assigned)
                {
                    square.FigureType = FigureType.Empty;
                    square.FigureColor = FigureColor.Empty;
                    square.IsOccupied = false;
                }
            }
        }
    }
    
    private void SetOnGoingFigureDesignation()
    {
        SetFiguresOnBoardImageClassification();
    }

    public void DeterminateStartingFiguresDesignation(bool isFromBeginning = true)
    {
        if (isFromBeginning)
        {
            SetStartingFigureDesignation();
        }
        else
        {
            SetOnGoingFigureDesignation();
        }
    }
    
    public bool IsGameFromBeginning()
    {
        var firstValidBoard = GameStates[1]; // first board empty
        
        for (var row = 0; row < 8; row++)
        {
            if (!firstValidBoard.Squares[row, 0].IsOccupied ||
                !firstValidBoard.Squares[row, 1].IsOccupied ||
                !firstValidBoard.Squares[row, 6].IsOccupied ||
                !firstValidBoard.Squares[row, 7].IsOccupied)
            {
                return false; 
            }
        }
        return true;
    }
    
    private static bool IsAbleToCapture((FigureType type, FigureColor color, (int row, int col) position) chaser, (int row, int col) attemptedPosition)
    {
        var chaserType = chaser.type;
        var chaserPosition = chaser.position;

        // Select a movement pattern based on the chaser's figure type
        var movementPattern = chaserType switch
        {
            FigureType.Pawn =>
                MovementPatterns.GetPawnMovementPattern(chaser.color, chaserPosition).Captures,
            FigureType.Rook =>
                MovementPatterns.GetRookMovementPattern(chaserPosition),
            FigureType.Knight =>
                MovementPatterns.GetKnightMovementPattern(chaserPosition),
            FigureType.Bishop =>
                MovementPatterns.GetBishopMovementPattern(chaserPosition),
            FigureType.Queen =>
                MovementPatterns.GetQueenMovementPattern(chaserPosition),
            FigureType.King =>
                MovementPatterns.GetKingMovementPattern(chaserPosition),
            _ => throw new ArgumentException($"Invalid figure type")
        };
        
        // Check if the attempted position is within the calculated movement pattern
        return movementPattern.Any(move => (move.row, move.col) == attemptedPosition);
    }

    private static bool IsAbleToCaptureKnightJump((int row, int col) kingPosition, FigureColor kingColor, ChessBoard board)
    {
        var potentialKnightPositions = MovementPatterns.GetKnightMovementPattern(kingPosition);

        // Iterate over each potential position to check for enemy knights
        foreach (var position in potentialKnightPositions)
        {
            var piece = board.Squares[position.row, position.col];
            if (piece.FigureType == FigureType.Knight && piece.FigureColor != kingColor)
            {
                return true;  // An enemy knight is in position to capture the king
            }
        }

        return false;  // No enemy knights are threatening the king
    }


    private static bool IsCheck(ChessBoard board, FigureColor movedPieceColor) 
    {
        var opponentsKingColor = movedPieceColor == FigureColor.White ? FigureColor.Black : FigureColor.White;
        var kingPosition = FindKingPosition(board, opponentsKingColor); 
        var figuresInKingsSight = ObtainFiguresInKingsSight(board, kingPosition); 
        foreach (var figure in figuresInKingsSight)
        {
            if (IsAbleToCapture((figure.Item1, figure.Item2, figure.Item3), kingPosition)
                || IsAbleToCaptureKnightJump(kingPosition, opponentsKingColor, board))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsPromotion(FigureType type, (int row, int col) position)
    {
        return type == FigureType.Pawn && position.col is 0 or 7;
    }
    
    //this function takes list of all figures in kings sight. 
    private static List<(FigureType type, FigureColor color, (int row, int col))> FilterOutObstructedFigures(
        (int row, int col) kingsPosition,
        List<(FigureType type, FigureColor color, (int row, int col) position)> figuresInKingsSight)
    {
        var filteredFigures = new List<(FigureType, FigureColor, (int, int))>();

        var directions = new List<(int rowOffset, int colOffset)>
        {
            (0, -1),   // Left
            (-1, -1),  // Left-Top Diagonal
            (-1, 0),   // Top
            (-1, 1),   // Right-Top Diagonal
            (0, 1),    // Right
            (1, 1),    // Right-Bottom Diagonal
            (1, 0),    // Bottom
            (1, -1)    // Left-Bottom Diagonal
        };

        // Iterate through each direction and find the first figure in that direction
        foreach (var direction in directions)
        {
            var currentPosition = kingsPosition;
            while (true)
            {
                currentPosition = (currentPosition.row + direction.rowOffset, currentPosition.col + direction.colOffset);

                // Check if this position contains a piece in the king's sight
                var figure = figuresInKingsSight.FirstOrDefault(f => f.position == currentPosition);
                if (figure != default)
                {
                    filteredFigures.Add(figure);
                    break; // Stop searching in this direction
                }

                // out of bounds
                if (!MovementPatterns.IsWithinBounds(currentPosition.row, currentPosition.col))
                    break;
            }
        }

        return filteredFigures;
    }


    private static List<(FigureType, FigureColor, (int, int))> ObtainFiguresInKingsSight(
        ChessBoard board,
        (int, int) kingsPosition)
    {
        var figuresInKingsSight = new List<(FigureType, FigureColor, (int, int))>();
        var kingsColor = board.Squares[kingsPosition.Item1, kingsPosition.Item2].FigureColor;
        
        var kingsLineOfSight = MovementPatterns.GetQueenMovementPattern(kingsPosition);
        foreach (var squarePosition in kingsLineOfSight) 
        {
            var square = board.Squares[squarePosition.row, squarePosition.col]; 
            if (square.IsOccupied)
            {
                figuresInKingsSight.Add((square.FigureType, square.FigureColor, squarePosition));
            }
        }
        
        return FilterOutObstructedFigures(kingsPosition, figuresInKingsSight).Where(f => f.color != kingsColor).ToList();
    }
    

    private static (int row, int col) FindKingPosition(ChessBoard board, FigureColor kingColor)
    {
        foreach (var square in board.Squares)
        {
            if (square.FigureType == FigureType.King)
            {
                switch (kingColor == FigureColor.White)
                {
                    case true when square.FigureColor == FigureColor.White:
                        return (square.Row, square.Col);
                    case false when square.FigureColor == FigureColor.Black:
                        return (square.Row, square.Col);
                }
            }
        }

        ConsoleDraw.DrawBoard(board);
        throw new InvalidOperationException("King not found on board"); 
    }

    private static void SetMovedPiece(ChessBoard board, FigureType type,  FigureColor color, (int, int) position)
    {
        board.Squares[position.Item1, position.Item2].FigureColor = color;
        board.Squares[position.Item1, position.Item2].FigureType = type;
    }

    private (FigureType, FigureColor) FindCapturingFigure(ChessBoard previousBoard, ChessBoard currentBoard)
    {
        for (var row = 0; row < 8; row++)
        {
            for (var col = 0; col < 8; col++)
            {
                Square previousSquare = previousBoard.Squares[row, col];
                Square currentSquare = currentBoard.Squares[row, col];   
            
                if (previousSquare.IsOccupied && !currentSquare.IsOccupied)
                {
                    return (previousSquare.FigureType, previousSquare.FigureColor);
                }
            }
        }
        
        throw new Exception("Capture figure not found");
    }
    
    
    private static (int row, int col) FindPositionOfCapture(
        ChessBoard previousBoard,
        ChessBoard currentBoard, 
        (int, int) unavailableSquarePosition)
    {
        for (var row = 0; row < 8; row++)
        {
            for (var col = 0; col < 8; col++)
            {
                //skips unwanted squares 
                if (row == unavailableSquarePosition.Item1 && col == unavailableSquarePosition.Item2)
                {
                    continue;
                }

                var previousSquare = previousBoard.Squares[row, col];
                var currentSquare = currentBoard.Squares[row, col];
                var squareIntensityDifference = Math.Abs(previousSquare.SquareIntensity
                                                         - currentSquare.SquareIntensity);
                if (squareIntensityDifference > Constans.PieceIntensityThreshold)
                {
                    return (currentSquare.Row, currentSquare.Col);
                }

            }
        }

        throw new Exception("Position of Capture not found");
    }

    private static bool ResolveCastle(List<(int row, int col)> reappearedPiecesPositions, ChessBoard board)
    {
        var figureColor = reappearedPiecesPositions[0].col == 0 ? FigureColor.White : FigureColor.Black;
        var kingPosition = Math.Abs(reappearedPiecesPositions[0].row - 0) < // top corner
                           Math.Abs(reappearedPiecesPositions[1].row - 7) //bottom corner
            ? reappearedPiecesPositions[0]
            : reappearedPiecesPositions[1];
        
        if (reappearedPiecesPositions[0].row == kingPosition.row)
        {
            SetMovedPiece(board, FigureType.King, figureColor, reappearedPiecesPositions[0]);
            SetMovedPiece(board, FigureType.Rook, figureColor, reappearedPiecesPositions[1]);
        }
        else
        {
            SetMovedPiece(board, FigureType.Rook, figureColor, reappearedPiecesPositions[0]);
            SetMovedPiece(board, FigureType.King, figureColor, reappearedPiecesPositions[1]);
        }

        return kingPosition.row > 4; //Marking true as king side for chess notation 
    }
    
    
    private static (FigureType type, FigureColor color, int row) FindCapturingFigureAndRowOfStartingPosition(
        List<(FigureType type, FigureColor color, (int row, int col) position)> disappearedPieces,
        (int row, int col) newPosition)
    {
        var color = FigureColor.Empty;
        var row = -1;
        for (var i = 0; i < disappearedPieces.Count; i++)
        {
            if (newPosition.row == disappearedPieces[i].position.row) 
            {
                color = disappearedPieces[i].color == FigureColor.White ? FigureColor.Black : FigureColor.White;
            }
            else
            {
                row = disappearedPieces[i].position.row;
            }
        }

        return (FigureType.Pawn, color, row);
    }
    
    // private void UpdatePromotedRecord((int row, int col) disappearedPiecesPosition, 
    //     (int row, int col) reappearedPiecesPositions, bool isCaptured)
    // {
    //     for (var i = 0; i < Promoted.Count; i++)
    //     {
    //         var record = Promoted[i];
    //         var lastPosition = record.position.Last();
    //         
    //         if (lastPosition == disappearedPiecesPosition)
    //         {
    //             record.position.Add(reappearedPiecesPositions);
    //             var updatedRecord = (record.position, isCaptured);
    //             Promoted[i] = updatedRecord; 
    //         }
    //     }
    // }

    private static void ResolveCapture((FigureType type, FigureColor color) capturingFigure,
        (int row, int col) capturedPosition,
        ChessBoard currentBoard)
    {
        currentBoard.Squares[capturedPosition.row, capturedPosition.col].FigureType = capturingFigure.type;
        currentBoard.Squares[capturedPosition.row, capturedPosition.col].FigureColor = capturingFigure.color;
    }

    private static bool CountCheck(
        List<(FigureType type, FigureColor color, (int row, int col) position)> disappearedPieces,
        List<(int row, int col)> reappearedPiecesPositions)
    {
        var dis = disappearedPieces.Count;
        var reap = reappearedPiecesPositions.Count;

        var isValid =
            (dis == 1 && reap == 1) ||  // normal move
            (dis == 1 && reap == 0) ||  // capture
            (dis == 2 && reap == 1) ||  // en passant
            (dis == 2 && reap == 2);    // castle

        return isValid;
    }

    private static bool IsValidMove((FigureType type, FigureColor color, (int row, int col) from) figure, 
        (int row, int col) endPosition,
        ChessBoard board)
    {
        if (!IsAbleToCapture(figure, endPosition)) return false;
        
        var isSlidingPiece = figure.type is FigureType.Rook or FigureType.Bishop or FigureType.Queen;
        return !isSlidingPiece || !IsObstructed(figure.from, endPosition, board);
    }
    
    private static bool IsObstructed((int row, int col) from, (int row, int col) to, ChessBoard board)
    {
        int dRow = Math.Sign(to.row - from.row);
        int dCol = Math.Sign(to.col - from.col);

        int currentRow = from.row + dRow;
        int currentCol = from.col + dCol;

        while ((currentRow, currentCol) != to)
        {
            if (board.Squares[currentRow, currentCol].IsOccupied)
                return true;

            currentRow += dRow;
            currentCol += dCol;
        }

        return false;
    }

    private static void SetInvalidPositionsOccupancy(List<(int row, int col)> validPositions,
        List<(int row, int col)> allPositions, ChessBoard board)
    {
        var validSet = new HashSet<(int row, int col)>(validPositions);

        foreach (var position in allPositions.Where(position => !validSet.Contains(position)))
        {
            board.Squares[position.row, position.col].IsOccupied = false;
        }
    }

    private static bool TryRepairMoveOrCapture(
        List<(FigureType type, FigureColor color, (int row, int col) position)> disappeared,
        List<(int row, int col)> reappeared,
        ChessBoard board)
    {
        var reachablePositions = reappeared.Where(reappearedPosition => 
            IsValidMove(disappeared.First(), reappearedPosition, board)).ToList();
        if (reachablePositions.Count != 1) return false;
        SetInvalidPositionsOccupancy(reachablePositions, reappeared, board);
        return true;
    }

    private static void RepairCastle(List<(int row, int col)> reappeared, ChessBoard board, FigureColor color)
    {
        List<int> validRows = [2, 3, 5, 6];
        var validCol = FigureColor.White == color ? 0 : 7;
        var validPositions = validRows.Select(row => (row, validCol)).ToList();
        SetInvalidPositionsOccupancy(validPositions, reappeared, board);
    }

    private static void RepairEnPassant(
        List<(FigureType type, FigureColor color, (int row, int col) position)> disappeared,
        List<(int row, int col)> reappeared,
        ChessBoard board)
    {
        var validPositions = MovementPatterns.GetPawnMovementPattern(disappeared.First().color,
            disappeared.First().position).Captures;
        validPositions.AddRange(MovementPatterns.GetPawnMovementPattern(disappeared.Last().color, 
            disappeared.Last().position).Captures);
        SetInvalidPositionsOccupancy(validPositions, reappeared, board);
        
    }

    private static bool TryRepairEnPassantOrCastle(
        List<(FigureType type, FigureColor color, (int row, int col) position)> disappeared,
        List<(int row, int col)> reappeared,
        ChessBoard board)
    {
        var isKingAndRook = 
            disappeared.Count == 2 &&
            disappeared.Select(d => d.type).ToHashSet().SetEquals([FigureType.King, FigureType.Rook]) &&
            disappeared.Select(d => d.color).Distinct().Count() == 1;
        
        var isPawns =
            disappeared.Count == 2 &&
            disappeared.All(d => d.type == FigureType.Pawn) &&
            disappeared.Select(d => d.color).Distinct().Count() == 2;
        
        if (!isKingAndRook || !isPawns) return false;
        if (isKingAndRook)
        {
            RepairCastle(reappeared, board, disappeared.First().color);
        }
        
        if (isPawns)
        {
            RepairEnPassant(disappeared, reappeared, board);
        }
        
        return true;
    }

    private static void RemakeInconsistentOccupancy(
        List<(int row, int col)> reappearedPositions,
        ChessBoard currentBoard,
        ChessBoard nextBoard)
    {
        var validPositions = reappearedPositions
            .Where(position => nextBoard.Squares[position.row, position.col].IsOccupied)
            .ToList();
        
        SetInvalidPositionsOccupancy(validPositions, reappearedPositions, currentBoard);
    }

    private static void TryRepairState(
        ChessBoard currentBoard,
        ChessBoard nextBoard,
        List<(FigureType type, FigureColor color, (int row, int col) position)> disappeared,
        List<(int row, int col)> reappeared,
        bool hasNextState)
    {
        var wasRepaired = false;   
        // First will check if the new appeared position is valid, If only one is valid, the other positions will be set
        // to not occupied.
        var dis = disappeared.Count;
        var reap = reappeared.Count;
        switch (dis)
        {
            case 1 when reap > 1:
                wasRepaired = TryRepairMoveOrCapture(disappeared, reappeared, currentBoard);
                break;
            case 2 when reap > 2:
                wasRepaired = TryRepairEnPassantOrCastle(disappeared, reappeared, currentBoard);
                break;
        }

        //then look it the future of the piece disappeared it's not the one we want. 
        if (!wasRepaired && hasNextState)
        {
            RemakeInconsistentOccupancy(reappeared, currentBoard, nextBoard);
        }

    }

    private static FigureType ReplacePawnPredictionWithBishop(FigureType type)
    {
        return type == FigureType.Pawn ? FigureType.Bishop : type;
    }

    private static List<(FigureType type, FigureColor color)> PredictTopFivePieces( 
        string imagePath,
        ChessBoard board,
        (int row, int col) position)
    {
        var image = CvInvoke.Imread(imagePath);
        var corners = board.Squares[position.row, position.col].Corners;
        return ImageProcessing.PredictTopFiveFigures(image, corners);
    }

    private static (FigureType type, FigureColor color) PredictPiece(
        string imagePath,
        ChessBoard board,
        (int row, int col) position)
    {
        var image = CvInvoke.Imread(imagePath);
        var corners = board.Squares[position.row, position.col].Corners;
        return ImageProcessing.PredictFigure(image, corners);
    }

    private static (
        List<(FigureType type, FigureColor color, (int row, int col) position)>,
        List<(int row, int col)> reappearedPiecesPositions) 
        DeterminateDisappearedAndReappearedPieces(ChessBoard previousBoard, ChessBoard currentBoard, bool onlyGenerateNotation)
    {
        List<(FigureType type, FigureColor color, (int row, int col) position)> disappearedPieces = [];
        List<(int row, int col)> reappearedPiecesPositions = [];
        
        for (var row = 0; row < 8; row++)
        {
            for (var col = 0; col < 8; col++)
            {
                var previousSquare = previousBoard.Squares[row, col];
                var currentSquare = currentBoard.Squares[row, col];

                if (previousSquare.IsOccupied && !currentSquare.IsOccupied)
                {
                    disappearedPieces.Add((previousSquare.FigureType, previousSquare.FigureColor, (row, col)));
                }
                else if (!previousSquare.IsOccupied && currentSquare.IsOccupied)
                {
                    reappearedPiecesPositions.Add((row, col));
                }
                else if (previousSquare.IsOccupied && currentSquare.IsOccupied && !onlyGenerateNotation)
                {
                    currentSquare.FigureType = previousSquare.FigureType;
                    currentSquare.FigureColor = previousSquare.FigureColor;
                }
            }
        }

        return (disappearedPieces, reappearedPiecesPositions);
    }
    
    private void TryRepair(ChessBoard currentBoard, int index, List<(FigureType, FigureColor, (int row, int col))> disappeared, List<(int row, int col)> reappeared, bool onlyGenerate)
    {
        if (index + 1 < GameStates.Count)
            TryRepairState(currentBoard, GameStates[index + 1], disappeared, reappeared, true);
        else
            TryRepairState(currentBoard, currentBoard, disappeared, reappeared, false);
    }

    private void HandleStandardMove((FigureType type, FigureColor color, (int row, int col) position) moved, (int row, int col) to, ChessBoard board, int index, bool onlyGenerate)
    {
        var isPromotion = IsPromotion(moved.type, to);

        if (!onlyGenerate)
        {
            if (isPromotion)
            {
                moved.type = ReplacePawnPredictionWithBishop(
                    PredictPiece(GetPicPathOnIndex(index), board, to).type);
            }

            SetMovedPiece(board, moved.type, moved.color, to);
        }
        else
        {
            if (isPromotion && index + 1 < GameStates.Count)
            {
                var nextBoard = GetGameStateOnIndex(index + 1);
                moved.type = nextBoard.Squares[to.row, to.col].FigureType;
            }

            _chessNotation.AddRecordToNotation(moved.type, moved.color, to, false, IsCheck(board, moved.color), isPromotion, moved.position.row);
        }
    }

    private void HandleClassicCapture((FigureType type, FigureColor color, (int row, int col) position) moved, ChessBoard prev, ChessBoard curr, int index, bool onlyGenerate)
    {
        var capturePos = FindPositionOfCapture(prev, curr, moved.position);
        var isPromotion = IsPromotion(moved.type, capturePos);

        if (!onlyGenerate)
        {
            if (isPromotion)
            {
                moved.type = ReplacePawnPredictionWithBishop(
                    PredictPiece(GetPicPathOnIndex(index), curr, capturePos).type);
            }

            ResolveCapture((moved.type, moved.color), capturePos, curr);
        }
        else
        {
            if (isPromotion && index + 1 < GameStates.Count)
            {
                var nextBoard = GetGameStateOnIndex(index + 1);
                moved.type = nextBoard.Squares[capturePos.row, capturePos.col].FigureType;
            }

            _chessNotation.AddRecordToNotation(moved.type, moved.color, capturePos, true, IsCheck(curr, moved.color), isPromotion, moved.position.row);
        }
    }

    private void HandleEnPassant(List<(FigureType type, FigureColor color, (int row, int col) position)> disappeared, (int row, int col) to, ChessBoard board, bool onlyGenerate)
    {
        var capture = FindCapturingFigureAndRowOfStartingPosition(disappeared, to);

        if (!onlyGenerate)
        {
            ResolveCapture((capture.type, capture.color), to, board);
        }
        else
        {
            _chessNotation.AddRecordToNotation(capture.row, to, IsCheck(board, capture.color));
        }
    }

    private void HandleCastle(List<(int row, int col)> reappeared, FigureColor color, ChessBoard board, bool onlyGenerate)
    {
        var isKingSide = ResolveCastle(reappeared, board);

        if (onlyGenerate)
        {
            _chessNotation.AddRecordToNotation(isKingSide, IsCheck(board, color));
        }
    }
    
    private void UpdateBoardState(ChessBoard previousBoard, ChessBoard currentBoard, int currentBoardIndex, bool onlyGenerateNotation)
    {
        var (disappearedPieces, reappearedPieces) =
            DeterminateDisappearedAndReappearedPieces(previousBoard, currentBoard, onlyGenerateNotation);

        if (!CountCheck(disappearedPieces, reappearedPieces))
        {
            TryRepair(currentBoard, currentBoardIndex, disappearedPieces, reappearedPieces, onlyGenerateNotation);
            (disappearedPieces, reappearedPieces) =
                DeterminateDisappearedAndReappearedPieces(previousBoard, currentBoard, onlyGenerateNotation);
            if (!CountCheck(disappearedPieces, reappearedPieces))
            {
                throw new InvalidOperationException("Self repair failed");
            }
        }

        switch (disappearedPieces.Count, reappearedPieces.Count)
        {
            case (1, 1):
                HandleStandardMove(disappearedPieces[0], reappearedPieces[0], currentBoard, currentBoardIndex, onlyGenerateNotation);
                break;
            case (1, 0):
                HandleClassicCapture(disappearedPieces[0], previousBoard, currentBoard, currentBoardIndex, onlyGenerateNotation);
                break;
            case (2, 1):
                HandleEnPassant(disappearedPieces, reappearedPieces[0], currentBoard, onlyGenerateNotation);
                break;
            case (2, 2):
                HandleCastle(reappearedPieces, disappearedPieces[0].color, currentBoard, onlyGenerateNotation);
                break;
        }
    }

    
    public void UpdateGameStates(bool onlyGenerateNotation = false)
    {
        if (GameStates.Count < 2)
        {
            Console.WriteLine("Not enough boards to track movement.");
            return;
        }
        
        // if (!onlyGenerateNotation) ConsoleDraw.DrawBoard(GameStates[1]); //delete it later
        // Start from the second board and compare with the previous one
        for (int i = 2; i < GameStates.Count; i++) //first board empty
        {
            ChessBoard previousBoard = GameStates[i - 1];
            ChessBoard currentBoard = GameStates[i];

            UpdateBoardState(previousBoard, currentBoard, i, onlyGenerateNotation);
            // if (!onlyGenerateNotation) ConsoleDraw.DrawBoard(currentBoard); //delete it later
        }
    }

    public void GenerateChessNotation()
    {
        UpdateGameStates(true);
        _chessNotation.GenerateChessNotation();
    }

    public string ReturnChessNotationAsString()
    {
        UpdateGameStates(true);
        return _chessNotation.GetNotationAsString();
    }


    //expandable
    private void WriteShortPaths()
    {
        var index = 0;
        foreach (var path in GetPicPaths())
        {
            Console.WriteLine($"Path on index: {index} Path: {Path.GetFileName(path)}");
            index++;
        }
    }
}