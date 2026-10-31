using UnityEngine;
using System.Collections.Generic;

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
    private CEntity _highlightTarget;
    private Renderer[] _targetRenderers;
    private readonly List<CEntity> _emptyTargets = new List<CEntity>();
    public bool ShowLabel { get; set; } = true;
    public bool ShowTarget { get; set; } = true;

    public IReadOnlyList<CEntity> IntentTargets
    {
        get
        {
            if (!ShowTarget || !isActiveAndEnabled || _enemy == null || !_enemy.isActiveAndEnabled
                || _enemy.IsDeathPresented || _autoAction == null || !_autoAction.isActiveAndEnabled)
            {
                return _emptyTargets;
            }
            return _autoAction.PlannedTargets;
        }
    }

    public CEntity IntentTarget
    {
        get
        {
            if (!ShowTarget || !isActiveAndEnabled || _enemy == null || !_enemy.isActiveAndEnabled || _enemy.IsDeathPresented
                || _autoAction == null || !_autoAction.isActiveAndEnabled || _autoAction.PlannedSkill == null)
            {
                return null;
            }

            CEntity target = _autoAction.PlannedTarget;
            return target != null && target.isActiveAndEnabled
                && (!(target is CPlayer hero) || !hero.IsDeathPresented) ? target : null;
        }
    }

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
        if (_enemy == null)
        {
            _enemy = GetComponent<CEnemy>();
        }

        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_enemy != null)
        {
            _animation = _enemy.AttackAnimation;
        }

        if (_enemy != null)
        {
            _modelRenderer = _enemy.GetComponentInChildren<SkinnedMeshRenderer>();
        }
    }

    private void OnEnable()
    {
        if (_animation != null)
        {
            _animation.OnAttackCompleted += ClearIntent;
        }

        if (_animation != null)
        {
            _animation.OnAttackInterrupted += ClearIntent;
        }
    }

    private void OnDisable()
    {
        if (_animation != null)
        {
            _animation.OnAttackCompleted -= ClearIntent;
        }

        if (_animation != null)
        {
            _animation.OnAttackInterrupted -= ClearIntent;
        }

        ClearIntent();
    }

    private void LateUpdate()
    {
        if (_plannedSkill == null)
        {
            return;
        }

        if (_enemy == null || !_enemy.isActiveAndEnabled || _enemy.IsDeathPresented || _animation == null)
        {
            ClearIntent();
            return;
        }

        // 공격이 중단되면 의도 표시 해제
        if (_isExecuting)
        {
            if (!_animation.IsAttacking)
            {
                ClearIntent();
            }

            return;
        }

        if (_animation.IsBusy || !IsSkillAvailable())
        {
            ClearIntent();
        }
    }

    public bool TryPrepareIntent()
    {
        if (!isActiveAndEnabled || _enemy == null || !_enemy.isActiveAndEnabled || _enemy.IsDeathPresented || _animation == null || _animation.IsBusy || _isExecuting)
        {
            return false;
        }

        // 이미 표시한 스킬 유지
        if (_plannedSkill != null && IsSkillAvailable())
        {
            return true;
        }

        _plannedSkill = _enemy.SelectSkill();
        return _plannedSkill != null;
    }

    public bool TryExecuteIntent()
    {
        if (!isActiveAndEnabled || _isExecuting)
        {
            return false;
        }

        if (_enemy == null || !_enemy.isActiveAndEnabled || _enemy.IsDeathPresented || !IsSkillAvailable())
        {
            ClearIntent();
            return false;
        }

        // 표시한 스킬로 재생 요청 후 거절되면 표시 해제
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
        if (!_isExecuting)
        {
            ClearIntent();
        }
    }

    private bool IsSkillAvailable()
    {
        return _plannedSkill != null
            && _enemy.GetSkillCandidates().Contains(_plannedSkill)
            && CEnemySkillSelector.GetSelectionWeight(_enemy.EnemyData, _plannedSkill, _enemy.CurrentHp) > 0;
    }

    private void ClearIntent()
    {
        _plannedSkill = null;
        _isExecuting = false;
    }

    private void OnGUI()
    {
        if (!CTempLeeTestUI.CanShow(this)) return;

        DrawTargetIntent();
        if (!ShowLabel)
        {
            return;
        }

        CSkillDataSO skill = _autoAction != null && _autoAction.PlannedSkill != null ? _autoAction.PlannedSkill : _plannedSkill;
        CEntity target = _autoAction != null ? _autoAction.PlannedTarget : null;
        bool executing = _autoAction != null && _autoAction.PlannedSkill != null ? _autoAction.IsExecuting : _isExecuting;
        if (skill == null || _camera == null || _enemy == null || !_enemy.isActiveAndEnabled || _enemy.IsDeathPresented)
        {
            return;
        }

        Vector3 anchor = _enemy.transform.position;
        if (_modelRenderer != null)
        {
            Bounds bounds = _modelRenderer.bounds;
            anchor = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }

        Vector3 position = _camera.WorldToScreenPoint(anchor);
        if (position.z <= 0.0f)
        {
            return;
        }

        float height = target != null ? 66.0f : 46.0f;
        Rect area = new Rect(position.x - 120.0f + _labelOffset.x, Screen.height - position.y - height - _labelOffset.y, 240.0f, height);
        // 화면 가장자리에서도 스킬 이름 보이게 위치 조정
        area.x = Mathf.Clamp(area.x, 0.0f, Mathf.Max(0.0f, Screen.width - area.width));
        area.y = Mathf.Clamp(area.y, 0.0f, Mathf.Max(0.0f, Screen.height - area.height));
        GUI.Box(area, GUIContent.none);
        Sprite icon = skill.SkillIcon;
        float left = area.x + 8.0f;
        if (icon != null)
        {
            Rect textureRect = icon.textureRect;
            Rect uv = new Rect(textureRect.x / icon.texture.width, textureRect.y / icon.texture.height, textureRect.width / icon.texture.width, textureRect.height / icon.texture.height);
            GUI.DrawTextureWithTexCoords(new Rect(left, area.y + 7.0f, 32.0f, 32.0f), icon.texture, uv);
            left += 40.0f;
        }

        GUI.Label(new Rect(left, area.y + 3.0f, area.xMax - left - 4.0f, 20.0f), executing ? "Playing" : "Next skill");
        GUI.Label(new Rect(left, area.y + 23.0f, area.xMax - left - 4.0f, 20.0f), skill.SkillName);
        if (target != null)
        {
            string targetText = skill.IsGroupTarget ? "Targets: " + IntentTargets.Count : "Target: " + target.name;
            GUI.Label(new Rect(left, area.y + 43.0f, area.xMax - left - 4.0f, 20.0f), targetText);
        }
    }

    private void DrawTargetIntent()
    {
        IReadOnlyList<CEntity> targets = IntentTargets;
        for (int i = 0; i < targets.Count; i++)
        {
            if (CEnemyTargetFilter.IsAvailable(targets[i])) DrawTargetIntent(targets[i]);
        }
    }

    private void DrawTargetIntent(CEntity target)
    {
        if (target == null || _camera == null || _animation == null) return;
        if (_highlightTarget != target)
        {
            _highlightTarget = target;
            _targetRenderers = target.GetComponentsInChildren<Renderer>();
        }

        Bounds bounds = new Bounds(target.transform.position, Vector3.one * 0.002f);
        if (_targetRenderers != null)
        {
            for (int i = 0; i < _targetRenderers.Length; i++)
            {
                if (_targetRenderers[i] != null && _targetRenderers[i].enabled)
                {
                    bounds.Encapsulate(_targetRenderers[i].bounds);
                }
            }
        }

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = bounds.center + Vector3.Scale(bounds.extents,
                new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            Vector3 point = _camera.WorldToScreenPoint(corner);
            if (point.z <= 0.0f) return;
            Vector2 screen = new Vector2(point.x, Screen.height - point.y);
            min = Vector2.Min(min, screen);
            max = Vector2.Max(max, screen);
        }

        CSkillDataSO skill = _autoAction.PlannedSkill;
        CEnemySkillFeedbackSO feedback = _animation.GetFeedback(skill);
        Color previous = GUI.color;
        GUI.color = feedback != null ? feedback.IntentColor : new Color(1.0f, 0.55f, 0.2f);
        const float width = 2.0f;
        GUI.DrawTexture(new Rect(min.x, min.y, max.x - min.x, width), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(min.x, max.y - width, max.x - min.x, width), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(min.x, min.y, width, max.y - min.y), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(max.x - width, min.y, width, max.y - min.y), Texture2D.whiteTexture);

        Vector3 origin = _modelRenderer != null ? _modelRenderer.bounds.center : _enemy.transform.position;
        Vector3 start = _camera.WorldToScreenPoint(origin);
        if (start.z > 0.0f)
        {
            Vector2 from = new Vector2(start.x, Screen.height - start.y);
            Vector2 to = new Vector2((min.x + max.x) * 0.5f, min.y - 8.0f);
            from.y = Mathf.Min(from.y, to.y - 12.0f);
            DrawIntentLine(from, to);
        }

        GUI.color = previous;
    }

    private void DrawIntentLine(Vector2 from, Vector2 to)
    {
        Vector2 delta = to - from;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        Matrix4x4 previous = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, from);
        GUI.DrawTexture(new Rect(from.x, from.y, delta.magnitude, 2.0f), Texture2D.whiteTexture);
        GUI.matrix = previous;
        const float size = 5.0f;
        GUI.DrawTexture(new Rect(to.x - size, to.y - size, size * 2.0f, size * 2.0f), Texture2D.whiteTexture);
    }
}
