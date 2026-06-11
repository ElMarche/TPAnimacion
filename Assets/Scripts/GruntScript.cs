using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GruntScript : MonoBehaviour
{
    private float LastShoot;
    private int Health = 3;
    private int maxHealth = 3;
    private bool isDead;
    private Animator animator;
    private Collider2D gruntCollider;
    private Rigidbody2D rig;
    private Vector2 patrolOrigin;
    private Vector2 patrolTarget;
    private bool canSeeJohn;

    [Header("Grunt Configure")]
    [SerializeField] private GameObject John;
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float deathAnimationDuration = 1.0f;

    [Header("Patrol")]
    [SerializeField] private bool canPatrol = true;
    [SerializeField] private Vector2 pointAOffset = new Vector2(-0.75f, 0.0f);
    [SerializeField] private Vector2 pointBOffset = new Vector2(0.75f, 0.0f);
    [SerializeField, Min(0.1f)] private float patrolTravelTime = 2.0f;
    [SerializeField, Min(0.1f)] private float distanceToPoints = 0.1f;

    [Header("Vision")]
    [SerializeField, Min(0.0f)] private float visionDistance = 1.5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField, Min(0.05f)] private float shootInterval = 0.35f;

    // for the health bar
    [SerializeField] private HealthBar healthBar;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        gruntCollider = GetComponent<Collider2D>();
        rig = GetComponent<Rigidbody2D>();
        patrolOrigin = transform.position;
        patrolTarget = GetPointB();
    }

    // Start is called before the first frame update
    void Start()
    {
        healthBar.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        if (GameManager.Instancia.GetEstados() != Estados.Playing)
        {
            StopActions();
            return;
        }

        canSeeJohn = CanSeeJohn();
        animator.SetBool("Shooting", canSeeJohn);
        animator.SetBool("Walking", !canSeeJohn && canPatrol);

        if (canSeeJohn)
        {
            FacePosition(John.transform.position);
            rig.velocity = Vector2.zero;

            if (Time.time > LastShoot + shootInterval)
            {
                Shoot();
                LastShoot = Time.time;
            }
        }
        else if (!canPatrol)
        {
            rig.velocity = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || !canPatrol || canSeeJohn
            || GameManager.Instancia.GetEstados() != Estados.Playing)
        {
            return;
        }

        Patrol();
    }

    private void Patrol()
    {
        Vector2 pointA = GetPointA();
        Vector2 pointB = GetPointB();
        float patrolDistance = Vector2.Distance(pointA, pointB);

        if (patrolDistance <= Mathf.Epsilon)
        {
            rig.velocity = Vector2.zero;
            animator.SetBool("Walking", false);
            return;
        }

        float speed = patrolDistance / Mathf.Max(0.1f, patrolTravelTime);
        Vector2 nextPosition = Vector2.MoveTowards(rig.position, patrolTarget, speed * Time.fixedDeltaTime);
        FacePosition(patrolTarget);
        rig.MovePosition(nextPosition);

        if (Vector2.Distance(nextPosition, patrolTarget) <= distanceToPoints)
        {
            patrolTarget = Vector2.Distance(patrolTarget, pointA) <= distanceToPoints ? pointB : pointA;
        }
    }

    private bool CanSeeJohn()
    {
        if (John == null || visionDistance <= 0.0f) return false;

        Vector2 origin = GetVisionOrigin();
        Vector2 direction = (Vector2)John.transform.position - origin;

        if (direction.sqrMagnitude > visionDistance * visionDistance) return false;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized, visionDistance, playerLayer);
        return hit.collider != null && hit.collider.gameObject == John;
    }

    private void FacePosition(Vector2 position)
    {
        float direction = position.x >= transform.position.x ? 1.0f : -1.0f;
        transform.localScale = new Vector3(direction, 1.0f, 1.0f);
    }

    private void StopActions()
    {
        canSeeJohn = false;
        rig.velocity = Vector2.zero;
        animator.SetBool("Walking", false);
        animator.SetBool("Shooting", false);
    }

    private void Shoot()
    {
        //Debug.Log("Shooting");        
        Vector3 Shootdirection;
        if (transform.localScale.x == 1.0f) Shootdirection = Vector2.right;
        else Shootdirection = Vector2.left;
        GameObject bullet = Instantiate(BulletPrefab, transform.position + Shootdirection * 0.1f, Quaternion.identity);
        bullet.GetComponent<BulletScript>().SetDirection(Shootdirection);
    }

    public void Hit()
    {
        if (isDead) return;

        SpawnHitEffect();
        Health--;
        healthBar.SetHealth(Health);
        if (Health <= 0)
        {
            StartCoroutine(Death());
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab == null) return;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Vector3 spawnPosition = spriteRenderer != null
            ? spriteRenderer.bounds.center
            : transform.position;

        Instantiate(hitEffectPrefab, spawnPosition, Quaternion.identity);
    }

    private IEnumerator Death()
    {
        isDead = true;
        animator.SetBool("Walking", false);
        animator.SetBool("Shooting", false);
        rig.velocity = Vector2.zero;
        rig.bodyType = RigidbodyType2D.Kinematic;

        if (gruntCollider != null)
        {
            gruntCollider.enabled = false;
        }

        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }

        animator.SetTrigger("Die");
        yield return new WaitForSeconds(deathAnimationDuration);

        GameManager.Instancia.decreaseGrunt();
        gameObject.SetActive(false);
    }

    private Vector2 GetPointA()
    {
        return patrolOrigin + pointAOffset;
    }

    private Vector2 GetPointB()
    {
        return patrolOrigin + pointBOffset;
    }

    private Vector2 GetVisionOrigin()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        return spriteRenderer != null ? spriteRenderer.bounds.center : transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = Application.isPlaying ? patrolOrigin : (Vector2)transform.position;
        Vector2 pointA = origin + pointAOffset;
        Vector2 pointB = origin + pointBOffset;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawWireSphere(pointA, 0.06f);
        Gizmos.DrawWireSphere(pointB, 0.06f);

        Vector2 visionOrigin = Application.isPlaying ? GetVisionOrigin() : (Vector2)transform.position;
        Vector2 visionDirection;

        if (John != null)
        {
            visionDirection = ((Vector2)John.transform.position - visionOrigin).normalized;
        }
        else
        {
            visionDirection = transform.localScale.x >= 0.0f ? Vector2.right : Vector2.left;
        }

        Gizmos.color = canSeeJohn ? Color.red : Color.yellow;
        Gizmos.DrawLine(visionOrigin, visionOrigin + visionDirection * visionDistance);
        Gizmos.DrawWireSphere(visionOrigin, visionDistance);
    }
}
