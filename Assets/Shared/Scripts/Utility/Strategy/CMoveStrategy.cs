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

        if (context.Target.TokenManager != null &&
            context.Target.TokenManager.ConsumeToken(EEffectType.Immobilize))
        {
            CPrint.Log($"[{context.Target.name}] 이동 불가 토큰 보유로 인해 위치 이동이 차단되었습니다.");
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

        int targetRankIndex = currentRankIndex;

        if (effectData.Damage == 0)
        {
            int offset = Random.Range(0, 2) == 0 ? -1 : 1;
            targetRankIndex = Mathf.Clamp(currentRankIndex + offset, 1, 4);
            if (targetRankIndex == currentRankIndex)
            {
                targetRankIndex = currentRankIndex == 1 ? 2 : currentRankIndex - 1;
            }
        }
        else
        {
            targetRankIndex = Mathf.Clamp(currentRankIndex + effectData.Damage, 1, 4);
        }

        if (targetRankIndex == currentRankIndex) return;

        int stepDirection = targetRankIndex > currentRankIndex ? 1 : -1;
        int steps = Mathf.Abs(targetRankIndex - currentRankIndex);

        CEntity[] allEntities = Object.FindObjectsByType<CEntity>(FindObjectsSortMode.None);

        for (int s = 0; s < steps; s++)
        {
            int nextStepIndex = currentRankIndex + stepDirection;
            ERank currentRankEnum = IndexToRank(currentRankIndex);
            ERank nextRankEnum = IndexToRank(nextStepIndex);

            CEntity occupant = FindOccupantAtRank(allEntities, context.Target, nextRankEnum);

            if (occupant != null)
            {
                if (occupant.TokenManager != null &&
                    occupant.TokenManager.ConsumeToken(EEffectType.Immobilize))
                {
                    CPrint.Log($"[열 이동 중단] {nextStepIndex}열에 있는 [{occupant.name}]가 이동 불가 상태라 밀어낼 수 없습니다.");
                    break;
                }

                TryChangeEntityRank(occupant, currentRankEnum);
                CPrint.Log($"[진형 연동] [{occupant.name}] 밀려남: Rank {nextStepIndex} → Rank {currentRankEnum}");
            }

            if (TryChangeEntityRank(context.Target, nextRankEnum))
            {
                CPrint.Log($"[열 이동] [{context.Target.name}] 위치 변경 : Rank {currentRankIndex} → Rank {nextStepIndex}");
                currentRankIndex = nextStepIndex;
            }
            else
            {
                break;
            }
        }
    }

    private CEntity FindOccupantAtRank(CEntity[] allEntity, CEntity movingEntity, ERank targetRank)
    {
        for (int i = 0; i < allEntity.Length; i++)
        {
            CEntity candidate = allEntity[i];
            if (candidate == null || candidate == movingEntity || candidate.IsDead) continue;
            if (!candidate.isActiveAndEnabled) continue;

            if (candidate.EntityType == movingEntity.EntityType && candidate.CurrentRank == targetRank)
            {
                return candidate;
            }
        }
        return null;
    }

    private bool TryChangeEntityRank(CEntity entity, ERank newRank)
    {
        if (entity is CPlayer player) return player.TrySetRank(newRank);
        if (entity is CEnemy enemy) return enemy.TrySetRank(newRank);
        return false;
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
