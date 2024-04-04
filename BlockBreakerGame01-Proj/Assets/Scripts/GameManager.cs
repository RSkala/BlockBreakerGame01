using System.Collections.Generic;
using System.Linq;
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

    [Tooltip("Press to start playing the game layouts in order")]
    [SerializeField] Button _startInOrderGameButton;

    [Tooltip("Press to start playing the game layouts in random order")]
    [SerializeField] Button _startRandomGameButton;

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

        // Show Title screen
        _startGameScreen.SetActive(true);

        if(_gameLayoutOverride != null)
        {
            Debug.LogWarning("_gameLayoutOverride is not null. This will ALWAYS be used as the game layout.");
        }

        if(_gameLayoutPrefabs.Length <= 0 && _gameLayoutOverride != null)
        {
            Debug.LogWarning("There are no game layout prefabs assigned in the GameManager Inspector. A game will not be created.");
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

        return _currentGameLayout;
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
}
