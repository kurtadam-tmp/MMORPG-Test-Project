using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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

    public static string ActiveHandoffToken { get; private set; }
    public static Guid SelectedCharacterId { get; private set; }

    private void Start()
    {
        StartCoroutine(FetchCharacterList());
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
        string className = ClassDropdown.options[ClassDropdown.value].text;
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
        StartCoroutine(SendSelectCharacterRequest(SelectedCharacterId));
    }

    private IEnumerator SendSelectCharacterRequest(Guid charId)
    {
        string token = LoginUIController.ActiveSessionToken;
        string jsonBody = $"{{\"SessionToken\":\"{token}\",\"CharacterId\":\"{charId}\",\"TargetZoneId\":1}}";

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
}
