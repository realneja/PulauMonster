// Pulau Monster · P7 Polish, Build & Demo · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using System.Collections;
using UnityEngine;

// Efek sprite beberapa frame (tebasan 4 frame) yang memanggil kembali pool saat selesai.
public class SpriteVfx : MonoBehaviour
{
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 16f;

    public void Play(Action<SpriteVfx> onDone)
    {
        gameObject.SetActive(true);
        StartCoroutine(Animate(onDone));
    }

    private IEnumerator Animate(Action<SpriteVfx> onDone)
    {
        float wait = 1f / Mathf.Max(1f, fps);
        for (int i = 0; i < frames.Length; i++)
        {
            body.sprite = frames[i];
            yield return new WaitForSeconds(wait);
        }
        onDone?.Invoke(this);
    }

#if UNITY_EDITOR
    public void EditorSet(SpriteRenderer renderer, Sprite[] sprites)
    {
        body = renderer;
        frames = sprites;
    }
#endif
}
