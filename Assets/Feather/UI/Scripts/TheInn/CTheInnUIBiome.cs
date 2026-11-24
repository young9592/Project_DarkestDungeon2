
using UnityEngine;

public class CTheInnUIBiome : MonoBehaviour
{
    #region Inspector
    [SerializeField] CTheInnUINavigationBar _navigationBar;

    [SerializeField] GameObject _selectBiomeUI;
    [SerializeField] GameObject _biomeUI;
    [SerializeField] GameObject _biome;
    #endregion

    private string _titleText = "경로 선택";

    public void BiomeUIOpen()
    {
        _navigationBar.ToggleUI(_biomeUI, _titleText);
    }

    public void SelectBiome()
    {
        _navigationBar.SelectBiome();
        _selectBiomeUI.SetActive(true);
        CSceneFlowManager.Instance.LoadScene(ESceneId.Sprawl);
    }
}
