using Microsoft.Extensions.ML;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");
// builder.WebHost.UseUrls("http://api:5000");

// Load ML.NET Model
builder.Services.AddPredictionEnginePool<ChessFiguresClassification.ModelInput, ChessFiguresClassification.ModelOutput>()
    .FromFile("ChessFiguresClassification.mlnet");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chess Classification API", Description = "Classifies chess pieces from images", Version = "v1" });
});

var app = builder.Build();
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chess Classification API V1"));
}

// API now accepts Base64-encoded image instead of file path
app.MapPost("/predict", async (PredictionEnginePool<ChessFiguresClassification.ModelInput, ChessFiguresClassification.ModelOutput> predictionEnginePool, ChessImageRequest request) =>
{
    var input = new ChessFiguresClassification.ModelInput
    {
        ImageSource = Convert.FromBase64String(request.Base64Image) // Convert Base64 string to byte array
    };

    var prediction = predictionEnginePool.Predict(input);
    return await Task.FromResult(prediction);
});

// ChessFiguresClassification.PrintLabelIndexMapping();
app.Run();

// Define the request model
public class ChessImageRequest
{
    public string Base64Image { get; set; } // Accepts Base64 image from client
}