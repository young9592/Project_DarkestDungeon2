using System.Collections.Generic;
using UnityEngine;

public class CHorseHoofAudio : MonoBehaviour
{
    #region Inspector
    [Header("SoundList")]
    [SerializeField] private List<AudioClip> _backRightHoofList;
    [SerializeField] private List<AudioClip> _backLeftHoofList;
    [SerializeField] private List<AudioClip> _frontRightHoofList;
    [SerializeField] private List<AudioClip> _frontLeftHoofList;
    #endregion

    #region Field
    private AudioSource _audioSource;
    #endregion

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        if (CPrint.NullUnityObjectCheck(gameObject.name, _audioSource))
        {
            enabled = false;
            return;
        }

        if (CPrint.NullUnityObjectListCheck(gameObject.name, _backLeftHoofList))
        {
            enabled = false;
            return;
        }

        if (CPrint.NullUnityObjectListCheck(gameObject.name, _backRightHoofList))
        {
            enabled = false;
            return;
        }

        if (CPrint.NullUnityObjectListCheck(gameObject.name, _frontLeftHoofList))
        {
            enabled = false;
            return;
        }

        if (CPrint.NullUnityObjectListCheck(gameObject.name, _frontRightHoofList))
        {
            enabled = false;
            return;
        }
    }

    private void PlayBackRightHoof()
    {
        int rand = Random.Range(0, _backRightHoofList.Count);

        _audioSource.clip = _backRightHoofList[rand];
        _audioSource.Play();
    }

    private void PlayBackLeftHoof()
    {
        int rand = Random.Range(0, _backLeftHoofList.Count);

        _audioSource.clip = _backLeftHoofList[rand];
        _audioSource.Play();
    }

    private void PlayFrontRightHoof()
    {
        int rand = Random.Range(0, _frontRightHoofList.Count);

        _audioSource.clip = _frontRightHoofList[rand];
        _audioSource.Play();
    }

    private void PlayFrontLeftHoof()
    {
        int rand = Random.Range(0, _frontLeftHoofList.Count);

        _audioSource.clip = _frontLeftHoofList[rand];
        _audioSource.Play();
    }
}
