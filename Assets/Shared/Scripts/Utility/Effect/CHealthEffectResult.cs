public readonly struct CHealthEffectResult
{
    public CEntity Source { get; }
    public CEntity Target { get; }
    public int AppliedAmount { get; }
    public bool IsHealing { get; }
    public bool IsCritical { get; }
    public bool IsMiss { get; }
    public bool IsPeriodic { get; }

    public CHealthEffectResult(CEntity source, CEntity target, int appliedAmount, bool isHealing, bool isCritical, bool isMiss, bool isPeriodic = false)
    {
        Source = source;
        Target = target;
        AppliedAmount = appliedAmount;
        IsHealing = isHealing;
        IsCritical = isCritical;
        IsMiss = isMiss;
        IsPeriodic = isPeriodic;
    }
}
