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

    public enum ActiveState
    {
        Success,
        Wait,
        Dead,
        Stun,
        Daze,
    }

    #region Inspector
    [SerializeField] private EntityType _type;
    [SerializeField] private int _speed;
    [SerializeField] private List<int> _randSpeed = new List<int>();
    [SerializeField] private Sprite _entityTurnImage;

    [Header("Test State")]
    [SerializeField] private bool _isStun = false;
    [SerializeField] private bool _isDaze = false;
    [SerializeField] private bool _isDead = false;
    [SerializeField] private static float _activeDelay = 3f;
    [SerializeField] private static float _stunDelay = 1f;
    [SerializeField] private static float _dazeDelay = 1f;
    // 행동권 : 0회 기본 / 1회 ~ 2회 보스
    [SerializeField] private int _extraTurn = 0;
    [SerializeField] private bool _isFreeTurn = false;
    #endregion

    #region Field
    private MeshRenderer _mesh;
    private Color _initColor;
    private Color _activeColor = Color.red;
    private Color _dazeColor = Color.cyan;
    private Color _stunColor = Color.yellow;
    private Color _freeTurnColor = Color.magenta;
    #endregion

    #region Property
    public int Speed(int i) => _speed + _randSpeed[i];
    public EntityType Type => _type;
    public Sprite EntityTurnIamge => _entityTurnImage;
    public float ExtraTurn => _extraTurn;

    public bool FreeTurn => _isFreeTurn;
    public static float ActiveDelay => _activeDelay;
    public static float StunDelay => _stunDelay;
    public static float DazeDelay => _dazeDelay;
    #endregion

    private void Awake()
    {
        _mesh = GetComponent<MeshRenderer>();
        _initColor = _mesh.material.color;
    }

    // 행동
    public ActiveState Active()
    {
        ActiveState state = ActiveState.Success;

        CheckState(ref state);

        if (state == ActiveState.Stun)
        {
            StartCoroutine(CoActiveDelay(_stunDelay));
            return state;
        }

        if(state == ActiveState.Daze)
        {
            StartCoroutine(CoActiveDelay(_dazeDelay));
            return state;
        }

        if (_type == EntityType.Player)
        {
            if (!Input.GetKeyDown(KeyCode.A))
            {
                state = ActiveState.Wait;
                return state;
            }
        }

        // 행동 성공
        _mesh.material.color = _activeColor;
        StartCoroutine(CoActiveDelay(_activeDelay));
        return state;
    }

    private IEnumerator CoActiveDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _mesh.material.color = _initColor;
    }

    // 일단 기절과 스턴만
    private void CheckState(ref ActiveState state)
    {
        if (_isStun)
        {
            _isStun = false;
            state = ActiveState.Stun;
            return;
        }

        if (_isDaze)
        {
            _isDaze = false;
            state = ActiveState.Daze;
            return;
        }
    }

    public void SetRoundRandomSpeed()
    {
        _randSpeed.Clear();

        for (int i = 0; i <= _extraTurn; i++)
        {
            _randSpeed.Add(Random.Range(0, 7));
        }
    }

    // 임시 상태이상 걸기
    public void TempInitState()
    {
        int rand = Random.Range(0, 100);

        if(rand < 40)
        {
            // none
        }
        else if(rand < 60)
        {
            _isFreeTurn = true;
            _mesh.material.color = _freeTurnColor;
        }
        else if(rand < 80)
        {
            _isDaze = true;
            _mesh.material.color = _dazeColor;
        }
        else if(rand < 100)
        {
            _isStun = true;
            _mesh.material.color = _stunColor;
        }
    }

    public void UseFreeAction()
    {
        _isFreeTurn = false;
    }
}
