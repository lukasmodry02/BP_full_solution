namespace ChessNotationsGenerator.CoreLogic;

public class ChessGameService
{
    private readonly Game _game = new();

    public string GenerateNotationFromFolder(string folderPath, bool sortByDate)
    {
        // Run the async code synchronously
        Task.Run(() => _game.LoadGameParallelAsync(folderPath, sortByDate)).GetAwaiter().GetResult();

        _game.DeterminateStartingFiguresDesignation(_game.IsGameFromBeginning());
        _game.UpdateGameStates();
        return _game.ReturnChessNotationAsString();
    }

}