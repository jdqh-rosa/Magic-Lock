using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[CreateAssetMenu(fileName = "CreateMenu", menuName = "RadialMenu/Menu", order = 2)]
public class CreateRadialMenu : ScriptableObject
{
    public RadialRing ringTemplate;
    public RadialElement elementTemplate;
    public List<RadialRing> rings = new List<RadialRing>();
    public List<RadialElement> elements = new List<RadialElement>();  
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i =0; i < rings.Count -1; i++)
        {
            if (rings[i] == null)
            {
                rings[i] = Instantiate(new RadialRing());
            }
        }
    }
}
