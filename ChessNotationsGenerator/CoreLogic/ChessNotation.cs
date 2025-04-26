using System.Text;

namespace ChessNotationsGenerator.CoreLogic;


public class ChessNotation
{
    private readonly List<string> _moves = []; 
    private static readonly char[] ColTransformArr = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h']; 

    public void AddRecordToNotation(FigureType type, FigureColor color, (int, int) positionOfCapture,
        bool isCapture, bool isCheck, bool isPromotion, int startRow)
    {
        _moves.Add(RefactorMovedPieceAndClassicCaptureToString(
            type, color, positionOfCapture, isCapture, isCheck, isPromotion, startRow));
    }

    public void AddRecordToNotation(int startingRow, (int, int) finishingPosition, bool isCheck)
    {
            _moves.Add(RefactorEnPassantMovedToString(startingRow, finishingPosition, isCheck));
    }

    public void AddRecordToNotation(bool isKingSide, bool isCheck)
    {
        _moves.Add(RefactorCastleMoveToString(isKingSide, isCheck));
    }

    private static void AddCapturedMarkIfCapturedToSb(StringBuilder sb, bool isCapture)
    {
        if (isCapture) sb.Append('x');
    }

    private static void AddCheckMarkIfInCheck(StringBuilder sb, bool isCheck)
    {
        if (isCheck) sb.Append('+');
    }

    //unicode added
    private static void AddPieceToSb(StringBuilder sb, (FigureType type, FigureColor color) figure)
    {
        if (figure.type == FigureType.Pawn) return;
        sb.Append($"{ToUnicodeSymbol(figure.type, figure.color)}");
    }
    
    // private static void AddPieceToSb(StringBuilder sb, (FigureType, FigureColor) figure)
    // {
    //     var type = figure.Item1;
    //     var color = figure.Item2;
    //     if (type == FigureType.Pawn) return;
    //     var piece = (char)type; 
    //     if (color == FigureColor.White)
    //     {
    //         piece = Char.ToUpper(piece);
    //     }
    //
    //     sb.Append(piece);
    // }


    private static void AddPromotionMarkIfPromotedToSb(StringBuilder sb, bool isPromotion,
        (FigureType promotedType, FigureColor promotedColor) figure)
    {
        if (!isPromotion) return;
        sb.Append($"={ToUnicodeSymbol(figure.promotedType, figure.promotedColor)}");
    }

    // private static void AddPromotionMarkIfPromotedToSb(StringBuilder sb, bool isPromotion,
    // (FigureType promotedType, FigureColor promotedColor) figure)
    // {
    //     if (!isPromotion) return;
    //     var piece = (char)figure.promotedType; 
    //     if (figure.promotedColor == FigureColor.White)
    //     {
    //         piece = Char.ToUpper(piece);
    //     }
    //     
    //     sb.Append($"={piece}");
    // }

    private static void AddPositionToSb(StringBuilder sb, (int, int) position)
    {
        sb.Append($"{ColTransformArr[position.Item1]}{position.Item2 + 1}");
    }
    
    private static void AddStartRowOfPawnMoveIfCapture(StringBuilder sb, FigureType type, int startRow, bool isCapture)
    {
        if (type == FigureType.Pawn && isCapture)
        {
            sb.Append($"{ColTransformArr[startRow]}");
        }
    }

    private static string RefactorCastleMoveToString(bool isKingSide, bool isCheck)
    {
        var castleNotation = isKingSide ? "O-O" : "O-O-O";
        return isCheck ? castleNotation + "+" : castleNotation;
    }

    private static string RefactorEnPassantMovedToString(
        int startRow,
        (int, int) endPosition,
        bool isCheck)
    {
        var sb = new StringBuilder();
        sb.Append($"{ColTransformArr[startRow]}");
        AddCapturedMarkIfCapturedToSb(sb, true);
        AddPositionToSb(sb, endPosition);
        AddCheckMarkIfInCheck(sb, isCheck);
        return sb.ToString();
    }

    private static string RefactorMovedPieceAndClassicCaptureToString(
        FigureType type,
        FigureColor color,
        (int, int) position,
        bool isCapture,
        bool isCheck,
        bool isPromotion,
        int startRow)
    {
        var promotedTo = (type, color);
        if (isPromotion) type = FigureType.Pawn;
        var sb = new StringBuilder();
        AddPieceToSb(sb, (type, color));
        AddStartRowOfPawnMoveIfCapture(sb, type, startRow, isCapture);
        AddCapturedMarkIfCapturedToSb(sb, isCapture);
        AddPositionToSb(sb, position);
        AddPromotionMarkIfPromotedToSb(sb, isPromotion, promotedTo);
        AddCheckMarkIfInCheck(sb, isCheck);
        
        return sb.ToString();
    }
    
    private static string ToUnicodeSymbol(FigureType type, FigureColor color)
    {
        return (type, color) switch
        {
            (FigureType.King, FigureColor.White) => "♔",
            (FigureType.Queen, FigureColor.White) => "♕",
            (FigureType.Rook, FigureColor.White) => "♖",
            (FigureType.Bishop, FigureColor.White) => "♗",
            (FigureType.Knight, FigureColor.White) => "♘",
            (FigureType.Pawn, FigureColor.White) => "♙",

            (FigureType.King, FigureColor.Black) => "♚",
            (FigureType.Queen, FigureColor.Black) => "♛",
            (FigureType.Rook, FigureColor.Black) => "♜",
            (FigureType.Bishop, FigureColor.Black) => "♝",
            (FigureType.Knight, FigureColor.Black) => "♞",
            (FigureType.Pawn, FigureColor.Black) => "♟",

            _ => ""
        };
    }
    
    public void Clear()
    {
        _moves.Clear();
    }

    private static string GenerateWhiteSpace(string move)
    {
        return "        ".Remove(0, move.Length - 2);
    }
    
    public void GenerateChessNotation()
    {
        var index = 1;
        var moves = _moves;
        Console.WriteLine("White  -  Black");
        foreach (var move in moves)
        {
            Console.Write(move + GenerateWhiteSpace(move));
            if (index % 2 == 0)
            {
                Console.WriteLine();
            }
            
            index++;
        }
    }
    
    public string GetNotationAsString()
    {
        var sb = new StringBuilder();
        int index = 1;

        const int indexColumnWidth = 4;
        const int whiteMoveColumnWidth = 18;
        const int blackMoveColumnWidth = 18;

        for (int i = 0; i < _moves.Count; i += 2)
        {
            string white = _moves[i];
            string black = (i + 1 < _moves.Count) ? NormalizeMove(_moves[i + 1]) : "";

            string indexFormatted = $"{index}.".PadLeft(indexColumnWidth);
            string whiteFormatted = white.PadRight(whiteMoveColumnWidth);
            string blackFormatted = black.PadRight(blackMoveColumnWidth);

            sb.AppendLine($"{indexFormatted} {whiteFormatted}{blackFormatted}");
            index++;
        }

        return sb.ToString();
    }
    
    // Pads plain pawn moves so they align better visually
    private static string NormalizeMove(string move)
    {
        if (string.IsNullOrWhiteSpace(move))
            return move;

        char first = move[0];
        // If move starts with a letter from a to h or number (likely a pawn move or castling)
        if (first is >= 'a' and <= 'h' or 'O')
        {
            return " " + move;
        }

        return move;
    }

}