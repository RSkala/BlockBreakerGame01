using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTurret : MonoBehaviour
{
    Vector2 _mouseLookPosition;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnFire(InputValue inputValue)
    {
        FireProjectile();
    }

    void OnMouseMove(InputValue inputValue)
    {
        // While the mouse input value is a Vector2, we need to get the value as a Vector3 so we can pass into ScreenToWorldPoint
        Vector3 mousePosition = inputValue.Get<Vector2>();

        // Get the mouse's screen position in world coordinates
        Vector3 mouseWorldPosition = GameManager.Instance.MainCamera.ScreenToWorldPoint(mousePosition);
        _mouseLookPosition = mouseWorldPosition;
    }

    void FireProjectile()
    {
        // Get the direction from the turret's fire point to the mouse position
        Vector2 dirTurretToMouse = (_mouseLookPosition - new Vector2(transform.position.x, transform.position.y)).normalized;

        // Get the signed angle from the 2D world up position to the aiming direction
        float angle = Vector2.SignedAngle(Vector2.up, dirTurretToMouse);

        // Create the new projectile at the firepoint position, facing in the aiming direction
        Quaternion projectileRotation = Quaternion.Euler(0.0f, 0.0f, angle);
        GameObject.Instantiate(
            GameManager.Instance.ProjectilePrefab,
            transform.position,
            projectileRotation,
            GameManager.Instance.ProjectileParentTransform);
    }
}
