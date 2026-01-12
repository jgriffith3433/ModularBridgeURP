using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central manager for all input in the game.
/// </summary>
public class InputManager : MonoBehaviour
{
    private GameInputActions inputActions;
    
    public GameInputActions InputActions => inputActions;
    
    private void Awake()
    {
        inputActions = new GameInputActions();
    }
    
    private void OnEnable()
    {
        inputActions.Enable();
    }
    
    private void OnDisable()
    {
        inputActions.Disable();
    }
}