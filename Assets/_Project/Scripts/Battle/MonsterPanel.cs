// Pulau Monster · P5 Battle & UI · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Panel satu monster di battle: nama, Lv, elemen, bar HP (+ angka dan EXP untuk partner) dan sprite beranimasi tanpa Animator.
public class MonsterPanel : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text elementText;
    [SerializeField] private Text hpText;
    [SerializeField] private Image hpFill;
    [SerializeField] private Image expFill;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private bool faceRight;
    [SerializeField] private Text starText; // tugas kelompok: ★ saat jurus sangat efektif

    private MonsterInstance shown;
    private MonsterData shownData;
    private float hpShow;
    private float lungeStart = -10f;
    private bool hurting, evolving, fainted;
    private Vector3 home;

    public MonsterInstance Shown => shown;
    public Transform Body => body != null ? body.transform : transform;

    public void ShowStar() => StartCoroutine(StarRoutine());

    private IEnumerator StarRoutine()
    {
        if (starText == null) yield break;
        starText.enabled = true;
        for (float t = 0f; t < 1.2f; t += Time.deltaTime)
        {
            starText.color = new Color(1f, 0.78f, 0.1f, Mathf.PingPong(t * 4f, 1f) * 0.6f + 0.4f);
            yield return null;
        }
        starText.enabled = false;
    }

    void Awake()
    {
        if (body != null) home = body.transform.localPosition;
        if (starText != null) starText.enabled = false;
    }

    // displayAs: nama & sprite lama tetap tampil sampai animasi evolusi selesai.
    public void Show(MonsterInstance m, bool snapHp = true, MonsterData displayAs = null)
    {
        shown = m;
        shownData = displayAs != null ? displayAs : m.Data;
        if (snapHp) hpShow = m.CurrentHP;
        fainted = false;
        Redraw();
        if (body != null)
        {
            body.enabled = true;
            body.color = Color.white;
            body.transform.localPosition = home;
            body.flipX = !faceRight;
            ApplyScale(shownData);
        }
    }

    private void Redraw()
    {
        if (shown == null) return;
        bool boss = shown.Level >= 9 && shownData.EvolvesTo == null && shownData.Id == "MON_012";
        if (nameText != null) nameText.text = (boss ? "★ " : "") + shownData.DisplayName;
        if (levelText != null) levelText.text = $"Lv {shown.Level}";
        if (elementText != null)
        {
            elementText.text = shownData.Element.ToString();
            elementText.color = UiTheme.ElementColor(shownData.Element);
        }
        if (expFill != null) expFill.fillAmount = shown.ExpToNext > 0 ? (float)shown.Exp / shown.ExpToNext : 0f;
    }

    private void ApplyScale(MonsterData data)
    {
        float w = data.Frames != null && data.Frames.Length > 0 ? data.Frames[0].rect.width : 16f;
        float s = w <= 16f ? 3f : w <= 24f ? 2.4f : 2f; // semua tampil ±48 piksel demo
        body.transform.localScale = new Vector3(s, s, 1f);
    }

    void Update()
    {
        if (shown == null || body == null) return;
        if (!hurting && !evolving && !fainted)
            body.sprite = shownData.IdleFrame(Mathf.FloorToInt(Time.time / 0.35f) % 2); // idle 2 frame

        float k = 1f - Mathf.Pow(0.85f, Time.deltaTime * 60f); // HP mendekat 15% per frame
        hpShow = Mathf.Lerp(hpShow, shown.CurrentHP, k);
        if (shown.CurrentHP <= 0 && hpShow < 0.5f) hpShow = 0f;
        float fill = shown.CurrentHP <= 0 ? 0f : hpShow / shown.MaxHP;
        if (hpFill != null)
        {
            hpFill.fillAmount = fill;
            hpFill.color = HpColors.For(fill);
        }
        if (hpText != null) hpText.text = $"{Mathf.RoundToInt(hpShow)}/{shown.MaxHP}";
        if (levelText != null) levelText.text = $"Lv {shown.Level}";
        if (expFill != null) expFill.fillAmount = shown.ExpToNext > 0 ? (float)shown.Exp / shown.ExpToNext : 0f;

        if (!fainted && !evolving)
        {
            float p = (Time.time - lungeStart) / 0.26f; // menerjang 14 px
            float dx = p < 1f ? Mathf.Sin(p * Mathf.PI) * 0.875f * (faceRight ? 1 : -1) : 0f;
            body.transform.localPosition = home + new Vector3(dx, 0f, 0f);
        }
    }

    public void PlayLunge() => lungeStart = Time.time;

    public void PlayHurt()
    {
        if (isActiveAndEnabled) StartCoroutine(Hurt());
    }

    public void PlayFaint()
    {
        if (isActiveAndEnabled) StartCoroutine(Faint());
    }

    public void PlayEvolve(MonsterData from, MonsterData to, float seconds)
    {
        if (isActiveAndEnabled) StartCoroutine(Evolve(from, to, seconds));
    }

    // Frame 3 (putih) berkedip 0,4 s.
    private IEnumerator Hurt()
    {
        hurting = true;
        Sprite hurt = shownData.IdleFrame(3);
        for (float t = 0f; t < 0.4f; t += Time.deltaTime)
        {
            bool on = Mathf.FloorToInt(t / 0.07f) % 2 == 0;
            body.sprite = on ? hurt : shownData.IdleFrame(0);
            body.color = on ? Color.white : new Color(1f, 1f, 1f, 0.6f);
            yield return null;
        }
        body.color = Color.white;
        hurting = false;
    }

    // Turun 20 px + memudar 0,5 s.
    private IEnumerator Faint()
    {
        fainted = true;
        Vector3 start = body.transform.localPosition;
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            float k = t / 0.5f;
            body.transform.localPosition = start + Vector3.down * (20f / 16f) * k;
            body.color = new Color(1f, 1f, 1f, 1f - k);
            yield return null;
        }
        body.enabled = false;
    }

    // Sprite lama ↔ baru makin cepat, sesekali putih; nama tetap lama sampai selesai.
    private IEnumerator Evolve(MonsterData from, MonsterData to, float seconds)
    {
        evolving = true;
        float t = 0f;
        float step = 0.3f;
        bool useNew = false;
        while (t < seconds)
        {
            useNew = !useNew;
            MonsterData d = useNew ? to : from;
            ApplyScale(d);
            body.sprite = d.IdleFrame(0);
            body.color = Random.value < 0.2f ? new Color(1f, 1f, 1f, 0.3f) : Color.white;
            yield return new WaitForSeconds(step);
            t += step;
            step = Mathf.Max(0.05f, step * 0.8f);
        }
        shownData = to;
        ApplyScale(to);
        body.color = Color.white;
        evolving = false;
        Redraw();
    }
}
