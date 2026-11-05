using System;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class CHeroMasterySaveData
{
    public int Version = 1;
    public string RunId;
    public int Points;
    public string[] MasteredSkillIds = new string[0];
}

public class CHeroMasterySaveStore
{
    private readonly string _path;

    public CHeroMasterySaveStore(string path)
    {
        _path = path;
    }

    public bool TryRead(out CHeroMasterySaveData data, out string message)
    {
        data = null;
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(_path))
        {
            message = "A mastery save path is required.";
            return false;
        }
        try
        {
            if (!File.Exists(_path)) return true;
            if (new FileInfo(_path).Length > 262144)
            {
                message = "Mastery save is too large.";
                return false;
            }
            data = JsonUtility.FromJson<CHeroMasterySaveData>(File.ReadAllText(_path, Encoding.UTF8));
            if (data == null || data.Version != 1 || string.IsNullOrWhiteSpace(data.RunId)
                || data.Points < 0 || data.MasteredSkillIds == null)
            {
                message = "Mastery save format is not supported.";
                return false;
            }
            return true;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
        {
            message = "Could not read mastery save. Existing save kept.";
            return false;
        }
    }

    public bool TryWrite(CHeroMasterySaveData data, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(_path) || data == null || data.Version != 1
            || string.IsNullOrWhiteSpace(data.RunId) || data.Points < 0 || data.MasteredSkillIds == null)
        {
            message = "Mastery save data is invalid.";
            return false;
        }
        try
        {
            string directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
            if (bytes.Length > 262144)
            {
                message = "Mastery save is too large.";
                return false;
            }
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
            message = "Could not save mastery. Points and upgrades kept.";
            return false;
        }
    }
}
