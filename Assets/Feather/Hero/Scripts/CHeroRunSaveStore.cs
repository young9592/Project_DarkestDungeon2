using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class CHeroRunMemberState
{
    public string MemberId;
    public string HeroId;
    public int Health;
    public int Stress;
    public bool IsDead;
}

[Serializable]
public class CHeroRunSaveData
{
    public int Version = 1;
    public string RunId;
    public int CompletedBattles;
    public string LastBattleId;
    public CHeroRunMemberState[] Members;
}

public class CHeroRunSaveStore
{
    private readonly string _path;

    public CHeroRunSaveStore(string path)
    {
        _path = path;
    }

    public static bool IsValid(CHeroRunSaveData data)
    {
        if (data == null || data.Version != 1 || string.IsNullOrWhiteSpace(data.RunId)
            || data.RunId.Length > 128 || data.CompletedBattles < 0
            || (data.CompletedBattles > 0 && string.IsNullOrWhiteSpace(data.LastBattleId))
            || data.Members == null || data.Members.Length < 1 || data.Members.Length > 4)
        {
            return false;
        }

        HashSet<string> ids = new HashSet<string>();
        for (int i = 0; i < data.Members.Length; i++)
        {
            CHeroRunMemberState member = data.Members[i];
            if (member == null || string.IsNullOrWhiteSpace(member.MemberId)
                || member.MemberId.Length > 128 || !ids.Add(member.MemberId)
                || string.IsNullOrWhiteSpace(member.HeroId) || member.HeroId.Length > 128
                || member.Health < 0 || member.Stress < 0 || member.Stress > 10
                || (member.IsDead && member.Health != 0))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryRead(out CHeroRunSaveData data, out string message)
    {
        data = null;
        message = string.Empty;
        try
        {
            if (string.IsNullOrWhiteSpace(_path)) throw new ArgumentException();
            if (!File.Exists(_path)) return true;
            if (new FileInfo(_path).Length > 262144) throw new ArgumentException();
            CHeroRunSaveData next = JsonUtility.FromJson<CHeroRunSaveData>(File.ReadAllText(_path, Encoding.UTF8));
            if (!IsValid(next)) throw new ArgumentException();
            data = next;
            return true;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
            || exception is ArgumentException || exception is NotSupportedException)
        {
            message = "Could not load expedition. Existing state kept.";
            return false;
        }
    }

    public bool TryWrite(CHeroRunSaveData data, out string message)
    {
        message = string.Empty;
        if (!IsValid(data))
        {
            message = "Expedition data is invalid.";
            return false;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(_path)) throw new ArgumentException();
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
            using (FileStream stream = new FileStream(_path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }

            if (File.Exists(_path)) File.Replace(_path + ".tmp", _path, _path + ".bak");
            else File.Move(_path + ".tmp", _path);
            return true;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
            || exception is ArgumentException || exception is NotSupportedException)
        {
            message = "Could not save expedition. Previous save kept.";
            return false;
        }
    }
}
