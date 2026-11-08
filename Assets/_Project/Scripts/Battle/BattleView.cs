// Pulau Monster · P5 Battle & UI · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Tampilan battle (View): pesan diketik 55 huruf/detik, menu aksi, tirai pembuka, kilat putih, cincin evolusi.
public class BattleView : MonoBehaviour
{
    [SerializeField] private Text messageText;
    [SerializeField] private Text hintText;
    [SerializeField] private TextMenu menu;
    [SerializeField] private MonsterPanel enemyPanel;
    [SerializeField] private MonsterPanel playerPanel;
    [SerializeField] private Image flash;
    [SerializeField] private RectTransform wipeTop;
    [SerializeField] private RectTransform wipeBottom;
    [SerializeField] private Image[] rings;
    [SerializeField] private float charsPerSecond = 55f;
    [SerializeField] private Text damageTemplate; // tugas kelompok: angka damage melayang
    [SerializeField] private Camera battleCamera;

    private Coroutine typing;
    private int lastPick;

    public MonsterPanel EnemyPanel => enemyPanel;
    public MonsterPanel PlayerPanel => playerPanel;
    public TextMenu Menu => menu;
    public event Action<int> ActionClicked;

    void Awake()
    {
        if (menu != null)
        {
            menu.Picked += OnPicked;
            menu.Close();
        }
        if (hintText != null) hintText.enabled = false;
        if (flash != null) SetAlpha(flash, 0f);
        if (rings != null) foreach (Image r in rings) if (r != null) r.enabled = false;
        if (damageTemplate != null) damageTemplate.gameObject.SetActive(false);
    }

    // Angka damage muncul di atas sprite yang terkena, naik 40 unit kanvas lalu memudar dalam 0,8 detik.
    public void ShowDamage(Transform target, int damage, bool critical)
    {
        if (damageTemplate == null || battleCamera == null) return;
        Text popup = Instantiate(damageTemplate, damageTemplate.transform.parent);
        popup.gameObject.SetActive(true);
        popup.text = critical ? damage + "!" : damage.ToString();
        popup.color = critical ? UiTheme.Gold : Color.white;
        Vector3 screen = battleCamera.WorldToScreenPoint(target.position + Vector3.up * 1.6f);
        StartCoroutine(FloatUp(popup, screen));
    }

    private IEnumerator FloatUp(Text popup, Vector3 screen)
    {
        for (float t = 0f; t < 0.8f; t += Time.deltaTime)
        {
            popup.transform.position = screen + Vector3.up * (t / 0.8f) * 40f * popup.canvas.scaleFactor;
            Color c = popup.color;
            c.a = 1f - Mathf.Clamp01((t - 0.4f) / 0.4f);
            popup.color = c;
            yield return null;
        }
        Destroy(popup.gameObject);
    }

    private void OnPicked(int index)
    {
        lastPick = index;
        menu.Close();
        ActionClicked?.Invoke(index);
    }

    public void ShowMessage(string text)
    {
        if (hintText != null) hintText.enabled = false;
        if (typing != null) StopCoroutine(typing);
        typing = StartCoroutine(Type(text));
    }

    public void ShowMenu(IReadOnlyList<string> labels, IReadOnlyList<bool> usable, string prompt, string hint)
    {
        if (typing != null) StopCoroutine(typing);
        messageText.text = prompt; // "Apa yang akan Pyrit lakukan?"
        if (hintText != null)
        {
            hintText.text = hint; // "Jurus elemen: Semburan Api"
            hintText.enabled = true;
        }
        int start = lastPick < usable.Count && usable[lastPick] ? lastPick : 0; // ingat pilihan terakhir
        menu.Open(new List<string>(labels).ToArray(), new List<bool>(usable).ToArray(), start);
    }

    public void HideMenu() => menu.Close();

    private IEnumerator Type(string text)
    {
        float perChar = 1f / charsPerSecond;
        int shown = 0;
        float timer = 0f;
        messageText.text = "";
        while (shown < text.Length)
        {
            timer += Time.deltaTime;
            while (timer >= perChar && shown < text.Length) { timer -= perChar; shown++; }
            messageText.text = text.Substring(0, shown);
            yield return null;
        }
        messageText.text = text;
        typing = null;
    }

    // Dua tirai menutup lalu membuka 0,48 s.
    public void PlayIntro()
    {
        if (wipeTop != null && wipeBottom != null) StartCoroutine(Intro());
    }

    private IEnumerator Intro()
    {
        float h = 360f;
        wipeTop.gameObject.SetActive(true);
        wipeBottom.gameObject.SetActive(true);
        for (float t = 0f; t < 0.48f; t += Time.deltaTime)
        {
            float k = 1f - t / 0.48f;
            wipeTop.sizeDelta = new Vector2(wipeTop.sizeDelta.x, h * k);
            wipeBottom.sizeDelta = new Vector2(wipeBottom.sizeDelta.x, h * k);
            yield return null;
        }
        wipeTop.gameObject.SetActive(false);
        wipeBottom.gameObject.SetActive(false);
    }

    // Kilat putih memudar 0,5 s.
    public void PlayFlash()
    {
        if (flash != null) StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            SetAlpha(flash, 1f - t / 0.5f);
            yield return null;
        }
        SetAlpha(flash, 0f);
    }

    // Tiga cincin emas melebar 125 px/s selama animasi evolusi.
    public void PlayEvolveRings(float seconds)
    {
        if (rings != null && rings.Length > 0) StartCoroutine(Rings(seconds));
    }

    private IEnumerator Rings(float seconds)
    {
        float[] start = new float[rings.Length];
        for (int i = 0; i < rings.Length; i++) start[i] = i * 0.5f;
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            for (int i = 0; i < rings.Length; i++)
            {
                Image r = rings[i];
                if (r == null) continue;
                float age = (t - start[i]) % 1.5f;
                bool on = t >= start[i];
                r.enabled = on;
                if (!on) continue;
                float size = 16f + age * 125f;
                r.rectTransform.sizeDelta = new Vector2(size, size) * UiTheme.PixelScale;
                SetAlpha(r, 1f - age / 1.5f);
            }
            yield return null;
        }
        foreach (Image r in rings) if (r != null) r.enabled = false;
    }

    private static void SetAlpha(Graphic g, float a)
    {
        Color c = g.color;
        c.a = a;
        g.color = c;
    }
}
