internal static class CHealthEffectMessagePresenter
{
    internal static void Show(CHealthEffectResult result)
    {
        if (result.Target == null || result.IsPeriodic) return;

        EDamageType type;
        if (result.IsMiss)
        {
            type = EDamageType.Miss;
        }
        else if (result.IsHealing)
        {
            if (result.AppliedAmount <= 0) return;
            type = result.IsCritical ? EDamageType.Heal_Critical : EDamageType.Heal;
        }
        else
        {
            type = result.IsCritical ? EDamageType.Damage_Critical : EDamageType.Damage;
        }

        CCombatFloatingMessageManager.ShowDamage(result.Target.gameObject, type, result.AppliedAmount);
    }
}
