using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class DynamicClassData
{
    public string classId;
    public string className;
    public string primaryRole;
    public string primaryAttribute;
    public string resourceType;
    public int baseHp;
}

[Serializable]
public class DynamicMapData
{
    public int zoneId;
    public string name;
    public int recommendedLevelMin;
    public int recommendedLevelMax;
}

public class CharacterSelectUIController : MonoBehaviour
{
    [Header("API Gateway Settings")]
    public string GatewayApiUrl = "http://localhost:5000";

    [Header("Character Selection UI")]
    public Transform CharacterListContainer;
    public GameObject CharacterButtonPrefab;
    public Text StatusText;

    [Header("Character Creation UI")]
    public InputField NewCharacterNameInput;
    public Dropdown ClassDropdown;
    public Dropdown ZoneDropdown;
    public Text ClassDetailsText;

    public static string ActiveHandoffToken { get; private set; }
    public static Guid SelectedCharacterId { get; private set; }
    public static int SelectedZoneId { get; private set; } = 1;

    private List<DynamicClassData> _loadedClasses = new List<DynamicClassData>();
    private List<DynamicMapData> _loadedMaps = new List<DynamicMapData>();

    private void Start()
    {
        StartCoroutine(FetchClassesAndMaps());
        StartCoroutine(FetchCharacterList());
    }

    private IEnumerator FetchClassesAndMaps()
    {
        // Fetch Classes from CMS REST API
        using (UnityWebRequest www = UnityWebRequest.Get($"{GatewayApiUrl}/api/editor/classes"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = "{\"items\":" + www.downloadHandler.text + "}";
                var wrapper = JsonUtilityHelper.FromJson<ClassArrayWrapper>(json);
                if (wrapper != null && wrapper.items != null)
                {
                    _loadedClasses = new List<DynamicClassData>(wrapper.items);
                    PopulateClassDropdown();
                }
            }
        }

        // Fetch Maps from CMS REST API
        using (UnityWebRequest www = UnityWebRequest.Get($"{GatewayApiUrl}/api/editor/maps"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = "{\"items\":" + www.downloadHandler.text + "}";
                var wrapper = JsonUtilityHelper.FromJson<MapArrayWrapper>(json);
                if (wrapper != null && wrapper.items != null)
                {
                    _loadedMaps = new List<DynamicMapData>(wrapper.items);
                    PopulateZoneDropdown();
                }
            }
        }
    }

    private void PopulateClassDropdown()
    {
        if (ClassDropdown == null) return;
        ClassDropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (var c in _loadedClasses)
        {
            options.Add(c.className);
        }

        ClassDropdown.AddOptions(options);
        ClassDropdown.onValueChanged.AddListener(OnClassSelected);
        OnClassSelected(0);
    }

    private void PopulateZoneDropdown()
    {
        if (ZoneDropdown == null) return;
        ZoneDropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (var m in _loadedMaps)
        {
            options.Add($"Zone #{m.zoneId}: {m.name} (Lvl {m.recommendedLevelMin}-{m.recommendedLevelMax})");
        }

        ZoneDropdown.AddOptions(options);
        ZoneDropdown.onValueChanged.AddListener(index =>
        {
            if (index >= 0 && index < _loadedMaps.Count)
            {
                SelectedZoneId = _loadedMaps[index].zoneId;
            }
        });
    }

    private void OnClassSelected(int index)
    {
        if (index >= 0 && index < _loadedClasses.Count && ClassDetailsText != null)
        {
            var c = _loadedClasses[index];
            ClassDetailsText.text = $"<b>{c.className}</b>\nRol: {c.primaryRole}\nAna Stat: {c.primaryAttribute} ({c.resourceType})\nBase HP: {c.baseHp}";
        }
    }

    private IEnumerator FetchCharacterList()
    {
        string token = LoginUIController.ActiveSessionToken;
        using (UnityWebRequest www = UnityWebRequest.Get($"{GatewayApiUrl}/api/character/list?sessionToken={token}"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[CharacterSelect] Characters Loaded: {www.downloadHandler.text}");
            }
        }
    }

    public void OnClickCreateCharacter()
    {
        StartCoroutine(SendCreateCharacterRequest());
    }

    private IEnumerator SendCreateCharacterRequest()
    {
        string token = LoginUIController.ActiveSessionToken;
        string className = ClassDropdown != null && ClassDropdown.options.Count > 0 
            ? ClassDropdown.options[ClassDropdown.value].text 
            : "Warrior";
        string jsonBody = $"{{\"SessionToken\":\"{token}\",\"Name\":\"{NewCharacterNameInput.text}\",\"CharacterClass\":\"{className}\"}}";

        using (UnityWebRequest www = new UnityWebRequest($"{GatewayApiUrl}/api/character/create", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                StartCoroutine(FetchCharacterList());
            }
        }
    }

    public void OnSelectCharacter(string characterIdGuid)
    {
        SelectedCharacterId = Guid.Parse(characterIdGuid);
        StartCoroutine(SendSelectCharacterRequest(SelectedCharacterId, SelectedZoneId));
    }

    private IEnumerator SendSelectCharacterRequest(Guid charId, int zoneId)
    {
        string token = LoginUIController.ActiveSessionToken;
        string jsonBody = $"{{\"SessionToken\":\"{token}\",\"CharacterId\":\"{charId}\",\"TargetZoneId\":{zoneId}}}";

        using (UnityWebRequest www = new UnityWebRequest($"{GatewayApiUrl}/api/character/select", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("WorldZoneScene");
            }
        }
    }

    [Serializable]
    public class ClassArrayWrapper { public DynamicClassData[] items; }

    [Serializable]
    public class MapArrayWrapper { public DynamicMapData[] items; }
}

public static class JsonUtilityHelper
{
    public static T FromJson<T>(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
