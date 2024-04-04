using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTurret : MonoBehaviour
{
    [Tooltip("The point at which a projectile will be fired from")]
    [SerializeField] Transform _turretFirePoint;
    
    [Tooltip("Rotation point of the turret's barrel")]
    [SerializeField] Transform _turretRotationPoint;

    // The mouse cursor's current position in game world coordinates
    Vector2 _mouseLookPosition;

    void Start()
    {
        
    }

    void Update()
    {
        // Rotate the turret barrel to point in the direction of the mouse position
        float angle = GetSignedAngleFromTurretToMouse();
        _turretRotationPoint.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
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
        // Get the angle the projectile should fired at
        float angle = GetSignedAngleFromTurretToMouse();

        // Create the new projectile at the firepoint position, facing in the aiming direction
        Quaternion projectileRotation = Quaternion.Euler(0.0f, 0.0f, angle);
        GameObject.Instantiate(
            GameManager.Instance.ProjectilePrefab,
            _turretFirePoint.position,
            projectileRotation,
            GameManager.Instance.ProjectileParentTransform);
    }

    float GetSignedAngleFromTurretToMouse()
    {
        // Get the direction from the turret's fire point to the mouse position
        Vector2 dirTurretToMouse = (_mouseLookPosition - new Vector2(transform.position.x, transform.position.y)).normalized;

        // Get the signed angle from the 2D world up position to the aiming direction
        return Vector2.SignedAngle(Vector2.up, dirTurretToMouse);
    }
}
