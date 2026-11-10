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
    private const int MaxSaveBytes = 262144;
    private const int PresetCount = 3;
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
                data = new CHeroPartySaveData { Version = 1, Presets = new CHeroSavedParty[PresetCount] };
                return true;
            }

            data = ReadValidSave(_path);
            if (data == null)
            {
                string backupPath = _path + ".bak";
                if (File.Exists(backupPath)) data = ReadValidSave(backupPath);
                if (data == null)
                {
                    message = "Could not read party save. Existing party kept.";
                    return false;
                }
                message = "Recovered the previous party save from backup.";
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

    private static bool IsValid(CHeroPartySaveData data)
    {
        return data != null && data.Version == 1 && data.Presets != null && data.Presets.Length == PresetCount;
    }

    private static CHeroPartySaveData ReadValidSave(string path)
    {
        if (new FileInfo(path).Length > MaxSaveBytes) return null;

        string json = File.ReadAllText(path, Encoding.UTF8);
        try
        {
            CHeroPartySaveData data = JsonUtility.FromJson<CHeroPartySaveData>(json);
            return IsValid(data) ? data : null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public bool TryWrite(CHeroPartySaveData data, out string message)
    {
        message = string.Empty;
        if (!IsValid(data))
        {
            message = "Party save format is not supported. Previous save kept.";
            return false;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
            if (bytes.Length > MaxSaveBytes)
            {
                message = "Party save is too large. Previous save kept.";
                return false;
            }

            // 새 파일을 끝까지 쓴 뒤 기존 저장 파일 교체
            using (FileStream stream = new FileStream(_path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }

            if (File.Exists(_path))
            {
                // 깨진 저장으로 정상 백업을 덮지 않음
                string backupPath = ReadValidSave(_path) != null ? _path + ".bak" : null;
                File.Replace(_path + ".tmp", _path, backupPath);
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
