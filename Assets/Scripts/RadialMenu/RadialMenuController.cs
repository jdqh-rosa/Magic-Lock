using System.Collections.Generic;
using UnityEngine;

public class RadialMenuController : MonoBehaviour
{
    public RadialMenu RadialMenuPrefab;
    protected RadialMenu RadialMenuInstance;

    [SerializeField]
    public List<RadialMenu> MenuLayers;
    public int Index = 0;

    public bool CreateBool = false;

    public List<string> Paths;

    public float LayerRadiusDifference = 100f;

    private void Start()
    {
        // Initialization if needed
        if (!MenuCheck())
        {
            MenuLayers = new List<RadialMenu>();
        }
        if (Paths == null)
        {
            Paths = new List<string>();
        }
        LogLayerCount();
    }

    public void Update()
    {
        
    }

    public void CreateMenu(RadialMenu radialMenu = null)
    {
        if (radialMenu == null)
        {
            radialMenu = RadialMenuPrefab;
        }
        if (MenuLayers == null)
        {
            MenuLayers = new List<RadialMenu>();
        }

        RadialMenuInstance = Instantiate(radialMenu, FindObjectOfType<Canvas>().transform);
        RadialMenuInstance.gameObject.SetActive(false);

        if (MenuLayers.Count > 0)
        {
            if (MenuLayers[MenuLayers.Count - 1] == null) return;
            MenuLayers[MenuLayers.Count - 1].Data.NextRing = RadialMenuInstance.Data;
            RadialMenuInstance.Data = MenuLayers[Index].Data.NextRing;
            RadialMenuInstance.radius = MenuLayers[MenuLayers.Count - 1].radius + LayerRadiusDifference;
            RadialMenuInstance.callback = MenuLayers[Index].callback;
        }

        MenuLayers.Add(RadialMenuInstance);

        if (Index == MenuLayers.Count - 1 && CreateBool)
        {
            MenuLayers[Index].SpawnButtons();
        }

        if (Paths == null)
        {
            Paths = new List<string>();
        }

        if (Paths.Count <= Index)
        {
            Paths.Add(MenuLayers[Index].Path);
        }
        else
        {
            Paths[Index] = MenuLayers[Index].Path;
        }
    }

    public void SpawnMenu()
    {
        if (!MenuCheck()) { return; }

        // If there are already multiple layers, respawn the menu
        if (MenuLayers.Count > 1)
        {
            RespawnMenu();
        }
        // Otherwise, if createBool is true, create a new menu
        else if (CreateBool)
        {
            CreateMenu();
        }

        // Activate the current menu layer and spawn its buttons
        MenuLayers[Index].gameObject.SetActive(true);
        MenuLayers[Index].SpawnButtons();
    }

    public void DestroyMenu()
    {
        if (!MenuCheck()) { return; }

        foreach (var layer in MenuLayers)
        {
            DestroyImmediate(layer.gameObject);
        }

        MenuLayers.Clear();
        Paths.Clear();
        Index = 0;
    }

    public void RespawnMenu()
    {
        List<RadialMenu> tempLayers= new List<RadialMenu>();

        foreach (RadialMenu menu in MenuLayers)
        {
            tempLayers.Add(menu);
        }
        DestroyMenu();

        foreach(RadialMenu menu in tempLayers)
        {
            CreateMenu(menu);
        }
        LogLayerCount();
    }

    public void TurnMenuRight()
    {
        if (!MenuCheck()) { return; }
        MenuLayers[Index].TurnMenu(-1);
    }

    public void TurnMenuLeft()
    {
        if (!MenuCheck()) { return; }
        MenuLayers[Index].TurnMenu(1);
    }

    public string NextRing()
    {
        if (!MenuCheck()) { return null; }

        var next = MenuLayers[Index].Data.NextRing;

        if ((next != null && CreateBool) || (Index + 1 < MenuLayers.Count && MenuLayers[Index + 1] != null))
        {
            Debug.Log(string.Format("index: {0}, count: {1}", Index + 1, MenuLayers.Count));

            if (Index + 1 == MenuLayers.Count)
            {
                return null;
            }
            else
            {
                MenuLayers[Index].NextRing(false);
                if (Paths.Count - 1 >= Index)
                {
                    Paths[Index] = MenuLayers[Index].Path;
                }
                else
                {
                    Paths.Add(MenuLayers[Index].Path);
                }
                MenuLayers[Index + 1].gameObject.SetActive(true);
            }

            Index++;
            return null;
        }
        else
        {
            MenuLayers[Index].NextRing(false);
            if (Paths.Count - 1 >= Index)
            {
                Paths[Index] = MenuLayers[Index].Path;
            }
            else
            {
                Paths.Add(MenuLayers[Index].Path);
            }

            string pathSummary = string.Join(", ", Paths);
            Debug.Log(pathSummary);

            return MenuLayers[Index].Path;
        }
    }

    public void PreviousRing()
    {
        if (!MenuCheck()) { return; }
        if (Index <= 0) return;
        MenuLayers[Index].gameObject.SetActive(false);
        --Index;
    }

    public bool MenuCheck()
    {
        LogLayerCount();
        if (MenuLayers == null || MenuLayers.Count == 0 || MenuLayers[Index] == null || MenuLayers.Count <= Index)
        {
            Debug.LogWarning("Menu layers have not been created. Please create the menu first.");
            return false;
        }
        return true;
    }

    void LogLayerCount(){
        Debug.Log($"Layer Count: {MenuLayers?.Count ?? 0}");
    }
}