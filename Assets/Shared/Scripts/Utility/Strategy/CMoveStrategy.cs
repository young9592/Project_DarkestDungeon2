using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 대상 또는 시전자의 열을 이동시키거나 섞음
/// </summary>
[CreateAssetMenu(menuName = "Strategy/Effect/MoveStrategy", fileName = "MoveStrategy_")]
public class CMoveStrategy : CEffectStrategy
{
    public override void Apply(CEffectContext context, CEffectSO effectData)
    {
        if (context == null || context.Target == null) return;

        if (!context.IsHit && context.Target != context.Caster)
        {
            CPrint.Log($"[{effectData.Name}] : 공격이 빗나가 실패했습니다.");
            return;
        }

        if (context.Target != context.Caster && context.Target.EntityData != null)
        {
            int moveResist = context.Target.EntityData.Resistance.Move;
            int baseChance = effectData.BaseChance > 0 ? effectData.BaseChance : 120;
            int finalChance = Mathf.Clamp(baseChance - moveResist, 0, 100);

            if (Random.Range(1, 101) > finalChance)
            {
                CPrint.Log($"[{context.Target.name}] 강제 이동 저항 성공!");
                return;
            }
        }

        int currentRankIndex = RankToIndex(context.Target.CurrentRank);
        if (currentRankIndex == 0) return;

        int nextRankIndex = currentRankIndex;

        if (effectData.Damage == 0)
        {
            int offset = Random.Range(0, 2) == 0 ? -1 : 1;
            nextRankIndex = Mathf.Clamp(currentRankIndex + offset, 1, 4);
            if (nextRankIndex == currentRankIndex)
            {
                nextRankIndex = currentRankIndex == 1 ? 2 : currentRankIndex - 1;
            }
        }
        else
        {
            nextRankIndex = Mathf.Clamp(currentRankIndex + effectData.Damage, 1, 4);
        }

        ERank newRank = IndexToRank(nextRankIndex);
        bool moved = false;

        if (context.Target is CPlayer player)
        {
            moved = player.TrySetRank(newRank);
        }
        else if (context.Target is CEnemy enemy)
        {
            moved = enemy.TrySetRank(newRank);
        }

        if (moved)
        {
            CPrint.Log($"[열 이동] [{context.Target.name}] 위치 변경 : Rank {currentRankIndex} → Rank {nextRankIndex}");
        }
    }

    private int RankToIndex(ERank rank)
    {
        switch (rank)
        {
            case ERank.Rank1: return 1;
            case ERank.Rank2: return 2;
            case ERank.Rank3: return 3;
            case ERank.Rank4: return 4;
            default: return 0;
        }
    }

    private ERank IndexToRank(int index)
    {
        return (ERank)(1 << (Mathf.Clamp(index, 1, 4) - 1));
    }
}
