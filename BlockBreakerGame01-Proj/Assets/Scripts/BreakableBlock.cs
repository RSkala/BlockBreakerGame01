using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    [SerializeField, Range(1, 3)] int _health;
    [SerializeField] bool _unbreakable;
    [SerializeField] BlockData _blockData;
    [SerializeField] SpriteRenderer _spriteRenderer;

    void Start()
    {
        if(_health <= 0)
        {
            Debug.LogWarning("BreakableBlock.Start - _health is invalid: " + _health + ". Check the Inspector values in the prefab.");
        }

        SetHealthColor();
    }

    void OnValidate()
    {
        SetHealthColor();
    }

    public void DealDamage(int damage)
    {
        // Do not deal damage to an unbreakable block
        if(_unbreakable)
        {
            return;
        }

        // Handle damage and remove this block from the scene if it runs out of health
        _health -= damage;
        _health = Mathf.Max(_health, 0);
        if(_health <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            SetHealthColor();
        }
    }

    void SetHealthColor()
    {
        if(_unbreakable)
        {
            _spriteRenderer.color = _blockData.UnbreakableBlockolor;
        }
        else
        {
            switch(_health)
            {
                case 1: _spriteRenderer.color = _blockData.BlockHealthColor1; break;
                case 2: _spriteRenderer.color = _blockData.BlockHealthColor2; break;
                case 3: _spriteRenderer.color = _blockData.BlockHealthColor3; break;
                default: break;
            }
        }
    }
}
