using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CHeroRankPresentation : MonoBehaviour
{
    [Header("Rank Movement")]
    [SerializeField, Min(0.05f)] private float _moveDuration = 0.35f;
    [SerializeField, Min(0.0f)] private float _passingOffset = 0.002f;

    private CPlayer _hero;
    private IReadOnlyList<CPlayer> _party;
    private Vector3 _frontPosition;
    private Vector3 _rankStep;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private ERank _destinationRank;
    private float _elapsed;
    private float _duration;
    private bool _isConfigured;
    private bool _isMoving;

    public bool IsBusy
    {
        get
        {
            if (HasPendingMovement())
            {
                return true;
            }

            if (_party != null)
            {
                for (int i = 0; i < _party.Count; i++)
                {
                    CPlayer member = _party[i];
                    if (member != null && member.RankPresentation != null
                        && member.RankPresentation.HasPendingMovement())
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    private void Awake()
    {
        _hero = GetComponent<CPlayer>();
    }

    public void Configure(Vector3 frontPosition, Vector3 rankStep, IReadOnlyList<CPlayer> party)
    {
        _party = party;
        _frontPosition = frontPosition;
        _rankStep = rankStep;
        _isConfigured = true;
        SnapToRank();
    }

    private bool HasPendingMovement()
    {
        return isActiveAndEnabled && _isConfigured && _hero != null
            && (_isMoving || (CSkillRankUtility.IsSingleRank(_hero.CurrentRank)
            && _hero.CurrentRank != _destinationRank));
    }

    public void SnapToRank()
    {
        if (!_isConfigured || _hero == null || !TryGetPosition(_hero.CurrentRank, out Vector3 position))
        {
            return;
        }

        _destinationRank = _hero.CurrentRank;
        _isMoving = false;
        transform.position = position;
    }

    private bool TryGetPosition(ERank rank, out Vector3 position)
    {
        for (int i = 0; i < 4; i++)
        {
            if (rank == (ERank)(1 << i))
            {
                position = _frontPosition + _rankStep * i;
                return true;
            }
        }

        position = transform.position;
        return false;
    }

    private void LateUpdate()
    {
        if (!_isConfigured || _hero == null)
        {
            return;
        }

        if (_hero.CurrentRank != _destinationRank
            && TryGetPosition(_hero.CurrentRank, out Vector3 position))
        {
            // 확정된 열만 읽어서 표시 위치 변경
            _destinationRank = _hero.CurrentRank;
            _startPosition = transform.position;
            _endPosition = position;
            _elapsed = 0.0f;
            _duration = _moveDuration * Mathf.Sqrt(Mathf.Clamp(
                Vector3.Distance(_startPosition, position) / Mathf.Max(_rankStep.magnitude, 0.0001f), 1.0f, 3.0f));
            _isMoving = true;
        }

        if (!_isMoving)
        {
            return;
        }

        _elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsed / _duration);
        float eased = Mathf.SmoothStep(0.0f, 1.0f, progress);
        Vector3 passingDirection = Vector3.Cross(Vector3.up, _rankStep).normalized;
        float direction = Vector3.Dot(_endPosition - _startPosition, _rankStep) >= 0.0f ? 1.0f : -1.0f;
        transform.position = Vector3.Lerp(_startPosition, _endPosition, eased)
            + passingDirection * (Mathf.Sin(progress * Mathf.PI) * _passingOffset * direction);
        if (progress >= 1.0f)
        {
            transform.position = _endPosition;
            _isMoving = false;
        }
    }

    private void OnDisable()
    {
        SnapToRank();
    }
}
