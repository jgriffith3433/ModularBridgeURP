using UnityEngine;

/// <summary>
/// Central game context that holds references to all major systems.
/// Follows standard Unity singleton pattern for global access.
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
        // Standard singleton setup
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Validate all systems are assigned
        ValidateSystems();
    }
    
    private void ValidateSystems()
    {
        if (gameSettings == null)
            Debug.LogError("[Game] GameSettings not assigned!");
        
        if (gridSystem == null)
            Debug.LogError("[Game] GridSystem not assigned!");
        
        if (bridgeSystem == null)
            Debug.LogError("[Game] BridgeSystem not assigned!");
        
        if (inputManager == null)
            Debug.LogError("[Game] InputManager not assigned!");
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}