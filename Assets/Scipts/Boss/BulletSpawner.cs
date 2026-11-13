using UnityEngine;
using System.Collections;

public class BulletSpawner : MonoBehaviour
{
    public enum SpawnerType
    {
        Straight,
        Spin
    }

    [Header("Bullet Attributes")]
    public GameObject bullet;
    public float bulletLife = 4f;
    public float speed = 5f;

    [Header("Spawner Attributes")]
    [SerializeField]
    private SpawnerType spawnerType = SpawnerType.Spin;

    [SerializeField]
    private float firingRate = 0.08f;

    [Header("Automatic Firing")]
    public bool automaticFire = false;
    public float automaticSpinSpeed = 60f;

    [Header("Boss Spiral")]
    public float spiralDuration = 3f;
    public float spiralRotations = 1f;

    [Header("Sound")]                                   
    public AudioSource audioSource;                     
    public AudioClip bulletSound;                      
    [Range(0f, 1f)]
    public float bulletVolume = 0.5f;                 

    private float timer = 0f;
    private bool spiralActive = false;

    private void Update()
    {
        if (!automaticFire || spiralActive)
            return;

        timer += Time.deltaTime;

        if (spawnerType == SpawnerType.Spin)
        {
            transform.Rotate(
                0f,
                0f,
                automaticSpinSpeed * Time.deltaTime
            );
        }

        if (timer >= firingRate)
        {
            Fire();
            timer = 0f;
        }
    }

    public IEnumerator FireOneSpiral()
    {
        if (spiralActive)
            yield break;

        spiralActive = true;

        float duration = Mathf.Max(
            0.01f, spiralDuration
        );

        float interval = Mathf.Max(
            0.01f, firingRate
        );

        float elapsed = 0f;
        float fireTimer = 0f;
        float startAngle = transform.eulerAngles.z;

        Fire();

        while (elapsed < duration)
        {
            float dt = Mathf.Min(
                Time.deltaTime,
                duration - elapsed
            );

            elapsed += dt;
            fireTimer += dt;

            float progress = elapsed / duration;

            float angle =
                startAngle +
                360f * spiralRotations * progress;

            transform.rotation = Quaternion.Euler(
                0f, 0f, angle
            );

            while (fireTimer >= interval)
            {
                Fire();
                fireTimer -= interval;
            }

            yield return null;
        }

        spiralActive = false;
    }

    private void Fire()
    {
        if (bullet == null)
            return;

        GameObject spawnedBullet = Instantiate(
            bullet,
            transform.position,
            transform.rotation
        );

      
        if (audioSource != null && bulletSound != null)     
            audioSource.PlayOneShot(bulletSound, bulletVolume);  

        Bullet bulletScript =
            spawnedBullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.speed = speed;
            bulletScript.bulletLife = bulletLife;
        }
    }
}