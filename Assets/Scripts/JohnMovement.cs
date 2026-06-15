using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class JohnMovement : MonoBehaviour
{
    [Header("John Configure")]
    [SerializeField] private float Speed;
    [SerializeField] private float JumpForce;
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private float GroundCheckDistance = 0.2f;
    [SerializeField] private float DeathZoneYDistance = -2.0f;

    [Header("Knife")]
    [SerializeField] private GameObject KnifePrefab;
    [SerializeField] private float knifeThrowDuration = 0.8f;
    [SerializeField] private float knifeReleaseDelay = 0.25f;

    [Header("Sounds")]
    [SerializeField] private AudioClip jumping;
    [SerializeField] private AudioClip landing;
    [SerializeField] private AudioClip hurting;
    [SerializeField] private AudioClip knifeSound;

    [Header("Death Movement")]
    [SerializeField] private float deathPushDistance = 0.25f;
    [SerializeField] private float deathPushDuration = 0.35f;
    [SerializeField] private float deathStartPush = 0.05f;
    [SerializeField] private GameObject ghostPrefab;

    // for the health bar
    [Header("Canvas")]
    [SerializeField] private HealthBar healthBar;

    private bool Grounded;
    private Rigidbody2D Rigidbody2D;
    private float Horizontal;
    private bool isDead;
    private Animator Animator;
    private float LastShoot;
    private float LastJump;
    private bool isThrowingKnife;
    private int Health = 5;
    private int maxHealth = 5;


    // Start is called before the first frame update
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>(); // get the component Rigidbody2D
        Animator = GetComponent<Animator>(); // get the component Animator from Unity
        healthBar.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        Estados currentState = GameManager.Instancia.GetEstados();

        if (currentState == Estados.Playing && !isDead && !isThrowingKnife)
        {
            Animator.SetFloat("velocityY", Rigidbody2D.velocity.y);

            Move();
            CheckGround();
            KeyDownActions();

            // Destroys John if it falls below certain limit
            if (transform.position.y <= DeathZoneYDistance)
            {
                Death();
            }
        }
        else if (currentState == Estados.JohnWin || currentState == Estados.JohnDead)
        {
            StopMovement();
        }
    }

    private void StopMovement()
    {
        Horizontal = 0.0f;
        Rigidbody2D.velocity = Vector2.zero;
        Animator.SetBool("running", false);
        Animator.SetFloat("velocityY", 0.0f);
    }

    private void Move()
    {
        Horizontal = Input.GetAxisRaw("Horizontal") * Speed; // GetAxisRaw to capture the movement on x Axis. (1,0 or -1)

        if (Horizontal < 0.0f) transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (Horizontal > 0.0f) transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        Animator.SetBool("running", Horizontal != 0.0f); // Horizontal == 0 means false.
    }

    private void Jump()
    {
        Rigidbody2D.AddForce(Vector2.up * JumpForce);
        Animator.SetTrigger("Jump");
        PlaySound(jumping);
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, GroundCheckDistance, GroundLayer);

        Grounded = hit.collider != null;
        Animator.SetBool("InGround", Grounded);

        Debug.DrawRay(origin, Vector3.down * GroundCheckDistance, Grounded ? Color.green : Color.red);
    }

    private void Shoot()
    {
        Vector3 direction;
        if (transform.localScale.x == 1.0f) direction = Vector2.right;
        else direction = Vector2.left;
        GameObject bullet = Instantiate(BulletPrefab, transform.position + direction * 0.1f, Quaternion.identity);
        bullet.GetComponent<BulletScript>().SetDirection(direction);
    }

    private IEnumerator ThrowKnife()
    {
        isThrowingKnife = true;
        StopMovement();
        Animator.SetTrigger("FireKnife");

        yield return new WaitForSeconds(knifeReleaseDelay);

        if (!isDead && GameManager.Instancia.GetEstados() == Estados.Playing)
        {
            Vector3 direction = transform.localScale.x >= 0.0f ? Vector3.right : Vector3.left;
            GameObject knife = Instantiate(
                KnifePrefab,
                transform.position + direction * 0.15f,
                Quaternion.identity
            );

            knife.transform.localScale = new Vector3(direction.x, 1.0f, 1.0f);
            knife.GetComponent<BulletScript>().SetDirection(direction);
        }

        float remainingDuration = Mathf.Max(0.0f, knifeThrowDuration - knifeReleaseDelay);
        yield return new WaitForSeconds(remainingDuration);
        isThrowingKnife = false;
    }

    public void Hit()
    {
        if (isDead) return;

        Health--;
        healthBar.SetHealth(Health);
        Animator.SetTrigger("Hit");
        PlaySound(hurting);

        if (Health == 0)
        {
            Death();
        }
    }

    private void Death()
    {
        isDead = true;
        Horizontal = 0.0f;
        Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        Rigidbody2D.velocity = Vector2.zero;
        Animator.SetBool("isDead", true);
        StartCoroutine(MoveBackOnDeath());
    }

    private void SpawnGhost()
    {
        if (ghostPrefab == null) return;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Vector3 spawnPosition = spriteRenderer != null
            ? spriteRenderer.bounds.center
            : transform.position;

        Instantiate(ghostPrefab, spawnPosition, Quaternion.identity);
    }

    private IEnumerator MoveBackOnDeath()
    {
        yield return new WaitForSeconds(deathStartPush);

        Vector2 startPosition = Rigidbody2D.position;
        float facingDirection = Mathf.Sign(transform.localScale.x);
        Vector2 endPosition = startPosition + Vector2.left * facingDirection * deathPushDistance;
        float elapsedTime = 0.0f;

        while (elapsedTime < deathPushDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / deathPushDuration);
            float easedProgress = Mathf.SmoothStep(0.0f, 1.0f, progress);
            Rigidbody2D.MovePosition(Vector2.Lerp(startPosition, endPosition, easedProgress));
            yield return new WaitForFixedUpdate();
        }

        SpawnGhost();
        Rigidbody2D.MovePosition(endPosition);
        GameManager.Instancia.ActualizarEstados(Estados.JohnDead);
    }

    void FixedUpdate()
    {
        if (isDead || isThrowingKnife || GameManager.Instancia.GetEstados() != Estados.Playing)
        {
            Rigidbody2D.velocity = Vector2.zero;
            return;
        }

        Rigidbody2D.velocity = new Vector2(Horizontal, Rigidbody2D.velocity.y);
    }

    private void KeyDownActions()
    {
        if (Input.GetKeyDown(KeyCode.W) && Grounded && Time.time > LastJump + 0.20f)
        {
            Jump();
            LastJump = Time.time;
        }

        if (Input.GetKey(KeyCode.Space) && Time.time > LastShoot + 0.25f)
        {
            Shoot();
            LastShoot = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.E) && Grounded && KnifePrefab != null)
        {
            StartCoroutine(ThrowKnife());
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
        {
            AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
            if (audioSource != null) audioSource.PlayOneShot(clip);
        }
    }

    public void LandingSound()
    {
        PlaySound(landing);
    }

    public void ThrowingKnifeSound()
    {
        PlaySound(knifeSound);
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;

        Gizmos.color = Grounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * GroundCheckDistance);
        Gizmos.DrawWireSphere(origin + Vector3.down * GroundCheckDistance, 0.02f);
    }
}
