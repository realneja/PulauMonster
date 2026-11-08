// Pulau Monster · P7 Polish, Build & Demo · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Gua gelap: Global Light turun ke 0,15 saat Player masuk; cahaya pemain, kristal, dan boss ikut gelapnya gua.
[RequireComponent(typeof(Collider2D))]
public class LightZone : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Light2D playerLight;
    [SerializeField] private Light2D[] extraLights; // kristal + BossLight
    [SerializeField] private float insideIntensity = 0.15f;
    [SerializeField] private float fadeSpeed = 2f;

    private float target = 1f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) target = insideIntensity;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) target = 1f;
    }

    void Update()
    {
        if (globalLight == null) return;
        globalLight.intensity = Mathf.MoveTowards(globalLight.intensity, target, fadeSpeed * Time.deltaTime);
        float dark = 1f - globalLight.intensity;
        if (playerLight != null) playerLight.intensity = dark;
        if (extraLights == null) return;
        foreach (Light2D l in extraLights)
            if (l != null) l.intensity = dark;
    }
}
