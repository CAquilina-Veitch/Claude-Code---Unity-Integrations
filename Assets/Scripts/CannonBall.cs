using UnityEngine;

public class CannonBall : MonoBehaviour
{
    [Header("Cannonball Settings")]
    public float speed = 5f;
    public float arcHeight = 2f;
    public float explosionRadius = 2f;
    public float explosionDamage = 50f;
    public GameObject explosionEffect;
    public LayerMask enemyLayer = 1;
    
    private Vector3 targetPosition;
    private Vector3 startPosition;
    private float journeyLength;
    private float journeyTime;
    private float elapsedTime;
    
    void Start()
    {
        startPosition = transform.position;
        journeyLength = Vector3.Distance(startPosition, targetPosition);
        journeyTime = journeyLength / speed;
    }
    
    void Update()
    {
        elapsedTime += Time.deltaTime;
        float fractionOfJourney = elapsedTime / journeyTime;
        
        if (fractionOfJourney >= 1f)
        {
            Explode();
            return;
        }
        
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
        
        float arc = arcHeight * Mathf.Sin(fractionOfJourney * Mathf.PI);
        currentPos.y += arc;
        
        transform.position = currentPos;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.LookAt(transform.position + direction);
    }
    
    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
    }
    
    void Explode()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);
        
        foreach (Collider enemyCol in enemies)
        {
            Enemy enemy = enemyCol.GetComponent<Enemy>();
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                enemy.TakeDamage(explosionDamage * damageMultiplier);
            }
        }
        
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayExplosion(transform.position);
        }
        
        Destroy(gameObject);
    }
}