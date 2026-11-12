
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CStoreSlot : MonoBehaviour, IPointerClickHandler
{
    #region Inspector
    [SerializeField] private Image _itemImage;
    [SerializeField] private Image _stateImage;
    [SerializeField] private TMP_Text _quantityText;

    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private Image _currencyImage;

    [SerializeField] private Sprite _relicsImage;
    [SerializeField] private Sprite _baublesImage;
    #endregion
    private CStore.StoreItem _storeItem;
    private CStore _store;
    public CStore.StoreItem storeItem
    {
        get { return _storeItem; }
        set
        {
            _storeItem = value;
            if (_storeItem != null && _storeItem._item != null)
            {
                CItem item = _storeItem._item;

                _itemImage.sprite = item._itemImage;
                _stateImage.sprite = item._stateImage;
                _stateImage.color = item._stateColor;
                _itemImage.color = Color.white;
                _quantityText.text = _storeItem._quantity.ToString();

                switch (item._currencyType)
                {
                    case CItem.ECurrencyType.Relics:
                        _currencyImage.sprite = _relicsImage;
                        _priceText.text = item._buyRelics.ToString();
                        break;
                    case CItem.ECurrencyType.Baubles:
                        _currencyImage.sprite = _baublesImage;
                        _priceText.text = item._buyBaubles.ToString();
                        break;
                    default:
                        _currencyImage.gameObject.SetActive(false);
                        _priceText.text = "";
                        break;
                }
                _currencyImage.gameObject.SetActive(true);
            }
            else
            {
                _itemImage.color = Color.clear;
                _stateImage.color = Color.clear;

                _quantityText.text = "";
                _priceText.text = "";

                _currencyImage.gameObject.SetActive(false);
            }
        }
    }
    public void SetPriceColor(bool canPurchase)
    {
        _priceText.color = canPurchase ? Color.white : Color.red;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.clickCount == 2)
        {
            if (_storeItem == null || _store == null)
                return;
            _store.TryPurchase(_storeItem);
        }
    }
    public void SetStore(CStore store)
    {
        _store = store;
    }
}
