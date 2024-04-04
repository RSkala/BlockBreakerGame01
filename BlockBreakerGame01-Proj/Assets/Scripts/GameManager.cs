using System.Collections.Generic;
using System.Linq;
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

    [Tooltip("Game Over / You Win screen displayed when the player breaks all breakable blocks")]
    [SerializeField] GameObject _gameOverScreen;

    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Cache the "main" camera to avoid the small overhead of repeatedly calling Camera.main
    public Camera MainCamera { get; private set; }

    // Parent of all fired projectiles. Used for containing the projectiles to keep the Hierarchy view clean.
    public Transform ProjectileParentTransform { get; private set; }

    // Current player turrent
    PlayerTurret _playerTurret;

    // Current number of active breakable blocks. Used for checking if the player has broken all breakable blocks.
    int _numActiveBreakableBlocks;

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

        StartNewGame();
    }

    void StartNewGame()
    {
        // Create player
        CreatePlayerTurret();

        // Hide game over screen
        _gameOverScreen.SetActive(false);

        // Get the number of breakable blocks in the scene for checking game over
        _numActiveBreakableBlocks = GetNumActiveBreakableBlocks();

        // Ensure there is at least 1 breakable block
        if(_numActiveBreakableBlocks <= 0)
        {
            Debug.LogError("There are no active breakable blocks in the scene on game start. Check the scene.");
        }
    }

    void CreatePlayerTurret()
    {
        // Create the player turret at the spawn point
        _playerTurret = GameObject.Instantiate(_playerTurretPrefab, _playerTurretPosition.position, Quaternion.identity);
        _playerTurret.name = "Player Turret";
    }

    int GetNumActiveBreakableBlocks()
    {
        // Find all Breakable Blocks in the scene
        List<BreakableBlock> breakableBlocks = Object.FindObjectsByType<BreakableBlock>(FindObjectsSortMode.None).ToList();

        // Filter out the blocks marked "Unbreakable"
        breakableBlocks.RemoveAll((breakableBlock) => {
            return breakableBlock.Unbreakable;
        });

        return breakableBlocks.Count;
    }

    public void OnBreakableBlockDestroyed()
    {
        --_numActiveBreakableBlocks;
        if(_numActiveBreakableBlocks <= 0)
        {
            // Player has destroyed all breakable blocks in the scene. Remove the player turret.
            Destroy(_playerTurret.gameObject);

            // Show the Game Over / You Win screen
            _gameOverScreen.SetActive(true);
        }
    }
}
