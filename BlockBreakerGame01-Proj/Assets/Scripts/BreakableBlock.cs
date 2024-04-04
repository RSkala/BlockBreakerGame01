using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    [SerializeField] int _health;

    void Start()
    {
        if(_health <= 0)
        {
            Debug.LogWarning("BreakableBlock.Start - _health is invalid: " + _health + ". Check the Inspector values in the prefab.");
        }
    }

    void Update()
    {
        
    }
}
