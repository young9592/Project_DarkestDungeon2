
using UnityEngine;

public class CTheInnUIBiome : MonoBehaviour
{
    #region Inspector
    [SerializeField] CTheInnUINavigationBar _navigationBar;
    [SerializeField] GameObject _biomeUI;

    [SerializeField] GameObject[] _biome1;
    [SerializeField] GameObject[] _biome2;

    [SerializeField] GameObject[] _selectBiomeUI1;
    [SerializeField] GameObject[] _selectBiomeUI2;

    #endregion
    private int _randomBiome1;
    private int _randomBiome2;

    private int _selectedBiomeIndex = -1;
    private int _selectedBiomeGroup = -1;

    private string _titleText = "Select Route";

    private void Start()
    {
        RandomBiome();
    }
    public void BiomeUIOpen()
    {
        _navigationBar.ToggleUI(_biomeUI, _titleText);
    }

    public void RandomBiome()
    {
        for (int i = 0; i < _biome1.Length; i++)
        {
            _biome1[i].SetActive(false);
            _biome2[i].SetActive(false);

            _selectBiomeUI1[i].SetActive(false);
            _selectBiomeUI2[i].SetActive(false);
        }

        _randomBiome1 = Random.Range(0, _biome1.Length);
        do
        {
            _randomBiome2 = Random.Range(0, _biome2.Length);
        }
        while (_randomBiome1 == _randomBiome2);

        _biome1[_randomBiome1].SetActive(true);
        _biome2[_randomBiome2].SetActive(true);

        _selectedBiomeIndex = -1;
        _selectedBiomeGroup = -1;
    }

    public void SelectBiome1(int index)
    {
        if (index != _randomBiome1)
        {
            return;
        }

        if (_selectedBiomeGroup == 1 && _selectedBiomeIndex == index)
        {
            _selectBiomeUI1[index].SetActive(false);
            _selectedBiomeIndex = -1;
            _selectedBiomeGroup = -1;
            return;
        }

        CloseSelectBiomeUI();

        _navigationBar.SelectBiome();

        _selectBiomeUI1[index].SetActive(true);
        _selectedBiomeIndex = index;
        _selectedBiomeGroup = 1;

    }

    public void SelectBiome2(int index)
    {
        if (index != _randomBiome2)
        {
            return;
        }

        if (_selectedBiomeGroup == 2 && _selectedBiomeIndex == index)
        {
            _selectBiomeUI2[index].SetActive(false);
            _selectedBiomeIndex = -1;
            _selectedBiomeGroup = -1;
            return;
        }

        CloseSelectBiomeUI();

        _selectBiomeUI2[index].SetActive(true);

        _navigationBar.SelectBiome();

        _selectedBiomeIndex = index;
        _selectedBiomeGroup = 2;

    }
    private void CloseSelectBiomeUI()
    {
        for (int i = 0; i < _selectBiomeUI1.Length; i++)
        {
            _selectBiomeUI1[i].SetActive(false);
            _selectBiomeUI2[i].SetActive(false);
        }
    }


}
