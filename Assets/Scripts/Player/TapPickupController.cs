using UnityEngine;

public class TapPickupController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                var pickup = hit.collider.GetComponent<PickupItem>();
                if (pickup != null)
                {
                    pickup.TryPickup();
                }
            }
        }
    }
}
