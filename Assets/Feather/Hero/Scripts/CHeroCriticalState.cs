using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CHeroCriticalState : MonoBehaviour
{
    private class Modifier
    {
        public int Amount;
        public int RemainingTurns;
    }

    private readonly List<Modifier> _modifiers = new List<Modifier>();

    public int ChanceModifier
    {
        get
        {
            int total = 0;
            for (int i = 0; i < _modifiers.Count; i++) total += _modifiers[i].Amount;
            return total;
        }
    }

    public static int GetChance(CEntity actor, int baseChance)
    {
        CHeroCriticalState state = actor != null ? actor.GetComponent<CHeroCriticalState>() : null;
        return Mathf.Clamp(baseChance + (state != null ? state.ChanceModifier : 0), 0, 100);
    }

    public static bool Roll(CEntity actor, int baseChance)
    {
        int chance = GetChance(actor, baseChance);
        return chance >= 100 || (chance > 0 && Random.Range(1, 101) <= chance);
    }

    public void AddModifier(int amount, int turns, bool includesCurrentTurn)
    {
        if (amount == 0 || turns <= 0) return;
        _modifiers.Add(new Modifier
        {
            Amount = amount,
            RemainingTurns = includesCurrentTurn ? turns - 1 : turns
        });
    }

    public void BeginTurn()
    {
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            if (_modifiers[i].RemainingTurns <= 0) _modifiers.RemoveAt(i);
            else _modifiers[i].RemainingTurns--;
        }
    }

    public void ClearModifiers()
    {
        _modifiers.Clear();
    }

    private void OnDisable()
    {
        ClearModifiers();
    }
}
