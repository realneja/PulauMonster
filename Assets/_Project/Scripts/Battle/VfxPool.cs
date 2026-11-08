// Pulau Monster · P7 Polish, Build & Demo · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;
using UnityEngine.Pool;

// ObjectPool<SpriteVfx>: tebasan dipakai ulang, tidak ada Instantiate tiap serangan.
public class VfxPool : MonoBehaviour
{
    [SerializeField] private SpriteVfx prefab;

    private ObjectPool<SpriteVfx> pool;

    void Awake()
    {
        pool = new ObjectPool<SpriteVfx>(
            () => Instantiate(prefab, transform),
            v => v.gameObject.SetActive(true),
            v => v.gameObject.SetActive(false),
            v => { if (v != null) Destroy(v.gameObject); }, // objek bisa sudah terhapus bersama scene
            false, 4, 16);
    }

    public void Play(Vector3 position)
    {
        if (prefab == null) return;
        SpriteVfx vfx = pool.Get();
        vfx.transform.position = position;
        vfx.Play(done => pool.Release(done));
    }
}
