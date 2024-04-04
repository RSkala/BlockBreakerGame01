using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameLayout : MonoBehaviour
{
    [Tooltip("The location where the player turret will be spawned in this Game Layout")]
    [SerializeField] Transform _playerTurretSpawnPosition;

    // Return the number of blocks that can be broken in this Game Layout
    public int NumBreakableBlocks { get { return _breakableBlocks.Count; } }

    // The player's current turret
    PlayerTurret _playerTurret;
    
    // List of all breakable blocks that can be broken
    List<BreakableBlock> _breakableBlocks;

    void Start()
    {
        // Hide the player turret spawn positions sprite
        _playerTurretSpawnPosition.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void Init(PlayerTurret _playerTurretPrefab)
    {
        gameObject.name = "[Game Layout]";
        CreatePlayerTurret(_playerTurretPrefab);
        InitBreakableBlocks();
    }

    void CreatePlayerTurret(PlayerTurret _playerTurretPrefab)
    {
        // Create the player turret at the spawn point using this GameLayout as the transform parent
        _playerTurret = GameObject.Instantiate(_playerTurretPrefab, _playerTurretSpawnPosition.position, Quaternion.identity, transform);
        _playerTurret.name = "Player Turret";
    }

    void InitBreakableBlocks()
    {
        // Find all Breakable Blocks in the scene
        _breakableBlocks = GetComponentsInChildren<BreakableBlock>().ToList();

        // Filter out the blocks marked "Invincible"
        _breakableBlocks.RemoveAll(breakableBlock => {
            return breakableBlock.Invincible;
        });
    }
}
