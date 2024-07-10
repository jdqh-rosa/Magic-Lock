using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RadialMenu : MonoBehaviour
{
    public RadialRing Data;
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
    public float rushMultiplier = 5f;
    private Color _selectedColor = new Color(1f, 1f, 1f, 0.75f);
    private Color _unselectedColor = new Color(1f, 1f, 1f, 0.5f);

    private void Start() {
        if (Buttons == null || Buttons.Length == 0)
        {
            if (Data.Elements.Length >= 0)
            {
                SpawnButtons();
            }
        }
    }

    // Update is called once per frame
    void Update() {
        if (Buttons == null || Buttons[Buttons.Length - 1] == null) return;
        //differentiate the selected element
        for (int i = 0; i < Data.Elements.Length; i++) {
            if (i == selectedInt) {
                //Debug.Log(selectedInt);
                Buttons[i].Background.color = _selectedColor;
            }
            else {
                Buttons[i].Background.color = _unselectedColor;
            }
        }
        Debug.Log($"Selected Element: {Data.Elements[selectedInt]},,, Selected Button: {Buttons[selectedInt]}");
    }

    public void SpawnButtons() {
        gameObject.SetActive(true);
        if (Buttons != null) return;
        lineCircle = Instantiate(lineCircle, transform);
        lineCircle.transform.localScale = new Vector3(radius, radius) * 0.5f;
        StartCoroutine(AnimateButtons());
    }

    IEnumerator AnimateButtons() {
        //determine size each element must take up
        var stepLength = 360f / Data.Elements.Length;

        //get distance between Icon and Background
        var iconDist = Vector3.Distance(buttonPrefab.Icon.transform.position, buttonPrefab.Background.transform.position);

        //position the elements
        Buttons = new RadialButton[Data.Elements.Length];
        NewButtonPositions = new float[Data.Elements.Length];
        CurrentButtonPositions = new float[Data.Elements.Length];

        for (int i = 0; i < Data.Elements.Length; i++) {
            RadialButton newButton = Instantiate(buttonPrefab);
            newButton.name = $"Button:{Data.Elements[i].name}";
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
    public RadialMenu NextRing(bool createNew = true) {
        Path = Data.Elements[selectedInt].Name;
        coroutineRushing = coroutineRunning;

        if (createNew == false) { return null; }

        if (Data.NextRing != null) {

            var newSubRing = Instantiate(gameObject, transform.parent).GetComponent<RadialMenu>();
            newSubRing.Parent = this;

            return newSubRing;
        }
        else {
            //callback?.Invoke(path);
            return null;
        }
    }


    bool coroutineRunning = false;
    bool coroutineRushing = false;
    public void TurnMenu(int tileAmount) {

        var stepLength = 360f / Data.Elements.Length;

        if (Buttons[Buttons.Length - 1] == null) return;

        if (!coroutineRunning) {
            coroutineRushing = coroutineRunning;
            selectedInt += tileAmount;

            while (selectedInt < 0 || selectedInt > Buttons.Length - 1) {
                selectedInt = (selectedInt > Buttons.Length - 1) ? selectedInt - Buttons.Length : selectedInt + Buttons.Length;
            }

            for (int i = 0; i < Buttons.Length; i++) {
                NewButtonPositions[i] = NormalizeAngle(stepLength * (i - selectedInt) + 90);
            }
            StartCoroutine(TurnAnimation());
        }
        else {
            coroutineRushing = coroutineRunning;
        }
    }

    private float speedMultiplier = 1f;
    IEnumerator TurnAnimation() {
        float timer = 0f;
        coroutineRunning = true;

        float currentTurnTime = turnTime;
        float completion = 0f;

        while (completion < 1) {
            if (coroutineRushing) {
                speedMultiplier = rushMultiplier;
            }

            timer += Time.deltaTime * speedMultiplier;
            completion = timer / currentTurnTime;
            completion *= speedMultiplier;

            for (int i = 0; i < Buttons.Length; i++) {
                if (Buttons[i] == null) continue;

                float currentAngle = CurrentButtonPositions[i];
                float targetAngle = NewButtonPositions[i];

                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, completion);

                Buttons[i].transform.localPosition = Helper.PolarToCart(newAngle, radius);
            }
            yield return new WaitForFixedUpdate();
        }
        for (int i = 0; i < Buttons.Length; i++) {
            Buttons[i].transform.localPosition = Helper.PolarToCart(NewButtonPositions[i], radius);
            CurrentButtonPositions[i] = NewButtonPositions[i];
        }
        coroutineRushing = coroutineRunning = false;
        speedMultiplier = 1f;
    }

    void ResetButtonPositions() {
        StopCoroutine(TurnAnimation());
        coroutineRushing = coroutineRunning = false;

        for (int i = 0; i < Buttons.Length; i++) {
            Buttons[i].transform.localPosition = Helper.PolarToCart(NewButtonPositions[i], radius);
            CurrentButtonPositions[i] = NewButtonPositions[i];
        }
    }

    private float NormalizeAngle(float a) => (a + 360) % 360f;

    private void OnDisable() {
        if (coroutineRunning) {
            ResetButtonPositions();
        }
    }

    public void DestroyMenu() {
        foreach (var button in Buttons) {
            DestroyImmediate(button.gameObject);
        }
        DestroyImmediate(lineCircle);
        DestroyImmediate(this);
    }
}