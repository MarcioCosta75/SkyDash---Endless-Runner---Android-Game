using UnityEngine;

/// <summary>
/// Flips the gun to face left or right. Wired to the movement buttons in
/// the scene, so the method names are part of that wiring.
/// </summary>
public class GunTip : MonoBehaviour
{
    public void RotateNegative180()
    {
        SetFacing(-180f);
    }

    public void RotatePositive180()
    {
        SetFacing(0f);
    }

    private void SetFacing(float yaw)
    {
        Vector3 angles = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(angles.x, yaw, angles.z);
    }
}
