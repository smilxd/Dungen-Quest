using Unity.Collections;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    static RaycastHit Hit;
    public float pickUpRange = 0.7f;
    public Transform playerCamera;
    public Transform Raycastpostion;
   public GameObject  crosshair;
    public LayerMask pickUpLayer;
    public LayerMask ignoreLayers;
    public GameObject rbody;
    public Rigidbody rb;
    public camscripts camscripts;
    private void Update()
    {
        PickUpitems();
        Itempickupdot();
    }

    private void FixedUpdate()
    {
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(playerCamera.position, playerCamera.forward * pickUpRange);
    }

    private void Itempickupdot() 
    {
       


        int raycastMask = ~ignoreLayers.value;

        RaycastHit hit;
        bool hitSomething = Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, pickUpRange, raycastMask);

        if (hitSomething)
        {
            bool hitOnPickupLayer = (pickUpLayer.value & (1 << hit.collider.gameObject.layer)) != 0;
            Debug.Log($"Hit object: {hit.transform.name} (layer {hit.collider.gameObject.layer}) on pickup layer: {hitOnPickupLayer}");
            crosshair.SetActive(hitOnPickupLayer);
        }
        else
        {
            crosshair.SetActive(false);
            Hit = new RaycastHit();
        }

        transform.rotation = playerCamera.rotation;
        transform.position = Raycastpostion.position;


        int pickupMask = pickUpLayer.value & ~ignoreLayers.value;
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out Hit, pickUpRange, pickupMask))
            {
                rb.linearDamping=100f;
                camscripts.enabled = !camscripts.enabled;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }


    private void PickUpitems()
    {




    }



}
