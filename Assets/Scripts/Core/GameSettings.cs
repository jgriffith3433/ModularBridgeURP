using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Grid")]
    [SerializeField] private GameGridSettings gridSettings;
    
    [Header("Bridge")]
    [SerializeField] private BridgeSettings bridgeSettings;
    
    public GameGridSettings GridSettings => gridSettings;
    public BridgeSettings BridgeSettings => bridgeSettings;
}