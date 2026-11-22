using System.Collections.Generic;
using UnityEngine;

public class CHorseAudio : MonoBehaviour
{
	#region Inspector
	[Header("AudioSource")]
	[SerializeField] private AudioSource _audioSource;

	[Header("Clips")]
	[SerializeField] private List<AudioClip> _clips = new List<AudioClip>();
    #endregion

    private void Awake()
    {
        if (_audioSource == null)
        {
            CPrint.Error($"{gameObject.name} : Null Find.");
            enabled = false;
            return;
        }

        if(CPrint.NullUnityObjectListCheck(gameObject.name, _clips))
        {
            enabled = false;
            return;
        }
    }

    public void Play()
    {
        int rand = Random.Range(0, _clips.Count);
        
        _audioSource.clip = _clips[rand];
        _audioSource.Play();
    }

}
