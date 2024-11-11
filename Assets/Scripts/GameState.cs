using Unity.Netcode;
using Unity.Networking.Transport;

public class Constant
{
    public const int BoardWidth = 8;
    public const int BoardHeight = 8;
}

public enum GameState
{
    WaitingForPlayers,
    InGame,
}