using Unity.Netcode;
using UnityEngine;

public class Disc : NetworkBehaviour
{
    Rigidbody rb;

    void Awake()
    {
        TryGetComponent(out rb);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire(Vector3 dir, float power)
    {
        FireOwnerRpc(dir, power);
    }

    [Rpc(SendTo.Owner)]
    private void FireOwnerRpc(Vector3 dir, float power)
    {
        dir = dir.normalized;
        rb.isKinematic = false;
        rb.AddForce(dir * power, ForceMode.Impulse);
    }
}
