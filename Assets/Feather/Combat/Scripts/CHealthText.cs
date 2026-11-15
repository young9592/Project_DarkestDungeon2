using TMPro;
using UnityEngine;

public class CHealthText : MonoBehaviour
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private TextMeshProUGUI _heathText;
    #endregion

    public void SetHealthInfomation(int currentHealth, int maxHealth)
    {
        string healthText = $"<color=#D70B20>체력:{currentHealth}/{maxHealth}";
        _heathText.text = healthText;
    }

    public void SetHealthInfomation(int currentHealth, int maxHealth, int currentStress, int maxStress)
    {
        string healthText = $"<color=#D70B20>체력 : {currentHealth}/{maxHealth}";
        string stressText = $"<color=#B6B6B6>스트레스 : {currentStress}/{maxStress}";
        _heathText.text = healthText + "\n" + stressText;
    }
}
