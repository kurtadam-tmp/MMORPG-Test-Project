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
