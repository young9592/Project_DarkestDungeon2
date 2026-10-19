using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTempEntity : MonoBehaviour
{
    public enum EntityType
    {
        Player,
        Enemy
    }


    #region Inspector
    [Header("Entity Inspector")]
    [SerializeField] private EntityType _type;
    [SerializeField] private int _speed;
    [SerializeField] private Sprite _portraitImage;
    [SerializeField] private CTokenManager _tokenManager;

    [Header("Test State")]
    // 행동권 : 1회 기본 / 2회 ~ 3회 보스
    [SerializeField] private int _actions = 1;
    [SerializeField] private bool _haveFreeAction = false;
    #endregion

    #region Field
    private MeshRenderer _mesh;
    private Color _initColor;
    private static Color _actionColor = Color.red;
    private static Color _dazeColor = Color.cyan;
    private static Color _stunColor = Color.yellow;
    private static Color _freeActionColor = Color.magenta;
    #endregion

    #region Property
    public CTokenManager TokenManager => _tokenManager;
    public Sprite PortraitImage => _portraitImage;
    public EntityType Type => _type;
    public int Speed => _speed;
    public float Actions => _actions;
    public bool HaveFreeAction => _haveFreeAction;
    #endregion

    private void Awake()
    {
        _mesh = GetComponent<MeshRenderer>();
        _initColor = _mesh.material.color;

        _tokenManager = new CTokenManager();
    }

    // 행동
    public bool Action()
    {
        if (_type == EntityType.Player)
        {
            if (!Input.GetKeyDown(KeyCode.A))
            {
                return false;
            }
        }

        // 행동 성공
        _mesh.material.color = _actionColor;
        StartCoroutine(CoActionColorDelay(3f));
        return true;
    }

    private IEnumerator CoActionColorDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _mesh.material.color = _initColor;
    }


    // 임시 상태이상 걸기
    public void TempInitState()
    {
        int rand = Random.Range(0, 100);

        if(rand < 40)
        {
            // none
        }
        else if(rand < 80)
        {
            _tokenManager.AddToken(EEffectType.Daze);
            _mesh.material.color = _dazeColor;
        }
        else if(rand < 100)
        {
            _tokenManager.AddToken(EEffectType.Stun);
            _mesh.material.color = _stunColor;
        }
    }

    // FreeAction은 내가 행동을 한 후에 프리액션이 있는지 체크하는거 같습니다.
    public void TempAddFreeAction()
    {
        _haveFreeAction = true;
    }
    public void UseFreeAction()
    {
        _haveFreeAction = false;
    }
}
