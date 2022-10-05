using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    public RadialRing Data;
    public RadialButton buttonPrefab;
    public float GapWidthDegree = 1f;
    public float radius = 100f;

    public GameObject lineCircle;

    public Action<string> callback;
    protected RadialButton[] Buttons;
    protected Vector3[] NewButtonPositions;
    protected RadialMenu Parent;
    [HideInInspector]
    public string Path;

    private int selectedInt = 0;

    public float speed = 8;
    private Color _selectedColor = new Color(1f, 1f, 1f, 0.75f);
    private Color _unselectedColor = new Color(1f, 1f, 1f, 0.5f);

    public void SpawnButtons()
    {
        StartCoroutine(AnimateButtons());
    }

    IEnumerator AnimateButtons()
    {
        //determine size each element must take up
        var stepLength = 360f / Data.Elements.Length;

        //get distance between Icon and Background
        var iconDist = Vector3.Distance(buttonPrefab.Icon.transform.position, buttonPrefab.Background.transform.position);

        //position the elements
        Buttons = new RadialButton[Data.Elements.Length];
        NewButtonPositions = new Vector3[Data.Elements.Length];

        for (int i = 0; i < Data.Elements.Length; i++)
        {
            RadialButton newButton = Instantiate(buttonPrefab) as RadialButton;
            Buttons[i] = newButton;
            newButton.transform.SetParent(transform, false);
            newButton.transform.localPosition = Helper.CalculateDegPos(stepLength * i + 90, radius);

            //set background circle
            Buttons[i].Background.sprite = Data.Elements[i].Circle;

            //set icon
            Buttons[i].Icon.transform.localPosition = Buttons[i].Background.transform.localPosition + Quaternion.AngleAxis(i * stepLength, Vector3.forward) * Vector3.up * iconDist;
            Buttons[i].Icon.sprite = Data.Elements[i].Icon;
            Buttons[i].Icon.sprite.name = Data.Elements[i].Icon.name;

            Buttons[i].Anim();
            newButton.Anim();
            yield return new WaitForSeconds(0.06f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Buttons[Buttons.Length - 1] == null) return;
        //differentiate the selected element
        for (int i = 0; i < Data.Elements.Length; i++)
        {
            if (i == selectedInt)
            {
                //Debug.Log(selectedInt);
                Buttons[i].Background.color = _selectedColor;
            }
            else
            {
                Buttons[i].Background.color = _unselectedColor;
            }
        }
    }

    public RadialMenu NextRing()
    {
        //Debug.Log(selectedInt);

        var path = Path + "/" + Data.Elements[selectedInt].Name;
        if (Data.NextRing != null)
        {
            var newSubRing = Instantiate(gameObject, transform.parent).GetComponent<RadialMenu>();
            newSubRing.Parent = this;

            for (var j = 0; j < newSubRing.transform.childCount; j++)
            {
                Destroy(newSubRing.transform.GetChild(j).gameObject);
            }

            newSubRing.Data = Data.NextRing;
            newSubRing.radius = radius + 100f;
            newSubRing.Path = path;
            newSubRing.callback = callback;

            return newSubRing;
        }
        else
        {
            callback?.Invoke(path);
            return null;
        }
        //gameObject.SetActive(false);
    }

    public void TurnMenu(int tileAmount)
    {
        var stepLength = 360f / Data.Elements.Length;

        if (Buttons[Buttons.Length - 1] == null) return;

        selectedInt += tileAmount;

        while (selectedInt < 0 || selectedInt > Buttons.Length - 1)
        {
            selectedInt = (selectedInt > Buttons.Length - 1) ? selectedInt - Buttons.Length : selectedInt + Buttons.Length;
        }

        for (int i = 0; i < Buttons.Length; i++)
        {
            NewButtonPositions[i] = Helper.CalculateDegPos((stepLength) * (i + selectedInt) + 90, radius);
        }
        StartCoroutine(TurnAnimation());
    }

    IEnumerator TurnAnimation()
    {
        float timer = 0f;

        while (timer <= 1 / speed)
        {
            timer += Time.deltaTime;
            for (int i = 0; i < Buttons.Length; i++)
            {
                if (Buttons[i] == null) yield return null;

                Buttons[i].transform.localPosition = Vector3.Lerp(Buttons[i].transform.localPosition, NewButtonPositions[i], timer * speed);
            }
            yield return null;
        }

        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].transform.localPosition = NewButtonPositions[i];
        }
    }

    private float NormalizeAngle(float a) => (a + 360) % 360f;

}
