using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTurret : MonoBehaviour
{
    Vector2 _mouseLookPosition;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnFire(InputValue inputValue)
    {
        GameManager.Instance.PrintDebugPlayerInputString("OnFire - " + inputValue.ToString());
    }

    void OnMouseMove(InputValue inputValue)
    {
        // While the mouse input value is a Vector2, we need to get the value as a Vector3 so we can pass into ScreenToWorldPoint
        Vector3 mousePosition = inputValue.Get<Vector2>();

        // Get the mouse's screen position in world coordinates
        Vector3 mouseWorldPosition = GameManager.Instance.MainCamera.ScreenToWorldPoint(mousePosition);
        _mouseLookPosition = mouseWorldPosition;
        GameManager.Instance.PrintDebugPlayerInputString("_mouseLookPosition:  " + _mouseLookPosition.ToString());
    }
}
