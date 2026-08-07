using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginUIController : MonoBehaviour
{
    [Header("API Gateway Settings")]
    public string GatewayApiUrl = "http://localhost:5000";

    [Header("UI Controls")]
    public InputField UsernameInput;
    public InputField PasswordInput;
    public Text StatusText;
    public Button LoginButton;
    public Button RegisterButton;

    public static string ActiveSessionToken { get; private set; }

    public void OnClickLogin()
    {
        StartCoroutine(SendLoginRequest());
    }

    public void OnClickRegister()
    {
        StartCoroutine(SendRegisterRequest());
    }

    private IEnumerator SendLoginRequest()
    {
        SetStatus("Authenticating...");
        string jsonBody = $"{{\"UsernameOrEmail\":\"{UsernameInput.text}\",\"Password\":\"{PasswordInput.text}\"}}";
        
        using (UnityWebRequest www = new UnityWebRequest($"{GatewayApiUrl}/api/auth/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                SetStatus("Login successful!");
                ActiveSessionToken = ExtractSessionToken(www.downloadHandler.text);
                UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelectScene");
            }
            else
            {
                SetStatus($"Login failed: {www.error}");
            }
        }
    }

    private IEnumerator SendRegisterRequest()
    {
        SetStatus("Registering new account...");
        string jsonBody = $"{{\"Username\":\"{UsernameInput.text}\",\"Password\":\"{PasswordInput.text}\",\"Email\":\"{UsernameInput.text}@mmorpg.local\"}}";
        
        using (UnityWebRequest www = new UnityWebRequest($"{GatewayApiUrl}/api/auth/register", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                SetStatus("Account created! Please click Login.");
            }
            else
            {
                SetStatus($"Registration failed: {www.error}");
            }
        }
    }

    private void SetStatus(string msg)
    {
        if (StatusText != null) StatusText.text = msg;
    }

    private string ExtractSessionToken(string jsonResponse)
    {
        int keyIndex = jsonResponse.IndexOf("\"sessionToken\":");
        if (keyIndex != -1)
        {
            int start = jsonResponse.IndexOf("\"", keyIndex + 15) + 1;
            int end = jsonResponse.IndexOf("\"", start);
            return jsonResponse.Substring(start, end - start);
        }
        return string.Empty;
    }
}
