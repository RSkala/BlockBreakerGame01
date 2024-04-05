using System.Collections.Generic;
using UnityEngine;

public class GameLogger : MonoBehaviour
{
    // Queue of our debug console logs
    Queue<string> _consoleLogs;

    // GUI Label values
    const float kPosX = 5.0f;
    const float kPosYSpace = 20.0f;
    const float kRectW = 400.0f;
    const float kRectH = 100.0f;

    // Target screen size is 1920x1080
    readonly Vector2 _nativeScreenSize = new Vector2(1920.0f, 1080.0f);

    void Start()
    {
        _consoleLogs = new Queue<string>();
    }

    public void ClearConsoleLogs()
    {
        _consoleLogs.Clear();
    }

    void OnGUI()
    {
        if(!GameManager.Instance.EnableLogger)
        {
            return;
        }

        // Scale the IMGUI depending on the current screen scale
        Vector3 scale = new Vector3 (Screen.width / _nativeScreenSize.x, Screen.height / _nativeScreenSize.y, 1.0f);
        GUI.matrix = Matrix4x4.TRS(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, scale);

        // Iterate through the console log queue and display in order
        float curYHeight = 0.0f;
        foreach(string consoleLog in _consoleLogs)
        {
            Rect rect = new Rect(kPosX, curYHeight, kRectW, kRectH);
            GUI.Label(rect, consoleLog);
            curYHeight += kPosYSpace;
        }
    }

    public void LogProjectileFired(Projectile projectile)
    {
        string log = "Projectile " + projectile.name + " fired - " + Time.realtimeSinceStartup.ToString();
        EnqueueLogMessage(log);
    }

    public void LogProjectileHitBlock(Projectile projectile, BreakableBlock breakableBlock)
    {
        string log = "Projectile " + projectile.name + " hit block " + breakableBlock.name + " - " + Time.realtimeSinceStartup.ToString();
        EnqueueLogMessage(log);
    }

    public void LogBlockDestroyed(BreakableBlock breakableBlock)
    {
        string log = "Block " + breakableBlock.name + " destroyed - " + Time.realtimeSinceStartup.ToString();
        EnqueueLogMessage(log);
    }

    void EnqueueLogMessage(string log)
    {
        // When the number of queued console logs exceeds the max, pop the oldest element
        if(_consoleLogs.Count >= GameManager.Instance.MaxVisibleLoggerMessages)
        {
            _consoleLogs.Dequeue();
        }

        // Add the new log to the queue
        _consoleLogs.Enqueue(log);

        // If enabled, print to the Unity Console window
        if(GameManager.Instance.LogMessagesToUnityConsole)
        {
            Debug.Log(log);
        }
    }
}
