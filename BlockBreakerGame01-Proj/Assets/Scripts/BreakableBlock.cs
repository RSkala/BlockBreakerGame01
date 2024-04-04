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

    public void DealDamage(int damage)
    {
        // Handle damage and remove this block from the scene if it runs out of health
        _health -= damage;
        _health = Mathf.Max(_health, 0);
        if(_health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
