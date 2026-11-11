
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CSlot : MonoBehaviour, IPointerClickHandler
{
    #region Inspector
    [SerializeField] private Image _itemImage;
    [SerializeField] private Image _stateImage;
    [SerializeField] private TMP_Text _quantityText;

    [SerializeField] private Sprite _defaultStateImage;
    [SerializeField] private int _slotIndex;
    #endregion

    private CInventory _inventory;
    private CItem _item;
    private int _quantity;

    private void Awake()
    {
        _inventory = GetComponentInParent<CInventory>();
    }
    public CItem item
    {
        get { return _item; }
        set
        {
            _item = value;

            if (_item != null)
            {
                _itemImage.sprite = item._itemImage;
                _stateImage.sprite = item._stateImage;
                _stateImage.color = item._stateColor;
                _itemImage.color = Color.white;
                _quantityText.text = _quantity.ToString();
            }
            else
            {
                _itemImage.color = Color.clear;

                _stateImage.sprite = _defaultStateImage;
                _stateImage.color = new Color32(68, 68, 68, 255);
                _quantityText.text = "";
            }

        }
    }

    public void SetQuantity(int quantity)
    {
        if (_item == null)
        {
            _quantity = 0;
            _quantityText.text = "";
            return;
        }

        _quantity = Mathf.Clamp(quantity, 0, _item._maxQuantity);

        if (_quantity >= 2)
        {
            _quantityText.text = _quantity.ToString();
        }
        else
        {
            _quantityText.text = "";
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Input.GetKey(KeyCode.LeftShift) ||
                Input.GetKey(KeyCode.RightShift))
            {
                _inventory.RemoveItem(_slotIndex);
            }
        }
    }
}
