using System.Collections.Generic;
using UnityEngine;

public static class CPrint
{
    private readonly static HashSet<string> _once = new HashSet<string>();

    public static void Log(string str)
    {
#if UNITY_EDITOR
        Debug.Log(str);
#endif
    }
    public static void Warn(string str)
    {
#if UNITY_EDITOR
        Debug.LogWarning(str);
#endif
    }
    public static void Error(string str)
    {

        if (_once.Contains(str))
        {
            return;
        }

        _once.Add(str);
#if UNITY_EDITOR
        Debug.LogError(str);
#endif
    }

    /*
     외부에서 사용하는 방법

    [Field]
    List<Object> _checkList = new List<Object>

    [Awake]
    _checkList.Add(_transform);
    _checkList.Add(_obj);

    if(CPrint.NullUnityObjectListCheck(gameObject.name, _checkList))
    {
        enable = false;
        return;
    }

    */


    /// <summary>
    /// 유니티의 Object들을 체크합니다.
    /// 체크 예외 범위 : List나 string같은 기본 자료형
    /// </summary>
    /// <typeparam name="T">UnityEngine.Object 자료형</typeparam>
    /// <param name="where">어디서 이 코드를 실행하는지 이름정도 기입</param>
    /// <param name="list">Null을 체크할 오브젝트들</param>
    /// <returns></returns>
    public static bool NullUnityObjectListCheck<T>(string where, List<T> list) where T : Object
    {
        if (list == null)
        {
            Error($"{where} 리스트가 NULL입니다.");
            return true;
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
            {
                Error($"{where}[{i}] NULL 발견 인스펙터 확인 요망");
                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// 유니티의 단일 Object를 체크합니다.
    /// 체크 예외 범위 : List나 string같은 기본 자료형
    /// </summary>
    /// <param name="where">어디서 이 코드를 실행하는지 이름정도 기입</param>
    /// <param name="obj">Null을 체크할 단일 오브젝트</param>
    /// <returns></returns>
    public static bool NullUnityObjectCheck(string where, Object obj)
    {
        if (obj == null)
        {
            Error($"{where} NULL 발견 인스펙터 확인 요망");
            return true;
        }

        return false;
    }
}
