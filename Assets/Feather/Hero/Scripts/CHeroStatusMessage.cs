using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CHeroStatusMessage : MonoBehaviour
{
    [Header("Status Messages")]
    [SerializeField] private CCombatFloatingMessageManager _messageManager;
    [SerializeField] private Vector2 _messageOffset = new Vector2(0.0f, 160.0f);
    [SerializeField, Min(0.05f)] private float _messageInterval = 0.35f;

    private CPlayer _hero;
    private readonly Queue<EEffectType> _pending = new Queue<EEffectType>();
    private Coroutine _routine;

    private void Awake()
    {
        _hero = GetComponent<CPlayer>();
    }

    public void Configure(CCombatFloatingMessageManager messageManager)
    {
        ClearPending();
        _messageManager = messageManager;
    }

    public void ShowAdded(EEffectType type)
    {
        if (!CanShow() || string.IsNullOrEmpty(GetLabel(type))) return;
        if (_messageManager == null) _messageManager = FindObjectOfType<CCombatFloatingMessageManager>();
        if (_messageManager == null || !_messageManager.isActiveAndEnabled || Camera.main == null) return;

        _pending.Enqueue(type);
        if (_routine == null) _routine = StartCoroutine(ShowPending());
    }

    private IEnumerator ShowPending()
    {
        yield return null;
        while (_pending.Count > 0 && CanShow())
        {
            EEffectType type = _pending.Dequeue();
            bool hasEffect = type == EEffectType.Bleed || type == EEffectType.Blight || type == EEffectType.Burn
                ? _hero.GetTotalDotDamageByType(type) > 0 : _hero.TokenManager.HasToken(type);
            if (!hasEffect || _messageManager == null || !_messageManager.isActiveAndEnabled || Camera.main == null) continue;

            _messageManager.ShowMessage(gameObject, GetMessageType(type), GetLabel(type) + " 추가", _messageOffset);
            yield return new WaitForSeconds(_messageInterval);
        }
        _pending.Clear();
        _routine = null;
    }

    private bool CanShow()
    {
        return isActiveAndEnabled && _hero != null && _hero.isActiveAndEnabled
            && !_hero.IsDead && !_hero.IsDeathPresented;
    }

    public void ClearPending()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        _pending.Clear();
    }

    private void OnDisable()
    {
        ClearPending();
    }

    private CCombatFloatingText.EMessageType GetMessageType(EEffectType type)
    {
        switch (type)
        {
            case EEffectType.Bleed: return CCombatFloatingText.EMessageType.Bleed;
            case EEffectType.Blight: return CCombatFloatingText.EMessageType.Blight;
            case EEffectType.Burn: return CCombatFloatingText.EMessageType.Burn;
            case EEffectType.Stun: return CCombatFloatingText.EMessageType.Stun;
            case EEffectType.Daze: return CCombatFloatingText.EMessageType.Daze;
            default: return CCombatFloatingText.EMessageType.None;
        }
    }

    private string GetLabel(EEffectType type)
    {
        switch (type)
        {
            case EEffectType.Bleed: return "출혈";
            case EEffectType.Blight: return "중독";
            case EEffectType.Burn: return "화상";
            case EEffectType.Crit: return "치명타";
            case EEffectType.Riposte: return "반격";
            case EEffectType.Dodge: return "회피";
            case EEffectType.DodgePlus: return "회피+";
            case EEffectType.Block: return "방어";
            case EEffectType.BlockPlus: return "방어+";
            case EEffectType.Guarded: return "보호";
            case EEffectType.Strength: return "힘";
            case EEffectType.Speed: return "속도";
            case EEffectType.Stealth: return "은신";
            case EEffectType.Daze: return "현기증";
            case EEffectType.Stun: return "기절";
            case EEffectType.Blind: return "실명";
            case EEffectType.Weak: return "약화";
            case EEffectType.Vulnerable: return "취약";
            case EEffectType.Taunt: return "도발";
            case EEffectType.Immobilize: return "이동 불가";
            case EEffectType.Combo: return "연계";
            default: return string.Empty;
        }
    }
}
