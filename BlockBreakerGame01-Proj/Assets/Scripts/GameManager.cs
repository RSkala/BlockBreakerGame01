using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Tooltip("The player turret from which projectiles are fired")]
    [SerializeField] PlayerTurret _playerTurretPrefab;

    [Tooltip("The projectile that will be fired from the player turret")]
    [field:SerializeField] public Projectile ProjectilePrefab { get; private set; }

    [Tooltip("Title / Start Game screen. Displayed when the game starts")]
    [SerializeField] GameObject _startGameScreen;

    [Tooltip("Press to start game during the Title / Start Game screen")]
    [SerializeField] Button _startGameButton;

    [Tooltip("Game Over / You Win screen displayed when the player breaks all breakable blocks")]
    [SerializeField] GameObject _gameOverScreen;

    [Tooltip("Press to start game in the Game Over / You Win screen")]
    [SerializeField] Button _playAgainButton;

    [Tooltip("The list of Game Layouts to choose from to play")]
    [SerializeField] GameLayout[] _gameLayoutPrefabs;

    [Header("Debug")]
    [Tooltip("Always use the selected game layout. This will override the layout prefab list and should be for debugging only!")]
    [SerializeField] GameLayout _gameLayoutOverride;

    [Tooltip("Enable/Disable Console Logger")]
    [field:SerializeField] public bool EnableLogger { get; private set; } = true;

    [Tooltip("Whether or not to print Console Logger messages to the Unity Console window")]
    [field:SerializeField] public bool LogMessagesToUnityConsole { get; private set; } = false;

    [Tooltip("Maximum number of log messages visible on the screen")]
    [field:SerializeField] public int MaxVisibleLoggerMessages { get; private set; }

    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Cache the "main" camera to avoid the small overhead of repeatedly calling Camera.main
    public Camera MainCamera { get; private set; }

    // Parent of all fired projectiles. Used for containing the projectiles to keep the Hierarchy view clean.
    public Transform ProjectileParentTransform { get; private set; }

    // Game Logger
    public GameLogger GameLogger { get; private set; }

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

        // Create the game logger
        CreateGameLogger();

        // Handle button clicks
        _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        _playAgainButton.onClick.AddListener(OnPlayAgainButtonClicked);

        // Show Title screen
        _startGameScreen.SetActive(true);

        if(_gameLayoutOverride != null)
        {
            Debug.LogWarning("_gameLayoutOverride is not null. This will ALWAYS be used as the game layout.");
        }
    }

    void OnValidate()
    {
        MaxVisibleLoggerMessages = Mathf.Max(1, MaxVisibleLoggerMessages);
    }

    void CreateGameLogger()
    {
        GameObject gameLoggerGO = new GameObject("Game Logger");
        GameLogger = gameLoggerGO.AddComponent<GameLogger>();
        GameLogger.transform.SetParent(transform);
    }

    void StartNewGame()
    {
        // Hide screens
        _startGameScreen.SetActive(false);
        _gameOverScreen.SetActive(false);

        // Create the projectile parent container in the scene
        CreateProjectileParent();

        // Create Game Layout
        CreateGameLayout();

        // Get the number of breakable blocks in the scene for checking game over
        _numActiveBreakableBlocks = _currentGameLayout.NumBreakableBlocks;

        // Ensure there is at least 1 breakable block
        if(_numActiveBreakableBlocks <= 0)
        {
            Debug.LogError("There are no active breakable blocks in the scene on game start. Check the scene.");
        }

        // Clear the logs from any previous game sessions
        GameLogger.ClearConsoleLogs();
    }

    void CreateProjectileParent()
    {
        GameObject projectileParent = new GameObject("[Projectiles]");
        ProjectileParentTransform = projectileParent.transform;
    }

    void CreateGameLayout()
    {
        GameLayout gameLayoutPrefab = GetGameLayoutPrefab();
        _currentGameLayout = GameObject.Instantiate(gameLayoutPrefab);
        _currentGameLayout.Init(_playerTurretPrefab);
    }

    GameLayout GetGameLayoutPrefab()
    {
        if(_gameLayoutOverride != null)
        {
            return _gameLayoutOverride;
        }

        if(_gameLayoutPrefabs.Length <= 0)
        {
            Debug.LogError("There are no game layout prefabs assigned in the GameManager Inspector. A game will not be created.");
            return null;
        }

        // Choose a random game layout from the list of game layout prefabs
        int randomIdx = Random.Range(0, _gameLayoutPrefabs.Length);
        return _gameLayoutPrefabs[randomIdx];
    }

    public void OnBreakableBlockDestroyed()
    {
        --_numActiveBreakableBlocks;
        if(_numActiveBreakableBlocks <= 0)
        {
            // Player has destroyed all breakable blocks in the scene. Remove the current game layout.
            Destroy(_currentGameLayout.gameObject);

            // Destroy the projectile parent transform, so all active projectiles will be destroyed
            Destroy(ProjectileParentTransform.gameObject);

            // Show the Game Over / You Win screen
            _gameOverScreen.SetActive(true);
        }
    }
    
    void OnStartGameButtonClicked()
    {
        StartNewGame();
    }

    void OnPlayAgainButtonClicked()
    {
        StartNewGame();
    }
}
