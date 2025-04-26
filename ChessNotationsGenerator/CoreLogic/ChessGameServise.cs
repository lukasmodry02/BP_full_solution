using Bakalarka;

namespace ChessNotationsGenerator.CoreLogic;

public class ChessGameService
{
    private readonly Game _game = new();

    public string GenerateNotationFromFolder(string folderPath, bool sortByDate)
    {
        _game.LoadGameParallel(folderPath, sortByDate);
        // if (_game.IsFirstBoardEmpty())
        // {
        //     return "Error: First board is not empty. Please upload a photo of an empty board as the first image.";
        // }
        _game.DeterminateStartingFiguresDesignation(_game.IsGameFromBeginning());
        _game.UpdateGameStates();
        return _game.ReturnChessNotationAsString();
    }
}