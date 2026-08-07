# Unity C# Integration Guide & Complete Client UI/HUD System (`MMORPG.Shared`)

This document provides a comprehensive integration guide, ready-to-use C# UI Controllers, step-by-step **Unity Editor Canvas & Inspector Wiring Instructions**, and **Visual VFX / Floating HP Bar / Damage Text Systems** for Unity developers building client applications targeting the **MMORPG Dedicated Zone Server**.

---

## 1. Assembly Import

1. Build the shared assembly:
   ```bash
   dotnet build src/MMORPG.Shared/MMORPG.Shared.csproj -c Release
   ```
2. Copy `src/MMORPG.Shared/bin/Release/netstandard2.1/MMORPG.Shared.dll` into your Unity project's `Assets/Plugins/` directory.

---

## 2. World-Space Floating Health Bar Script (`FloatingHealthBar.cs`)

Attach this component to your Player and Mob Prefabs (`WorldSpace` Canvas):

```csharp
using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider HealthSlider;
    public Image FillImage;
    public Text NameText;
    public Text LevelText;

    [Header("Colors")]
    public Color PlayerColor = new Color(0f, 0.95f, 0.4f);
    public Color HostileMobColor = new Color(1f, 0.2f, 0.2f);

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        // Billboard effect: Always face the active game camera
        if (_mainCamera != null)
        {
            transform.rotation = _mainCamera.transform.rotation;
        }
    }

    public void Initialize(string entityName, int level, bool isHostile)
    {
        if (NameText != null) NameText.text = entityName;
        if (LevelText != null) LevelText.text = $"Lvl {level}";
        if (FillImage != null) FillImage.color = isHostile ? HostileMobColor : PlayerColor;
    }

    public void UpdateHealth(int currentHp, int maxHp)
    {
        if (HealthSlider != null && maxHp > 0)
        {
            HealthSlider.value = (float)currentHp / maxHp;
        }
    }
}
```

---

## 3. Floating Damage Numbers System (`DamageTextManager.cs`)

Spawn floating damage numbers over entities when taking combat damage:

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance { get; private set; }

    [Header("Prefab Settings")]
    public GameObject DamageTextPrefab;
    public Canvas WorldSpaceCanvas;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnDamageText(Vector3 worldPosition, int damageAmount, bool isCritical)
    {
        if (DamageTextPrefab == null || WorldSpaceCanvas == null) return;

        GameObject textObj = Instantiate(DamageTextPrefab, worldPosition + Vector3.up * 2f, Quaternion.identity, WorldSpaceCanvas.transform);
        Text txt = textObj.GetComponentInChildren<Text>();

        if (txt != null)
        {
            txt.text = damageAmount.ToString();
            txt.fontSize = isCritical ? 36 : 24;
            txt.color = isCritical ? Color.yellow : Color.red;
        }

        StartCoroutine(AnimateAndDestroy(textObj));
    }

    private IEnumerator AnimateAndDestroy(GameObject obj)
    {
        float duration = 1.0f;
        float elapsed = 0f;
        Vector3 startPos = obj.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            obj.transform.position = startPos + Vector3.up * (elapsed * 1.5f);
            yield return null;
        }

        Destroy(obj);
    }
}
```

---

## 4. Visual Effects & VFX Pooler (`VFXManager.cs`)

Manage skill particle effects, impact VFX, and level-up animations:

```csharp
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Particle Effect Prefabs")]
    public GameObject WarriorSlashVFX;
    public GameObject MageFireballVFX;
    public GameObject LevelUpVFX;
    public GameObject DeathPoofVFX;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySkillVFX(int skillId, Vector3 targetPosition)
    {
        GameObject prefabToSpawn = skillId switch
        {
            1 => WarriorSlashVFX,
            2 => MageFireballVFX,
            _ => WarriorSlashVFX
        };

        if (prefabToSpawn != null)
        {
            GameObject vfxObj = Instantiate(prefabToSpawn, targetPosition, Quaternion.identity);
            Destroy(vfxObj, 3.0f); // Auto-recycle after 3 seconds
        }
    }

    public void PlayLevelUpVFX(Vector3 playerPosition)
    {
        if (LevelUpVFX != null)
        {
            GameObject vfxObj = Instantiate(LevelUpVFX, playerPosition, Quaternion.identity);
            Destroy(vfxObj, 4.0f);
        }
    }
}
```

---

## 5. Login & Character Select UI Controllers (`LoginUIController.cs`)

```csharp
using System.Collections;
using System.Text;
using MMORPG.Shared.DTOs;
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
```

---

## 6. Architecture Overview Pipeline

```
[Unity Client] ---> HTTP REST (Session/Handoff) ---> [Auth Gateway API]
                                                            |
                                                   (ZoneHandoffToken)
                                                            v
[Unity Client] ---> UDP Port 7777 (Handshake) ---> [Dedicated Zone Server]
                                                            |
                                                    (30 Hz Game Loop)
                                                            |
                 +------------------------------------------+
                 |
                 v
[Visual FX / Floating HP Bar / Damage Numbers / HUD UI Sync]
```
