using Microsoft.AspNetCore.Mvc;
using ChessNotationsGenerator.CoreLogic;
using Microsoft.AspNetCore.Diagnostics;

namespace ChessNotationsGenerator.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    IWebHostEnvironment env,
    ChessGameService chessGameService
) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Index(List<IFormFile> images, bool sortByDate = false)
    {
        if (images.Count <= 2)
        {
            ViewBag.Massage = images.Count == 0
                ? "No images were uploaded."
                : "Please upload at least three images to generate notation." + Environment.NewLine +
                  "Including first empty board calibration image."; 
            return View();
        }

        // Ensure the /uploads folder exists
        var baseUploadPath = Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(baseUploadPath);

        // Create a unique subdirectory for this upload session
        var sessionId = Guid.NewGuid().ToString();
        var folderPath = Path.Combine(baseUploadPath, sessionId);
        Directory.CreateDirectory(folderPath);

        // Save the uploaded images into the session directory
        foreach (var image in images)
        {
            var filePath = Path.Combine(folderPath, image.FileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            image.CopyTo(stream);
        }

        try
        {
            var result = chessGameService.GenerateNotationFromFolder(folderPath, sortByDate);
            if (result.StartsWith("Error:"))
            {
                ViewBag.Massage = result;
            }
            else
            {
                ViewBag.Notation = result;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception during generation.");
            throw;
        }
        finally
        {
            try
            {
                Directory.Delete(folderPath, recursive: true);
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Failed to delete temporary upload folder: {folderPath}. Error: {ex.Message}");
            }
        }

        return View();
    }


    //public IActionResult Privacy() => View();

    [Route("Home/Error")]
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (feature != null)
        {
            logger.LogError(feature.Error, "Unhandled exception at path: {Path}", feature.Path);
        }

        return View(); 
    }

}
