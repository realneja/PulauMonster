// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Layar hitam di Canvas order 100: memudar keluar sebelum pindah scene, memudar masuk sesudahnya.
public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Image black;
    [SerializeField] private float seconds = 0.3f;

    public bool IsBlack => black != null && black.color.a >= 0.99f;

    void Awake()
    {
        if (black != null) black.raycastTarget = false;
    }

    public IEnumerator FadeOut() => Fade(1f);
    public IEnumerator FadeIn() => Fade(0f);

    public void SetBlack(bool on)
    {
        if (black == null) return;
        Color c = black.color;
        c.a = on ? 1f : 0f;
        black.color = c;
    }

    private IEnumerator Fade(float target)
    {
        if (black == null) yield break;
        Color c = black.color;
        float start = c.a;
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            c.a = Mathf.Lerp(start, target, t / seconds);
            black.color = c;
            yield return null;
        }
        c.a = target;
        black.color = c;
    }
}
