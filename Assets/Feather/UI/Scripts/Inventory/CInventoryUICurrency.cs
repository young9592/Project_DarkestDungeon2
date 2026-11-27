using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CInventoryUICurrency : MonoBehaviour
{
    #region Inspector
    [SerializeField] private TMP_Text _relicsText;
    [SerializeField] private TMP_Text _baublesText;
    #endregion

    public void UpdateCurrencyText(int relics, int baubles)
    {
        _relicsText.text = relics.ToString();
        _baublesText.text = baubles.ToString();
    }
}
