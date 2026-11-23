using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSceneTransition : MonoBehaviour
{
    private enum ECurrentState
    {
        None,
        EncounterCombat,
        //TODO
    }

    #region Inspector
    [Header("Fade Option")]
    [SerializeField] private List<Material> _fadeMaterials;
    #endregion

    #region Field
    private ECurrentState _state = ECurrentState.None;
    #endregion

    public IEnumerator CoFadeOut(ESceneId prevId, ESceneId nextId, GameObject loadPanel)
    {
        // TODO : 각 연출마다 다른 방향으로 진행
        if ((prevId == ESceneId.Valley || prevId == ESceneId.Sprawl) && nextId == ESceneId.Battle)
        {
            loadPanel.GetComponent<Image>().material = _fadeMaterials[0];
            _state = ECurrentState.EncounterCombat;
        }
        else
        {
            loadPanel.GetComponent<Image>().material = _fadeMaterials[0];
            _state = ECurrentState.None;
        }


        Material mat = loadPanel.GetComponent<Image>().material;
        int percent = Shader.PropertyToID("_percent");
        loadPanel.SetActive(true);

        float delay = 1.5f;
        float nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float t = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);
            mat.SetFloat(percent, t);
            yield return null;
        }

        mat.SetFloat(percent, 1f);
    }

    public IEnumerator CoFadeIn(ESceneId prevId, ESceneId nextId, GameObject loadPanel)
    {
        // TODO : 각 연출마다 다른 방향으로 진행
        if ((prevId == ESceneId.Valley || prevId == ESceneId.Sprawl) && nextId == ESceneId.Battle)
        {
            loadPanel.GetComponent<Image>().material = _fadeMaterials[1];
            _state = ECurrentState.EncounterCombat;
        }
        else
        {
            loadPanel.GetComponent<Image>().material = _fadeMaterials[0];
            _state = ECurrentState.None;
        }


        Material mat = loadPanel.GetComponent<Image>().material;
        int percent = Shader.PropertyToID("_percent");
        loadPanel.SetActive(true);

        float delay = 2f;
        float nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float t = 0;

            switch (_state)
            {
                case ECurrentState.EncounterCombat:
                    t = Mathf.Clamp01((nextTime - Time.time) / delay);
                    break;
                default:
                    t = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);
                    break;
            }
            mat.SetFloat(percent, 1f - t);
            yield return null;
        }
    }
}
