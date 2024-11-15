using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RadialRing", menuName = "RadialMenu/Ring", order = 1)]
public class RadialRing : ScriptableObject
{
    public RadialElement[] Elements;
    public List<RadialElement> ElementsList = new List<RadialElement>();
    public RadialRing NextRing;
    public string MenuName;
    public float Radius;
    public float GapWidthDegree;

    public GameObject LineCircle;

    public Action<string> CallBack;
    [HideInInspector]
    public string Path;

    public float TurnTime;
    public float RushMultiplier;

    public void CopyFrom(RadialMenu radialMenu)
    {
        MenuName = radialMenu.name;
        Radius = radialMenu.radius;
        GapWidthDegree = radialMenu.GapWidthDegree;
        LineCircle = radialMenu.lineCircle;
        CallBack = radialMenu.callback;
        Path = radialMenu.path;
        TurnTime = radialMenu.turnTime;
        RushMultiplier = radialMenu.rushMultiplier;

        Elements = new RadialElement[radialMenu.Data.Elements.Length];
        Array.Copy(radialMenu.Data.Elements, Elements, radialMenu.Data.Elements.Length);
        
        ElementsList = radialMenu.Data.ElementsList;

        if (radialMenu.Data.NextRing){
            //NextRing = ScriptableObject.CreateInstance<RadialRing>();
            //NextRing.CopyFrom(radialMenu.Data.NextRing);
        }
    }

    public void AddElement(RadialElement radialElement)
    {
        RingCleanArray();
        RingCleanList();
        
        RadialElement[] tempElements = Elements;
        Elements = new RadialElement[tempElements.Length + 1];
        Array.Copy(tempElements, 0, Elements, 0, tempElements.Length);
        Elements[Elements.Length-1] = radialElement;
        
        ElementsList.Add(radialElement);
    }

    private void RingCleanArray()
    {
        RadialElement[] newElements = new RadialElement[Elements.Length];
        int length = 0;
        for (int i = 0; i < Elements.Length; i++) {
            if (Elements[i]) {
                newElements[i] = Elements[i];
                length++;
            }
        }
        Elements = new RadialElement[length];
        Array.Copy(newElements, 0, Elements, 0, length);
    }
    private void RingCleanList()
    {
        List<RadialElement> _cleanList = new List<RadialElement>();
        foreach (RadialElement radialElement in ElementsList) {
            if (!radialElement) _cleanList.Add(radialElement); 
        }

        foreach (var element in _cleanList) {
            ElementsList.Remove(element);
        }
    }

    public RadialMenu ToRadialMenu()
    {
        var radialMenu = new RadialMenu
        {
            name = this.MenuName,
            radius = this.Radius,
            GapWidthDegree = this.GapWidthDegree,
            lineCircle = this.LineCircle,
            callback = this.CallBack,
            path = this.Path,
            turnTime = this.TurnTime,
            rushMultiplier = this.RushMultiplier
        };

        var elements = new RadialElement[Elements.Length];
        Array.Copy(Elements, elements, Elements.Length);

        radialMenu.Data = new RadialRing
        {
            Elements = elements,
            NextRing = this.NextRing
        };

        return radialMenu;
    }
}