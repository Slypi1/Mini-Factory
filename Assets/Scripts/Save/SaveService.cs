using UnityEngine;

public class SaveService : ISaveService
{
    private const string SaveKey = "MiniFactory_Save";

    public void Save(FactoryState state)
    {
        if (state == null)
            return;

        state.LastSaveUnixTime =
            System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        string json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public FactoryState Load()
    {
        if (!HasSave())
            return null;

        string json = PlayerPrefs.GetString(SaveKey);

        try
        {
            return JsonUtility.FromJson<FactoryState>(json);
        }
        catch (System.SystemException exception)
        {
            Debug.LogWarning(
                $"Save could not be loaded: {exception.Message}");
            return null;
        }
    }

    public bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}
