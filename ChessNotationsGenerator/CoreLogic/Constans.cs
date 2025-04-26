namespace ChessNotationsGenerator.CoreLogic;

public static class Constans
{
    public const int MinIntensity = 0; //80 working values with cannon d10
    public const int MaxIntensity = 250; //127 
    public const double SquareIntensityThreshold = 5; //prev 10
    public const double PieceIntensityThreshold = 20; //It's usually somewhere around 50
    public const double WhitePieceIntensityThreshold = 100; //100 and more are usually white
    public const double BlackPieceIntensityThreshold = 30; //30 and low are usually black
    public const int MonitorHeight = 768;
    public const int MonitorWidth = 1024;
    public const int MaxTiltAngle = 60;
    public const double AngleThreshold = 0.7; 
    public const int DistanceThreshold = 20;
    public const double CenterLineMaxOffset = 1; // Minimum allowed spacing between lines4
    public const int MaxLineSpacing = 20; // Maximum allowed spacing between lines4
    public const int MinContourArea = 120; // prev 120  
    public const int MultipartBodyLengthLimit = 524288000; // 500 MB
}