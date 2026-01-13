using UnityEngine;
using UnityEngine.InputSystem;

namespace ModularBridge.Input
{
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
}