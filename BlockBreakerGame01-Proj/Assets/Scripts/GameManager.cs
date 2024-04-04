using System.Reflection;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Tooltip("If true, player input values will be printed to the Unity Console window")]
    [SerializeField] bool _debugPlayerInput;

    public static GameManager Instance { get; private set; }

    // Cache the "main" camera to avoid the small overhead of repeatedly calling Camera.main
    public Camera MainCamera { get; private set; }

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
    }

    public void PrintDebugPlayerInputString(string debugPlayerInputString)
    {
        if(_debugPlayerInput)
        {
            Debug.Log(debugPlayerInputString);
        }
    }
}
