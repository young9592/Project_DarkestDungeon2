
using System;
using System.Collections.Generic;
using UnityEngine;

public class CInventory : MonoBehaviour
{
    [Serializable]
    public class InventoryItem
    {
        public CItem _item;
        public int quantity;
    }
    #region Inspector
    [SerializeField] CInventoryUICategory _inventoryCgyUI;
    [SerializeField] private CInventoryUICurrency _currencyUI;
    [SerializeField] private List<InventoryItem> _items = new List<InventoryItem>();

    [SerializeField] private CItem _testItem;
    [SerializeField] private CItem _testItem1;
    [SerializeField] private CItem _testItem2;
    [SerializeField] private CItem _testItem3;

    [SerializeField] private CSlot[] _slots;
    #endregion
    #region Property
    public int MaxSlotCount => _slots.Length;
    #endregion
    #region Filter
    private CItem.EItemState _currentItemState = CItem.EItemState.None;
    private bool _showAllItems = true;

    #endregion
    private List<InventoryItem> _displayItems = new List<InventoryItem>();

    private void Start()
    {
        FreshSlot();
        _inventoryCgyUI.UpdateItemQuantity();
        UpdateCurrnecy();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            AddItem(_testItem, 10);
            AddItem(_testItem1, 10);
            AddItem(_testItem2, 2);
            AddItem(_testItem3, 1);
        }
    }

    private void UpdateCurrnecy()
    {
        int relics = 0;
        int baubles = 0;

        foreach(InventoryItem item in _items)
        {
            if (item == null || item._item == null)
                continue;
            if (item._item._itemState != CItem.EItemState.Currency)
                continue;
            switch (item._item._currencyType)
            {
                case CItem.ECurrencyType.Relics:
                    relics += item.quantity;
                    break;
                case CItem.ECurrencyType.Baubles:
                    baubles += item.quantity;
                    break;
            }

        }
        _currencyUI.UpdateCurrencyText(relics, baubles);
    }

    public void FreshSlot()
    {
        _displayItems.Clear();

        if (_showAllItems)
        {
            _displayItems.AddRange(_items);
        }

        else
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] == null || _items[i]._item == null)
                    continue;

                if (_items[i]._item._itemState == _currentItemState)
                {
                    _displayItems.Add(_items[i]);
                }
            }
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            if (i < _displayItems.Count &&
                _displayItems[i] != null &&
                _displayItems[i]._item != null)
            {
                _slots[i].item = _displayItems[i]._item;
                _slots[i].SetQuantity(_displayItems[i].quantity);
            }
            else
            {
                _slots[i].item = null;
                _slots[i].SetQuantity(0);
            }
        }
    }

    public void AddItem(CItem item, int quantity)
    {
        SortItem();
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i]._item == item)
            {
                int addQuantity = Mathf.Min(
                    quantity,
                    item._maxQuantity - _items[i].quantity
                );

                _items[i].quantity += addQuantity;
                quantity -= addQuantity;

                if (quantity <= 0)
                {
                    FreshSlot();
                    return;
                }
            }
        }

        while (quantity > 0 && _items.Count < _slots.Length)
        {
            int addQuantity = Mathf.Min(quantity, item._maxQuantity);

            InventoryItem newItem = new InventoryItem();
            newItem._item = item;
            newItem.quantity = addQuantity;

            _items.Add(newItem);
            quantity -= addQuantity;
        }

        FreshSlot();
        _inventoryCgyUI.UpdateItemQuantity();
        UpdateCurrnecy();
    }

    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _displayItems.Count)
            return;

        InventoryItem selectedItem = _displayItems[slotIndex];

        if (selectedItem == null)
            return;

        int index = _items.IndexOf(selectedItem);

        if (index < 0)
            return;

        _items[index] = null;

        FreshSlot();
        _inventoryCgyUI.UpdateItemQuantity();
        UpdateCurrnecy();
    }

    public void SortItem()
    {
        _items.RemoveAll(item => item == null || item._item == null);

        FreshSlot();
    }

    public void SetItemFilter(CItem.EItemState state)
    {
        _currentItemState = state;
        _showAllItems = false;

        FreshSlot();
    }

    public void ShowAllItems()
    {
        _showAllItems = true;
        SortItem();
    }
    public int GetCurrentItemCount()
    {
        int count = 0;

        foreach (InventoryItem item in _items)
        {
            if (item == null || item._item == null)
                continue;

            count++;
        }

        return count;
    }
}
