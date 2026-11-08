// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Pesan singkat di kotak emas (Canvas order 30), antre satu per satu, 2,2 detik.
public class ToastView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Text label;
    [SerializeField] private float seconds = 2.2f;

    private readonly Queue<string> queue = new Queue<string>();
    private Coroutine showing;

    public void Show(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        queue.Enqueue(text);
        if (showing == null) showing = StartCoroutine(Drain());
    }

    private IEnumerator Drain()
    {
        while (queue.Count > 0)
        {
            label.text = queue.Dequeue();
            root.SetActive(true);
            yield return new WaitForSecondsRealtime(seconds);
            root.SetActive(false);
            yield return new WaitForSecondsRealtime(0.15f);
        }
        showing = null;
    }
}
