using System.Drawing;
using ChessNotationsGenerator.CoreLogic;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Bakalarka;

public static class ChessboardTrainingDataExporter
{
    private static int _callCounter = 0;
    
    
    private static int GetCallCounter()
    {
        return _callCounter;
    }

    private static void IncrementCallCounter()
    {
        _callCounter++;
    }

    private static (int row, int col) GetRandomPosition(int maxRow, int maxCol)
    {
        var random = new Random();
        var row = random.Next(0, maxRow);
        var col = random.Next(0, maxCol);
        return (row, col);
    }

    public static void ExtractAndSaveFigureRoi(ChessBoard chessBoard, Mat inputImage, string savePath)
    {
        IncrementCallCounter();
        // var position = DetermineFigurePosition(chessBoard);
        var position = GetRandomPosition(8, 8);
        
        // Mat roi = ExtractCircularRoi(chessBoard, inputImage, position);
        Mat roi = ExtractSquareRoi(chessBoard, inputImage, position);
        
        if (roi.IsEmpty)
        {
            Console.WriteLine("Failed to extract ROI.");
            return;
        }

        SaveRoiToFile(roi, savePath, position, GetCallCounter());
    }

    private static (int row, int col) DetermineFigurePosition(ChessBoard chessBoard)
    {
        foreach (var square in chessBoard.Squares)
        {
            if (square.IsOccupied)
            {
                return (square.Row, square.Col);
            }
        }

        return (0, 0);
    }

    private static Mat ExtractCircularRoi(ChessBoard chessBoard, Mat inputImage, (int row, int col) position)
    {
        var square = chessBoard.Squares[position.row, position.col];
        PointF[] corners = square.Corners;
        
        // Compute square bounding box
        int x = (int)Math.Min(corners[0].X, corners[2].X);
        int y = (int)Math.Min(corners[0].Y, corners[2].Y);
        int width = (int)Math.Abs(corners[0].X - corners[2].X);
        int height = (int)Math.Abs(corners[0].Y - corners[2].Y);

        // Ensure square dimensions are within bounds
        Rectangle roiRect = new Rectangle(x, y, width, height);
        Mat squareRoi = new Mat(inputImage, roiRect);

        // Create a black canvas of the same size as the square
        Mat finalImage = new Mat(new Size(width, height), Emgu.CV.CvEnum.DepthType.Cv8U, 3);
        finalImage.SetTo(new MCvScalar(0, 0, 0)); // Black background

        // Calculate center and radius for circular mask within the square
        int centerX = width / 2;
        int centerY = height / 2;
        int radius = Math.Min(width, height) / 3;

        // Create a circular mask of the same size as the square
        Mat mask = new Mat(new Size(width, height), Emgu.CV.CvEnum.DepthType.Cv8U, 1);
        mask.SetTo(new MCvScalar(0));
        CvInvoke.Circle(mask, new Point(centerX, centerY), radius, new MCvScalar(255), -1);

        // Apply the mask to extract only the circular region from the square
        Mat circularRoi = new Mat();
        CvInvoke.BitwiseAnd(squareRoi, squareRoi, circularRoi, mask);

        // Overlay the circular ROI onto the black square background
        circularRoi.CopyTo(finalImage, mask);

        return finalImage;
    }

    
    private static Mat ExtractSquareRoi(ChessBoard chessBoard, Mat inputImage, (int row, int col) position)
    {
        var square = chessBoard.Squares[position.row, position.col];
        PointF[] corners = square.Corners;

        if (corners.Length != 4)
        {
            Console.WriteLine("Invalid square corners.");
            return new Mat();
        }

        // Compute bounding box for the square
        int x = (int)Math.Min(corners[0].X, corners[2].X);
        int y = (int)Math.Min(corners[0].Y, corners[2].Y);
        int width = (int)Math.Abs(corners[0].X - corners[2].X);
        int height = (int)Math.Abs(corners[0].Y - corners[2].Y);

        // Ensure extracted region is within bounds
        Rectangle roiRect = new Rectangle(x, y, width, height);
        Mat squareRoi = new Mat(inputImage, roiRect);

        // Create a black background of the same size as the square
        Mat finalImage = new Mat(new Size(width, height), Emgu.CV.CvEnum.DepthType.Cv8U, 3);
        finalImage.SetTo(new MCvScalar(0, 0, 0)); // Black background

        // Create white mask for the square
        Mat mask = new Mat(new Size(width, height), Emgu.CV.CvEnum.DepthType.Cv8U, 1);
        mask.SetTo(new MCvScalar(255)); // Full visibility

        // Copy the extracted square onto the black background
        squareRoi.CopyTo(finalImage, mask);

        return finalImage;
    }



    private static void SaveRoiToFile(Mat roi, string savePath, (int row, int col) position, int index)
    {
        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(savePath);

            // Generate unique filename based on position
            string fileName = $"roi_{position.row}_{position.col}_{index}.png";
            string fullPath = Path.Combine(savePath, fileName);

            // Save image
            CvInvoke.Imwrite(fullPath, roi);
            Console.WriteLine($"ROI saved: {fullPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving ROI: {ex.Message}");
        }
    }
}