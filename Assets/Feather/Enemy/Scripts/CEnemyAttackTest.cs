using UnityEngine;

public class CEnemyAttackTest : MonoBehaviour
{
    [Header("Temp_Lee Test")]
    [SerializeField] private CEnemyAttackAnimation _enemy;

    private string _lastRequest = "No request yet";
    private int _completedAttackCount;
    private int _completedHitCount;

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
        GUILayout.BeginArea(new Rect(20.0f, 20.0f, 320.0f, 225.0f), GUI.skin.box);
        GUILayout.Label("Enemy Animation Test");
        GUILayout.Label("State: " + GetStateLabel());

        if (GUILayout.Button("Request Attack", GUILayout.Height(36.0f)))
        {
            _lastRequest = _enemy.TryPlayAttack() ? "Attack accepted" : "Rejected: busy / unavailable";
        }

        if (GUILayout.Button("Request Hit", GUILayout.Height(36.0f)))
        {
            _lastRequest = _enemy.TryPlayHit() ? "Hit accepted" : "Rejected: busy / unavailable";
        }

        GUILayout.Label(_lastRequest);
        GUILayout.Label("Attack completions: " + _completedAttackCount);
        GUILayout.Label("Hit completions: " + _completedHitCount);
        GUILayout.EndArea();
    }

    private string GetStateLabel()
    {
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
