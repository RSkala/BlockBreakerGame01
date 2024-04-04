using System.Reflection;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Tooltip("The player turret from which projectiles are fired")]
    [SerializeField] PlayerTurret _playerTurretPrefab;

    [Tooltip("Position at which to spawn the player turret")]
    [SerializeField] Transform _playerTurretPosition;

    [Tooltip("The projectile that will be fired from the player turret")]
    [field:SerializeField] public Projectile ProjectilePrefab { get; private set; }

    public static GameManager Instance { get; private set; }

    // Cache the "main" camera to avoid the small overhead of repeatedly calling Camera.main
    public Camera MainCamera { get; private set; }

    // Parent of all fired projectiles. Used for containing the projectiles to keep the Hierarchy view clean.
    public Transform ProjectileParentTransform { get; private set; }

    void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning(GetType().ToString() + "." + MethodBase.GetCurrentMethod().Name + " Singleton Instance already exists!");
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    void Start()
    {
        MainCamera = Camera.main;

        // Create the projectile parent in the scene
        GameObject projectileParent = new GameObject("[Projectiles]");
        ProjectileParentTransform = projectileParent.transform;

        // Hide the player turret spawn positions sprite
        _playerTurretPosition.GetComponent<SpriteRenderer>().enabled = false;

        // Create the player turret at the spawn point
        PlayerTurret playerTurret = GameObject.Instantiate(_playerTurretPrefab, _playerTurretPosition.position, Quaternion.identity);
        playerTurret.name = "Player Turret";
    }
}
