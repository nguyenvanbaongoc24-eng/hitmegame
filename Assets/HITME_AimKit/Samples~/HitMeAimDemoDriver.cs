using UnityEngine;
using HitMe.AimKit;

/// <summary>
/// VISUAL DEMO ONLY: place in a test scene, not in the production match.
/// Uses Input.GetMouseButton; the main game must own input and locked actions.
/// </summary>
public class HitMeAimDemoDriver : MonoBehaviour
{
    public HitMeAimVisual2D visual;
    public Camera gameCamera;
    public Transform demoPlayer;
    public Sprite previewWeapon;
    [Range(0.1f, 2f)] public float lengthMultiplier = 1f;

    private void Update()
    {
        if (visual == null || demoPlayer == null) return;
        if (!Input.GetMouseButton(0)) { visual.Hide(); return; }
        Camera cam = gameCamera != null ? gameCamera : Camera.main;
        if (cam == null) return;
        Vector3 pointer = Input.mousePosition;
        pointer.z = -cam.transform.position.z; // This sample expects a 2D XY scene at z=0.
        Vector3 world = cam.ScreenToWorldPoint(pointer);
        Vector2 origin = demoPlayer.position;
        Vector2 delta = (Vector2)world - origin;
        if (delta.sqrMagnitude < 0.07f) { visual.Hide(); return; }
        visual.ShowAim(origin, delta.normalized, delta.magnitude * lengthMultiplier, previewWeapon);
    }
}
