namespace ChessNotationsGenerator.CoreLogic;

public static class MovementPatterns
{
    private static void AddMoveIfWithinBounds(List<(int row, int col)> move, (int row, int col) start, int rowOffset, int colOffset)
    {
        var (newRow, newCol) = (start.row + rowOffset, start.col + colOffset);

        // Check if the new position is within the chessboard's boundaries
        if (IsWithinBounds(newRow, newCol))
        {
            move.Add((newRow, newCol));
        }
    }

    public static (List<(int row, int col)> Moves, List<(int row, int col)> Captures) GetPawnMovementPattern(FigureColor color, (int row, int col) start)
    {
        var moves = new List<(int row, int col)>();
        var captures = new List<(int row, int col)>();
        var direction = color == FigureColor.White ? 1 : -1;

        // Normal forward move       
        AddMoveIfWithinBounds(moves, start, direction, 0);

        // Capture moves
        AddMoveIfWithinBounds(captures, start, direction, 1);
        AddMoveIfWithinBounds(captures, start, direction, -1);

        return (moves, captures);
    }

    public static List<(int row, int col)> GetRookMovementPattern((int row, int col) start)
    {
        var moves = new List<(int row, int col)>();
        for (var i = 1; i <= 7; i++)
        {
            AddMoveIfWithinBounds(moves, start, i, 0);
            AddMoveIfWithinBounds(moves, start, -i, 0);
            AddMoveIfWithinBounds(moves, start, 0, i);
            AddMoveIfWithinBounds(moves, start, 0, -i);
        }
        return moves;
    }

    public static List<(int row, int col)> GetBishopMovementPattern((int row, int col) start)
    {
        var moves = new List<(int row, int col)>();
        for (var i = 1; i <= 7; i++)
        {
            AddMoveIfWithinBounds(moves, start, i, i);
            AddMoveIfWithinBounds(moves, start, -i, -i);
            AddMoveIfWithinBounds(moves, start, i, -i);
            AddMoveIfWithinBounds(moves, start, -i, i);
        }
        
        return moves;
    }

    public static List<(int row, int col)> GetQueenMovementPattern((int row, int col) start)
    {
        var moves = new List<(int row, int col)>();
        moves.AddRange(GetRookMovementPattern(start));
        moves.AddRange(GetBishopMovementPattern(start));
        return moves;
    }

    public static List<(int row, int col)> GetKnightMovementPattern((int row, int col) start)
    {
        var moves = new List<(int row, int col)> {
            (start.row + 2, start.col + 1), (start.row + 1, start.col + 2), (start.row - 1, start.col + 2), (start.row - 2, start.col + 1),
            (start.row - 2, start.col - 1), (start.row - 1, start.col - 2), (start.row + 1, start.col - 2), (start.row + 2, start.col - 1)
        };
        return moves.Where(move => IsWithinBounds(move.row, move.col)).ToList();
    }

    public static List<(int row, int col)> GetKingMovementPattern((int row, int col) start)
    {
        var moves = new List<(int row, int col)> {
            (start.row + 1, start.col), (start.row + 1, start.col + 1), (start.row, start.col + 1), (start.row - 1, start.col + 1),
            (start.row - 1, start.col), (start.row - 1, start.col - 1), (start.row, start.col - 1), (start.row + 1, start.col - 1)
        };
        return moves.Where(move => IsWithinBounds(move.row, move.col)).ToList();
    }

    public static bool IsWithinBounds(int row, int col)
    {
        return row is >= 0 and < 8 && col is >= 0 and < 8;
    }
    
}