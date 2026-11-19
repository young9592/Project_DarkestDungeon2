using System;
using System.Collections;
using UnityEngine;
using static CIcon;
using static UnityEngine.Rendering.DebugUI;

public enum EDamageType
{
    Damage,
    Damage_Critical,
    Miss,

    Heal,
    Heal_Critical,

    Stress,
    Stress_Heal,
}

// 토큰 획득 혹은 소모
public enum ETokenType
{
    Add,                // 토큰 획득 메시지
    Consume,            // 토큰 소모 메세지
}

// 도트 획득 혹은 데미지 받음
public enum EDotType
{
    Add,                // 도트 추가
    Damage,             // 도트 데미지
    Resist,             // 저항
}

public enum EEntityState
{
    DeathBlow,          // 죽음의 일격 [ 공통 ]
    DeathDoor,          // 죽음의 문턱 [ 플레이어 ]
    DeathArmorBreak,    // 죽음 저항 토큰 파괴 [ 적 ]
}

public enum EOtherType
{
    Cure,               // 디버프 치유됨
}

public class CCombatFloatingMessageManager : MonoBehaviour
{
    #region Inspector
    [Header("Draw Canvas")]
    [SerializeField] private Canvas _canvas;

    [Header("Floating Text Prefab")]
    [SerializeField] private GameObject _floatingTextRoot;
    [SerializeField] private GameObject _floatingTextPrefab;

    [Header("Icon Prefab")]
    [SerializeField] private GameObject _iconPrefabRoot;
    [SerializeField] private GameObject _deathStampPrefab;
    [SerializeField] private GameObject _stressPrefab;
    [SerializeField] private GameObject _stressHealPrefab;
    #endregion

    #region Field
    private static CCombatFloatingMessageManager _instance;
    #endregion
    private void Awake()
    {
        _instance = this;
    }


