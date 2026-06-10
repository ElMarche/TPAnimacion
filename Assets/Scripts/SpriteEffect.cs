using UnityEngine;

public class SpriteEffect : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameDuration = 0.06f;

    private SpriteRenderer spriteRenderer;
    private float elapsedTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0 || frameDuration <= 0.0f)
        {
            Destroy(gameObject);
            return;
        }

        elapsedTime += Time.deltaTime;
        int frameIndex = Mathf.FloorToInt(elapsedTime / frameDuration);

        if (frameIndex >= frames.Length)
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer.sprite = frames[frameIndex];
    }
}
