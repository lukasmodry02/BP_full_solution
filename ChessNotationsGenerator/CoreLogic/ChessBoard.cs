namespace ChessNotationsGenerator.CoreLogic;

public class ChessBoard
{
    public Square[,] Squares { get; private set; } = new Square[8, 8];

    public ChessBoard(List<Square> squares) 
    {
        foreach (var square in squares)
        {
            Squares[square.Row, square.Col] = square;
        }
    }

    public ChessBoard DeepClone()
    {
        var clonedSquares = new List<Square>();

        for (var row = 0; row < 8; row++)
        {
            for (var col = 0; col < 8; col++)
            {
                clonedSquares.Add(Squares[row, col].DeepClone());
            }
        }

        return new ChessBoard(clonedSquares);
    }




}