using System;
using System.Collections.Generic;
using UnityEngine;

public class CStore : MonoBehaviour
{

    [Serializable]
    public class StoreItem
    {
        public CItem _item;
        public int _quantity;
    }

    #region Inspector
    [SerializeField] private List<StoreItem> _storeItems = new List<StoreItem>();
    [SerializeField] private CStoreSlot[] _storeSlots;
    #endregion
    private CInventory _inventory;
    private void Start()
    {
        _inventory = FindObjectOfType<CInventory>();
        if (_inventory == null)
        {
            Debug.Log("인벤토리 없음!");
            return;
        }
        RandomStoreQuantity();
        RefreshStore();
    }

    private void RandomStoreQuantity()
    {
        foreach (StoreItem item in _storeItems)
        {
            if (item == null || item._item == null)
                continue;
            if (item._item._itemState == CItem.EItemState.Trinkets)
            {
                item._quantity = 1;
            }
            else
            {
                item._quantity = UnityEngine.Random.Range(1, 5);
            }

        }
    }

    public void RefreshStore()
    {
        for (int i = 0; i < _storeSlots.Length; i++)
        {
            _storeSlots[i].SetStore(this);
            if (i < _storeItems.Count &&
                _storeItems[i] != null &&
                _storeItems[i]._item != null)
            {
                _storeSlots[i].storeItem = _storeItems[i];
            }
            else
            {
                _storeSlots[i].storeItem = null;
            }
        }
        UpdatePriceColor();
    }

    private bool CanPurchase(CItem item)
    {
        int price = 0;
        switch (item._currencyType)
        {
            case CItem.ECurrencyType.Relics:
                price = item._buyRelics;
                break;
            case CItem.ECurrencyType.Baubles:
                price = item._buyBaubles;
                break;

            default:
                return false;
        }
        int currency = _inventory.GetCurrencyQuantity(item._currencyType);

        return currency >= price;
    }
    public void TryPurchase(StoreItem storeitem)
    {
        if (storeitem == null || storeitem._item == null)
            return;

        if (storeitem._quantity <= 0)
            return;
        CItem item = storeitem._item;

        int price = 0;

        switch (item._currencyType)
        {
            case CItem.ECurrencyType.Relics:
                price = item._buyRelics;
                break;
            case CItem.ECurrencyType.Baubles:
                price = item._buyBaubles;
                break;
            default:
                return;
        }

        if (_inventory.GetCurrencyQuantity(item._currencyType) < price)
            return;
        if (!_inventory.CanAddItem(item, 1))
            return;
        if (!_inventory.RemoveCurrency(item._currencyType, price))
            return;
        _inventory.AddItem(item, 1);
        storeitem._quantity--;
        if(storeitem._quantity <= 0)
        {
            _storeItems.Remove(storeitem);
        }
        RefreshStore();
    }
    public void UpdatePriceColor()
    {
        for (int i = 0; i < _storeSlots.Length; i++)
        {
            if (i >= _storeItems.Count ||
                _storeItems[i] == null ||
                _storeItems[i]._item == null)
                continue;

            bool canPurchase = CanPurchase(_storeItems[i]._item);
            _storeSlots[i].SetPriceColor(canPurchase);
        }
    }
  
}
