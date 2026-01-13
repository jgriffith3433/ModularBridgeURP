using UnityEngine;

/// <summary>
/// Central game context that holds references to all major systems.
/// Singleton pattern - this is the only class that should use singleton.
/// </summary>
public class Game : MonoBehaviour
{
    private static Game instance;
    public static Game Instance => instance;
    
    [Header("Settings")]
    [SerializeField] private GameSettings gameSettings;
    
    [Header("System References")]
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private BridgeSystem bridgeSystem;
    [SerializeField] private InputManager inputManager;
    
    // Public accessors for systems
    public GameSettings Settings => gameSettings;
    public GridSystem Grid => gridSystem;
    public BridgeSystem Bridges => bridgeSystem;
    public InputManager Input => inputManager;
    
    private void Awake()
    {
        // Singleton setup
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Validate all systems are assigned
        ValidateSystems();
    }
    
    private void ValidateSystems()
    {
        if (gameSettings == null)
            throw new System.Exception("[Game] GameSettings not assigned!");
        
        if (gridSystem == null)
            throw new System.Exception("[Game] GridSystem not assigned!");
        
        if (bridgeSystem == null)
            throw new System.Exception("[Game] BridgeSystem not assigned!");
        
        if (inputManager == null)
            throw new System.Exception("[Game] InputManager not assigned!");
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}