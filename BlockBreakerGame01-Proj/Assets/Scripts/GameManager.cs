using System.Reflection;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Tooltip("The projectile that will be fired from the player turret")]
    [field:SerializeField] public Projectile ProjectilePrefab { get; private set; }

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
}
