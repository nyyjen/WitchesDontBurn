using UnityEngine;

public class ShootingWater : MonoBehaviour
{
    [Tooltip("Seconds before the projectile self-destroys if it doesn't hit anything")]
    public float lifeTime = 5f;

    private void Start()
    {
        // safety destroy in case the spawner doesn't
        if (lifeTime > 0f)
            Destroy(gameObject, lifeTime);
    }

    // Handle trigger-based collisions
    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessHit(other);
    }

    // Handle physics collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessHit(collision.collider);
    }

    private void ProcessHit(Collider2D collider)
    {
        if (collider == null) return;

        // Try to find a WindowStatus on the hit object or its parents
        // var window = collider.GetComponent<WindowStatus>();
        // if (window == null)
        //     window = collider.GetComponentInParent<WindowStatus>();

        // if (window != null)
        // {
        //     // call extinguish if available
        //     window.Extinguish();
        // }

        // destroy projectile on any hit
        Destroy(gameObject);
    }
}