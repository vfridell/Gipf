using GipfLib.Models;
using System.Threading;
using System.Threading.Tasks;

namespace GipfLib.AI
{
    public interface IGipfAI
    {
        void BeginNewGame(bool PlayingWhite);
        Move MakeBestMove(Game game);
        Move PickBestMove(Board board);
        Task<Move> PickBestMoveAsync(Board board, CancellationToken aiCancelToken);
        bool playingWhite { get; }

        string Name { get; }
    }
}
