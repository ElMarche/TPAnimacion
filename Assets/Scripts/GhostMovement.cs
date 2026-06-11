using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float riseSpeed = 0.45f;
    [SerializeField] private float zigzagAmplitude = 0.12f;
    [SerializeField] private float zigzagFrequency = 1.5f;
    [SerializeField] private float lifetime = 2.5f;

    [Header("Appearance")]
    [SerializeField] private Sprite[] animationFrames;
    [SerializeField] private float frameDuration = 0.15f;
    [SerializeField, Range(0.0f, 1.0f)] private float fadeStart = 0.55f;

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private Color startColor;
    private float elapsedTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        startColor = spriteRenderer.color;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        float horizontalOffset = Mathf.Sin(elapsedTime * zigzagFrequency * Mathf.PI * 2.0f)
            * zigzagAmplitude;
        transform.position = startPosition
            + Vector3.up * riseSpeed * elapsedTime
            + Vector3.right * horizontalOffset;

        AnimateSprite();
        FadeOut();

        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void AnimateSprite()
    {
        if (animationFrames == null || animationFrames.Length == 0 || frameDuration <= 0.0f)
        {
            return;
        }

        int frameIndex = Mathf.FloorToInt(elapsedTime / frameDuration) % animationFrames.Length;
        spriteRenderer.sprite = animationFrames[frameIndex];
    }

    private void FadeOut()
    {
        float normalizedTime = Mathf.Clamp01(elapsedTime / lifetime);
        float fadeProgress = Mathf.InverseLerp(fadeStart, 1.0f, normalizedTime);
        Color color = startColor;
        color.a = Mathf.Lerp(startColor.a, 0.0f, fadeProgress);
        spriteRenderer.color = color;
    }
}
