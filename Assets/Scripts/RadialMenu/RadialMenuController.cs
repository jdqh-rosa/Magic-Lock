using System.Collections.Generic;
using UnityEngine;

public class RadialMenuController : MonoBehaviour
{
    public RadialMenu RadialMenuPrefab;
    protected RadialMenu RadialMenuInstance;

    public Canvas MenuCanvas;

    [SerializeField] public List<RadialMenu> MenuLayers;
    public int Index = 0;

    public bool PremadeBool = false;

    public List<string> Paths = new List<string>();

    public float LayerRadiusDifference = 100f;

    private void Start()
    {
        // Initialization if needed
        if (!MenuCheck()) {
            MenuLayers = new List<RadialMenu>();
        }

        if (PremadeBool) {
            if (RadialMenuPrefab != null) {
                PopulateMenuLayersFromPrefab();
                MenuLayers[Index].Init();
            }
        }

        //if (Paths == null)
        //{
        //    Paths = new List<string>();
        //}
        LogLayerCount();
    }

    public void Update() { }

    public void CreateMenu(RadialMenu radialMenu = null)
    {
        if (PremadeBool) {
            PopulateMenuLayersFromPrefab();
            return;
        }

        if (!radialMenu) {
            radialMenu = RadialMenuPrefab;
        }

        if (MenuLayers == null) {
            MenuLayers = new List<RadialMenu>();
        }

        RadialMenuInstance = Instantiate(radialMenu, MenuCanvas.transform);
        RadialMenuInstance.gameObject.SetActive(false);

        if (MenuLayers.Count > 0) {
            if (!MenuLayers[^1]) return;
            MenuLayers[^1].Data.NextRing = RadialMenuInstance.Data;
            RadialMenuInstance.radius = MenuLayers[^1].radius + LayerRadiusDifference;
            RadialMenuInstance.callback = MenuLayers[Index].callback;
        }

        MenuLayers.Add(RadialMenuInstance);

        if (Index == MenuLayers.Count - 1) {
            MenuLayers[Index].SpawnButtons();
        }

        if (Paths == null) {
            Paths = new List<string>();
        }

        if (Paths.Count <= Index) {
            Paths.Add(MenuLayers[Index].path);
        }
        else {
            Paths[Index] = MenuLayers[Index].path;
        }
    }

    public void SpawnMenu()
    {
        if (!MenuCheck()) {
            return;
        }

        // If there are already multiple layers, respawn the menu
        if (MenuLayers.Count > 1) {
            RespawnMenu();
        }
        // Otherwise, if createBool is true, create a new menu
        else if (PremadeBool) {
            CreateMenu();
        }

        // Activate the current menu layer and spawn its buttons
        MenuLayers[Index].gameObject.SetActive(true);
        MenuLayers[Index].SpawnButtons();
    }

    public void DestroyMenu()
    {
        if (!MenuCheck()) {
            return;
        }

        foreach (var layer in MenuLayers) {
            DestroyImmediate(layer.gameObject);
        }

        MenuLayers.Clear();
        Paths.Clear();
        Index = 0;
    }

    public void RespawnMenu()
    {
        List<RadialMenu> tempLayers = new List<RadialMenu>();

        foreach (RadialMenu menu in MenuLayers) {
            tempLayers.Add(menu);
        }

        DestroyMenu();

        foreach (RadialMenu menu in tempLayers) {
            CreateMenu(menu);
        }

        LogLayerCount();
    }

    public void TurnMenuRight()
    {
        if (!MenuCheck()) {
            return;
        }

        MenuLayers[Index].TurnMenu(-1);
    }

    public void TurnMenuLeft()
    {
        if (!MenuCheck()) {
            return;
        }

        MenuLayers[Index].TurnMenu(1);
    }

