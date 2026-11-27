
using TMPro;
using UnityEngine;

public class CInventoryUICategory : MonoBehaviour
{
    #region Inspector
    [SerializeField] private CInventory _inventory;
    [SerializeField] private TMP_Text _itemQuantityText;
    #endregion

    public void AllItemButton()
    {
        _inventory.ShowAllItems();
    }
    public void CombatItemButton()
    {
        _inventory.SetItemFilter(CItem.EItemState.Combat);
    }

    public void InnItemButton()
    {
        _inventory.SetItemFilter(CItem.EItemState.Inn);
    }

    public void TrinketItemButton()
    {
        _inventory.SetItemFilter(CItem.EItemState.Trinkets);
    }
    public void SortItemButton()
    {
        _inventory.SortItem();
    }
    public void UpdateItemQuantity()
    {
        int currentCount = _inventory.GetCurrentItemCount();
        int maxCount = _inventory.MaxSlotCount;

        _itemQuantityText.text = $"{currentCount} / {maxCount}";
    }
}
