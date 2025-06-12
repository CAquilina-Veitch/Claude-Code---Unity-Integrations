using UnityEngine;

public class CameraMiddlePositioner : MonoBehaviour
{
    private Camera mainCamera;
    private Transform parentTransform;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("No main camera found in scene");
        }

        parentTransform = transform.parent;
        if (parentTransform == null)
        {
            Debug.LogWarning("CameraMiddlePositioner: No parent found");
        }
    }

    void Update()
    {
        if (mainCamera == null || parentTransform == null)
            return;

        Vector3 parentPosition = parentTransform.position;
        Vector3 cameraPosition = mainCamera.transform.position;
        
        transform.position = Vector3.Lerp(parentPosition, cameraPosition, 0.5f);
    }
}