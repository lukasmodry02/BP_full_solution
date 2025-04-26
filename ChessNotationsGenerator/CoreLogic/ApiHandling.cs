using System.Text;
using System.Text.Json;
using Bakalarka;

namespace ChessNotationsGenerator.CoreLogic;

public static class ApiHandling
{
    private static readonly HttpClient HttpClient = new HttpClient(
        new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true }
    );
    private static readonly string ApiUrl = "http://localhost:5000/predict"; //normal
    // private const string ApiUrl = "http://api:5000/predict"; //docker

    
    private static (FigureType, FigureColor) ParseIndexToLabel(int index)
    {
        return index switch
        {
            0 => (FigureType.King, FigureColor.Black),
            1 => (FigureType.Rook, FigureColor.White),
            2 => (FigureType.King, FigureColor.White),
            3 => (FigureType.Bishop, FigureColor.Black),
            4 => (FigureType.Pawn, FigureColor.White),
            5 => (FigureType.Knight, FigureColor.White),
            6 => (FigureType.Bishop, FigureColor.White),
            7 => (FigureType.Rook, FigureColor.Black),
            8 => (FigureType.Knight, FigureColor.Black),
            9 => (FigureType.Pawn, FigureColor.Black),
            10 => (FigureType.Empty, FigureColor.Empty),
            11 => (FigureType.Queen, FigureColor.Black),
            12 => (FigureType.Queen, FigureColor.White),
            _ => (FigureType.Empty, FigureColor.Empty)
        };
    }

    public static async Task<string> PredictChessPiece(byte[] imageBytes)
    {
        try
        {
            var base64Image = Convert.ToBase64String(imageBytes);
            var requestBody = new { Base64Image = base64Image };
            var jsonRequest = JsonSerializer.Serialize(requestBody);
    
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(ApiUrl, content);
    
            if (response.IsSuccessStatusCode)
            {
                await using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
    
                if (doc.RootElement.TryGetProperty("predictedLabel", out var predictedLabel))
                {
                    return predictedLabel.GetString() ?? "Unknown";
                }
    
                return "Error: Expected 'predictedLabel' in JSON response";
            }
    
            return $"Error: {response.StatusCode}";
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
    
    public static async Task<List<(FigureType type, FigureColor color)>> PredictTopKLabels(byte[] imageBytes, int topK = 5)
    {
        try
        {
            var base64Image = Convert.ToBase64String(imageBytes);
            var requestBody = new { Base64Image = base64Image };
            var jsonRequest = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(ApiUrl, content);

            if (!response.IsSuccessStatusCode)
                return new List<(FigureType, FigureColor)>();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            if (!doc.RootElement.TryGetProperty("score", out var scoreArray))
                return new List<(FigureType, FigureColor)>();

            var scores = scoreArray.EnumerateArray().Select(s => s.GetSingle()).ToArray();

            var topIndices = scores
                .Select((score, index) => new { score, index })
                .OrderByDescending(x => x.score)
                .Take(topK)
                .Select(x => x.index)
                .ToList();

            return topIndices.Select(ParseIndexToLabel).ToList();
        }
        catch
        {
            return new List<(FigureType, FigureColor)>();
        }
    }
    
}