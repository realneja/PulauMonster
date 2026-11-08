// Pulau Monster · P7 Polish, Build & Demo · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;

// Game feel tanpa mengubah logika: tebasan, percikan warna elemen, guncangan kamera, hit-stop.
public class BattleFeel : MonoBehaviour
{
    [SerializeField] private Transform cameraRig;
    [SerializeField] private SpriteRenderer playerBody;
    [SerializeField] private SpriteRenderer enemyBody;
    [SerializeField] private VfxPool hitVfx;
    [SerializeField] private PixelBurst burst;

    private BattleSystem battle;
    private Vector3 cameraHome;

    public void Construct(BattleSystem battleSystem)
    {
        battle = battleSystem;
        if (cameraRig != null) cameraHome = cameraRig.localPosition;
        battle.Hit += OnHit;
        battle.Healed += OnHealed;
    }

    private void OnHit(MonsterInstance target, int damage)
    {
        SpriteRenderer body = target == battle.Player ? playerBody : enemyBody;
        if (body == null) return;
        if (hitVfx != null) hitVfx.Play(body.bounds.center);
        Color c = battle.LastHitElement.HasValue
            ? StarterGiver.ElementColor(battle.LastHitElement.Value)
            : Color.white;
        if (burst != null)
            burst.Play(body.bounds.center, c, battle.LastHitElement.HasValue ? 22 : 12, 90f, 0.5f);
        StartCoroutine(Shake(battle.LastHitCritical ? 0.35f : 0.2f));
        StartCoroutine(HitStop()); // timeScale 0,05 selama 0,06 s
    }

    private void OnHealed(MonsterInstance target)
    {
        if (burst != null && playerBody != null)
            burst.Play(playerBody.bounds.center, HpColors.High, 16, 50f, 0.6f);
    }

    private IEnumerator Shake(float seconds)
    {
        if (cameraRig == null) yield break;
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            float k = 1f - t / seconds;
            cameraRig.localPosition = cameraHome + (Vector3)(Random.insideUnitCircle * 0.12f * k);
            yield return null;
        }
        cameraRig.localPosition = cameraHome;
    }

    private IEnumerator HitStop()
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(0.06f);
        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        if (battle != null)
        {
            battle.Hit -= OnHit;
            battle.Healed -= OnHealed;
        }
        Time.timeScale = 1f;
    }
}
