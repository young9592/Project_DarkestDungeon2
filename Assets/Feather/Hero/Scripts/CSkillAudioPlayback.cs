using System;
using System.Collections.Generic;
using UnityEngine;

public class CSkillAudioPlayback
{
    private class CPendingSound
    {
        public EHeroSkillSoundPhase Phase;
        public float Remaining;
        public int QueuedFrame;
        public Action Play;
    }

    private readonly List<CPendingSound> _pending = new List<CPendingSound>();

    public void Schedule(EHeroSkillSoundPhase phase, float delay, Action play)
    {
        Cancel(phase);
        if (play == null) return;
        if (delay <= 0.0f) { play(); return; }
        _pending.Add(new CPendingSound { Phase = phase, Remaining = delay, QueuedFrame = Time.frameCount, Play = play });
    }

    public void Tick(float deltaTime)
    {
        for (int i = 0; i < _pending.Count;)
        {
            CPendingSound sound = _pending[i];
            if (sound.QueuedFrame == Time.frameCount) { i++; continue; }
            sound.Remaining -= Mathf.Max(0.0f, deltaTime);
            if (sound.Remaining > 0.0f) { i++; continue; }
            _pending.RemoveAt(i);
            sound.Play();
        }
    }

    public void Cancel(EHeroSkillSoundPhase phase)
    {
        for (int i = _pending.Count - 1; i >= 0; i--)
        {
            if (_pending[i].Phase == phase) _pending.RemoveAt(i);
        }
    }

    public void CancelAll()
    {
        _pending.Clear();
    }
}
