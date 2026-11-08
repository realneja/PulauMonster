// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System;
using UnityEngine;
using UnityEngine.UI;

// Kotak dialog gaya demo: panel navy + label nama emas + ▼, atau kotak narasi biru di atas layar hitam.
public class DialogueView : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject root;
    [SerializeField] private Image box;
    [SerializeField] private Sprite boxSprite;       // UI_Panel_Navy
    [SerializeField] private Sprite narrationSprite; // UI_Panel_Narr
    [SerializeField] private Text body;
    [SerializeField] private RectTransform speakerTag;
    [SerializeField] private Text speaker;
    [SerializeField] private Text continueHint;
    [SerializeField] private TextMenu choices;
    [SerializeField] private Button clickCatcher;
    [SerializeField] private int normalOrder = 20;
    [SerializeField] private int narrationOrder = 110; // di atas Fader 100

    public event Action Clicked;
    public TextMenu Choices => choices;

    void Awake()
    {
        if (clickCatcher != null) clickCatcher.onClick.AddListener(() => Clicked?.Invoke());
        Hide();
    }

    public void Show(bool narration)
    {
        root.SetActive(true);
        if (box != null) box.sprite = narration ? narrationSprite : boxSprite;
        if (canvas != null) canvas.sortingOrder = narration ? narrationOrder : normalOrder;
        if (speakerTag != null) speakerTag.gameObject.SetActive(!narration);
        ShowHint(false);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
        HideChoices();
    }

    public void SetSpeaker(string name)
    {
        bool has = !string.IsNullOrEmpty(name);
        if (speakerTag != null)
        {
            speakerTag.gameObject.SetActive(has);
            if (has) speakerTag.sizeDelta = new Vector2(Mathf.Min(488f, name.Length * 20f + 64f), speakerTag.sizeDelta.y);
        }
        if (speaker != null) speaker.text = has ? name : "";
    }

    public void SetBody(string text)
    {
        if (body != null) body.text = text;
    }

    public void ShowHint(bool on)
    {
        if (continueHint != null) continueHint.gameObject.SetActive(on);
    }

    public void ShowChoices(string[] options)
    {
        if (choices == null) return;
        choices.Open(options);
    }

    public void HideChoices()
    {
        if (choices != null) choices.Close();
    }

    void Update()
    {
        if (continueHint != null && continueHint.gameObject.activeSelf)
            continueHint.enabled = Mathf.FloorToInt(Time.unscaledTime / 0.3f) % 2 == 0;
    }
}
