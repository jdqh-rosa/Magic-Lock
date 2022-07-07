using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenuDepr : MonoBehaviour
{
    public float speed = 8;
    public float radius = 100f;
    public Text label;
    public RadialButtonDepr buttonPrefab;
    public RadialButtonDepr selected;
    private int selectedInt = 0;
    private RadialButtonDepr[] buttons;
    private Vector3[] newButtonPositions;

    // Start is called before the first frame update
    public void SpawnButtons(Interactable obj)
    {
        StartCoroutine(AnimateButtons(obj));
    }

    IEnumerator AnimateButtons(Interactable obj)
    {
        buttons = new RadialButtonDepr[obj.options.Length];
        newButtonPositions = new Vector3[obj.options.Length];

        for (int i = 0; i < obj.options.Length; i++) {
            RadialButtonDepr newButton = Instantiate(buttonPrefab) as RadialButtonDepr;
            buttons[i] = newButton;
            newButton.transform.SetParent(transform, false);
            newButton.transform.localPosition = Helper.CalculateDegPos((360 / obj.options.Length) * i + 90, radius);
            newButton.circle.color = obj.options[i].color;
            newButton.icon.sprite = obj.options[i].sprite;
            newButton.title = obj.options[i].title;
            newButton.myMenu = this;
            newButton.Anim();
            yield return new WaitForSeconds(0.06f);
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space)) {
            if (selected) {
                Debug.Log(selected.title + " was selected.");
            }
            Destroy(gameObject);
        }
    }

    public void TurnMenu(int tileAmount)
    {
        if (buttons[buttons.Length - 1] == null) return; 

        selectedInt += tileAmount;

        while (selectedInt < 0 || selectedInt > buttons.Length - 1) {
            selectedInt = (selectedInt > buttons.Length - 1) ? selectedInt - buttons.Length : selectedInt + buttons.Length;
        }

        selected = buttons[selectedInt];

        for (int i = 0; i < buttons.Length; i++) {
            newButtonPositions[i] = Helper.CalculateDegPos((360 / buttons.Length) * (i + selectedInt) + 90, radius);
        }
        StartCoroutine(TurnAnimation());
    }

    IEnumerator TurnAnimation()
    {
        float timer = 0f;

        while (timer <= 1 / speed) {
            timer += Time.deltaTime;
            for (int i = 0; i < buttons.Length; i++) {
                if (buttons[i] == null) yield return null;

                buttons[i].transform.localPosition = Vector3.Lerp(buttons[i].transform.localPosition, newButtonPositions[i], timer * speed);
            }
            yield return null;
        }

        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].transform.localPosition = newButtonPositions[i];
        }
    }
}
