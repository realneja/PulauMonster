// Pulau Monster · P7 Polish, Build & Demo · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using UnityEngine;

// Partikel piksel ringan dengan pool sendiri: List aktif + Stack bebas, tanpa Instantiate tiap percikan.
public class PixelBurst : MonoBehaviour
{
    private class Particle
    {
        public SpriteRenderer sr;
        public Vector2 velocity;
        public float life, max;
    }

    [SerializeField] private Sprite pixel; // UI_Pixel 4x4
    [SerializeField] private float gravity = 2.5f;
    [SerializeField] private string sortingLayer = "Characters";
    [SerializeField] private int sortingOrder = 100;

    private readonly List<Particle> active = new List<Particle>();
    private readonly Stack<Particle> free = new Stack<Particle>();

    // Speed dalam piksel demo (dibagi 16 = PPU), ukuran acak 1 atau 2 piksel.
    public void Play(Vector3 pos, Color color, int count, float speed, float life)
    {
        for (int i = 0; i < count; i++)
        {
            Particle p = free.Count > 0 ? free.Pop() : Create();
            float a = Random.Range(0f, Mathf.PI * 2f);
            float s = Random.Range(speed * 0.3f, speed) / 16f;
            p.velocity = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * s;
            p.life = p.max = life;
            p.sr.color = color;
            p.sr.transform.position = pos;
            float size = Random.value < 0.5f ? 0.25f : 0.5f; // UI_Pixel 4 px → 1 atau 2 piksel demo
            p.sr.transform.localScale = new Vector3(size, size, 1f);
            p.sr.gameObject.SetActive(true);
            active.Add(p);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = active.Count - 1; i >= 0; i--) // mundur: aman saat menghapus
        {
            Particle p = active[i];
            p.life -= dt;
            if (p.life <= 0f)
            {
                p.sr.gameObject.SetActive(false);
                active.RemoveAt(i);
                free.Push(p); // dipakai lagi
                continue;
            }
            p.velocity.y -= gravity * dt;
            p.sr.transform.position += (Vector3)(p.velocity * dt);
            Color c = p.sr.color;
            c.a = p.life / p.max;
            p.sr.color = c;
        }
    }

    private Particle Create()
    {
        var go = new GameObject("Pixel");
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = pixel;
        sr.sortingLayerName = sortingLayer;
        sr.sortingOrder = sortingOrder;
        go.SetActive(false);
        return new Particle { sr = sr };
    }
}
