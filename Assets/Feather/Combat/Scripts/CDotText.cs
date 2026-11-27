using TMPro;
using UnityEngine;

public class CDotText : MonoBehaviour
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private TextMeshProUGUI _dotText;
    #endregion

    public void SetDotInfomation(EEffectType type, int duration, int damage)
    {
        int dotImageIdx = GetDotImage(type);
        string dotDesc = "";

        if (type == EEffectType.Horror)
        {
            dotDesc = $"<sprite={dotImageIdx}><color={GetColor(type)}>{damage} ((75%), {duration}턴)</color>";
        }
        else
        {
            dotDesc = $"<sprite={dotImageIdx}><color={GetColor(type)}>{damage} ({duration}턴)</color>";
        }

        _dotText.text = dotDesc;
    }

    private int GetDotImage(EEffectType type)
    {
        int dotImageIdx = 0;

        switch (type)
        {
            case EEffectType.Bleed:
                dotImageIdx = 0;
                break;
            case EEffectType.Blight:
                dotImageIdx = 1;
                break;
            case EEffectType.Burn:
                dotImageIdx = 2;
                break;
            case EEffectType.Horror:
                dotImageIdx = 3;
                break;
            case EEffectType.Regen:
                dotImageIdx = 4;
                break;
        }

        return dotImageIdx;
    }
    private string GetColor(EEffectType type)
    {
        string color = "";

        switch (type)
        {
            case EEffectType.Bleed:
                color = "#BC0606";
                break;
            case EEffectType.Blight:
                color = "#006235";
                break;
            case EEffectType.Burn:
                color = "#BC7406";
                break;
            case EEffectType.Horror:
                color = "#545454";
                break;
            case EEffectType.Regen:
                color = "#5CFF07";
                break;
        }

        if (color.Equals(""))
        {
            CPrint.Error($"{gameObject.name} : 잘못된 타입이 들어옴.");
            enabled = false;
        }

        return color;
    }
}
