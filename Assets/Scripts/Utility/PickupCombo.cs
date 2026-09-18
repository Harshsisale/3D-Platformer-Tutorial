using System;

public class PickupCombo
{
    public float Window { get; }
    public int MaximumMultiplier { get; }
    public int ChainCount { get; private set; }
    public int Multiplier => Math.Max(1, Math.Min(ChainCount, MaximumMultiplier));
    public float RemainingTime { get; private set; }

    private float expiresAt;

    public PickupCombo(float window, int maximumMultiplier)
    {
        Window = Math.Max(0.1f, window);
        MaximumMultiplier = Math.Max(1, maximumMultiplier);
    }

    public int Collect(int baseScore, float currentTime)
    {
        Tick(currentTime);
        if (baseScore <= 0)
        {
            return baseScore;
        }

        ChainCount++;
        expiresAt = currentTime + Window;
        RemainingTime = Window;
        return baseScore * Multiplier;
    }

    public void Tick(float currentTime)
    {
        if (ChainCount == 0)
        {
            return;
        }

        RemainingTime = Math.Max(0f, expiresAt - currentTime);
        if (RemainingTime <= 0f)
        {
            Reset();
        }
    }

    public void Reset()
    {
        ChainCount = 0;
        RemainingTime = 0f;
        expiresAt = 0f;
    }
}