    // 데미지, 힐, 스트레스 출력
    public static void ShowDamage(GameObject target, EDamageType type, int value, Vector3 offset = default, float delay = 0f)
    {
        if (_instance == null)
        {
            return;
        }

        CEntity entity = target.GetComponent<CEntity>();

        switch (type)
        {
            case EDamageType.Damage:
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Damage, value.ToString(), offset, delay);
                break;
            case EDamageType.Damage_Critical:
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Damage_Critical, value.ToString(), offset, delay);
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Damage_Critical_Text, "치명타!", offset, delay);
                break;
            case EDamageType.Miss:
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Miss, "빗나감!", offset, delay);
                break;
            case EDamageType.Heal:
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Heal, value.ToString(), offset, delay);
                break;
            case EDamageType.Heal_Critical:
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Heal, value.ToString(), offset, delay);
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Heal_Critical_Text, value.ToString(), offset, delay);
                break;
            case EDamageType.Stress:
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Stress, value.ToString(), offset, delay);
                _instance.DrawIcon(target, CIcon.IconType.Stress, delay);
                break;
            case EDamageType.Stress_Heal:
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Stress, value.ToString(), offset, delay);
                _instance.DrawIcon(target, CIcon.IconType.StressHeal, delay);
                break;
            default:
                break;
        }
    }

    // 토큰 추가 및 사용
    public static void ShowToken(GameObject target, ETokenType type, EEffectType effectType, Vector3 offset = default, float delay = 0f)
    {
        if (_instance == null)
        {
            return;
        }

        switch (effectType)
        {
            case EEffectType.Crit:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.CriticalToken, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Riposte:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Riposte, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Dodge:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Dodge, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.DodgePlus:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Dodge_Plus, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Block:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Block, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.BlockPlus:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Block_Plus, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Guard:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Guard, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Strength:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Strength, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Speed:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Speed, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Stealth:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Stealth, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Daze:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Daze, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Daze, "현기증!", offset, delay);
                        break;
                }
                break;
            case EEffectType.Stun:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Stun, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Stun, "기절!", offset, delay);
                        break;
                }
                break;
            case EEffectType.Blind:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Blind, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Weak:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Weak, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Vulnerable:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Vulnerable, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Taunt:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Taunt, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Immobilize:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Immobilize, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.Combo:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Combo, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Combo_Use, "연계!", offset, delay);
                        break;
                }
                break;
            case EEffectType.DenialOfArms:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.DenialOfArms, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.DenialOfReach:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.DenialOfReach, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.DenialOfFortitude:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.DenialOfFortitude, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
            case EEffectType.DenialOfReason:
                switch (type)
                {
                    case ETokenType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.DenialOfReason, "추가", offset, delay);
                        break;
                    case ETokenType.Consume:
                        break;
                }
                break;
        }

    }

    // 도트 추가 및 데미지 출력 [ 출혈 / 중독 / 화상 / 재생 / 공포 ]
    public static void ShowDot(GameObject target, EDotType type, EEffectType effectType, int value, Vector3 offset = default, float delay = 0f)
    {
        if (_instance == null)
        {
            return;
        }

        switch (effectType)
        {

            case EEffectType.Bleed:
                switch (type)
                {
                    case EDotType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Bleed, "출혈", offset, delay);
                        break;
                    case EDotType.Damage:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Bleed, value.ToString(), offset, delay);
                        break;
                    case EDotType.Resist:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Bleed, "저항", offset, delay);
                        break;
                }
                break;
            case EEffectType.Blight:
                switch (type)
                {
                    case EDotType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Blight, "중독", offset, delay);
                        break;
                    case EDotType.Damage:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Blight, value.ToString(), offset, delay);
                        break;
                    case EDotType.Resist:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Blight, "저항", offset, delay);
                        break;
                }
                break;
            case EEffectType.Burn:
                switch (type)
                {
                    case EDotType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Burn, "화상", offset, delay);
                        break;
                    case EDotType.Damage:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Burn, value.ToString(), offset, delay);
                        break;
                    case EDotType.Resist:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Burn, "저항", offset, delay);
                        break;
                }
                break;
            case EEffectType.Horror:
                switch (type)
                {
                    case EDotType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Horror_Back_Text, "공포", offset, delay);
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Horror, "공포", offset, delay);
                        break;
                    case EDotType.Damage:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Stress, value.ToString(), offset, delay);
                        _instance.DrawIcon(target, CIcon.IconType.Stress, delay);
                        break;
                }
                break;
            case EEffectType.Regen:
                switch (type)
                {
                    case EDotType.Add:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Regen, "회복", offset, delay);
                        break;
                    case EDotType.Damage:
                        _instance.DrawText(target, CCombatFloatingText.EMessageType.Heal, value.ToString(), offset, delay);
                        break;
                }
                break;
        }
    }
    // 죽음 관련 3종 출력
    public static void ShowDeath(GameObject target, EEntityState entityState, Vector3 offset = default, float delay = 0f)
    {
        if (_instance == null)
        {
            return;
        }

        switch (entityState)
        {
            case EEntityState.DeathBlow:
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Death, "죽음의 일격!", offset, delay);
                _instance.DrawIcon(target, CIcon.IconType.DeathBlow, delay);
                break;
            case EEntityState.DeathDoor:
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Death_Door, "죽음의 문턱!", offset, delay);
                break;
            case EEntityState.DeathArmorBreak:
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Death_Armor_Break, "죽음 저항 파괴!", offset, delay);
                break;
        }
    }
    // 기타 상황들 출력 [ 치료 ]
    public static void ShowOther(GameObject target, EOtherType otherType, Vector3 offset = default, float delay = 0f)
    {
        if (_instance == null)
        {
            return;
        }

        switch (otherType)
        {
            case EOtherType.Cure:
                _instance.DrawText(target, CCombatFloatingText.EMessageType.Cure, "치료!", offset, delay);
                break;
        }
    }
    // 일반 텍스트 출력
    public static void ShowDefault(GameObject target, string text, Vector3 offset = default, float delay = 0f)
    {
        if (_instance == null)
        {
            return;
        }

        _instance.DrawText(target, CCombatFloatingText.EMessageType.None, text, offset, delay);
    }

    private void DrawText(GameObject target, CCombatFloatingText.EMessageType messageType, string text, Vector3 offset = default, float delay = 0f)
    {
        GameObject textPrefab = Instantiate(_floatingTextPrefab, _floatingTextRoot.transform);
        CCombatFloatingText floatText = textPrefab.GetComponent<CCombatFloatingText>();

        if (CCombatCameraManager.MessageWaiting && messageType == CCombatFloatingText.EMessageType.Stress)
        {
            textPrefab.SetActive(false);
            StartCoroutine(CoActionDelayShow(() => floatText.SetText(_canvas, target, messageType, text, offset)));
        }
        else if (delay == 0f)
        {
            floatText.SetText(_canvas, target, messageType, text, offset);
        }
        else
        {
            textPrefab.SetActive(false);
            StartCoroutine(CoDelayShow(() => floatText.SetText(_canvas, target, messageType, text, offset), delay));
        }
    }

    private void DrawIcon(GameObject target, CIcon.IconType iconType, float delay = 0f)
    {
        GameObject iconPrefab = default;

        switch (iconType)
        {
            case CIcon.IconType.DeathBlow:
                iconPrefab = _deathStampPrefab;
                break;
            case CIcon.IconType.Stress:
                iconPrefab = _stressPrefab;
                break;
            case CIcon.IconType.StressHeal:
                iconPrefab = _stressHealPrefab;
                break;
        }

        GameObject showPrefab = Instantiate(iconPrefab, _iconPrefabRoot.transform);
        CIcon icon = showPrefab.GetComponent<CIcon>();

        if (CCombatCameraManager.MessageWaiting && iconType != CIcon.IconType.DeathBlow)
        {
            showPrefab.SetActive(false);
            StartCoroutine(CoActionDelayShow(() => icon.SetIcon(_canvas, target)));
        }
        else if (delay == 0f)
        {
            icon.SetIcon(_canvas, target);
        }
        else
        {
            showPrefab.SetActive(false);
            StartCoroutine(CoDelayShow(() => icon.SetIcon(_canvas, target), delay));
        }
    }





    // 삭제예정

    // 상태 메세지 출력
    public void ShowMessage(GameObject target, CCombatFloatingText.EMessageType messageType, string text, Vector3 offset = default, float delay = 0f)
    {
        GameObject textPrefab = Instantiate(_floatingTextPrefab, _floatingTextRoot.transform);
        CCombatFloatingText floatText = textPrefab.GetComponent<CCombatFloatingText>();

        if (delay == 0f)
        {
            floatText.SetText(_canvas, target, messageType, text, offset);
        }
        else
        {
            StartCoroutine(CoDelayShow(() => floatText.SetText(_canvas, target, messageType, text, offset), delay));
        }
    }

    public GameObject ShowIcon(GameObject target, CIcon.IconType iconType, float delay = 0f)
    {
        GameObject iconPrefab = default;

        switch (iconType)
        {
            case CIcon.IconType.DeathBlow:
                iconPrefab = _deathStampPrefab;
                break;
            case CIcon.IconType.Stress:
                iconPrefab = _stressPrefab;
                break;
            case CIcon.IconType.StressHeal:
                iconPrefab = _stressHealPrefab;
                break;
        }


        GameObject showPrefab = Instantiate(iconPrefab, _iconPrefabRoot.transform);
        CIcon icon = showPrefab.GetComponent<CIcon>();

        if (delay == 0f)
        {
            icon.SetIcon(_canvas, target);
        }
        else
        {
            StartCoroutine(CoDelayShow(() => icon.SetIcon(_canvas, target), delay));
        }

        return showPrefab;
    }
    // 메세지를 일정시간뒤에 출력하는 코루틴
    private IEnumerator CoDelayShow(Action action, float delay)
    {

        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
    private IEnumerator CoActionDelayShow(Action action)
    {
        yield return new WaitUntil(() => !CCombatCameraManager.MessageWaiting );
        action.Invoke();
    }
}
