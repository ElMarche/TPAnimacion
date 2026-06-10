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

    [Header("Grunt Configure")]
    [SerializeField] private GameObject John;
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private float deathAnimationDuration = 1.0f;
    // for the health bar
    [SerializeField] private HealthBar healthBar;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        gruntCollider = GetComponent<Collider2D>();
        rig = GetComponent<Rigidbody2D>();
        healthBar.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {       // Calculating for the enemy srpite to face the player
        if (John == null || isDead) return;
        Vector3 direction = John.transform.position - transform.position;
        if (direction.x >= 0.0f)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
        else { transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f); }

        float distance = Mathf.Abs(John.transform.position.x - transform.position.x);
        if (distance < 1.0f && Time.time > LastShoot + 0.35f)
        {
            Shoot();
            LastShoot = Time.time;
        }
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

        Health--;
        healthBar.SetHealth(Health);
        if (Health <= 0)
        {
            StartCoroutine(Death());
        }
    }

    private IEnumerator Death()
    {
        isDead = true;
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
}
