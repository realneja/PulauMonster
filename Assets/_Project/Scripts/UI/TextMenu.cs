// Pulau Monster · P5 Battle & UI (suara P6) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Menu teks ▶ yang bisa keyboard (panah/WASD + Enter), gamepad, klik dan hover. Dipakai battle, dialog, judul, jeda.
public class TextMenu : MonoBehaviour
{
    [SerializeField] private Text[] rows;
    [SerializeField] private int columns = 1;
    [SerializeField] private Color normal = new Color32(0xED, 0xE9, 0xF7, 255);
    [SerializeField] private Color selected = new Color32(0xFF, 0xC6, 0x1A, 255);
    [SerializeField] private Color disabled = new Color32(0x5C, 0x56, 0x80, 255);
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip confirmClip;
    [SerializeField] private AudioClip cancelClip;

    private string[] labels = new string[0];
    private bool[] usable = new bool[0];
    private int count;
    private bool acceptInput;

    public AudioService Sfx { get; set; }
    public int Selected { get; private set; }
    public bool IsOpen => count > 0 && gameObject.activeInHierarchy;
    public event Action<int> Picked;

    void Awake()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            int index = i; // salin ke variabel lokal sebelum lambda
            Button button = rows[i].GetComponent<Button>();
            if (button == null) button = rows[i].gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => Click(index));

            EventTrigger trigger = rows[i].GetComponent<EventTrigger>();
            if (trigger == null) trigger = rows[i].gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entry.callback.AddListener(_ => Hover(index));
            trigger.triggers.Add(entry);
        }
    }

    public void Open(string[] options, bool[] enabled = null, int start = 0)
    {
        labels = options;
        count = Mathf.Min(options.Length, rows.Length);
        usable = enabled ?? new bool[count];
        if (enabled == null) for (int i = 0; i < count; i++) usable[i] = true;
        for (int i = 0; i < rows.Length; i++) rows[i].gameObject.SetActive(i < count);
        Selected = Mathf.Clamp(start, 0, Mathf.Max(0, count - 1));
        if (count > 0 && !usable[Selected]) Selected = FirstUsable();
        gameObject.SetActive(true);
        acceptInput = false; // abaikan tombol pembuka
        Redraw();
    }

    public void Close()
    {
        count = 0;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (count == 0) return;
        if (!acceptInput) { acceptInput = true; return; }
        if (MenuInput.Down) Move(+columns);
        else if (MenuInput.Up) Move(-columns);
        else if (MenuInput.Right && columns > 1) Move(+1);
        else if (MenuInput.Left && columns > 1) Move(-1);
        if (MenuInput.Confirm) Pick(Selected);
    }

    private void Move(int delta)
    {
        if (count <= 1) return;
        int next = ((Selected + delta) % count + count) % count;
        for (int guard = 0; guard < count && !usable[next]; guard++)
            next = ((next + Mathf.Clamp(delta, -1, 1)) % count + count) % count;
        if (next == Selected) return;
        Selected = next;
        PlaySfx(moveClip);
        Redraw();
    }

    private void Hover(int index)
    {
        if (index >= count || index == Selected || !usable[index]) return;
        Selected = index;
        Redraw();
    }

    private void Click(int index)
    {
        if (index >= count || !acceptInput) return;
        Selected = index;
        Redraw();
        Pick(index);
    }

    private void Pick(int index)
    {
        if (index >= count) return;
        if (!usable[index])
        {
            PlaySfx(cancelClip);
            return;
        }
        PlaySfx(confirmClip);
        Picked?.Invoke(index);
    }

    private int FirstUsable()
    {
        for (int i = 0; i < count; i++) if (usable[i]) return i;
        return 0;
    }

    private void Redraw()
    {
        for (int i = 0; i < count; i++)
        {
            bool on = i == Selected;
            rows[i].text = (on ? "▶ " : "  ") + labels[i];
            rows[i].color = !usable[i] ? disabled : on ? selected : normal;
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (Sfx != null && clip != null) Sfx.PlaySfx(clip, 0f);
    }
}
