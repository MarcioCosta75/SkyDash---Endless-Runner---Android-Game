using UnityEngine;

public class GunTip : MonoBehaviour
{
    public void RotateNegative180()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, -180f, transform.rotation.eulerAngles.z);
    }

    public void RotatePositive180()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 0f, transform.rotation.eulerAngles.z);
    }
}
