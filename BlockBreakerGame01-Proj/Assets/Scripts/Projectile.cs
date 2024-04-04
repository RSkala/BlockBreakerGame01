using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("How quickly the projectile moves")]
    [SerializeField] float _moveSpeed;

    [Tooltip("How long a projectile stays alive (in seconds) before being removed from the scene")]
    [SerializeField] float _lifetime;

    Rigidbody2D _rigidbody2D;
    float _timeAlive;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _timeAlive = 0.0f;

        if(Mathf.Approximately(_moveSpeed, 0.0f))
        {
            Debug.LogWarning("Projectile.Start - _moveSpeed is zero. The projectile will not move. Check the Inspector values in the prefab.");
        }

        if(Mathf.Approximately(_lifetime, 0.0f))
        {
            Debug.LogWarning("Projectile.Start - _lifetime is zero. The projectile will immediately die. Check the Inspector values in the prefab.");
        }
    }

    void FixedUpdate()
    {
        // Get this projectile's forward direction. Note that in 2D, the up should be considered the forward direction.
        Vector2 movementDirection = _rigidbody2D.transform.up;

        // Move this projectile in the forward direction using its speed
        Vector2 newPosition = _rigidbody2D.position + movementDirection * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody2D.MovePosition(newPosition);

        _timeAlive += Time.fixedDeltaTime;
        if(_timeAlive >= _lifetime)
        {
            Destroy(gameObject);
        }
    }
}
