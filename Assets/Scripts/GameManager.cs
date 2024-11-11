
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField]
    private MultiPlayManager multiPlayManager;
    
    // index 0 for white, 1 for black
    private NetworkList<ulong> playerIds;
    public NetworkVariable<bool> isWhiteTurn = new NetworkVariable<bool>();
    // public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>();

    private void Awake()
    {
        playerIds = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ResisterPlayerServerRpc();

        isWhiteTurn.OnValueChanged += OnTurnChanged;
        
        if (IsServer)
        {
            Waiting2Players().Forget();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        isWhiteTurn.OnValueChanged -= OnTurnChanged;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(300, 10, 300, 300));

        if (IsClient || IsServer)
        {
        }

        GUILayout.EndArea();
    }
    
    [ServerRpc(RequireOwnership = false)]
    void ResisterPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerIds.Add(serverRpcParams.Receive.SenderClientId);
        Debug.Log($"{playerIds.Count.ToString()} players registered");
    }

    async UniTask Waiting2Players()
    {
        await UniTask.WaitUntil(() => playerIds.Count == 2);
        Play();
    }

    private void Play()
    {
        Debug.Log("Play");
        var id = isWhiteTurn.Value ? 0 : 1; 
        var player = NetworkManager.Singleton.ConnectedClients[playerIds[id]].PlayerObject;
        var playerComponent = player.GetComponent<Player>();
        playerComponent.DoMyTurn();
    }

    public ulong CurrentPlayerId()
    {
        return playerIds[isWhiteTurn.Value ? 0 : 1];
    }

    [ServerRpc(RequireOwnership = false)]
    void FlipTurnServerRpc()
    {
        isWhiteTurn.Value = !isWhiteTurn.Value;
    }
    
    public void FlipTurn()
    {
        FlipTurnServerRpc();
    }

    void OnTurnChanged(bool oldTurn, bool newTurn)
    {
        Debug.Log($"{oldTurn.ToString()} turn changed to {newTurn.ToString()}");
        
        if (IsServer)
        {
            Play();
        }
    }

    void Update() { }
}
