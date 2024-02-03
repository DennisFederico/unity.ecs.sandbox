using UnityEngine;
using UnityEngine.InputSystem;

public class MousePositionDebug : MonoBehaviour {
    private void Update() {
        if (Input.GetMouseButtonDown(1)) {
            var readValue = Mouse.current.position.ReadValue();
            var mousePosition = Input.mousePosition;
            var worldPos = Camera.main.ScreenToWorldPoint(readValue);
            Debug.Log($"Mouse {mousePosition} - World Pos: {worldPos}");
            
            Vector3 mousePos = Input.mousePosition;
            {
                Debug.Log(mousePos.x);
                Debug.Log(mousePos.y);
            }
        }
    }
}