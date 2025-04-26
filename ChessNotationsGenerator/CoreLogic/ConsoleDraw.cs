using System.Text;
using ChessNotationsGenerator.CoreLogic;

namespace Bakalarka;

public static class ConsoleDraw
{
    public static void DrawBoard(ChessBoard board)
    {
        var sb = new StringBuilder();
        var letter = 'a';
        var lineNumber = 1;
        sb.Append("  ");
        for (int i = 0; i < 8; i++)
        {
            sb.Append(lineNumber + " ");
            lineNumber++;
        }
        sb.Append(Environment.NewLine);

        for (int i = 0; i < 8; i++)
        {
            sb.Append(letter);
            for (int j = 0; j < 8; j++)
            {
                var square = board.Squares[i, j];
                if (!square.IsOccupied)
                {
                    sb.Append("|" + "." );
                }
                else
                {
                    var piece = (char)square.FigureType;
                    if (square.FigureColor == FigureColor.White)
                    {
                        piece = Char.ToUpper(piece);
                    }
                    sb.Append("|" + piece);
                }
            }
            sb.Append("|" + letter + Environment.NewLine);
            letter++;
        }
        
        lineNumber = 1;
        sb.Append("  ");
        for (int i = 0; i < 8; i++)
        {
            sb.Append(lineNumber + " ");
            lineNumber++;
        }
        
        Console.WriteLine(sb.ToString());
        Console.WriteLine();
        Console.WriteLine();
    }
}