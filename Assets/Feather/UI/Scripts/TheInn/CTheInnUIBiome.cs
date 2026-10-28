
using UnityEngine;

public class CTheInnUIBiome : MonoBehaviour
{
    [SerializeField] GameObject _biomeUI;

    [SerializeField] GameObject TheSprawl;
    [SerializeField] GameObject TheShroud;
    [SerializeField] GameObject TheTangle;
    [SerializeField] GameObject TheFoetor;

    public void BiomeUIOpen()
    {
        _biomeUI.SetActive(!_biomeUI.activeSelf);
    }
}
