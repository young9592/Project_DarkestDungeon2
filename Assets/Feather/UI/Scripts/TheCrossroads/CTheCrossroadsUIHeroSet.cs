
using UnityEngine;

//HeroRosterUI
public class CTheCrossroadsHeroSetUI : MonoBehaviour
{
    #region Inspector
    [SerializeField] private GameObject[] _hero3DModels;
    [SerializeField] private GameObject[] _glowingSlots;
    [SerializeField] private GameObject[] _heroColorImage;
    #endregion

    //영웅을 클릭했을때 나오는 모델 + UI
    public void UpdateHeroUI(int index)
    {
        if (index < 0 || index > _hero3DModels.Length)
        {
            return;
        }

        for (int i = 0; i < _hero3DModels.Length; i++)
        {
            _hero3DModels[i].SetActive(i == index);
            _glowingSlots[i].SetActive(i == index);
            _heroColorImage[i].SetActive(i == index);
        }
    }
}
