using System.Diagnostics;
using Bakalarka;
using ChessNotationsGenerator.CoreLogic;
using Emgu.CV;
using Microsoft.AspNetCore.Http.Features;

// Testing the app in the console enviroment
// Console.OutputEncoding = System.Text.Encoding.UTF8;
// var game = new Game();
// var folderPath = @"C:\prace\skola\bakalarka\images\shoot6\game_2_promotion";
// Stopwatch stopwatch = Stopwatch.StartNew();
//
// game.LoadGameParallel(folderPath, false);
// game.DeterminateStartingFiguresDesignation(game.IsGameFromBeginning());
// game.UpdateGameStates(); // only generate notation
// stopwatch.Stop();
//
// Console.WriteLine(game.ReturnChessNotationAsString());
//
// Console.WriteLine($"Run time: {stopwatch.Elapsed.Minutes} : {stopwatch.Elapsed.Seconds} : {stopwatch.Elapsed.Milliseconds}");
//  end of console testing
//control massage


var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.UseUrls("http://0.0.0.0:5002"); //added for docker

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = Constans.MultipartBodyLengthLimit;
});


// Add services to the container
builder.Services.AddScoped<ChessGameService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); //commented for docker
app.UseStaticFiles();   

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

//api testing
//test 1
// var image = CvInvoke.Imread(@"C:\prace\skola\bakalarka\images\shoot6\image_classification\test\black_knight.png");
// var prediction = ImageProcessing.GetFigurePrediction(image).Result;
// Console.WriteLine($"Type: {prediction.type} ; Color: {prediction.color}");

//test 2
// var image = CvInvoke.Imread(@"C:\prace\skola\bakalarka\images\shoot6\image_classification\test\black_knight.png");
// var predictions = ImageProcessing.GetTopFiveFiguresPrediction(image).Result;
// foreach (var prediction in predictions)
// {
//     Console.WriteLine(prediction);
// }



/*
 * number 0
 * Timing of the functions before any optimizations.
 *  @"C:\prace\skola\bakalarka\images\shoot6\magnus_on_going_game"
 *  Run time: 1 : 35 : 619
 *
 *  C:\prace\skola\bakalarka\images\shoot6\game_2_promotion
 *  Run time: 4 : 50 : 68
 *
 *  C:\prace\skola\bakalarka\images\shoot6\game_1_magnus
 *  Run time: 1 : 47 : 294
 *
 */
 
/*
 * number 1
 * Timing of the functions precalculating the edges for contour occupancy instead of doing it 64 times on roi
 *  @"C:\prace\skola\bakalarka\images\shoot6\magnus_on_going_game"
 *  Run time: 1 : 31 : 939
 *
 *  C:\prace\skola\bakalarka\images\shoot6\game_2_promotion
 *  Run time: 4 : 37 : 219
 *
 *  C:\prace\skola\bakalarka\images\shoot6\game_1_magnus
 *  Run time: 1 : 42 : 633
 *
 */
 
/*
 * number 3
 * Timing of the functions Parallel boards initialization.
 *  @"C:\prace\skola\bakalarka\images\shoot6\magnus_on_going_game"
 *  Run time: 0 : 49 : 719
 *
 *  C:\prace\skola\bakalarka\images\shoot6\game_2_promotion
 *  Run time: 2 : 41 : 247
 *
 *  C:\prace\skola\bakalarka\images\shoot6\game_1_magnus
 *  Run time: 1 : 3 : 415
 *
 */
 
/*
 * All combined
 *  @"C:\prace\skola\bakalarka\images\shoot6\magnus_on_going_game"
 *  Run time: 0 : 47 : 215
 *
 *  C:\prace\skola\bakalarka\images\shoot6\game_2_promotion
 *  Run time: 2 : 29 : 124
 *
 *  C:\prace\skola\bakalarka\images\shoot6\game_1_magnus
 *  Run time: 0 : 59 : 15
 *
 */