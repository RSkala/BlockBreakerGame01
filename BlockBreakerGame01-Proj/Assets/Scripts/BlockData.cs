using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "ScriptableObjects/BlockData")]
public class BlockData : ScriptableObject
{
    [Tooltip("The color of a block when it has 1 health")]
    [field:SerializeField] public Color BlockHealthColor1 { get; private set; } = Color.black;

    [Tooltip("The color of a block when it has 2 health")]
    [field:SerializeField] public Color BlockHealthColor2 { get; private set; } = Color.black;

    [Tooltip("The color of a block when it has 3 health")]
    [field:SerializeField] public Color BlockHealthColor3 { get; private set; } = Color.black;

    [Tooltip("The color of a block when it has 4 health")]
    [field:SerializeField] public Color BlockHealthColor4 { get; private set; } = Color.black;

    [Tooltip("The color of a block when it has 5 health")]
    [field:SerializeField] public Color BlockHealthColor5 { get; private set; } = Color.black;

    [Tooltip("The color of a block when it is unbreakable")]
    [field:SerializeField] public Color UnbreakableBlockolor { get; private set; } = Color.black;
}
