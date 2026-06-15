using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Position and size")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.62f, 0f);
    [SerializeField] private Vector2 barSize = new Vector2(0.75f, 0.08f);
    [SerializeField] private float borderSize = 0.015f;

    [Header("Appearance")]
    [SerializeField]
    private Color backgroundColor =
        new Color(0.08f, 0.08f, 0.08f, 1f);

    [SerializeField]
    private Color fullHealthColor =
        new Color(0.2f, 0.85f, 0.25f, 1f);

    [SerializeField]
    private Color lowHealthColor =
        new Color(0.9f, 0.15f, 0.1f, 1f);

    [SerializeField] private int sortingOrder = 20;

    [Header("Visibility")]
    [SerializeField] private bool hideAtFullHealth = false;
    [SerializeField] private bool hideWhenDead = true;

    private static Sprite sharedSquareSprite;

    private EnemyHealth health;
    private Transform barRoot;
    private Transform fillTransform;
    private SpriteRenderer backgroundRenderer;
    private SpriteRenderer fillRenderer;
    private float lastHealthPercent = -1f;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        CreateBar();
    }

    private void LateUpdate()
    {
        if (health == null || barRoot == null)
            return;

        KeepBarFacingRight();
        UpdateBar();
    }

    private void CreateBar()
    {
        if (sharedSquareSprite == null)
            sharedSquareSprite = CreateSquareSprite();

        GameObject rootObject = new GameObject("HealthBar");
        barRoot = rootObject.transform;
        barRoot.SetParent(transform, false);
        barRoot.localPosition = localOffset;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(barRoot, false);
        backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = sharedSquareSprite;
        backgroundRenderer.color = backgroundColor;
        backgroundRenderer.sortingOrder = sortingOrder;
        backgroundObject.transform.localScale = new Vector3(
            barSize.x + borderSize * 2f,
            barSize.y + borderSize * 2f,
            1f);

        GameObject fillObject = new GameObject("Fill");
        fillTransform = fillObject.transform;
        fillTransform.SetParent(barRoot, false);
        fillRenderer = fillObject.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = sharedSquareSprite;
        fillRenderer.sortingOrder = sortingOrder + 1;

        UpdateBar(force: true);
    }

    private void UpdateBar(bool force = false)
    {
        int maxHealth = health.MaxHealth;
        float healthPercent = maxHealth > 0
            ? Mathf.Clamp01((float)health.CurrentHealth / maxHealth)
            : 0f;

        if (!force && Mathf.Approximately(lastHealthPercent, healthPercent))
            return;

        lastHealthPercent = healthPercent;

        fillTransform.localScale = new Vector3(
            barSize.x * healthPercent,
            barSize.y,
            1f);

        fillTransform.localPosition = new Vector3(
            -(barSize.x * (1f - healthPercent)) * 0.5f,
            0f,
            -0.01f);

        fillRenderer.color = Color.Lerp(
            lowHealthColor,
            fullHealthColor,
            healthPercent);

        bool visible =
            !(hideAtFullHealth && healthPercent >= 1f) &&
            !(hideWhenDead && health.IsDead);

        backgroundRenderer.enabled = visible;
        fillRenderer.enabled = visible;
    }

    private void KeepBarFacingRight()
    {
        float parentDirection = Mathf.Sign(transform.lossyScale.x);
        if (Mathf.Approximately(parentDirection, 0f))
            parentDirection = 1f;

        barRoot.localScale = new Vector3(parentDirection, 1f, 1f);
    }

    private static Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(
            1,
            1,
            TextureFormat.RGBA32,
            false);

        texture.name = "RuntimeHealthBarPixel";
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f);

        sprite.name = "RuntimeHealthBarSquare";
        return sprite;
    }
}
