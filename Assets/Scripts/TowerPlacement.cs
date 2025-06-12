using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    [Header("Tower Placement")]
    public GameObject[] towerPrefabs;
    public LayerMask groundLayer = 1;
    public LayerMask towerLayer = 1;
    public Material previewMaterial;
    
    private Camera playerCamera;
    private int selectedTowerIndex = 0;
    private GameObject previewTower;
    private bool placementMode = false;
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
    }
    
    void Update()
    {
        HandleInput();
        
        if (placementMode)
        {
            UpdatePreview();
        }
    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectTower(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectTower(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectTower(2);
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            TogglePlacementMode();
        }
        
        if (placementMode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceTower();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            TrySelectTower();
        }
    }
    
    void SelectTower(int index)
    {
        if (index >= 0 && index < towerPrefabs.Length)
        {
            selectedTowerIndex = index;
            
            if (!placementMode)
            {
                TogglePlacementMode();
            }
        }
    }
    
    void TogglePlacementMode()
    {
        placementMode = !placementMode;
        
        if (placementMode)
        {
            CreatePreview();
        }
        else
        {
            DestroyPreview();
        }
    }
    
    void CreatePreview()
    {
        if (selectedTowerIndex >= 0 && selectedTowerIndex < towerPrefabs.Length)
        {
            previewTower = Instantiate(towerPrefabs[selectedTowerIndex]);
            
            Collider[] colliders = previewTower.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
            
            Tower towerScript = previewTower.GetComponent<Tower>();
            if (towerScript != null)
            {
                towerScript.enabled = false;
            }
            
            Renderer[] renderers = previewTower.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (previewMaterial != null)
                {
                    renderer.material = previewMaterial;
                }
                else
                {
                    Color color = renderer.material.color;
                    color.a = 0.5f;
                    renderer.material.color = color;
                }
            }
        }
    }
    
    void UpdatePreview()
    {
        if (previewTower == null) return;
        
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            previewTower.transform.position = hit.point;
            previewTower.SetActive(true);
        }
        else
        {
            previewTower.SetActive(false);
        }
    }
    
    void TryPlaceTower()
    {
        if (previewTower == null) return;
        
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            if (CanPlaceTower(hit.point))
            {
                Tower towerPrefab = towerPrefabs[selectedTowerIndex].GetComponent<Tower>();
                if (towerPrefab != null && GameManager.Instance.SpendMoney(towerPrefab.cost))
                {
                    GameObject newTower = Instantiate(towerPrefabs[selectedTowerIndex], hit.point, Quaternion.identity);
                    newTower.layer = LayerMask.NameToLayer("Tower");
                }
            }
        }
    }
    
    bool CanPlaceTower(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 1f, towerLayer);
        return colliders.Length == 0;
    }
    
    void TrySelectTower()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            Tower tower = hit.collider.GetComponent<Tower>();
            if (tower != null)
            {
                if (tower.CanUpgrade())
                {
                    tower.Upgrade();
                }
            }
        }
    }
    
    void CancelPlacement()
    {
        placementMode = false;
        DestroyPreview();
    }
    
    void DestroyPreview()
    {
        if (previewTower != null)
        {
            Destroy(previewTower);
            previewTower = null;
        }
    }
}