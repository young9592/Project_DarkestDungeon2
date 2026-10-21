using System.Collections;
using UnityEngine;

public class CChapterUIAnimation : MonoBehaviour
{
    #region Inspector
    [SerializeField] RectTransform _chapterMenu;
    [SerializeField] CChapterUIButton _chapterButton;
    [SerializeField] private GameObject _text;

    //시작 위치
    [SerializeField] private float _startX = -404f;
    //도착 위치
    [SerializeField] private float _targetX = -1012f;
    //이동 시간
    [SerializeField] private float _moveDuration = 1.0f;
    #endregion

    //챕터 메뉴 등장
    private IEnumerator MoveChapterMenuEnter()
    {
        float elapsedTime = 0f;

        Vector2 startPosition = new Vector2(_startX, -21f);
        Vector2 targetPosition = new Vector2(_targetX, -21f);

        _chapterMenu.anchoredPosition = startPosition;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / _moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            _chapterMenu.anchoredPosition =
                Vector2.Lerp(startPosition, targetPosition, t);

            yield return null;
        }
        _chapterMenu.anchoredPosition = targetPosition;
        _text.SetActive(true);
    }

    //챕터 메뉴 퇴장
    private IEnumerator MoveChapterMenuExit()
    {
        float elapsedTime = 0f;

        Vector2 startPosition = new Vector2(_startX, -21f);
        Vector2 targetPosition = new Vector2(_targetX, -21f);

        _chapterMenu.anchoredPosition = targetPosition;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / _moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            _chapterMenu.anchoredPosition =
                Vector2.Lerp(targetPosition, startPosition, t);

            yield return null;
        }
        _chapterMenu.anchoredPosition = startPosition;
        _text.SetActive(false);
        _chapterButton.ResetSignature();
    }

    //챕터 코루틴 시작 함수
    public void ChapterMenuEnter()
    {
        StartCoroutine(MoveChapterMenuEnter());
    }

    //챕터 코루틴 퇴장
    public void ChapterMenuExit()
    {
        StartCoroutine(MoveChapterMenuExit());
    }
}
