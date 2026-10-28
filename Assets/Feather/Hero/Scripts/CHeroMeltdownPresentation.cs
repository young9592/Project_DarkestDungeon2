using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CHeroMeltdownPresentation : MonoBehaviour
{
    private CPlayer _hero;
    private CHeroAnimation _animation;
    private SkinnedMeshRenderer[] _renderers;
    private float[][] _defaultFaceWeights;
    private Renderer[] _heldWeapons;
    private bool[] _previousWeaponVisibility;
    private bool _weaponsHidden;

    public event Action<CPlayer> OnQueued;
    public event Action<CPlayer> OnStarted;
    public event Action<CPlayer> OnCompleted;
    public event Action<CPlayer> OnInterrupted;

    public bool IsBusy
    {
        get { return _animation != null && (_animation.IsMeltdownQueued || _animation.IsMeltingDown); }
    }

    private void Awake()
    {
        _hero = GetComponent<CPlayer>();
        _animation = GetComponent<CHeroAnimation>();
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        _defaultFaceWeights = new float[_renderers.Length][];
        for (int i = 0; i < _renderers.Length; i++)
        {
            Mesh mesh = _renderers[i].sharedMesh;
            int count = mesh != null ? mesh.blendShapeCount : 0;
            _defaultFaceWeights[i] = new float[count];
            for (int j = 0; j < count; j++)
            {
                _defaultFaceWeights[i][j] = _renderers[i].GetBlendShapeWeight(j);
            }
        }
        CacheHeldWeapons();
    }

    private void OnEnable()
    {
        if (_hero == null || _animation == null) return;
        _hero.OnMeltdownTriggered += Queue;
        _animation.OnMeltdownStarted += Started;
        _animation.OnMeltdownCompleted += Completed;
        _animation.OnMeltdownInterrupted += Interrupted;
    }

    private void Queue(CPlayer hero)
    {
        if (_hero.ActionState == EEntityActionState.Idle || _hero.ActionState == EEntityActionState.Selecting)
        {
            _hero.CancelSkillSelection();
        }
        if (_animation.TryQueueMeltdown()) OnQueued?.Invoke(_hero);
    }

    private void Started()
    {
        HideHeldWeapons();
        OnStarted?.Invoke(_hero);
    }

    private void Completed()
    {
        RestoreHeldWeapons();
        OnCompleted?.Invoke(_hero);
    }
    private void Interrupted()
    {
        RestoreHeldWeapons();
        ResetFace();
        OnInterrupted?.Invoke(_hero);
    }

    private void CacheHeldWeapons()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        List<Renderer> weapons = new List<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (IsHeldWeapon(renderers[i].name)) weapons.Add(renderers[i]);
        }
        _heldWeapons = weapons.ToArray();
        _previousWeaponVisibility = new bool[_heldWeapons.Length];
    }

    private bool IsHeldWeapon(string meshName)
    {
        switch (meshName)
        {
            case "msh_man_at_arms_wpn_mace":
            case "msh_man_at_arms_wpn_shield":
            case "msh_highwayman_wpn_blade":
            case "msh_highwayman_wpn_pistol":
            case "msh_grave_robber_wpn_dagger":
            case "msh_grave_robber_wpn_pickaxe":
            case "msh_grave_robber_wpn_bottle":
            case "msh_plague_doctor_wpn_dagger":
            case "msh_plague_doctor_wpn_flask":
                return true;
            default:
                return false;
        }
    }

    private void HideHeldWeapons()
    {
        if (_weaponsHidden || _heldWeapons == null) return;
        for (int i = 0; i < _heldWeapons.Length; i++)
        {
            Renderer weapon = _heldWeapons[i];
            if (weapon == null) continue;
            _previousWeaponVisibility[i] = weapon.forceRenderingOff;
            weapon.forceRenderingOff = true;
        }
        _weaponsHidden = true;
    }

    private void RestoreHeldWeapons()
    {
        if (!_weaponsHidden || _heldWeapons == null) return;
        for (int i = 0; i < _heldWeapons.Length; i++)
        {
            if (_heldWeapons[i] != null)
            {
                _heldWeapons[i].forceRenderingOff = _previousWeaponVisibility[i];
            }
        }
        _weaponsHidden = false;
    }

    private void ResetFace()
    {
        if (_renderers == null || _defaultFaceWeights == null) return;
        for (int i = 0; i < _renderers.Length; i++)
        {
            SkinnedMeshRenderer renderer = _renderers[i];
            if (renderer == null || renderer.sharedMesh == null) continue;
            int count = Mathf.Min(renderer.sharedMesh.blendShapeCount, _defaultFaceWeights[i].Length);
            for (int j = 0; j < count; j++)
            {
                renderer.SetBlendShapeWeight(j, _defaultFaceWeights[i][j]);
            }
        }
    }

    private void OnDisable()
    {
        if (_animation != null) _animation.CancelMeltdown();
        RestoreHeldWeapons();
        if (_hero == null || _animation == null) return;
        _hero.OnMeltdownTriggered -= Queue;
        _animation.OnMeltdownStarted -= Started;
        _animation.OnMeltdownCompleted -= Completed;
        _animation.OnMeltdownInterrupted -= Interrupted;
    }
}

