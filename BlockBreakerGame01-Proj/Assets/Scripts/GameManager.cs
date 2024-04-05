using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Player / Game")]
    [Tooltip("The player turret from which projectiles are fired")]
    [SerializeField] PlayerTurret _playerTurretPrefab;

    [Tooltip("The projectile that will be fired from the player turret")]
    [field:SerializeField] public Projectile ProjectilePrefab { get; private set; }

    [Tooltip("Number of seconds to wait until after the last brick is broken to display the game over screen")]
    [SerializeField] float _endGameDelay;

    [Tooltip("The list of Game Layouts to choose from to play")]
    [SerializeField] GameLayout[] _gameLayoutPrefabs;

    [Header("UI")]
    [Tooltip("Title / Start Game screen. Displayed when the game starts")]
    [SerializeField] GameObject _startGameScreen;

    [Tooltip("Press to start playing the game layouts in order")]
    [SerializeField] Button _startInOrderGameButton;

    [Tooltip("Press to start playing the game layouts in random order")]
    [SerializeField] Button _startRandomGameButton;

    [Tooltip("Game Over / You Win screen displayed when the player breaks all breakable blocks")]
    [SerializeField] GameObject _gameOverScreen;

    [Tooltip("Press to start game in the Game Over / You Win screen")]
    [SerializeField] Button _playAgainButton;

    [Tooltip("Information about the player's projectiles at game end")]
    [SerializeField] Text _projectileSummaryText;

    [Tooltip("How long the player took to finish the level")]
    [SerializeField] Text _gameSessionTimeText;

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

    // The number of projectiles launched during a single game session
    public int NumProjectilesLaunched { get; private set; }

    // Current Game Layout that is playing
    GameLayout _currentGameLayout;

    // Current number of active breakable blocks. Used for checking if the player has broken all breakable blocks.
    int _numActiveBreakableBlocks;

    // Order in which the levels are played
    GameProgressionType _gameProgressionType;

    // For an InOrder game, the index of the current GameLayout in the list
    int _inOrderGameCurrentIdx;

    // For a Random game, the list of GameLayouts that have not yet been played
    List<GameLayout> _availableRandomGameLayouts;

    // The time at which the most recent game started
    float _gameStartTime;

    // The elapsed time the player played the most recent game session
    float _gameSessionTime;

    // Projectile Pool
    List<Projectile> _projectilePool = new List<Projectile>();

    // Projectile pool size
    const int kMaxProjectiles = 200;

    enum GameProgressionType
    {
        InOrder,
        Random
    }

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
        _startInOrderGameButton.onClick.AddListener(OnStartInOrderGameButtonClicked);
        _startRandomGameButton.onClick.AddListener(OnStartRandomGameButtonClicked);
        _playAgainButton.onClick.AddListener(OnPlayAgainButtonClicked);

        // Init Projectile Parent and Projectile Pool
        CreateProjectileParent();
        InitProjectilePool();

        // Show Title screen
        _startGameScreen.SetActive(true);

        // On Start, find any GameLayouts and destroy them. This is to help iteration time when building and testing GameLayouts
        DestroyActiveGameLayouts();

        if(_gameLayoutOverride != null)
        {
            Debug.LogWarning("_gameLayoutOverride is not null. This will ALWAYS be used as the game layout.");
        }

        if(_gameLayoutPrefabs.Length <= 0 && _gameLayoutOverride != null)
        {
            Debug.LogWarning("There are no game layout prefabs assigned in the GameManager Inspector. A game will not be created.");
        }
    }

    void InitProjectilePool()
    {
        for(int i = 0; i < kMaxProjectiles; ++i)
        {
            CreateAndAddNewProjectile();
        }
    }

    void DestroyActiveGameLayouts()
    {
        GameLayout[] activeGameLayouts = Object.FindObjectsByType<GameLayout>(FindObjectsSortMode.None);
        foreach(GameLayout activeGameLayout in activeGameLayouts)
        {
            Debug.Log("destroying active game layout " + activeGameLayout.name);
            Destroy(activeGameLayout.gameObject);
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

    void StartInOrderGame()
    {
        // Set the game type
        _gameProgressionType = GameProgressionType.InOrder;

        // Set the index into the game layout prefab list
        _inOrderGameCurrentIdx = 0;

        // Get the first game layout
        _currentGameLayout = _gameLayoutPrefabs[_inOrderGameCurrentIdx];

        // Start the game
        StartNewGame();
    }

    void ContinueInOrderGame()
    {
        // Increment the index into the game layout prefab list and ensure it does not overrun
        _inOrderGameCurrentIdx++;
        if(_inOrderGameCurrentIdx >= _gameLayoutPrefabs.Length)
        {
            _inOrderGameCurrentIdx = 0;
        }

        // Get the game layout at the current index
        _currentGameLayout = _gameLayoutPrefabs[_inOrderGameCurrentIdx];

        // Start the game
        StartNewGame();
    }

    void StartRandomGame()
    {
        // Set the game type
        _gameProgressionType = GameProgressionType.Random;

        // Set the available game layouts to all of the game layout prefabs
        _availableRandomGameLayouts = _gameLayoutPrefabs.ToList();

        // Get a random index into the available list
        int randomIdx = Random.Range(0, _availableRandomGameLayouts.Count);

        // Get the game layout and remove the game layout from the available list
        _currentGameLayout = _availableRandomGameLayouts[randomIdx];
        _availableRandomGameLayouts.Remove(_currentGameLayout);

        // Start the game
        StartNewGame();
    }

    void ContinueRandomGame()
    {
        if(_availableRandomGameLayouts.Count <= 0)
        {
            // The available game layout list is empty. Reset it.
            _availableRandomGameLayouts = _gameLayoutPrefabs.ToList();
        }

        // Get a random index into the available list
        int randomIdx = Random.Range(0, _availableRandomGameLayouts.Count);

        // Get the game layout and remove the game layout from the available list
        _currentGameLayout = _availableRandomGameLayouts[randomIdx];
        _availableRandomGameLayouts.Remove(_currentGameLayout);

        // Start the game
        StartNewGame();
    }

    void StartNewGame()
    {
        // Hide screens
        _startGameScreen.SetActive(false);
        _gameOverScreen.SetActive(false);

        // Reset Projectile count
        NumProjectilesLaunched = 0;

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

        // Record the current time
        _gameStartTime = Time.realtimeSinceStartup;
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

        return _currentGameLayout;
    }

    public void OnBreakableBlockDestroyed()
    {
        --_numActiveBreakableBlocks;
        if(_numActiveBreakableBlocks <= 0)
        {
            // Record game session time elapsed
            _gameSessionTime = Time.realtimeSinceStartup - _gameStartTime;

            // Player has destroyed all breakable blocks in the scene. End the game after a delay.
            StartCoroutine(EndGame());
        }
    }

    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(_endGameDelay);

        // Player has destroyed all breakable blocks in the scene. Remove the current game layout.
        // Check for null, as it is null on some rare occasions, possibly from garbage collection.
        if(_currentGameLayout != null)
        {
            Destroy(_currentGameLayout.gameObject);
        }

        // Deactivate all projectiles in the pool
        SetAllProjectilesInactive();

        // Set projectile summary text
        _projectileSummaryText.text = "You fired " + NumProjectilesLaunched + " projectiles this round!";

        // Set game time text
        _gameSessionTimeText.text = "You took " + System.Math.Round(_gameSessionTime, 2) + " seconds to beat this level!";

        // Show the Game Over / You Win screen
        _gameOverScreen.SetActive(true);
    }

    void OnStartInOrderGameButtonClicked()
    {
        StartInOrderGame();
    }

    void OnStartRandomGameButtonClicked()
    {
        StartRandomGame();
    }

    void OnPlayAgainButtonClicked()
    {
        if(_gameProgressionType == GameProgressionType.InOrder)
        {
            ContinueInOrderGame();
        }
        else
        {
            ContinueRandomGame();
        }
    }

    public void IncrementNumProjectilesLaunched() => NumProjectilesLaunched++;

    Projectile CreateAndAddNewProjectile()
    {
        Projectile newProjectile = GameObject.Instantiate<Projectile>(ProjectilePrefab, Vector3.zero, Quaternion.identity, ProjectileParentTransform);
        newProjectile.name = "Projectile-" + _projectilePool.Count;
        _projectilePool.Add(newProjectile);
        return newProjectile;
    }

    public Projectile GetInactiveProjectile()
    {
        Projectile inactiveProjectile = _projectilePool.FirstOrDefault(projectile => !projectile.IsActive);
        if(inactiveProjectile == null)
        {
            // No inactive projectiles available in the pool. Log warning and create a new one.
            Debug.LogWarning("No available projectiles in the pool. Increase the pool size.");
            inactiveProjectile = CreateAndAddNewProjectile();
        }
        return inactiveProjectile;
    }

    void SetAllProjectilesInactive()
    {
        foreach(Projectile projectile in _projectilePool)
        {
            projectile.Deactivate();
        }
    }
}
