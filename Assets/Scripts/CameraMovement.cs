using UnityEngine;

/// <summary>
/// Drives the endless upward scroll. Everything that must travel with the
/// player is parented under this object, so moving it moves the whole run.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed = 2.5f;

    /// <summary>Current scroll speed in world units per second.</summary>
    public float CameraSpeed
    {
        get => cameraSpeed;
        set => cameraSpeed = Mathf.Max(0f, value);
    }

    private void Update()
    {
        transform.position += new Vector3(0f, cameraSpeed * Time.deltaTime, 0f);
    }
}
