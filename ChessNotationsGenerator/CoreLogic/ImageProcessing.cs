using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace ChessNotationsGenerator.CoreLogic;
public static class ImageProcessing
{
    private static Mat LoadPicFromPath(string filePath)
    {
        var image = CvInvoke.Imread(filePath);
        return image;
    }
    
    internal static DateTime? GetDateTaken(string imagePath)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(imagePath);
            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var dateTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);

            if (DateTime.TryParse(dateTime, out var parsedDate))
                return parsedDate;
        }
        catch
        {
            Console.WriteLine("Failed to extract DateTaken");
        }
        return null;
    }
    
    public static Mat BlackAndWhite(Mat inputImage)
    {
        var blackAndWhite = new Mat();
        var gray = new Mat();
        CvInvoke.CvtColor(inputImage, gray, ColorConversion.Bgr2Gray);
        CvInvoke.Threshold(gray, blackAndWhite, 160, 255, ThresholdType.Binary);
        return blackAndWhite;
    }
    
    private static Mat ConvertToLab(Mat inputImage)
    {
        Mat labImage = new Mat();
        CvInvoke.CvtColor(inputImage, labImage, ColorConversion.Bgr2Lab);
        return labImage;
    }


    private static Mat ConvertToGrayScale(Mat inputImage) 
    {
        var gray = new Mat();
        CvInvoke.CvtColor(inputImage, gray, ColorConversion.Bgr2Gray);
        return gray;
    }
    
    
    private static byte[] ConvertToByteArray(Mat inputImage)
    {
        using var buffer = new VectorOfByte();
        CvInvoke.Imencode(".png", inputImage, buffer);
        return buffer.ToArray();
    }

    private static async Task<List<(FigureType type, FigureColor color)>> GetTopFiveFiguresPrediction(Mat inputImage)
    {
        var imageBytes = ConvertToByteArray(inputImage);
        return await ApiHandling.PredictTopKLabels(imageBytes);
    }

    private static async Task<(FigureType type, FigureColor color)> GetFigurePrediction(Mat inputImage) 
    {
        var imageBytes = ConvertToByteArray(inputImage);
        var predictedLabel = await ApiHandling.PredictChessPiece(imageBytes);

        return ParseLabelToEnum(predictedLabel);
    }

    private static (FigureType, FigureColor) ParseLabelToEnum(string label)
    {
        if (label == "empty") 
            return (FigureType.Empty, FigureColor.Empty); // No piece

        var parts = label.Split('_'); 
        if (parts.Length != 2)
        {
            Console.WriteLine("Invalid label fce: ParseLabelToEnum");
            return (FigureType.Empty, FigureColor.Empty); // Invalid response
        }


        var color = parts[0] switch
        {
            "b" => FigureColor.Black,
            "w" => FigureColor.White,
            _ => FigureColor.Empty
        };

        var type = parts[1] switch
        {
            "pawn" => FigureType.Pawn,
            "rook" => FigureType.Rook,
            "knight" => FigureType.Knight,
            "bishop" => FigureType.Bishop,
            "queen" => FigureType.Queen,
            "king" => FigureType.King,
            _ => FigureType.Empty
        };

        return (type, color);
    }
    
    private static Mat GaussianBlur(Mat inputImage)
    {
        var blur = new Mat();
        CvInvoke.GaussianBlur(inputImage, blur, new Size(5, 5), 5.0);
        return blur; 
    }
    
    private static Mat CannyMethod(Mat inputImage)
    {
        // Compute average pixel intensity directly from Mat
        MCvScalar meanScalar = CvInvoke.Mean(inputImage);
        double averageIntensity = meanScalar.V0;
    
        double lowerThreshold = Math.Max(Constans.MinIntensity, 0.7 * averageIntensity);
        double upperThreshold = Math.Min(Constans.MaxIntensity, 1.1 * averageIntensity);
    
        Mat canny = new Mat();
        CvInvoke.Canny(inputImage, canny, lowerThreshold, upperThreshold);
        return canny;
    }
    
    private static Mat ExtractEdges(Mat inputImage)
    {
        return CannyMethod(GaussianBlur(ConvertToGrayScale(inputImage)));
    }
    
    private static Mat PrepareImageForAnalysis(Mat inputImage) 
    {
        return MorphologyClosingOp(ExtractEdges(inputImage));
    }
    
    //will fill gaps in lines from hough transform
    private static Mat MorphologyClosingOp(Mat inputImage)
    {
        var closedEdges = new Mat();
    
        // Define the kernel for the morphological operation
        Mat kernel = CvInvoke.GetStructuringElement(
            ElementShape.Rectangle,
            new Size(5, 5), 
            new Point(-1, -1) 
        );
    
        CvInvoke.MorphologyEx(
            inputImage, 
            closedEdges, 
            MorphOp.Close, 
            kernel, 
            new Point(-1, -1), 
            3,                
            BorderType.Reflect, 
            new MCvScalar(1)   
        );

        return closedEdges;
    }

    private static List<LineSegment2D> ProcessLines(List<LineSegment2D> lines, bool isHorizontal, int width, int height)
    {
        return AddMissingLines(
            FilterEvenlySpacedLines(
                MergeAndExtendLines(lines, isHorizontal, width, height),
                isHorizontal),
            isHorizontal, width, height);
    }

    private static LineSegment2D[] HoughTransform(Mat inputImage)
    {
        var lines = CvInvoke.HoughLinesP(inputImage, 1, Math.PI / 90, 10, 80, 2); 
        var horLines = new List<LineSegment2D>();
        var verLines = new List<LineSegment2D>();
        var imageWidth = inputImage.Width;
        var imageHeight = inputImage.Height;

        // Separate lines into horizontal and vertical groups
        foreach (var line in lines)
        {
            var angle = Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X) * 180.0 / Math.PI;
            
            if (Math.Abs(angle) <= Constans.MaxTiltAngle || Math.Abs(angle - 180) <= Constans.MaxTiltAngle) 
            {
                horLines.Add(line);
            }
            else 
            {
                verLines.Add(line);
            }
        }
        
        horLines = ProcessLines(horLines, isHorizontal: true, imageWidth, imageHeight);
        verLines = ProcessLines(verLines, isHorizontal: false, imageWidth, imageHeight);

        // Return the combined set of horizontal and vertical lines
        return horLines.Concat(verLines).ToArray();
    }

    private static double Median(List<double> values)
    {
        var throwAwayValues = values.ToList();
        if (throwAwayValues.Count == 0) throw new InvalidOperationException("Cannot compute median of an empty list.");
        throwAwayValues.Sort();
        var midIndex = throwAwayValues.Count / 2;
        
        if (throwAwayValues.Count % 2 != 0)
            return throwAwayValues[midIndex];

        return (throwAwayValues[midIndex - 1] + throwAwayValues[midIndex]) / 2.0;
    }
    
    private static List<LineSegment2D> FilterEvenlySpacedLines(List<LineSegment2D> lines, bool isHorizontal)
    {
        var sortedLines = lines.OrderBy(line => isHorizontal ? line.P1.Y : line.P1.X).ToList();
        //HACK need to solve this later
        if (isHorizontal)
        {
            lines.RemoveAt(0);
            lines.RemoveAt(0);
            lines.RemoveAt(0);
            lines.RemoveAt(lines.Count - 1);
        }
        
        List<LineSegment2D> filteredLines = [];
        List<double> gaps = [];
        List<double> angles = [];

        var middleIndex = sortedLines.Count / 2;
        var lastAddedIndexForward = middleIndex;
        var lastAddedIndexBackward = middleIndex;
        filteredLines.Add(sortedLines[middleIndex]);

        // Reference position is always the last processed line (even if skipped)
        double lastPosition = isHorizontal ? sortedLines[middleIndex].P1.Y : sortedLines[middleIndex].P1.X;

        // Compute gaps and angles
        angles.Add(CalculateAngle(sortedLines[0]));
        for (var i = 1; i < sortedLines.Count; i++)
        {
            gaps.Add(CalculateDistance(sortedLines[i], sortedLines[i - 1], isHorizontal));
            angles.Add(CalculateAngle(sortedLines[i]));
        }

        var medianDistance = Median(gaps);
        var medianAngle = Median(angles);

        // Process forward from middle to end
        for (var i = middleIndex + 1; i < sortedLines.Count; i++)
        {
            var currentPosition = isHorizontal ? sortedLines[i].P1.Y : sortedLines[i].P1.X;
            var distance = Math.Abs(currentPosition - lastPosition);
            var currentAngle = CalculateAngle(sortedLines[i]);

            if (Math.Abs(distance - medianDistance) <= Constans.DistanceThreshold &&
                Math.Abs(currentAngle - medianAngle) <= Constans.AngleThreshold)
            {
                filteredLines.Add(sortedLines[i]);
                lastPosition = currentPosition; // Update reference position
                lastAddedIndexForward = i;
            }
        }

        // Ensure at least 4 lines exist after forward processing (with angle criteria)
        while (filteredLines.Count < 4 && lastAddedIndexForward < sortedLines.Count - 1)
        {
            lastAddedIndexForward++;
            var currentAngle = CalculateAngle(sortedLines[lastAddedIndexForward]);

            if (Math.Abs(currentAngle - medianAngle) <= Constans.AngleThreshold)
            {
                filteredLines.Add(sortedLines[lastAddedIndexForward]);
            }
        }

        // Reset lastPosition to middle for backward processing
        lastPosition = isHorizontal ? sortedLines[middleIndex].P1.Y : sortedLines[middleIndex].P1.X;

        // Process backward from middle to start
        for (var i = middleIndex - 1; i >= 0; i--)
        {
            var currentPosition = isHorizontal ? sortedLines[i].P1.Y : sortedLines[i].P1.X;
            var distance = Math.Abs(currentPosition - lastPosition);
            var currentAngle = CalculateAngle(sortedLines[i]);

            if (Math.Abs(distance - medianDistance) <= Constans.DistanceThreshold &&
                Math.Abs(currentAngle - medianAngle) <= Constans.AngleThreshold)
            {
                filteredLines.Insert(0, sortedLines[i]);
                lastPosition = currentPosition; // Update reference position
                lastAddedIndexBackward = i;
            }
        }

        // Ensure at least 8 lines exist after full processing (with angle criteria)
        while (filteredLines.Count < 8 && lastAddedIndexBackward > 0)
        {
            lastAddedIndexBackward--;
            var currentAngle = CalculateAngle(sortedLines[lastAddedIndexBackward]);

            if (Math.Abs(currentAngle - medianAngle) <= Constans.AngleThreshold)
            {
                filteredLines.Insert(0, sortedLines[lastAddedIndexBackward]); // Insert at the beginning
            }
        }

        return filteredLines;
    }

    private static bool AreGapsUniform(List<double> gaps)
    {
        for (var i = 1; i < gaps.Count; i++)
        {
            if (Math.Abs(gaps[i] - gaps[i - 1]) > Constans.DistanceThreshold * 1.5)
            {
                return false;
            }
        }
        return true;
    }

    private static LineSegment2D CreateNewLine(LineSegment2D baseLine, double shift, bool isHorizontal)
    {
        // Shift line position accordingly
        Point newP1, newP2;
        if (isHorizontal)
        {
            newP1 = new Point(baseLine.P1.X, (int)(baseLine.P1.Y + shift));
            newP2 = new Point(baseLine.P2.X, (int)(baseLine.P2.Y + shift));
        }
        else
        {
            newP1 = new Point((int)(baseLine.P1.X + shift), baseLine.P1.Y);
            newP2 = new Point((int)(baseLine.P2.X + shift), baseLine.P2.Y);
        }

        return new LineSegment2D(newP1, newP2);
    }


    private static LineSegment2D CreateNewLine(double position, double angle, bool isHorizontal, int imageWidth,
        int imageHeight)
    {
        var angleRad = angle * Math.PI / 180;
        var tanAngle = Math.Tan(angleRad);

        int x1, y1, x2, y2;
        if (isHorizontal)
        {
            x1 = 0;
            y1 = (int)position;

            x2 = imageWidth;
            y2 = (int)(y1 + tanAngle * (x2));
        }
        else
        {
            y1 = 0;
            x1 = (int)position;

            y2 = imageHeight;
            x2 = (int)(x1 + y2 / tanAngle);
        }
        //I do not check if the position is in the bound of an image

        return new LineSegment2D(new Point(x1, y1), new Point(x2, y2));
    }

    private static bool IsInsertionSideBeginning(List<LineSegment2D> lines, bool isHorizontal, int imageWidth, int imageHeight)
    {
        var firstLine = lines.First();
        var lastLine = lines.Last();

        var frontDistance = isHorizontal ? firstLine.P1.Y : firstLine.P1.X;
        var lastDistance = isHorizontal ? imageHeight - lastLine.P1.Y : imageWidth - lastLine.P1.X;
        
        return frontDistance > lastDistance; 
    }

    private static void AddFinishingAndStartingLines(List<LineSegment2D> lines, bool isHorizontal, int imageWidth, int imageHeight)
    {
        if (lines.Count < 2)
        {
            throw new ArgumentException("At least two lines are required to determine gaps.");
        }

        var insertAtBeginning = IsInsertionSideBeginning(lines, isHorizontal, imageWidth, imageHeight);

        // Get reference line based on insertion side
        LineSegment2D referenceLine = insertAtBeginning ? lines.First() : lines.Last();
        LineSegment2D secondReferenceLine = insertAtBeginning ? lines[1] : lines[^2];

        // Calculate the gap to maintain uniform spacing
        double gap = CalculateDistance(referenceLine, secondReferenceLine, isHorizontal);
    
        // Calculate new line position
        var newLinePosition = isHorizontal
            ? (insertAtBeginning ? referenceLine.P1.Y - gap : referenceLine.P1.Y + gap)
            : (insertAtBeginning ? referenceLine.P1.X - gap : referenceLine.P1.X + gap);

        // Ensure new line stays within image bounds
        newLinePosition = Math.Clamp(newLinePosition, 0, isHorizontal ? imageHeight : imageWidth);

        // Use the reference line's angle for consistency
        double referenceAngle = CalculateAngle(referenceLine);

        // Create the new line
        LineSegment2D newLine = CreateNewLine(newLinePosition, referenceAngle, isHorizontal, imageWidth, imageHeight);

        // Insert the new line at the correct position
        if (insertAtBeginning)
        {
            lines.Insert(0, newLine);
        }
        else
        {
            lines.Add(newLine);
        }
    }

    private static double ChooseBestReferenceGap(List<double> gaps, double imageHeight)
    { 
        var maxPossibleGap = imageHeight / 8;
        var gapsCopy = gaps.ToList();
        gapsCopy.Sort();
        
        for (var i = gapsCopy.Count / 2; i >= 0; i--)
        {
            if (gapsCopy[i] < maxPossibleGap)
            {
                //Console.WriteLine("Taken gap: " + gapsCopy[i]);
                //Console.WriteLine("Smallest gap: " + gapsCopy[0]);
                return gapsCopy[i];
            }
        }

        return maxPossibleGap;

    }

    private static void AddLineToBestFittingGap(List<LineSegment2D> sortedLines, List<double> gaps, bool isHorizontal, double imageHeight)
    {   
        if (sortedLines.Count < 2)
        {
            throw new ArgumentException("At least two lines are required to determine gaps.");
        }

        var referenceGap = ChooseBestReferenceGap(gaps, imageHeight);
        var bestGapIndex = -1;
        
        for (int i = 0; i < gaps.Count; i++)
        {
            if (gaps[i] >= referenceGap * 1.5)
            {
                bestGapIndex = i;
                break;
            }
        }

        if (bestGapIndex == -1)
        {
            sortedLines.RemoveAt(0);
            sortedLines.RemoveAt(sortedLines.Count - 1);
            return;
        }
        
        var referenceLine = sortedLines[bestGapIndex];
        var newLine = CreateNewLine(referenceLine, referenceGap, isHorizontal);
        sortedLines.Insert(bestGapIndex + 1, newLine);
    }

    private static List<LineSegment2D> AddMissingLines(List<LineSegment2D> lines, bool isHorizontal, int imageWidth, int imageHeight)
    {
        if (lines.Count >= 9) return lines; // No need to add lines if there are already 9

        // Sort lines by position (Y for horizontal, X for vertical)
        var sortedLines = lines.OrderBy(line => isHorizontal ? line.P1.Y : line.P1.X).ToList();

        while (sortedLines.Count < 9)
        {
            List<double> gaps = [];
            for (var i = 1; i < sortedLines.Count; i++)
            {
                gaps.Add(CalculateDistance(sortedLines[i - 1], sortedLines[i], isHorizontal));
            }

            if (AreGapsUniform(gaps))
            {
                AddFinishingAndStartingLines(sortedLines, isHorizontal, imageWidth, imageHeight);
            }
            else
            {
                AddLineToBestFittingGap(sortedLines, gaps, isHorizontal, imageHeight);
            }

        }

        return sortedLines;
    }
    
    private static double CalculateDistance(LineSegment2D line1, LineSegment2D line2, bool isHorizontal)
    {
        if (isHorizontal)
        {
            // Use average Y positions of both endpoints
            var y1 = (line1.P1.Y + line1.P2.Y) / 2.0;
            var y2 = (line2.P1.Y + line2.P2.Y) / 2.0;
            return Math.Abs(y2 - y1);
        }

        // Use average X positions of both endpoints
        var x1 = (line1.P1.X + line1.P2.X) / 2.0;
        var x2 = (line2.P1.X + line2.P2.X) / 2.0;
        return Math.Abs(x2 - x1);
    }

    private static LineSegment2D MergeCluster(List<LineSegment2D> cluster, bool isHorizontal)
    {
        if (cluster.Count == 1) return cluster[0]; 

        if (isHorizontal)
        {
            int avgY1 = (int)cluster.Average(line => line.P1.Y);
            int avgY2 = (int)cluster.Average(line => line.P2.Y);
            int fixedX1 = cluster[0].P1.X; 
            int fixedX2 = cluster[0].P2.X;

            return new LineSegment2D(new Point(fixedX1, avgY1), new Point(fixedX2, avgY2));
        }
            
        var avgX1 = (int)cluster.Average(line => line.P1.X);
        var avgX2 = (int)cluster.Average(line => line.P2.X);
        var fixedY1 = cluster[0].P1.Y; 
        var fixedY2 = cluster[0].P2.Y;

        return new LineSegment2D(new Point(avgX1, fixedY1), new Point(avgX2, fixedY2));
        
    }
    
    // Helper function to merge nearby lines based on their primary coordinate first version
    //TODO make this function cluster based.    
    //TODO if the line cross another line we need to delete it.
    private static List<LineSegment2D> MergeAndExtendLines(List<LineSegment2D> lines, bool isHorizontal, int imageWidth, int imageHeight)
    {
        // Sort lines by their average position
        lines = lines.OrderBy(line => isHorizontal ? (line.P1.Y + line.P2.Y) / 2.0 : (line.P1.X + line.P2.X) / 2.0).ToList();

            List<LineSegment2D> mergedLines = [];
            List<LineSegment2D> currentCluster = [];

        foreach (var line in lines)
        {
            if (currentCluster.Count == 0)
            {
                // Start a new cluster
                currentCluster.Add(line);
                continue;
            }

            // Get distance from the last added line in the cluster
            LineSegment2D referenceClusterLine = currentCluster[currentCluster.Count / 2]; // middle of the cluster
            double distance = CalculateDistance(referenceClusterLine, line, isHorizontal);

            if (distance <= Constans.MaxLineSpacing)
            {
                // Line is close enough, add it to the cluster
                currentCluster.Add(line);
            }
            else
            {
                // Merge the current cluster into a single line
                mergedLines.Add(MergeCluster(currentCluster, isHorizontal));

                // Start a new cluster with the current line
                currentCluster = [line];
            }
        }

        // Merge the last remaining cluster
        if (currentCluster.Count > 0)
        {
            mergedLines.Add(MergeCluster(currentCluster, isHorizontal));
        }

        // Extend all lines after merging
        for (var i = 0; i < mergedLines.Count; i++)
        {
            mergedLines[i] = ExtendLineToBounds(mergedLines[i], isHorizontal, imageWidth, imageHeight);
        }
        
        return mergedLines;
    }

    
    private static double CalculateAngle(LineSegment2D line)
    {
        return Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X) * (180.0 / Math.PI);
    }

    private static LineSegment2D ExtendLineToBounds(LineSegment2D line, bool isHorizontal, int imageWidth, int imageHeight)
    {
        double angle = CalculateAngle(line);
        double angleRad = angle * Math.PI / 180.0;
        double tanAngle = Math.Tan(angleRad);

        if (isHorizontal)
        {
            int newX1 = 0;
            int newY1 = (int)(line.P1.Y + tanAngle * (newX1 - line.P1.X));

            int newX2 = imageWidth;
            int newY2 = (int)(line.P1.Y + tanAngle * (newX2 - line.P1.X));

            newY1 = Math.Clamp(newY1, 0, imageHeight);
            newY2 = Math.Clamp(newY2, 0, imageHeight);

            return new LineSegment2D(new Point(newX1, newY1), new Point(newX2, newY2));
        }
        else
        {
            int newY1 = 0;
            int newX1 = (int)(line.P1.X + (newY1 - line.P1.Y) / tanAngle);

            int newY2 = imageHeight;
            int newX2 = (int)(line.P1.X + (newY2 - line.P1.Y) / tanAngle);

            newX1 = Math.Clamp(newX1, 0, imageWidth);
            newX2 = Math.Clamp(newX2, 0, imageWidth);

            return new LineSegment2D(new Point(newX1, newY1), new Point(newX2, newY2));
        }
    }
    
    private static PointF[,] ExtractGridPoints(LineSegment2D[] horLines, LineSegment2D[] verLines)
    {
        PointF[,] gridPoints = new PointF[9, 9]; // 9x9 points for 8x8 grid

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                gridPoints[row, col] = FindIntersection(horLines[row], verLines[col]);
            }
        }
        
        return gridPoints;
    }
    
    private static PointF FindIntersection(LineSegment2D line1, LineSegment2D line2)
    {
        float a1 = line1.P2.Y - line1.P1.Y;
        float b1 = line1.P1.X - line1.P2.X;
        float c1 = a1 * line1.P1.X + b1 * line1.P1.Y;

        float a2 = line2.P2.Y - line2.P1.Y;
        float b2 = line2.P1.X - line2.P2.X;
        float c2 = a2 * line2.P1.X + b2 * line2.P1.Y;

        float delta = a1 * b2 - a2 * b1;
        if (delta == 0)
            throw new ArgumentException("Lines do not intersect");

        float x = (b2 * c1 - b1 * c2) / delta;
        float y = (a1 * c2 - a2 * c1) / delta;
        return new PointF(x, y);
    }
    
    private static Mat ExtractSquareRoi(Mat inputImage, PointF[] corners)
    {
        // Compute bounding box for the square
        int x = (int)Math.Min(corners[0].X, corners[2].X);
        int y = (int)Math.Min(corners[0].Y, corners[2].Y);
        int width = (int)Math.Abs(corners[0].X - corners[2].X);
        int height = (int)Math.Abs(corners[0].Y - corners[2].Y);

        // Ensure extracted region is within bounds
        Rectangle roiRect = new Rectangle(x, y, width, height);
        Mat squareRoi = new Mat(inputImage, roiRect);

        // Create a black background of the same size as the square
        Mat finalImage = new Mat(new Size(width, height), DepthType.Cv8U, 3);
        finalImage.SetTo(new MCvScalar(0, 0, 0)); // Black background

        // Create a white mask for the square 
        Mat mask = new Mat(new Size(width, height), DepthType.Cv8U, 1);
        mask.SetTo(new MCvScalar(255)); // Full visibility

        // Copy the extracted square onto the black background
        squareRoi.CopyTo(finalImage, mask);

        return finalImage;
    }

    public static List<(FigureType type, FigureColor color)> PredictTopFiveFigures(Mat inputImage, PointF[] corners)
    {
        return GetTopFiveFiguresPrediction(ExtractSquareRoi(inputImage, corners)).Result;
    }

    public static (FigureType type, FigureColor color) PredictFigure(Mat inputImage, PointF[] corners)
    {
        return GetFigurePrediction(ExtractSquareRoi(inputImage, corners)).Result;
    }
    
    private static bool IsSquareOccupiedImageClassification(Mat inputImage, PointF[] corners)
    {
        return PredictFigure(inputImage, corners).type != FigureType.Empty;
    }
    
    private static bool IsSquareOccupiedIntensity(Square emptySquare, Square currentSquare)
    {
        var intensityDifference = Math.Abs(currentSquare.SquareIntensity - emptySquare.SquareIntensity);
        return intensityDifference > Constans.SquareIntensityThreshold;
    }
    
    
    // //Note: My gpu Radeon 780M have no cuda support so no everything is running on cpu... can improve on other systems
    // private static bool IsSquareOccupiedContour_gpu(Mat edgesCpu, PointF[] corners)
    // {
    //     if (!CudaInvoke.HasCuda)
    //     {
    //         // Fallback to original CPU implementation
    //         return IsSquareOccupiedContour(edgesCpu, corners);
    //     }
    //
    //     using var edgesGpu = new CudaImage<Bgra, byte>(edgesCpu);
    //
    //     float centerX = (corners[0].X + corners[2].X) / 2;
    //     float centerY = (corners[0].Y + corners[2].Y) / 2;
    //
    //     float radius = Math.Min(
    //         Math.Abs(corners[0].X - corners[1].X) / 3,
    //         Math.Abs(corners[0].Y - corners[3].Y) / 3
    //     );
    //
    //     using var maskCpu = new Mat(edgesCpu.Size, DepthType.Cv8U, 1);
    //     maskCpu.SetTo(new MCvScalar(0));
    //     CvInvoke.Circle(maskCpu, new Point((int)centerX, (int)centerY), (int)radius, new MCvScalar(255), -1);
    //
    //     using var maskGpu = new CudaImage<Gray, byte>(maskCpu);
    //
    //     using var maskedEdgesGpu = new CudaImage<Gray, byte>(edgesCpu.Size);
    //     CudaInvoke.BitwiseAnd(edgesGpu, maskGpu, maskedEdgesGpu, null);
    //
    //     using var maskedEdgesCpu = new Mat();
    //     maskedEdgesGpu.Download(maskedEdgesCpu);
    //     
    //     using var contours = new VectorOfVectorOfPoint();
    //     CvInvoke.FindContours(maskedEdgesCpu, contours, null, RetrType.External, ChainApproxMethod.ChainApproxTc89L1);
    //
    //     using var hulls = new VectorOfVectorOfPoint();
    //     for (int i = 0; i < contours.Size; i++)
    //     {
    //         using var contour = contours[i];
    //         var hull = new VectorOfPoint();
    //         CvInvoke.ConvexHull(contour, hull);
    //         hulls.Push(hull);
    //     }
    //
    //     var notAcceptedAreas = new List<int>();
    //     for (int i = 0; i < hulls.Size; i++)
    //     {
    //         using var hull = hulls[i];
    //         var area = CvInvoke.ContourArea(hull);
    //         if (area > Constans.MinContourArea)
    //         {
    //             return true;
    //         }
    //         notAcceptedAreas.Add((int)area);
    //     }
    //
    //     return notAcceptedAreas.Count > 1;
    // }
    
    private static bool IsSquareOccupiedContour(Mat edges, PointF[] corners)
    {
        // Get the center of the square
        float centerX = (corners[0].X + corners[2].X) / 2;
        float centerY = (corners[0].Y + corners[2].Y) / 2;

        // Define circular ROI size
        float radius = Math.Min(
            Math.Abs(corners[0].X - corners[1].X) / 3,
            Math.Abs(corners[0].Y - corners[3].Y) / 3
        );

        // Create a mask for the circular ROI
        Mat mask = new Mat(edges.Size, DepthType.Cv8U, 1);
        mask.SetTo(new MCvScalar(0));
        CvInvoke.Circle(mask, new Point((int)centerX, (int)centerY), (int)radius, new MCvScalar(255), -1);

        // Apply mask to edges
        Mat maskedEdges = new Mat();
        CvInvoke.BitwiseAnd(edges, mask, maskedEdges);

        // Find contours
        using VectorOfVectorOfPoint contours = new();
        CvInvoke.FindContours(maskedEdges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxTc89L1);

        // Apply Convex Hull to each contour
        using VectorOfVectorOfPoint hulls = new();
        foreach (var contour in contours.ToArrayOfArray())
        {
            VectorOfPoint hull = new VectorOfPoint();
            CvInvoke.ConvexHull(new VectorOfPoint(contour), hull);
            hulls.Push(hull);
        }
        
        var notAcceptedAreas = new List<int>();
        foreach (var hull in hulls.ToArrayOfArray())
        {
            var area = CvInvoke.ContourArea(new VectorOfPoint(hull));
            if (area > Constans.MinContourArea)
            {
                //TODO delete this for thesis purpose only
                // VisualizeContour(edges, new VectorOfPoint(hull));
                return true; // If a significant contour is found, the square is occupied
            }

            notAcceptedAreas.Add((int)area);
        }

        return notAcceptedAreas.Count > 1; // Multiple smaller contours is also accepted
    }
    
    private static bool IsSquareOccupiedHybrid(Mat inputImageClassification, Mat edges, PointF[] corners, Square currentSquare, Square emptySquare)
    {
        var byIntensity = IsSquareOccupiedIntensity(currentSquare, emptySquare); 
        var byContour = IsSquareOccupiedContour(edges, corners);
        var byClassification = IsSquareOccupiedImageClassification(inputImageClassification, corners);
        
        Console.WriteLine($"Intensity = {byIntensity}, Contour = {byContour}, Classification = {byClassification}");
        var votes = Convert.ToInt32(byIntensity) + Convert.ToInt32(byContour) + Convert.ToInt32(byClassification);
        return votes >= 2;
    }

    private static double CalculateSquareIntensity(Mat inputImage, PointF[] corners)
    {
        // Get the center of the square based on its corners
        float centerX = (corners[0].X + corners[2].X) / 2; // Top-left and bottom-right
        float centerY = (corners[0].Y + corners[2].Y) / 2;

        float radius = Math.Min(
            Math.Abs(corners[0].X - corners[1].X) / 4, // 1/4 of square width
            Math.Abs(corners[0].Y - corners[3].Y) / 4  // 1/4 of square height
        );

        // Create a mask for the circular ROI
        Mat mask = new Mat(inputImage.Size, DepthType.Cv8U, 1);
        mask.SetTo(new MCvScalar(0)); // Black mask

        // Draw a white-filled circle at the center with the smaller radius
        CvInvoke.Circle(mask, new Point((int)centerX, (int)centerY), (int)radius, new MCvScalar(255), -1);

        Mat labImage = new Mat();
        CvInvoke.CvtColor(inputImage, labImage, ColorConversion.Bgr2Lab);

        // Calculate the mean intensity within the circular mask
        var labChannels = new VectorOfMat();
        CvInvoke.Split(labImage, labChannels);
        var lightness = labChannels[0];

        var meanIntensity = CvInvoke.Mean(lightness, mask).V0;
        
        return meanIntensity;
    }

    
    private static SquareColor DeterminateSquareColor(int row, int col)
    {
        return (row + col) % 2 == 1 ? SquareColor.White : SquareColor.Black;
    }

    //TODO delete index, this is there just to generate pictures for thesis. 
    internal static ChessBoard InitializeChessBoardFromImage(Mat inputImage, int index, ChessBoard? referenceBoard = null)
    {
         var lines = HoughTransform(PrepareImageForAnalysis(inputImage)); 
         // if (index == 3) VisualizeLines(inputImage, lines);
         var gridPoints = ExtractGridPoints(
             lines.Take(9).ToArray(), 
             lines.Skip(9).Take(9).ToArray());
         var squares = new List<Square>();
         var blurredImage = new Mat();
         CvInvoke.GaussianBlur(inputImage, blurredImage, new Size(5, 5), 2.0);
         var edges = new Mat();
         CvInvoke.Canny(blurredImage, edges, 30, 85);
         
         for (var row = 0; row < 8; row++)
         {
             for (var col = 0; col < 8; col++)
             {
                 PointF[] corners =
                 [
                     gridPoints[row, col],
                     gridPoints[row, col + 1],
                     gridPoints[row + 1, col + 1],
                     gridPoints[row + 1, col]
                 ];

                 var square = new Square(
                     row,
                     col,
                     CalculateSquareIntensity(inputImage, corners),
                     corners,
                     DeterminateSquareColor(row, col));
                 
                 squares.Add(square);
             }
         }

         var chessBoard = new ChessBoard(squares);
         if (referenceBoard == null) 
         {
             foreach (var square in chessBoard.Squares)
             {
                 square.IsOccupied = false;
             }
         }
         else
         {
             for (var row = 0; row < 8; row++)
             {
                 for (var col = 0; col < 8; col++)
                 {
                     var currentSquare = chessBoard.Squares[row, col];
                     var referenceSquare = referenceBoard.Squares[row, col];
                     
                     currentSquare.IsOccupied = IsSquareOccupiedHybrid(inputImage, edges, currentSquare.Corners, currentSquare, referenceSquare);
                 }
             }
         }

         // ChessboardTrainingDataExporter.ExtractAndSaveFigureRoi(
         //     chessBoard, 
         //     inputImage, 
         //     @"C:\prace\skola\bakalarka\images\shoot6\image_classification\exported\empty"); //learning purpose
         // VisualizeLines(inputImage, lines, "ChessBoard initialization"); //testing purpose
         return chessBoard; 
    }
    
    
    //expandable
    
    private static void DisplayAndWait(Mat image, string description = "image")
    {
        // Calculate a scaling factor if image is larger than the screen
        var scalingFactor = 1.0;
        if (image.Width > Constans.MonitorWidth || image.Height > Constans.MonitorHeight)
        {
            var widthScaling = (double)Constans.MonitorWidth / image.Width;
            var heightScaling = (double)Constans.MonitorHeight / image.Height;

            // Use the smaller scaling factor to maintain an aspect ratio
            scalingFactor = Math.Min(widthScaling, heightScaling);
        }

        // Resize the image if necessary
        Mat resizedImage = new Mat();
        if (scalingFactor < 1.0)
        {
            CvInvoke.Resize(image, resizedImage, new Size(
                (int)(image.Width * scalingFactor),
                (int)(image.Height * scalingFactor)));
        }
        else
        {
            resizedImage = image; // No resizing needed
        }

        // Display the resized image
        CvInvoke.Imshow(description, resizedImage);
        CvInvoke.WaitKey(0);
    } 
    
    private static void VisualizeContour(Mat inputImage, VectorOfPoint contour, string windowName = "Contours")
    {
        Mat imageWithContour = inputImage.Clone();
        CvInvoke.DrawContours(imageWithContour, new VectorOfVectorOfPoint(contour), -1, new MCvScalar(0, 255, 0), 5);
        DisplayAndWait(imageWithContour);
    }
    
    //will display input image if there is now lines
    private static void VisualizeLines(Mat inputImage, LineSegment2D[] lines, string message = "Lines")
    {
        Mat outputImage = new Mat();
        if (inputImage.NumberOfChannels == 1)  // Check if it's a grayscale image
        {
            CvInvoke.CvtColor(inputImage, outputImage, ColorConversion.Gray2Bgr);
        }
        else
        {
            outputImage = inputImage.Clone();
        }

        MCvScalar lineColor = new MCvScalar(0, 255, 0); // Green
        int lineThickness = 2;

        foreach (var line in lines)
        {
            CvInvoke.Line(outputImage, line.P1, line.P2, lineColor, lineThickness);
        }

        //Console.WriteLine($"Message: {message}, Number of lines: {lines.Length}");

        DisplayAndWait(outputImage);
    }


}