// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using System.Collections;
using UnityEngine;

// Dialog sebagai urutan coroutine: Say, Narrate, Choose. Pemain terkunci (InputGate) selama urutan berjalan.
public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private DialogueView view;
    [SerializeField] private float charsPerSecond = 55f;
    [SerializeField] private AudioClip blipClip; // SFX_Dialog_Blip, tiap 3 huruf

    private InputGate gate;
    private AudioService audio;
    private bool clicked;
    private int picked = -1;

    public bool IsRunning { get; private set; }

    public void Construct(GameServices services)
    {
        gate = services.Input;
        audio = services.Audio;
        view.Clicked += () => clicked = true;
        view.Choices.Picked += index => picked = index;
        view.Choices.Sfx = audio;
    }

    public void Run(IEnumerator sequence)
    {
        if (!IsRunning && sequence != null)
            StartCoroutine(RunRoutine(sequence));
    }

    private IEnumerator RunRoutine(IEnumerator sequence)
    {
        IsRunning = true;
        gate.Lock(); // pemain berhenti
        yield return null; // lewati tombol pembuka
        clicked = false;
        yield return sequence; // urutan milik NPC / cutscene
        view.Hide();
        yield return null; // cegah dialog terbuka lagi
        gate.Unlock();
        IsRunning = false;
    }

    public IEnumerator Say(string speaker, string text)
    {
        view.Show(false);
        yield return TypeLine(speaker, text, true);
    }

    public IEnumerator Narrate(string text) // kotak biru
    {
        view.Show(true);
        yield return TypeLine("", text, true);
    }

    public IEnumerator Choose(string speaker, string prompt, string[] options, Action<int> result)
    {
        view.Show(false);
        yield return TypeLine(speaker, prompt, false); // tanpa ▼
        picked = -1;
        view.ShowChoices(options);
        yield return new WaitUntil(() => picked >= 0);
        view.HideChoices();
        result?.Invoke(picked);
    }

    // DialogueData biasa untuk NPC tanpa cabang (Sari).
    public IEnumerator PlayData(DialogueData data, string fallbackSpeaker)
    {
        if (data == null)
        {
            yield return Say(fallbackSpeaker, "Halo!");
            yield break;
        }
        string who = string.IsNullOrEmpty(data.Speaker) ? fallbackSpeaker : data.Speaker;
        foreach (string line in data.Lines)
            yield return Say(who, line);
    }

    private IEnumerator TypeLine(string speaker, string text, bool waitAdvance)
    {
        view.SetSpeaker(speaker);
        view.ShowHint(false);
        clicked = false;
        float perChar = 1f / Mathf.Max(1f, charsPerSecond);
        int shown = 0;
        float timer = 0f;
        view.SetBody("");
        while (shown < text.Length)
        {
            timer += Time.unscaledDeltaTime;
            if (AdvancePressed())
            {
                shown = text.Length;
                break;
            }
            while (timer >= perChar && shown < text.Length)
            {
                timer -= perChar;
                shown++;
                if (audio != null && blipClip != null && shown % 3 == 0 && !char.IsWhiteSpace(text[shown - 1]))
                    audio.PlaySfx(blipClip, 0.08f);
            }
            view.SetBody(text.Substring(0, shown));
            yield return null;
        }
        view.SetBody(text);
        if (!waitAdvance) yield break;
        view.ShowHint(true);
        yield return null;
        yield return new WaitUntil(AdvancePressed);
        view.ShowHint(false);
    }

    private bool AdvancePressed()
    {
        bool pressed = clicked || MenuInput.Confirm; // klik di mana saja atau E/Spasi/Enter
        clicked = false;
        return pressed;
    }
}
