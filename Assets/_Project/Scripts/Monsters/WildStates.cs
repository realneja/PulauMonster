// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEngine;

// Wander: jalan acak atau diam di sekitar Home. Pindah ke Chase saat Player terlihat.
public class WanderState : IState
{
    private readonly WildMonster owner;
    private Vector2 goal;
    private float timer;

    public WanderState(WildMonster owner) => this.owner = owner;

    public void Enter()
    {
        owner.SetDesiredVelocity(Vector2.zero);
        timer = 0f;
    }

    public void Tick(float deltaTime)
    {
        if (owner.CanSeeTarget)
        {
            owner.EnterChase();
            return;
        }
        timer -= deltaTime;
        if (timer <= 0f)
        {
            timer = Random.Range(1f, 2.5f);
            goal = Random.value < 0.35f
                ? owner.Position
                : owner.Home + Random.insideUnitCircle * owner.WanderRadius;
        }
        Vector2 to = goal - owner.Position;
        owner.SetDesiredVelocity(to.magnitude < 0.1f ? Vector2.zero : to.normalized * owner.WanderSpeed);
    }

    public void Exit() => owner.SetDesiredVelocity(Vector2.zero);
}

// Chase: mendekati Player dengan tanda !. Kembali saat Player hilang atau terlalu jauh.
public class ChaseState : IState
{
    private readonly WildMonster owner;

    public ChaseState(WildMonster owner) => this.owner = owner;

    public void Enter() { }

    public void Tick(float deltaTime)
    {
        if (owner.LostTarget || owner.Target == null)
        {
            owner.EnterReturn();
            return;
        }
        Vector2 to = (Vector2)owner.Target.position - owner.Position;
        owner.SetDesiredVelocity(to.normalized * owner.ChaseSpeed);
    }

    public void Exit() => owner.SetDesiredVelocity(Vector2.zero);
}

// Return: pulang ke Home, lalu Wander lagi.
public class ReturnState : IState
{
    private readonly WildMonster owner;

    public ReturnState(WildMonster owner) => this.owner = owner;

    public void Enter() { }

    public void Tick(float deltaTime)
    {
        if (owner.CanSeeTarget)
        {
            owner.EnterChase();
            return;
        }
        Vector2 to = owner.Home - owner.Position;
        if (to.magnitude < 0.15f)
        {
            owner.EnterWander();
            return;
        }
        owner.SetDesiredVelocity(to.normalized * owner.WanderSpeed);
    }

    public void Exit() => owner.SetDesiredVelocity(Vector2.zero);
}

// Flee (tugas kelompok): menjauh dari Player bila partner terlalu kuat, lalu pulang saat sudah jauh.
public class FleeState : IState
{
    private readonly WildMonster owner;

    public FleeState(WildMonster owner) => this.owner = owner;

    public void Enter() { }

    public void Tick(float deltaTime)
    {
        if (owner.LostTarget || owner.Target == null)
        {
            owner.EnterReturn();
            return;
        }
        Vector2 away = owner.Position - (Vector2)owner.Target.position;
        owner.SetDesiredVelocity(away.normalized * owner.WanderSpeed * 1.5f);
    }

    public void Exit() => owner.SetDesiredVelocity(Vector2.zero);
}
