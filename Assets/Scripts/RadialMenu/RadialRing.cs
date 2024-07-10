using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RadialRing", menuName = "RadialMenu/Ring", order = 1)]
public class RadialRing : ScriptableObject
{
    public RadialElement[] Elements;
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
        Path = radialMenu.Path;
        TurnTime = radialMenu.turnTime;
        RushMultiplier = radialMenu.rushMultiplier;

        Elements = new RadialElement[radialMenu.Data.Elements.Length];
        Array.Copy(radialMenu.Data.Elements, Elements, radialMenu.Data.Elements.Length);

        if (radialMenu.Data.NextRing != null)
        {
            //NextRing = ScriptableObject.CreateInstance<RadialRing>();
            //NextRing.CopyFrom(radialMenu.Data.NextRing);
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
            Path = this.Path,
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
