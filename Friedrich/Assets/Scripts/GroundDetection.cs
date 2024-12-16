using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float checkRadius = 0.1f;
    public bool isGrounded;

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}
