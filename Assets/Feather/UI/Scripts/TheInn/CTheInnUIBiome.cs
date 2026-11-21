
using UnityEngine;

public class CTheInnUIBiome : MonoBehaviour
{
    #region Inspector
    [SerializeField] CTheInnUINavigationBar _navigationBar;

    [SerializeField] GameObject _selectBiomeUI;
    [SerializeField] GameObject _biomeUI;
    [SerializeField] GameObject _biome;
    #endregion

    private string _titleText = "Select Route";

    public void BiomeUIOpen()
    {
        _navigationBar.ToggleUI(_biomeUI, _titleText);
    }

    public void SelectBiome()
    {
        _navigationBar.SelectBiome();
        _selectBiomeUI.SetActive(true);
        CSceneFlowManager.Instance.LoadScene(ESceneId.Temp_Sin_TheSprawl, CSceneFlowManager.EFadeType.Default);
    }
}
