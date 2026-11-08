// Pulau Monster · P5 Battle & UI (versi demo) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;
using UnityEngine.UI;

// HUD dunia: potret partner, bar HP + angka, bar EXP, Ramuan xN, panel nama area. Disembunyikan selama battle.
public class PartnerHud : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject partnerPanel;
    [SerializeField] private Image portrait;
    [SerializeField] private Text nameText;
    [SerializeField] private Image hpFill;
    [SerializeField] private Text hpText;
    [SerializeField] private Image expFill;
    [SerializeField] private Text potionsText;
    [SerializeField] private Text areaText;

    private PlayerParty party;
    private BattleSession session;
    private MonsterInstance shown;

    public void Construct(PlayerParty playerParty, BattleSession battleSession)
    {
        party = playerParty;
        session = battleSession;
        party.PartnerChanged += OnPartnerChanged;
        party.PotionsChanged += Redraw;
        session.Started += () => root.SetActive(false);
        session.Ended += (_, __) => root.SetActive(true);
        OnPartnerChanged();
        root.SetActive(true);
    }

    void OnDestroy()
    {
        if (party == null) return;
        party.PartnerChanged -= OnPartnerChanged;
        party.PotionsChanged -= Redraw;
        if (shown != null) shown.Changed -= Redraw;
    }

    public void SetArea(string areaName)
    {
        if (areaText != null) areaText.text = areaName;
    }

    private void OnPartnerChanged()
    {
        if (shown != null) shown.Changed -= Redraw;
        shown = party.Partner;
        if (shown != null) shown.Changed += Redraw;
        Redraw();
    }

    private void Redraw()
    {
        bool has = party != null && party.Partner != null;
        if (partnerPanel != null) partnerPanel.SetActive(has);
        if (potionsText != null && party != null) potionsText.text = $"Ramuan x{party.Potions}";
        if (!has) return;
        MonsterInstance m = party.Partner;
        if (portrait != null) portrait.sprite = m.Data.IdleFrame(0);
        if (nameText != null) nameText.text = $"{m.Data.DisplayName}  Lv {m.Level}";
        float fill = m.MaxHP > 0 ? (float)m.CurrentHP / m.MaxHP : 0f;
        if (hpFill != null) { hpFill.fillAmount = fill; hpFill.color = HpColors.For(fill); }
        if (hpText != null) hpText.text = $"{m.CurrentHP}/{m.MaxHP}";
        if (expFill != null) expFill.fillAmount = m.ExpToNext > 0 ? (float)m.Exp / m.ExpToNext : 0f;
    }

    void Update()
    {
        if (portrait != null && party != null && party.Partner != null)
            portrait.sprite = party.Partner.Data.IdleFrame(Mathf.FloorToInt(Time.time / 0.35f) % 2);
    }
}
