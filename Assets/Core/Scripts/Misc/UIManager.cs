using UnityEngine;

public class UIManager : MonoBehaviour
{
    public RectTransform test;
    public Vector2 worldPosStick;
    public Camera cam;

    private void Update()
    {
        test.position = cam.WorldToScreenPoint(worldPosStick);
    }
}