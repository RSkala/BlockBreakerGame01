using System.Collections.Generic;
using UnityEngine;

public class GameLogger : MonoBehaviour
{
    Queue<string> _consoleLogs;

    const float kPosX = 5.0f;
    const float kPosYSpace = 20.0f;
    const float kRectW = 400.0f;
    const float kRectH = 100.0f;

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
        if(_consoleLogs.Count >= GameManager.Instance.MaxVisibleLoggerMessages)
        {
            _consoleLogs.Dequeue();
        }

        _consoleLogs.Enqueue(log);
        if(GameManager.Instance.LogMessagesToUnityConsole)
        {
            Debug.Log(log);
        }
    }
}
