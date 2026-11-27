using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class CHeroBattleSaveData
{
    public int Version = 1;
    public string SaveId;
    public CHeroRunSaveData Heroes;
    public CHeroMasterySaveData Mastery;
    public string BattleId;
    public bool IsBattleFinished;
}

public class CHeroBattleSaveStore
{
    private const int MaxSaveBytes = 262144;
    private readonly string _path;

    public CHeroBattleSaveStore(string path)
    {
        _path = path;
    }

    public static bool IsValid(CHeroBattleSaveData data)
    {
        if (data == null || data.Version != 1 || string.IsNullOrWhiteSpace(data.SaveId)
            || data.SaveId.Length > 128 || !CHeroRunSaveStore.IsValid(data.Heroes)
            || data.Mastery == null || data.Mastery.Version != 1
            || data.Mastery.RunId != data.Heroes.RunId || data.Mastery.Points < 0
            || data.Mastery.MasteredSkillIds == null || data.Mastery.MasteredSkillIds.Length > 44
            || string.IsNullOrWhiteSpace(data.BattleId) || data.BattleId.Length > 128
            || (data.IsBattleFinished && (data.Heroes.CompletedBattles == 0
                || data.Heroes.LastBattleId != data.BattleId))
            || (!data.IsBattleFinished && data.Heroes.LastBattleId == data.BattleId)) return false;
        HashSet<string> ids = new HashSet<string>();
        for (int i = 0; i < data.Mastery.MasteredSkillIds.Length; i++)
        {
            string id = data.Mastery.MasteredSkillIds[i];
            if (string.IsNullOrWhiteSpace(id) || id.Length > 128 || !ids.Add(id)) return false;
        }
        return true;
    }

    public bool TryRead(out CHeroBattleSaveData data, out string message)
    {
        return TryRead(out data, out message, out _);
    }

    private bool TryRead(out CHeroBattleSaveData data, out string message, out bool fromBackup)
    {
        data = null;
        message = string.Empty;
        fromBackup = false;
        try
        {
            if (string.IsNullOrWhiteSpace(_path)) throw new ArgumentException();
            if (!File.Exists(_path)) return true;

            data = ReadValidSave(_path);
            if (data != null) return true;

            string backupPath = _path + ".bak";
            if (File.Exists(backupPath))
            {
                data = ReadValidSave(backupPath);
                if (data != null)
                {
                    fromBackup = true;
                    message = "Recovered the previous battle save from backup.";
                    return true;
                }
            }
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
            || exception is ArgumentException || exception is NotSupportedException)
        {
        }
        message = "Could not load battle state. Current state kept.";
        return false;
    }

    private static CHeroBattleSaveData ReadValidSave(string path)
    {
        if (new FileInfo(path).Length > MaxSaveBytes) return null;

        string json = File.ReadAllText(path, Encoding.UTF8);
        try
        {
            CHeroBattleSaveData data = JsonUtility.FromJson<CHeroBattleSaveData>(json);
            return IsValid(data) ? data : null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public bool IsCurrent(string saveId)
    {
        return TryRead(out CHeroBattleSaveData current, out string message)
            && (current != null ? current.SaveId : string.Empty) == saveId;
    }

    public bool TryWrite(CHeroBattleSaveData data, string expectedSaveId, out string message)
    {
        message = "Battle state is invalid.";
        if (!IsValid(data)) return false;
        if (!TryRead(out CHeroBattleSaveData current, out _, out bool fromBackup)
            || (current != null ? current.SaveId : string.Empty) != expectedSaveId)
        {
            message = "Battle save changed. Reload before saving.";
            return false;
        }
        try
        {
            if (string.IsNullOrWhiteSpace(_path)) throw new ArgumentException();
            string directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
            if (bytes.Length > MaxSaveBytes) throw new ArgumentException();
            using (FileStream stream = new FileStream(_path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            // 복구 후 첫 저장은 깨진 파일로 백업을 덮지 않음
            if (File.Exists(_path)) File.Replace(_path + ".tmp", _path, fromBackup ? null : _path + ".bak");
            else File.Move(_path + ".tmp", _path);
            message = string.Empty;
            return true;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
            || exception is ArgumentException || exception is NotSupportedException)
        {
            message = "Could not save battle state. Previous save kept.";
            return false;
        }
    }
}