    public string NextRing()
    {
        if (!MenuCheck()) {
            return null;
        }

        // Get the next data ring.
        var next = MenuLayers[Index].Data?.NextRing;
        // Check if we can navigate to the next menu.
        bool hasNextLayer = (PremadeBool && next != null) ||
                            (Index + 1 < MenuLayers.Count && MenuLayers[Index + 1] != null);

        if (hasNextLayer) {
#if UNITY_EDITOR
            Debug.Log($"index: {Index + 1}, count: {MenuLayers.Count}", this);
#endif

            if (Index + 1 == MenuLayers.Count) {
                return null;
            }

            MenuLayers[Index].NextRing(false);

            // Update or add the path for the current menu.
            if (Paths.Count > Index) {
                Paths[Index] = MenuLayers[Index].path;
            }
            else {
                Paths.Add(MenuLayers[Index].path);
            }

            // Move to the next menu layer.
            Index++;

            // Show the next ring.
            MenuLayers[Index].gameObject.SetActive(true);

            if (!MenuLayers[Index].initialized) {
                MenuLayers[Index].Init();
            }

            return null;
        }

        // Handle the case where there are no further layers.
        MenuLayers[Index].NextRing(false);

        // Update or add the path for the current menu.
        if (Paths.Count > Index) {
            Paths[Index] = MenuLayers[Index].path;
        }
        else {
            Paths.Add(MenuLayers[Index].path);
        }

        // Summarize the paths taken.
        string pathSummary = string.Join(", ", Paths);

#if UNITY_EDITOR
        Debug.Log(pathSummary, this);
#endif

        // Return the final path.
        return MenuLayers[Index].path;
    }

    public void PreviousRing()
    {
        if (!MenuCheck()) {
            return;
        }

        if (Index <= 0) return;
        MenuLayers[Index].gameObject.SetActive(false);
        --Index;
    }

    /// <summary>
    /// Finds all layers of RadialMenu starting from the RadialMenuPrefab and adds them to MenuLayers.
    /// </summary>
    private void PopulateMenuLayersFromPrefab()
    {
        if (!RadialMenuPrefab) {
            Debug.LogError("RadialMenuPrefab is not assigned.");
            return;
        }

        if (MenuLayers == null) {
            MenuLayers = new List<RadialMenu>();
        }

        // Clear existing layers and destroy instances to rebuild from the prefab.
        DestroyMenu();

        RadialRing currentLayer = RadialMenuPrefab.Data;
        // Track visited layers.
        HashSet<RadialRing> visitedLayers = new HashSet<RadialRing>(); 
        
        while (currentLayer) {
            
            // Break the loop if this layer has already been processed.
            if (visitedLayers.Contains(currentLayer))
            {
                Debug.LogWarning("Circular reference detected. Stopping layer population.");
                break;
            }

            // Mark the current layer as visited.
            visitedLayers.Add(currentLayer);
            
            // Instantiate a new radial menu and configure its properties.
            RadialMenu newMenu = Instantiate(RadialMenuPrefab, MenuCanvas.transform);
            newMenu.gameObject.SetActive(false);
            // Add the current layer's data ring to the new menu.
            newMenu.AddDataRing(currentLayer);

            // Calculate and set the radius based on the previous menu's radius.
            if (MenuLayers.Count > 0) {
                newMenu.radius = MenuLayers[^1].radius + LayerRadiusDifference;
            }

            // Set the callback if there's a previous menu.
            if (MenuLayers.Count > 1) {
                newMenu.callback = MenuLayers[^2].callback;
            }

            // Add the new menu to the list.
            MenuLayers.Add(newMenu);
            // Move to the next layer in the hierarchy.
            currentLayer = currentLayer.NextRing;
        }

        // Log the number of layers found for debugging purposes.
        Debug.Log($"Populated {MenuLayers.Count} menu layers from RadialMenuPrefab.");
    }

    public bool MenuCheck()
    {
        LogLayerCount();
        if (MenuLayers == null || MenuLayers.Count == 0 || !MenuLayers[Index] || MenuLayers.Count <= Index) {
#if UNITY_EDITOR
            Debug.LogWarning("Menu layers have not been created. Please create the menu first.", this);
#endif
            return false;
        }

        return true;
    }

    void LogLayerCount()
    {
#if UNITY_EDITOR
        Debug.Log($"Layer Count: {MenuLayers?.Count ?? 0}", this);
#endif
    }
}