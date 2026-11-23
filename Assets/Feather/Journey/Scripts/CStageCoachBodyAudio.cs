using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CStageCoachBodyAudio : MonoBehaviour
{
    #region Inspector
    [Header("AudioSource")]
    [SerializeField] private AudioSource _audioSource;

    [Header("Clips")]
    [SerializeField] private AudioClip _clip;
    #endregion

    private void Awake()
    {
        if (_audioSource == null || _clip == null)
        {
            CPrint.Error($"{gameObject.name} : Null Find.");
            enabled = false;
            return;
        }

        _audioSource.clip = _clip;
        _audioSource.loop = true;
    }

    public void Play()
    {
        _audioSource.Play();
    }

    public void Stop()
    {
        _audioSource.Stop();
    }
}
