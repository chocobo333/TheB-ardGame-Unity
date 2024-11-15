using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class Disc : NetworkBehaviour
{
    Rigidbody rb;

    public bool beDestoryed = false;

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
        if (transform.position.y < -10)
        {
            DelayedDestory(20).Forget();
            return;
        }
    }

    async UniTask DelayedDestory(int frames)
    {
        gameObject.SetActive(false);
        beDestoryed = true;
        await UniTask.DelayFrame(frames);
        Destroy(gameObject);
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

    public float Speed()
    {
        return rb.linearVelocity.magnitude;
    }
}
