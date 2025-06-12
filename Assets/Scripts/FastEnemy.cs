using UnityEngine;

public class FastEnemy : Enemy
{
    [Header("Fast Enemy Settings")]
    public float dashSpeed = 8f;
    public float dashDuration = 1f;
    public float dashCooldown = 5f;
    
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private float originalSpeed;
    
    protected override void Start()
    {
        base.Start();
        originalSpeed = moveSpeed;
        moveSpeed = 3f;
        maxHealth = 60f;
        moneyReward = 35;
        scoreReward = 15;
    }
    
    protected override void Update()
    {
        base.Update();
        
        HandleDash();
    }
    
    void HandleDash()
    {
        if (isDashing)
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashDuration)
            {
                EndDash();
            }
        }
        else
        {
            dashCooldownTimer += Time.deltaTime;
            if (dashCooldownTimer >= dashCooldown && !isDead)
            {
                StartDash();
            }
        }
    }
    
    void StartDash()
    {
        isDashing = true;
        dashTimer = 0f;
        moveSpeed = dashSpeed;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }
    }
    
    void EndDash()
    {
        isDashing = false;
        dashCooldownTimer = 0f;
        moveSpeed = originalSpeed;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.white;
        }
    }
}