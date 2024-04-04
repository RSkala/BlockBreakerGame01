using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Tooltip("The player turret from which projectiles are fired")]
    [SerializeField] PlayerTurret _playerTurretPrefab;

    [Tooltip("The projectile that will be fired from the player turret")]
    [field:SerializeField] public Projectile ProjectilePrefab { get; private set; }

    [Tooltip("Game Over / You Win screen displayed when the player breaks all breakable blocks")]
    [SerializeField] GameObject _gameOverScreen;

    [Tooltip("Press to start game")]
    [SerializeField] Button _startGameButton;

    [Tooltip("The list of Game Layouts to choose from to play")]
    [SerializeField] GameLayout[] _gameLayoutPrefabs;

    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Cache the "main" camera to avoid the small overhead of repeatedly calling Camera.main
    public Camera MainCamera { get; private set; }

    // Parent of all fired projectiles. Used for containing the projectiles to keep the Hierarchy view clean.
    public Transform ProjectileParentTransform { get; private set; }

    // Current Game Layout that is playing
    GameLayout _currentGameLayout;

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

        // Handle button clicks
        _startGameButton.onClick.AddListener(OnStartGameButtonClicked);

        // Start a new game
        StartNewGame();
    }

    void StartNewGame()
    {
        // Create Game Layout
        CreateGameLayout();

        // Hide game over screen
        _gameOverScreen.SetActive(false);

        // Get the number of breakable blocks in the scene for checking game over
        _numActiveBreakableBlocks = _currentGameLayout.NumBreakableBlocks;

        // Ensure there is at least 1 breakable block
        if(_numActiveBreakableBlocks <= 0)
        {
            Debug.LogError("There are no active breakable blocks in the scene on game start. Check the scene.");
        }
    }

    void CreateGameLayout()
    {
        if(_gameLayoutPrefabs.Length <= 0)
        {
            Debug.LogError("There are no game layout prefabs assigned in the GameManager Inspector. A game will not be created.");
            return;
        }

        // Choose a random game layout from the list of game layout prefabs
        int randomIdx = Random.Range(0, _gameLayoutPrefabs.Length);
        _currentGameLayout = GameObject.Instantiate(_gameLayoutPrefabs[randomIdx]);
        _currentGameLayout.Init(_playerTurretPrefab);
    }

    public void OnBreakableBlockDestroyed()
    {
        --_numActiveBreakableBlocks;
        if(_numActiveBreakableBlocks <= 0)
        {
            // Player has destroyed all breakable blocks in the scene. Remove the current game layout.
            Destroy(_currentGameLayout.gameObject);

            // Show the Game Over / You Win screen
            _gameOverScreen.SetActive(true);
        }
    }
    
    void OnStartGameButtonClicked()
    {
        StartNewGame();
    }
}
