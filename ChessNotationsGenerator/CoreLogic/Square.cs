using System.Drawing;

namespace ChessNotationsGenerator.CoreLogic;

public class Square(int row, int col, double intensity, PointF[] corners, SquareColor squareColor) 
{
    //init == will be only shuffle with on initialization
    public int Row { get; init; } = row; //position on the board
    public int Col { get; init; } = col; //position on the board
    public PointF[] Corners { get; init; } = corners; //position in the real picture.  
    public FigureType FigureType { get; set; } = FigureType.Empty;
    public FigureColor FigureColor { get; set; } = FigureColor.Empty;
    public SquareColor SquareColor { get; init; } = squareColor;
    public double SquareIntensity { get; init; } = intensity;
    public bool IsOccupied { get; set; } //false by default isOccupied
    
    public Square DeepClone()
    {
        var square = new Square(
            Row,
            Col,
            SquareIntensity,
            Corners.ToArray(),
            SquareColor
        )
        {
            IsOccupied = IsOccupied,
            FigureType = FigureType,
            FigureColor = FigureColor,
        };
        return square;
    }

}
