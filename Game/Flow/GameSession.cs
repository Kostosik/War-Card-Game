using GameProject.Game.Operations;
using System.Buffers;

namespace GameProject.Game.Flow;

internal sealed class GameSession
{
    public OperationState? CurrentOperation { get; private set; }

    public OperationState StartNewOperation()
    {
        CurrentOperation = new OperationState();
        return CurrentOperation;
    }
}