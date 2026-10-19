using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CTempEntity : MonoBehaviour
{
    public enum EEntityType
    {
        Player,
        Enemy
    }

    // 추가행동 악세사리 착용여부
    public enum EJewelryType
    {
        None,
        TurnStart,
        TurnEnd,
    }

    #region Inspector
    [Header("Entity Inspector")]
    [SerializeField] private EEntityType _type;
    [SerializeField] private int _currentHealth = 10;
    [SerializeField] private int _speed;
    [SerializeField] private Sprite _portraitImage;
    [SerializeField] private CTokenManager _tokenManager;
    // 행동권 : 1회 기본 / 2회 ~ 3회 보스
    [SerializeField] private int _actions = 1;

    [Header("ExtraTurn")]
    [SerializeField] private bool _hasExtraTurn = false;
    // 임시 악세사리 여부 체크
    [SerializeField] private EJewelryType _equipJewelry = EJewelryType.None;

    [Header("Test State")]
    // 임시 도트데미지 상태이상 처리
    [SerializeField] private List<CEffect> _dotEffectList;
    [SerializeField] private float _testDelay = 3.0f;
    #endregion

    #region Property
    public CTokenManager TokenManager => _tokenManager;
    public Sprite PortraitImage => _portraitImage;
    public EEntityType Type => _type;
    public int Speed => _speed;
    public float Actions => _actions;
    public bool HasExtraTurn { get; set; }
    public EJewelryType EquipJewelry => _equipJewelry;
    #endregion

    private void Awake()
    {
        _tokenManager = new CTokenManager();
    }

    // 행동
    public bool Action()
    {
        if (_type == EEntityType.Player)
        {
            if (!Keyboard.current.aKey.isPressed)
            {
                return false;
            }
        }

        // 행동 성공
        StartCoroutine(CoActionColorDelay(3f));
        return true;
    }

    private IEnumerator CoActionColorDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }


    // 임시 상태이상 걸기
    public void TempInitState()
    {
        int rand = Random.Range(0, 100);

        if(rand < 40)
        {
            // none
        }
        // TODO : DOT 공격 추가
        else if (rand < 90)
        {
            _tokenManager.AddToken(EEffectType.Daze);
        }
        else if (rand < 100)
        {
            _tokenManager.AddToken(EEffectType.Stun);
        }
    }

    // 아직 실제 상속과 연결할 순 없으니 임시로..
    public void Attack(int damage)
    {

    }

    public void TakeDamage(int finalDamage)
    {

    }

    public void ApplyStatusEffect(CEffect statusEffect)
    {

    }
}
