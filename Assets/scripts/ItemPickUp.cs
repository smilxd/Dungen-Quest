using System.Collections;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public float pickUpRange = 0.7f;
    public Transform playerCamera;
    public Transform Raycastpostion;
    public GameObject crosshair;
    public LayerMask pickUpLayer;
    public LayerMask ignoreLayers;

    public camscripts camscripts;
    public PlayerMovement playerMovement;

    [Tooltip("Чувствительность вращения при перемещении мыши")]
    public float rotationSensitivity = 3f;

    [Tooltip("Расстояние от точки удержания до объекта (сек)")]
    public float holdDistance = 0.5f;

    [Tooltip("Время плавного перемещения к точке удержания (сек)")]
    public float pickupMoveDuration = 0.3f;

    [Tooltip("Время плавного возврата на исходную позицию (сек)")]
    public float dropMoveDuration = 0.5f;

    private GameObject heldObj;
    private Rigidbody heldRb;
    private Collider[] heldColliders;
    private float savedMoveSpeed;
    private bool wasCamEnabled;
    private Coroutine moveCoroutine;
    private bool isHolding = false;
    private Vector3 originalPos;
    private Quaternion originalRot;

    private void Update()
    {
        Itempickupdot();

        if (isHolding && heldObj != null)
        {
            HandleRotationInput();

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.E))
            {
                ReturnHeldObject();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(playerCamera.position, playerCamera.forward * pickUpRange);
    }

    private void Itempickupdot()
    {
        if (playerCamera == null || crosshair == null || Raycastpostion == null) return;

        int raycastMask = ~ignoreLayers.value;
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, pickUpRange, raycastMask);

        if (hitSomething)
        {
            bool hitOnPickupLayer = (pickUpLayer.value & (1 << hit.collider.gameObject.layer)) != 0;
            crosshair.SetActive(hitOnPickupLayer);
        }
        else
        {
            crosshair.SetActive(false);
        }

        if (!isHolding && Input.GetKeyDown(KeyCode.E))
        {
            int pickupMask = pickUpLayer.value & ~ignoreLayers.value;
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, pickUpRange, pickupMask))
            {
                PickUpObject(hit.collider.gameObject);
            }
        }

        
        transform.rotation = playerCamera.rotation;
        transform.position = Raycastpostion.position;
    }

    private void PickUpObject(GameObject pickUpObj)
    {
        if (pickUpObj == null || isHolding) return;

        heldObj = pickUpObj;
        originalPos = heldObj.transform.position;
        originalRot = heldObj.transform.rotation;
        heldRb = heldObj.GetComponent<Rigidbody>();

 
        heldColliders = heldObj.GetComponentsInChildren<Collider>();

       
        if (heldRb != null)
        {
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
            heldRb.useGravity = false;
            heldRb.isKinematic = true;
        }

     
        foreach (Collider col in heldColliders)
        {
            col.enabled = false;
        }

        
        if (playerMovement != null)
        {
            savedMoveSpeed = playerMovement.moveSpeed;
            playerMovement.moveSpeed = 0f;
        }
        if (camscripts != null)
        {
            wasCamEnabled = camscripts.enabled;
            camscripts.enabled = false;
        }

        
        crosshair.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(SmoothMoveToHold(heldObj, pickupMoveDuration));
    }

    private IEnumerator SmoothMoveToHold(GameObject obj, float duration)
    {
        if (obj == null) yield break;

        Transform t = obj.transform;
        Vector3 startPos = t.position;
        Quaternion startRot = t.rotation;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float f = Mathf.Clamp01(elapsed / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, f);

         
            Vector3 targetPos = playerCamera.position + playerCamera.forward * holdDistance;
            Quaternion targetRot = playerCamera.rotation;

            t.position = Vector3.Lerp(startPos, targetPos, smooth);
            t.rotation = Quaternion.Slerp(startRot, targetRot, smooth);
            yield return null;
        }

      
        heldObj.transform.position = playerCamera.position + playerCamera.forward * holdDistance;
        heldObj.transform.rotation = playerCamera.rotation;

        isHolding = true;
        moveCoroutine = null;
    }

    private void ReturnHeldObject()
    {
        if (heldObj == null) return;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        isHolding = false;

        
        moveCoroutine = StartCoroutine(SmoothReturnToOriginal(heldObj, dropMoveDuration));
    }

    private IEnumerator SmoothReturnToOriginal(GameObject obj, float duration)
    {
        if (obj == null) yield break;

        Transform t = obj.transform;
        Vector3 startPos = t.position;
        Quaternion startRot = t.rotation;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float f = Mathf.Clamp01(elapsed / duration);
            float smooth = Mathf.SmoothStep(0f, 1f, f);

            t.position = Vector3.Lerp(startPos, originalPos, smooth);
            t.rotation = Quaternion.Slerp(startRot, originalRot, smooth);
            yield return null;
        }

        
        t.position = originalPos;
        t.rotation = originalRot;

       
        if (heldColliders != null)
        {
            foreach (Collider col in heldColliders)
            {
                col.enabled = true;
            }
        }

        if (heldRb != null)
        {
            heldRb.useGravity = true;
            heldRb.isKinematic = false;
        }

       
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = savedMoveSpeed;
        }
        if (camscripts != null)
        {
            camscripts.enabled = wasCamEnabled;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        heldObj = null;
        heldRb = null;
        moveCoroutine = null;
    }

    private void HandleRotationInput()
    {
        if (!isHolding || heldObj == null) return;

        if (Input.GetMouseButton(0))
        {
            float x = Input.GetAxis("Mouse X") * rotationSensitivity;
            float y = Input.GetAxis("Mouse Y") * rotationSensitivity;

            Quaternion rotationX = Quaternion.AngleAxis(-x, Vector3.up);
            Quaternion rotationY = Quaternion.AngleAxis(y, Vector3.right);

            heldObj.transform.localRotation = (rotationX * rotationY) * heldObj.transform.localRotation;
        }
    }
}