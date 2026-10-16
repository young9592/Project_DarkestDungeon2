using UnityEngine;

public class CEnemyIntentTest : MonoBehaviour
{
    [Header("Temp_Lee Intent Test")]
    [SerializeField] private CEnemy _enemy;
    [SerializeField] private Camera _camera;
    [SerializeField] private Vector2 _labelOffset = new Vector2(0.0f, 18.0f);

    private CEnemyAttackAnimation _animation;
    private Renderer _modelRenderer;
    private CSkillDataSO _plannedSkill;
    private bool _isExecuting;
    private CEnemyAI _autoAction;

    public void BindAutoAction(CEnemyAI autoAction)
    {
        _autoAction = autoAction;
    }

    public CSkillDataSO PlannedSkill
    {
        get { return _plannedSkill; }
    }

    public bool IsExecuting
    {
        get { return _isExecuting; }
    }

    private void Awake()
    {
        if (_enemy == null) _enemy = GetComponent<CEnemy>();
        if (_camera == null) _camera = Camera.main;
        if (_enemy != null) _animation = _enemy.AttackAnimation;
        if (_enemy != null) _modelRenderer = _enemy.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private void OnEnable()
    {
        if (_animation != null) _animation.OnAttackCompleted += ClearIntent;
        if (_animation != null) _animation.OnAttackInterrupted += ClearIntent;
    }

    private void OnDisable()
    {
        if (_animation != null) _animation.OnAttackCompleted -= ClearIntent;
        if (_animation != null) _animation.OnAttackInterrupted -= ClearIntent;
        ClearIntent();
    }

    private void LateUpdate()
    {
        if (_plannedSkill == null) return;
        if (_enemy == null || !_enemy.isActiveAndEnabled || _animation == null)
        {
            ClearIntent();
            return;
        }

        // 중단된 공격은 완료 알림이 오지 않아서 따로 표시를 지웁니다.
        if (_isExecuting)
        {
            if (!_animation.IsAttacking) ClearIntent();
            return;
        }

        if (_animation.IsBusy || !IsSkillAvailable()) ClearIntent();
    }

    public bool TryPrepareIntent()
    {
        if (!isActiveAndEnabled || _enemy == null || !_enemy.isActiveAndEnabled
            || _animation == null || _animation.IsBusy || _isExecuting) return false;

        // 이미 표시한 스킬은 다시 추첨하지 않습니다.
        if (_plannedSkill != null && IsSkillAvailable()) return true;
        _plannedSkill = _enemy.SelectSkill();
        return _plannedSkill != null;
    }

    public bool TryExecuteIntent()
    {
        if (!isActiveAndEnabled || _isExecuting) return false;
        if (_enemy == null || !_enemy.isActiveAndEnabled || !IsSkillAvailable())
        {
            ClearIntent();
            return false;
        }

        // 표시한 스킬을 그대로 전달하고 재생이 거절되면 표시를 지웁니다.
        if (!_enemy.TryPlayAttack(_plannedSkill))
        {
            ClearIntent();
            return false;
        }

        _isExecuting = true;
        return true;
    }

    public void CancelIntent()
    {
        if (!_isExecuting) ClearIntent();
    }

    private bool IsSkillAvailable()
    {
        return _plannedSkill != null && _enemy.GetSkillCandidates().Contains(_plannedSkill)
            && CEnemySkillSelector.GetSelectionWeight(_enemy.EnemyData, _plannedSkill, _enemy.CurrentHp) > 0;
    }

    private void ClearIntent()
    {
        _plannedSkill = null;
        _isExecuting = false;
    }

    private void OnGUI()
    {
        CSkillDataSO skill = _autoAction != null && _autoAction.PlannedSkill != null ? _autoAction.PlannedSkill : _plannedSkill;
        CEntity target = _autoAction != null ? _autoAction.PlannedTarget : null;
        bool executing = _autoAction != null && _autoAction.PlannedSkill != null ? _autoAction.IsExecuting : _isExecuting;
        if (skill == null || _camera == null || _enemy == null || !_enemy.isActiveAndEnabled) return;
        Vector3 anchor = _enemy.transform.position;
        if (_modelRenderer != null)
        {
            Bounds bounds = _modelRenderer.bounds;
            anchor = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }
        Vector3 position = _camera.WorldToScreenPoint(anchor);
        if (position.z <= 0.0f) return;

        float height = target != null ? 66.0f : 46.0f;
        Rect area = new Rect(position.x - 120.0f + _labelOffset.x,
            Screen.height - position.y - height - _labelOffset.y, 240.0f, height);
        // 가장자리 열에서도 스킬 이름이 화면 밖으로 잘리지 않게 합니다.
        area.x = Mathf.Clamp(area.x, 0.0f, Mathf.Max(0.0f, Screen.width - area.width));
        area.y = Mathf.Clamp(area.y, 0.0f, Mathf.Max(0.0f, Screen.height - area.height));
        GUI.Box(area, GUIContent.none);
        Sprite icon = skill.SkillIcon;
        float left = area.x + 8.0f;
        if (icon != null)
        {
            Rect textureRect = icon.textureRect;
            Rect uv = new Rect(textureRect.x / icon.texture.width, textureRect.y / icon.texture.height,
                textureRect.width / icon.texture.width, textureRect.height / icon.texture.height);
            GUI.DrawTextureWithTexCoords(new Rect(left, area.y + 7.0f, 32.0f, 32.0f), icon.texture, uv);
            left += 40.0f;
        }
        GUI.Label(new Rect(left, area.y + 3.0f, area.xMax - left - 4.0f, 20.0f), executing ? "Playing" : "Next skill");
        GUI.Label(new Rect(left, area.y + 23.0f, area.xMax - left - 4.0f, 20.0f), skill.SkillName);
        if (target != null) GUI.Label(new Rect(left, area.y + 43.0f, area.xMax - left - 4.0f, 20.0f), "Target: " + target.name);
    }
}
