
using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Player : NetworkBehaviour
{
    private GameManager gameManager;
    [SerializeField] private GameObject DiscPrefab;
    
    private PlayerInput _input;

    private bool charging = false;
    private bool fired = false;
    private float power = 0;

    private NetworkVariable<NetworkObjectReference> focusdDisc = new NetworkVariable<NetworkObjectReference>();
    
    private void Awake()
    {
        TryGetComponent(out _input);
        _input.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        // _input.enabled = IsOwner;
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        _input.enabled = false;
        base.OnNetworkDespawn();
    }

    [ServerRpc]
    void MoveServerRpc(Vector3 pos)
    {
        transform.position = pos;
        MoveClientRpc(pos);
    }
    
    [Rpc(SendTo.NotServer)]
    void MoveClientRpc(Vector3 pos)
    {
        Debug.Log("MoveClientRpc");
        transform.position = pos;
    }

    void Start()
    {
        // TODO: Change GameManager into Singleton?
        gameManager = FindFirstObjectByType<GameManager>();
        
        if (IsOwner)
        {
            var pos = Camera.main!.transform.position;
            pos.y = pos.y - 1;
            MoveServerRpc(pos);
        }
    }

    private void OnEnable()
    {
        _input.actions["Fire"].started += OnFireStart;
        _input.actions["Fire"].canceled += OnFireStop;
    }

    private void OnDisable()
    {
        _input.actions["Fire"].started -= OnFireStart;
        _input.actions["Fire"].canceled -= OnFireStop;
    }

    bool IsMyTurn()
    {
        var clientId = NetworkManager.Singleton.LocalClientId;
        return gameManager.CurrentPlayerId() == clientId;
    }


    void Update()
    {
        if (!IsSpawned) return;

        if (charging)
        {
            // hard-coding
            power += 5f * Time.deltaTime;
        }
    }

    public async UniTask TestRun()
    {
        Debug.Log("Hello");
        await UniTask.WaitForSeconds(3);
        Debug.Log($"I'm player {NetworkManager.Singleton.LocalClientId}");
        gameManager.FlipTurn();
    }

    private void OnFireStart(InputAction.CallbackContext ctx)
    {
        if (IsOwner)
        {
            Debug.Log($"I'm player {NetworkManager.Singleton.LocalClientId}, {NetworkObjectId} and fire start");
            charging = true;
        }
    }
    private void OnFireStop(InputAction.CallbackContext ctx)
    {
        if (IsOwner)
        {
            Debug.Log($"I'm player {NetworkManager.Singleton.LocalClientId} {NetworkObjectId} and fire stop");
            charging = false;
            fired = true;
        }
    }
    
    /*
     * Called By Server
     */
    public void  DoMyTurn()
    {
        DoMyTurnOwnerRpc();
    }

    [Rpc(SendTo.Owner)]
    private void DoMyTurnOwnerRpc()
    {
        PlayTurn().Forget();
    }

    private async UniTask PlayTurn()
    {
        Debug.Log("PlayTurn");
        if (IsOwner)
        {
            _input.enabled = true;
            power = 0;
            fired = false;
                
            Debug.Log("Your turn");
            
            SpawnDiscServerRpc(NetworkManager.Singleton.LocalClientId);

            await UniTask.WaitUntil(() => fired);

            if (focusdDisc.Value.TryGet(out var disc))
            {
                var discComponent = disc.GetComponent<Disc>();
                var dir = new Vector3(0, 0, (new Vector3(0, 0, 0) - transform.position).z);
                discComponent.Fire(dir, power);
            }
            
            _input.enabled = false;
            gameManager.FlipTurn();
        }
    }

    [ServerRpc]
    private void SpawnDiscServerRpc(ulong ownerId)
    {
        var isWhite = gameManager.isWhiteTurn.Value;
        var disc = Instantiate(
            DiscPrefab,
            transform.position,
            Quaternion.Euler(isWhite? 0 : 180, 0, 0)
        );
        disc.GetComponent<NetworkObject>().SpawnWithOwnership(ownerId);
        focusdDisc.Value = disc;
    }
    
}
