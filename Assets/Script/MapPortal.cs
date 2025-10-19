using UnityEngine;

public class MapPortal : MonoBehaviour
{
    public Transform otherPortal;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

        }
    }

}
