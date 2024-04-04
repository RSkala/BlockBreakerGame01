using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    [Tooltip("Health value for a breakable block. Will take this many hits before it is destroyed.")]
    [SerializeField, Range(1, 5)] int _health;

    [Tooltip("This breakable block cannot be damaged")]
    [field:SerializeField] public bool Invincible { get; private set; }

    [Tooltip("Used for setting values of a breakable block")]
    [SerializeField] BlockData _blockData;

    [Tooltip("Used for setting the color of a block which represents its damage state")]
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
        // Do not deal damage to an invincible block
        if(Invincible)
        {
            return;
        }

        // Handle damage and remove this block from the scene if it runs out of health
        _health -= damage;
        _health = Mathf.Max(_health, 0);
        if(_health <= 0)
        {
            Destroy(gameObject);
            GameManager.Instance.OnBreakableBlockDestroyed();
        }
        else
        {
            SetHealthColor();
        }
    }

    void SetHealthColor()
    {
        if(Invincible)
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
                case 4: _spriteRenderer.color = _blockData.BlockHealthColor4; break;
                case 5: _spriteRenderer.color = _blockData.BlockHealthColor5; break;
                default: break;
            }
        }
    }
}
