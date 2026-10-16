using UnityEngine;

public class CHeroTargetView : MonoBehaviour
{
    private CEntity _entity;
    private CEnemy _enemy;
    private Renderer[] _renderers;
    private Camera _camera;
    private Vector3 _rankStep;
    private bool _isHighlighted;
    private bool _isHovered;

    public CEntity Entity { get { return _entity; } }
    public CEnemy Enemy { get { return _enemy; } }
    public bool IsHighlighted { get { return _isHighlighted; } }

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
    }

    public void Configure(CEntity enemy, Camera camera, Vector3 rankStep)
    {
        _entity = enemy;
        _enemy = enemy as CEnemy;
        _camera = camera;
        _rankStep = rankStep;
    }

    public Bounds GetWorldBounds()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.one * 0.002f);
        if (_renderers != null)
        {
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null) bounds.Encapsulate(_renderers[i].bounds);
        }
        int size = _enemy != null && _enemy.EnemyData != null ? _enemy.EnemyData.FormationSize : 1;
        Vector3 footprint = new Vector3(Mathf.Abs(_rankStep.x) * size, bounds.size.y, Mathf.Abs(_rankStep.z) * size);
        // 모델이 작아도 차지하는 자리 전체를 같은 적으로 선택합니다.
        bounds.Encapsulate(new Bounds(new Vector3(transform.position.x, bounds.center.y, transform.position.z), footprint));
        return bounds;
    }

    public bool TryGetHit(Ray ray, out float distance)
    {
        distance = 0.0f;
        return _entity != null && _entity.isActiveAndEnabled && GetWorldBounds().IntersectRay(ray, out distance);
    }

    public void SetHighlight(bool highlighted, bool hovered)
    {
        _isHighlighted = highlighted;
        _isHovered = highlighted && hovered;
    }

    private void OnDisable() { SetHighlight(false, false); }

    private void OnGUI()
    {
        if (!_isHighlighted || _camera == null || _entity == null || !_entity.isActiveAndEnabled) return;
        Bounds bounds = GetWorldBounds();
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
        Color previous = GUI.color;
        GUI.color = _isHovered ? Color.yellow : Color.green;
        const float width = 2.0f;
        GUI.DrawTexture(new Rect(min.x, min.y, max.x - min.x, width), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(min.x, max.y - width, max.x - min.x, width), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(min.x, min.y, width, max.y - min.y), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(max.x - width, min.y, width, max.y - min.y), Texture2D.whiteTexture);
        GUI.color = previous;
    }
}
