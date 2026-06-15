using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour
{
    [Header ("Crate configure")]
    [SerializeField] private int Health = 2;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject crateExplosionPrefab;

    public void Hit()
    {
        SpawnHitEffect();
        Health--;
        if (Health <= 0)
        {
            StartCoroutine(Explode());
        }
    }

    private IEnumerator Explode()
    {
        SpawnExplosionPrefab();
        yield return new WaitForSeconds(0.2f);
        gameObject.SetActive(false);
    }

    private void SpawnExplosionPrefab()
    {
        if (crateExplosionPrefab == null) return;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Vector3 spawnPosition = spriteRenderer != null
            ? spriteRenderer.bounds.center
            : transform.position;

        Instantiate(crateExplosionPrefab, spawnPosition, Quaternion.identity);
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
}
