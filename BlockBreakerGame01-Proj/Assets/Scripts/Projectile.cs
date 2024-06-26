using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("How quickly the projectile moves")]
    [SerializeField] float _moveSpeed;

    [Tooltip("How long a projectile stays alive (in seconds) before being removed from the scene")]
    [SerializeField] float _lifetime;

    [Tooltip("How much damage a projectile deals to a breakable block")]
    [SerializeField] int _damage;

    public bool IsActive { get; private set; } = false;

    Rigidbody2D _rigidbody2D;
    float _timeAlive;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();

        CheckInspectorValues();

        // Start all projectiles deactivated
        Deactivate();
    }

    void CheckInspectorValues()
    {
        if(Mathf.Approximately(_moveSpeed, 0.0f))
        {
            Debug.LogWarning("Projectile.Start - _moveSpeed is zero. The projectile will not move. Check the Inspector values in the prefab.");
        }

        if(Mathf.Approximately(_lifetime, 0.0f))
        {
            Debug.LogWarning("Projectile.Start - _lifetime is zero. The projectile will immediately die. Check the Inspector values in the prefab.");
        }

        if(_damage <= 0)
        {
            Debug.LogWarning("Projectile.Start - _damage is invalid. The projectile will deal no damage to blocks. Check the Inspector values in the prefab.");
        }
    }

    void FixedUpdate()
    {
        // Get this projectile's forward direction. Note that in 2D, the up should be considered the forward direction.
        Vector2 movementDirection = _rigidbody2D.transform.up;

        // Move this projectile in the forward direction using its speed
        Vector2 newPosition = _rigidbody2D.position + movementDirection * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody2D.MovePosition(newPosition);

        // Remove this projectile from the scene when its time expires
        _timeAlive += Time.fixedDeltaTime;
        if(_timeAlive >= _lifetime)
        {
            Deactivate();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check the collided gameObject's component to determine how to handle this projectile collision
        if(collision.gameObject.TryGetComponent<BreakableBlock>(out var breakableBlock))
        {
            // Log this projectile's collision with a block
            GameManager.Instance.GameLogger.LogProjectileHitBlock(this, breakableBlock);

            // Set new projectile rotation
            SetProjectileRotationFromCollisionData(collision);

            // Deal damage to the collided breakable block
            breakableBlock.DealDamage(_damage);
        }
        else if(collision.gameObject.TryGetComponent<Wall>(out _))
        {
            SetProjectileRotationFromCollisionData(collision);
        }
    }

    void SetProjectileRotationFromCollisionData(Collision2D collision)
    {
        // Get the contact points from the collision
        List<ContactPoint2D> contactPoints = new List<ContactPoint2D>();
        int numContacts = collision.GetContacts(contactPoints);
        if(numContacts > 0)
        {
            // Get the first contact point and its contact normal
            ContactPoint2D contactPoint = contactPoints[0];
            Vector2 contactNormal = contactPoint.normal;

            // Get the direction of this projectile's forward direction
            Vector2 forwardMovementDir = _rigidbody2D.transform.up;

            // Calculate the reflection vector
            Vector2 reflectionVector = Vector2.Reflect(forwardMovementDir, contactNormal);

            // Get the angle between the world up and the reflection vector
            float angle = Vector2.SignedAngle(Vector2.up, reflectionVector);
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
        }
    }

    public void Activate()
    {
        _timeAlive = 0.0f;
        IsActive = true;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        _timeAlive = 0.0f;
        IsActive = false;
        gameObject.SetActive(false);
    }
}
