using UnityEngine;

public class CTheCrossroadsUICustomName : MonoBehaviour
{
    #region Inspector
    [SerializeField] private CTheCrossroadsUIHeroDescription _heroDescription;
    [SerializeField] private string[] _customNames;
    [SerializeField] private string[] _randomNames;
    #endregion

    private int _selectedHeroIndex = -1;

    public void SetCustomName(int heroIndex, string customName)
    {
        if (heroIndex < 0 || heroIndex >= _customNames.Length)
        {
            return;
        }

        _customNames[heroIndex] = customName;
        _heroDescription.UpdateCustomNameText(customName);
    }

    public void UpdateCustomName(int heroIndex)
    {
        if (heroIndex < 0 || heroIndex >= _customNames.Length)
        {
            return;
        }

        _heroDescription.UpdateCustomNameText(_customNames[heroIndex]);
    }

    public void RandomCustomName(int heroIndex)
    {
        if (heroIndex < 0 || heroIndex >= _customNames.Length)
        {
            return;
        }

        if (_randomNames == null || _randomNames.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, _randomNames.Length);

        SetCustomName(heroIndex, _randomNames[randomIndex]);
    }

    public void SelectHero(int heroIndex)
    {

        if (heroIndex < 0 || heroIndex >= _customNames.Length)
        {
            return;
        }

        _selectedHeroIndex = heroIndex;
        UpdateCustomName(_selectedHeroIndex);
    }

    public void RandomSelectedCustomName()
    {
        if (_selectedHeroIndex < 0)
        {
            return;
        }

        RandomCustomName(_selectedHeroIndex);
    }

    public void SetCustomNameForSelectedHero(string customName)
    {

        if (_selectedHeroIndex < 0 ||
            _selectedHeroIndex >= _customNames.Length)
        {
            return;
        }

        SetCustomName(_selectedHeroIndex, customName);
    }
}