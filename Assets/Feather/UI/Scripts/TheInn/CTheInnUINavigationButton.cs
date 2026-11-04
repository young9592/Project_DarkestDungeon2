
using UnityEngine;

public class CTheInnUINavigationButton : MonoBehaviour
{
    #region Inspector
    [SerializeField] private CTheInnUINavigationBar _navigationBar;
    [SerializeField] private CTheInnUINavigationBar.EInnUI _currentUI;

    [SerializeField] private CTheInnUIBiome _biome;
    [SerializeField] private CTheInnUICoach _coach;
    [SerializeField] private CTheInnUIUpgrade _upgrade;
    [SerializeField] private CTheInnUIStore _store;
    [SerializeField] private CTheInnUICandle _candle;
    [SerializeField] private CTheInnUIJournal _journal;
    #endregion

    public void OpenIcon()
    {
        switch (_currentUI)
        {
            case CTheInnUINavigationBar.EInnUI.Biome:
                _biome.BiomeUIOpen();
                break;

            case CTheInnUINavigationBar.EInnUI.Coach:
                _coach.CoachUIOpen();
                break;

            case CTheInnUINavigationBar.EInnUI.Upgrade:
                _upgrade.UpgradeUIOpen();
                break;

            case CTheInnUINavigationBar.EInnUI.Store:
                _store.StoreUIOpen();
                break;

            case CTheInnUINavigationBar.EInnUI.Candle:
                _candle.CandleUIOpen();
                break;

            case CTheInnUINavigationBar.EInnUI.Journal:
                _journal.JournalUIOpen();
                break;

        }
    }

}
