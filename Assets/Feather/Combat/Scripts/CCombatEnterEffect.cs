using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CCombatEnterEffect : MonoBehaviour
{
    #region Inspector
    [Header("Connect")]
    [SerializeField] private GameObject _leftSword;
    [SerializeField] private GameObject _rightSword;
    [SerializeField] private GameObject _centerSkull;
    #endregion

    #region Field
    private RectTransform _leftSwordTr;
    private RectTransform _rightSwordTr;
    private RectTransform _centerSkullTr;

    private Image _leftSwordImage;
    private Image _rightSwordImage;
    private Image _centerSkullImage;

    private Material _centerSkullMaterial;
    #endregion

    private void Awake()
    {
        if (_leftSword == null || _rightSword == null || _centerSkull == null)
        {
            CPrint.Error($"{gameObject.name} : Null Find.");
            enabled = false;
            return;
        }

        _leftSwordTr = _leftSword.GetComponent<RectTransform>();
        _rightSwordTr = _rightSword.GetComponent<RectTransform>();
        _centerSkullTr = _centerSkull.GetComponent<RectTransform>();

        _leftSwordImage = _leftSword.GetComponent<Image>();
        _rightSwordImage = _rightSword.GetComponent<Image>();
        _centerSkullImage = _centerSkull.GetComponent<Image>();

        _centerSkullMaterial = _centerSkullImage.material;

        if (_leftSwordTr == null || _rightSwordTr == null || _centerSkullTr == null)
        {
            CPrint.Error($"{gameObject.name} : Tr is Null.");
            enabled = false;
            return;
        }

        if (_leftSwordImage == null || _rightSwordImage == null || _centerSkullImage == null)
        {
            CPrint.Error($"{gameObject.name} : Image is Null.");
            enabled = false;
            return;
        }

        if(_centerSkullMaterial == null)
        {
            CPrint.Error($"{gameObject.name} : Material is Null.");
            enabled = false;
            return;
        }


    }

    private void Start()
    {
        StartCoroutine(CoAnimation());
    }

    private IEnumerator CoAnimation()
    {

        // 1. 칼이 중앙으로 오면서 해골이 나타나는 연출
        _leftSwordImage.color = Color.black;
        _rightSwordImage.color = Color.black;
        _centerSkullImage.color = Color.black;

        float delay = 1f;
        float nextTime = Time.time + delay;

        Vector2 initLeft = _leftSwordTr.anchoredPosition;
        Vector2 initRight = _rightSwordTr.anchoredPosition;
        Vector2 initCenterScale = _centerSkullTr.localScale;

        while (nextTime > Time.time)
        {
            // 크기 및 위치 변경
            float t = 1f - Mathf.Exp(-5f * Time.deltaTime);
            float prograss = (_centerSkullTr.localScale.x - 1f) / (initCenterScale.x - 1f);

            if (prograss > 0.4f)
            {
                _centerSkullImage.color = Color.white;
            }

            _leftSwordTr.anchoredPosition = Vector2.Lerp(_leftSwordTr.anchoredPosition, Vector2.zero, t);
            _rightSwordTr.anchoredPosition = Vector2.Lerp(_rightSwordTr.anchoredPosition, Vector2.zero, t);
            _centerSkullTr.localScale = Vector2.Lerp(_centerSkullTr.localScale, Vector2.one, t);
            _centerSkullMaterial.SetFloat("_prograss", prograss);

            // 페이드 인
            float gotoWhite = Mathf.Clamp01(1f - (nextTime - Time.time) / delay);
            Color changeColor = new Color32((byte)(gotoWhite * 255), (byte)(gotoWhite * 255), (byte)(gotoWhite * 255), 255);

            _leftSwordImage.color = changeColor;
            _rightSwordImage.color = changeColor;

            yield return null;
        }

        // 2.잠시 대기
        yield return new WaitForSeconds(0.2f);

        // 3. 다시 칼이 밖으로 나가면서 사라지는 연출
        delay = 1f;
        nextTime = Time.time + delay;

        while (nextTime > Time.time)
        {
            float t = 1f - Mathf.Exp(-5f * Time.deltaTime);
            float prograss = (_centerSkullTr.localScale.x - 1f) / (initCenterScale.x - 1f);

            float gotoBlack = Mathf.Clamp01((nextTime - Time.time) / delay);
            Color changeColor = new Color32((byte)(gotoBlack * 255), (byte)(gotoBlack * 255), (byte)(gotoBlack * 255), (byte)(gotoBlack * 255));


            if (prograss > 0.4f)
            {
                _centerSkullImage.color = Color.black;
            }

            _leftSwordTr.anchoredPosition = Vector2.Lerp(_leftSwordTr.anchoredPosition, initLeft, t);
            _rightSwordTr.anchoredPosition = Vector2.Lerp(_rightSwordTr.anchoredPosition, initRight, t);
            _centerSkullTr.localScale = Vector2.Lerp(_centerSkullTr.localScale, initCenterScale, t);
            _centerSkullMaterial.SetFloat("_prograss", prograss);

            _leftSwordImage.color = changeColor;
            _rightSwordImage.color = changeColor;

            yield return null;
        }

        gameObject.SetActive(false);
        yield return null;
    }

}
