
using UnityEngine;
[CreateAssetMenu(menuName = "Data/Item", fileName = "ItemSO_")]
public class CItem : ScriptableObject
{
    public enum EItemState
    {
        None,
        Currency,
        Combat,
        Trinkets,
        Inn
    }
    public enum ECurrencyType
    {
        None,
        Relics,
        Baubles
    }

    #region Inspector
    public string _itemName;
    public Sprite _itemImage;

    public EItemState _itemState;
    public ECurrencyType _currencyType;

    public Sprite _stateImage;
    public Color _stateColor = Color.white;

    public int _buyRelics;
    public int _buyBaubles;

    public int _maxQuantity;
    #endregion
}
