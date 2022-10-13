using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    public RadialRing Data;
    public RadialRing[] Rings;
    public RadialButton buttonPrefab;
    public float GapWidthDegree = 1f;
    public float radius = 100f;

    public GameObject lineCircle;

    public Action<string> callback;
    protected RadialButton[] Buttons;
    protected float[] NewButtonPositions;
    protected float[] CurrentButtonPositions;
    protected RadialMenu Parent;
    [HideInInspector]
    public string Path;

    private int selectedInt = 0;

    public float turnTime = 1f;
    private Color _selectedColor = new Color(1f, 1f, 1f, 0.75f);
    private Color _unselectedColor = new Color(1f, 1f, 1f, 0.5f);

    private void Start()
    {
        lineCircle = Instantiate(lineCircle, transform);
        lineCircle.transform.localScale = new Vector3(radius, radius) * 0.5f;
    }

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
        NewButtonPositions = new float[Data.Elements.Length];
        CurrentButtonPositions = new float[Data.Elements.Length];

        for (int i = 0; i < Data.Elements.Length; i++)
        {
            RadialButton newButton = Instantiate(buttonPrefab) as RadialButton;
            float angle = stepLength * i + 90;
            Buttons[i] = newButton;
            newButton.transform.SetParent(transform, false);
            newButton.transform.localPosition = Helper.PolarToCart(angle, radius);
            CurrentButtonPositions[i] = angle;

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
            newSubRing.callback = callback;

            return newSubRing;
        }
        else
        {
            Debug.Log(path);
            callback?.Invoke(path);
            return null;
        }
        //gameObject.SetActive(false);
    }


    bool coroutineRunning = false;
    public void TurnMenu(int tileAmount)
    {

        var stepLength = 360f / Data.Elements.Length;

        if (Buttons[Buttons.Length - 1] == null) return;

        if (!coroutineRunning)
        {

            selectedInt += tileAmount;

            while (selectedInt < 0 || selectedInt > Buttons.Length - 1)
            {
                selectedInt = (selectedInt > Buttons.Length - 1) ? selectedInt - Buttons.Length : selectedInt + Buttons.Length;
            }

            for (int i = 0; i < Buttons.Length; i++)
            {
                NewButtonPositions[i] = NormalizeAngle(stepLength * (i + selectedInt) + 90);
            }
            StartCoroutine(TurnAnimation());
        }
        else
        {
            StopCoroutine(TurnAnimation());
            ResetButtonPositions();
        }
    }

    IEnumerator TurnAnimation()
    {
        float timer = 0f;
        float completion = 0;
        coroutineRunning = true;

        while (1 - completion >= 0.001)
        {
            timer += Time.fixedDeltaTime;
            for (int i = 0; i < Buttons.Length; i++)
            {
                if (Buttons[i] == null) yield return null;

                float differAngle = NewButtonPositions[i] - CurrentButtonPositions[i];
                //if(differAngle > 180) { differAngle -= 180; }
                if (differAngle > 360 / Buttons.Length) { differAngle = NewButtonPositions[i] - 360; }
                 else if (differAngle < -360 / Buttons.Length) { differAngle = 360 / Buttons.Length; }

                completion = timer / (turnTime - (turnTime % Time.fixedDeltaTime));
                float stepAngle = differAngle / (turnTime / Time.fixedDeltaTime);

                Buttons[i].transform.localPosition = Helper.OrbitPoint(transform.localPosition, Buttons[i].transform.localPosition, stepAngle);
            }
            yield return null;
        }
        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].transform.localPosition = Helper.PolarToCart(NewButtonPositions[i], radius);
            CurrentButtonPositions[i] = NewButtonPositions[i];
        }
        coroutineRunning = false;
    }

    void ResetButtonPositions()
    {
        coroutineRunning = false;

        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].transform.localPosition = Helper.PolarToCart(NewButtonPositions[i], radius);
            CurrentButtonPositions[i] = NewButtonPositions[i];
        }
        coroutineRunning = false;
    }

    private float NormalizeAngle(float a) => (a + 360) % 360f;

}
