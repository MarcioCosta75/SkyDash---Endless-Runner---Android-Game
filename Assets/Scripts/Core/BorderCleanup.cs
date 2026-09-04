using UnityEngine;

/// <summary>
/// Destroys anything that falls past the bottom of the play area.
/// Sits on the "Border" object so every falling item is cleaned up in one
/// place, instead of each pickup having to remember to do it.
/// </summary>
public class BorderCleanup : MonoBehaviour
{
    [Tooltip("Objects with these tags are never destroyed by the border.")]
    [SerializeField]
    private string[] protectedTags = { "Player", "MainCamera", "MagneticField" };

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject candidate = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        for (int i = 0; i < protectedTags.Length; i++)
        {
            if (!string.IsNullOrEmpty(protectedTags[i]) && candidate.CompareTag(protectedTags[i]))
            {
                return;
            }
        }

        Destroy(candidate);
    }
}
