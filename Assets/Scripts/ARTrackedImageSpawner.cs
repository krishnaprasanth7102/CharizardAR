using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARTrackedImageSpawner : MonoBehaviour
{
    [SerializeField] private GameObject charizardPrefab;

    [Tooltip("How far above the image Charizard spawns (in metres, local Y of the image transform).")]
    [SerializeField] private float spawnHeightOffset = 0.1f;

    [Tooltip("How far in front of the image Charizard spawns (in metres, negative local Z faces the camera).")]
    [SerializeField] private float spawnDepthOffset = 0.15f;

    [Tooltip("Uniform scale applied to the spawned Charizard.")]
    [SerializeField] private float spawnScale = 0.2f;

    // Keep reference to the spawned Charizard instance for the current tracked image.
    private GameObject charizardInstance;

    private ARTrackedImageManager arTrackedImageManager;

    private void OnEnable()
    {
        arTrackedImageManager = GetComponent<ARTrackedImageManager>();
        if (arTrackedImageManager == null)
        {
            Debug.LogError("[ARTrackedImageSpawner] ARTrackedImageManager component missing.");
            return;
        }
        arTrackedImageManager.trackablesChanged.AddListener(OnImageChanged);
    }

    private void OnDisable()
    {
        if (arTrackedImageManager != null)
            arTrackedImageManager.trackablesChanged.RemoveListener(OnImageChanged);
    }

    private void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Handle newly detected images
        foreach (var addedImage in eventArgs.added)
        {
            if (charizardPrefab == null)
            {
                Debug.LogError("[ARTrackedImageSpawner] Charizard prefab not assigned.");
                continue;
            }

            // Destroy any existing instance first (safety check)
            if (charizardInstance != null)
                Destroy(charizardInstance);

            // Instantiate Charizard as child of the tracked image so it follows the image pose.
            charizardInstance = Instantiate(charizardPrefab, addedImage.transform);

            // Place it in FRONT of the image (toward the camera) and slightly above.
            // In AR Foundation image local space: Y = up (away from surface), -Z = toward camera.
            charizardInstance.transform.localPosition = new Vector3(0f, spawnHeightOffset, -spawnDepthOffset);
            charizardInstance.transform.localRotation = Quaternion.identity;
            charizardInstance.transform.localScale = Vector3.one * spawnScale;
        }

        // Updated images – parent relationship keeps Charizard aligned automatically.
        foreach (var updatedImage in eventArgs.updated) { }

        // Removed images – clean up spawned Charizard
        foreach (var removedImage in eventArgs.removed)
        {
            if (charizardInstance != null)
            {
                Destroy(charizardInstance);
                charizardInstance = null;
            }
        }
    }
}