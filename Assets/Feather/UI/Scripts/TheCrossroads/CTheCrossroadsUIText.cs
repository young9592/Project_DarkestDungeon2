
using TMPro;
using UnityEngine;

//HeroDescriptionUI 담당
public class CTheCrossroadsUIText : MonoBehaviour
{
    #region Inspector
    [SerializeField] private Transform[] _heroRanks;
    [SerializeField] private Transform[] _heroTargets;

    [SerializeField] private float[] _rankScale = new float[4];
    [SerializeField] private float[] _targetScale = new float[4];

    [SerializeField] private TMP_Text _characterNameText;
    [SerializeField] private TMP_Text _heroBonusListText;
    [SerializeField] private TMP_Text _heroQuoteText;
    [SerializeField] private TMP_Text _heroGorlText;

    [SerializeField] private string[] _characterName;
    [TextArea(3,5)]
    [SerializeField] private string[] _heroQuote;
    [TextArea(5,5)]
    [SerializeField] private string[] _heroBonusList;
    [SerializeField] private string[] _heroGorl;
    #endregion

    //클릭 한 것에 따라 텍스트 업데이트
    public void UpdateTextUI(int index)
    {
        _characterNameText.text = _characterName[index];
        _heroQuoteText.text = _heroQuote[index];
        _heroBonusListText.text = _heroBonusList[index];
        _heroGorlText.text = _heroGorl[index];
        UpdateHeroScaleUI(index);
    }
    //클릭에 따라 랭크 + 타겟 이미지 업뎃
    private void UpdateHeroScaleUI(int index)
    {
        switch (index)
        {
            case 0: //역병의사
                _rankScale = new float[] { 0.4f, 0.5f, 0.2f, 0.1f };
                _targetScale = new float[] { 0.2f, 0.2f, 0.1f, 0.1f };
                break;
            case 1: // 도굴꾼
                _rankScale = new float[] { 0.4f, 0.5f, 0.5f, 0.2f };
                _targetScale = new float[] { 0.2f, 0.5f, 0.3f, 0.2f };
                break;
            case 2: // 노상강도
                _rankScale = new float[] { 0.4f, 0.5f, 0.5f, 0.3f };
                _targetScale = new float[] { 0.3f, 0.5f, 0.3f, 0.2f };
                break;

            case 3: // 중보병
                _rankScale = new float[] { 0.2f, 0.3f, 0.5f, 0.3f };
                _targetScale = new float[] { 0.3f, 0.3f, 0.1f, 0.0f };
                break;
        }

        for (int i = 0; i < 4; i++)
        {
            _heroRanks[i].localScale =
                Vector3.one * _rankScale[i];
        }

        for (int i = 0; i < 4; i++)
        {
            _heroTargets[i].localScale =
                Vector3.one * _targetScale[i];
        }
    }
}
