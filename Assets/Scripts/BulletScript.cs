using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Rigidbody2D Rigidbody2D;
    public float Speed; // public can be worked on from Unity editor.
    private Vector2 Direction;
    public AudioClip Sound;
    [SerializeField] private bool damageJohn = true;
    [SerializeField] private bool damageGrunts = true;
    [SerializeField] private float lifetime = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        if (Sound != null && Camera.main != null)
        {
            AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
            if (audioSource != null) audioSource.PlayOneShot(Sound);
        }

        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Rigidbody2D.velocity = Direction * Speed;
    }

    public void SetDirection(Vector2 direction)
    {
        Direction = direction;
    }

    public void DestroyBullet()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        JohnMovement John = collision.GetComponent<JohnMovement>();
        GruntScript Grunt = collision.GetComponent<GruntScript>();
        Crate crate = collision.GetComponent<Crate>();

        if (damageJohn && John != null) John.Hit();
        if (damageGrunts && Grunt != null) Grunt.Hit();
        if (crate != null) crate.Hit();
        {
            
        }

        DestroyBullet();
    }
}
