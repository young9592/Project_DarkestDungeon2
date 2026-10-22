using TMPro;
using UnityEngine;

public class CTheCrossroadsUICustomNameInput : MonoBehaviour
{
    #region Inspector
    [SerializeField] private GameObject _customNameText;
    [SerializeField] private TMP_InputField _customNameInput;
    [SerializeField] private CTheCrossroadsUICustomName _customNameUI;
    #endregion

    private void Awake()
    {
        _customNameInput.gameObject.SetActive(false);
    }


    public void OpenInput()
    {
        _customNameText.SetActive(false);
        _customNameInput.gameObject.SetActive(true);

        _customNameInput.text = "";

        _customNameInput.ActivateInputField();
    }

    public void EndInput()
    {
        string customName = _customNameInput.text.Trim();

        if (!string.IsNullOrEmpty(customName))
        {
            _customNameUI.SetCustomNameForSelectedHero(customName);
        }

        _customNameInput.gameObject.SetActive(false);
        _customNameText.SetActive(true);

    }
}