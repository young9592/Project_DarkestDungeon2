using UnityEngine;

public class CEnemyAttackTest : MonoBehaviour
{
    [Header("Temp_Lee Test")]
    [SerializeField] private CEnemyAttackAnimation _enemy;

    [Header("Pair Test")]
    [SerializeField] private CEnemyAttackAnimation _pairedEnemy;
    [SerializeField] private string _panelTitle = "Enemy Animation Test";
    [SerializeField] private Vector2 _panelPosition = new Vector2(20.0f, 20.0f);

    private string _lastRequest = "No request yet";
    private int _completedAttackCount;
    private int _completedHitCount;
    private bool _showDetails = true;

    private void Awake()
    {
        if (_enemy != null) return;

        Debug.LogError($"{name}: Test Enemy reference is missing.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        if (_enemy == null) return;

        _enemy.OnAttackCompleted += HandleAttackCompleted;
        _enemy.OnHitCompleted += HandleHitCompleted;
    }

    private void OnDisable()
    {
        if (_enemy == null) return;

        _enemy.OnAttackCompleted -= HandleAttackCompleted;
        _enemy.OnHitCompleted -= HandleHitCompleted;
    }

    private void OnGUI()
    {
        float panelHeight = _showDetails ? 165.0f : 30.0f;
        GUILayout.BeginArea(new Rect(_panelPosition.x, _panelPosition.y, 320.0f, panelHeight), GUI.skin.box);
        if (GUILayout.Button(_panelTitle + (_showDetails ? " [-]" : " [+]"))) _showDetails = !_showDetails;
        if (!_showDetails)
        {
            GUILayout.EndArea();
            return;
        }
        if (_enemy == null)
        {
            GUILayout.Label("Test Enemy reference is missing.");
            GUILayout.EndArea();
            return;
        }
        GUILayout.Label("State: " + GetStateLabel());

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Attack", GUILayout.Height(28.0f)))
        {
            _lastRequest = _enemy.TryPlayAttack() ? "Attack accepted" : "Rejected: busy / unavailable";
        }

        if (GUILayout.Button("Hit", GUILayout.Height(28.0f)))
        {
            _lastRequest = _enemy.TryPlayHit() ? "Hit accepted" : "Rejected: busy / unavailable";
        }
        if (GUILayout.Button(_enemy.gameObject.activeSelf ? "Disable" : "Enable", GUILayout.Height(28.0f)))
        {
            ToggleEnemy();
        }
        GUILayout.EndHorizontal();

        GUILayout.Label(_lastRequest);
        GUILayout.Label("Completed - Attack: " + _completedAttackCount + " / Hit: " + _completedHitCount);
        if (_pairedEnemy != null && GUILayout.Button("Attack here + Hit other", GUILayout.Height(28.0f)))
        {
            RequestPair();
        }
        GUILayout.EndArea();
    }

    private void ToggleEnemy()
    {
        bool isActive = !_enemy.gameObject.activeSelf;
        _enemy.gameObject.SetActive(isActive);
        _lastRequest = isActive ? "Enemy enabled" : "Disabled: no completion";
    }

    private void RequestPair()
    {
        if (_enemy == null || _pairedEnemy == null) return;

        // 두 적의 연출만 동시에 요청하고 전투 판정은 하지 않습니다.
        bool attackAccepted = _enemy.TryPlayAttack();
        bool hitAccepted = _pairedEnemy.TryPlayHit();
        _lastRequest = "Attack: " + (attackAccepted ? "OK" : "rejected")
            + " / Other hit: " + (hitAccepted ? "OK" : "rejected");
    }

    private string GetStateLabel()
    {
        if (!_enemy.isActiveAndEnabled) return "Disabled";
        if (_enemy.IsAttacking) return "Attack";
        if (_enemy.IsHitReacting) return "Hit";
        return "Idle";
    }

    private void HandleAttackCompleted()
    {
        _completedAttackCount++;
        _lastRequest = "Attack completed";
    }

    private void HandleHitCompleted()
    {
        _completedHitCount++;
        _lastRequest = "Hit completed";
    }
}
