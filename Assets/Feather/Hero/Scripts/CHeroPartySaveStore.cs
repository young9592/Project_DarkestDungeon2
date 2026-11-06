using System;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class CHeroSavedMember
{
    public string HeroId;
    public int Rank;
    public string[] SkillIds;
}

[Serializable]
public class CHeroSavedParty
{
    public bool IsSaved;
    public string Name;
    public CHeroSavedMember[] Members;
}

[Serializable]
public class CHeroPartySaveData
{
    public int Version;
    public CHeroSavedParty LastApplied;
    public CHeroSavedParty[] Presets;
}

public class CHeroPartySaveStore
{
    private readonly string _path;

    public CHeroPartySaveStore(string path)
    {
        _path = path;
    }

    public bool TryRead(out CHeroPartySaveData data, out string message)
    {
        data = null;
        message = string.Empty;
        try
        {
            if (!File.Exists(_path))
            {
                data = new CHeroPartySaveData { Version = 1, Presets = new CHeroSavedParty[3] };
                return true;
            }

            if (new FileInfo(_path).Length > 262144)
            {
                message = "Save file is too large. Existing party kept.";
                return false;
            }

            data = JsonUtility.FromJson<CHeroPartySaveData>(File.ReadAllText(_path, Encoding.UTF8));
            if (data == null || data.Version != 1 || data.Presets == null || data.Presets.Length != 3)
            {
                message = "Save format is not supported. Existing party kept.";
                return false;
            }

            // JsonUtility에서 빈 참조가 객체로 복원되는 경우도 빈 슬롯으로 처리
            if (data.LastApplied != null && !data.LastApplied.IsSaved) data.LastApplied = null;
            for (int i = 0; i < data.Presets.Length; i++)
            {
                if (data.Presets[i] != null && !data.Presets[i].IsSaved) data.Presets[i] = null;
            }

            return true;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
        {
            message = "Could not read party save. Existing party kept.";
            return false;
        }
    }

    public bool TryWrite(CHeroPartySaveData data, out string message)
    {
        message = string.Empty;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
            // 새 파일을 끝까지 쓴 뒤 기존 저장 파일 교체
            using (FileStream stream = new FileStream(_path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }

            if (File.Exists(_path))
            {
                File.Replace(_path + ".tmp", _path, _path + ".bak");
            }
            else
            {
                File.Move(_path + ".tmp", _path);
            }

            return true;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException || exception is NotSupportedException)
        {
            message = "Could not save party. Previous save kept.";
            return false;
        }
    }
}
