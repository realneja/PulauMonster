// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam

// Hanya satu state aktif: Exit lama lalu Enter baru, selalu berurutan.
public class StateMachine
{
    public IState Current { get; private set; }

    public void ChangeTo(IState next)
    {
        if (next == Current) return;
        Current?.Exit();
        Current = next;
        Current?.Enter();
    }

    public void Tick(float deltaTime) => Current?.Tick(deltaTime);
}
