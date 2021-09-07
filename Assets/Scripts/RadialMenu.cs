using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    public float speed = 0.5f;
    public Text label;
    public RadialButton buttonPrefab;
    public RadialButton selected;
    private int selectedInt;
    private RadialButton[] buttons;
    private Vector3[] newButtonPositions;

    // Start is called before the first frame update
    public void SpawnButtons(Interactable obj)
    {
        StartCoroutine(AnimateButtons(obj));
    }

    IEnumerator AnimateButtons(Interactable obj)
    {
        buttons = new RadialButton[obj.options.Length];
        newButtonPositions = new Vector3[obj.options.Length];

        for (int i = 0; i < obj.options.Length; i++) {
            RadialButton newButton = Instantiate(buttonPrefab) as RadialButton;
            buttons[i] = newButton;
            newButton.transform.SetParent(transform, false);
            float theta = (2 * Mathf.PI / obj.options.Length) * i;
            float xPos = Mathf.Sin(theta);
            float yPos = Mathf.Cos(theta);
            newButton.transform.localPosition = new Vector2(xPos, yPos) * 100f;
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

        if (Input.GetKeyUp(KeyCode.RightArrow)) {
            TurnMenu(1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            TurnMenu(-1);
        }
    }

    public void TurnMenu(int tileAmount)
    {
        selectedInt += tileAmount;
        for (int i = 0; i < buttons.Length; i++) {
            float theta = (2 * Mathf.PI / buttons.Length) * (i + selectedInt);
            float xPos = Mathf.Sin(theta);
            float yPos = Mathf.Cos(theta);
            newButtonPositions[i] = new Vector2(xPos, yPos) * 100f;
        }
        StartCoroutine(TurnAnimation());
    }

    IEnumerator TurnAnimation()
    {
        float timer = 0f;
        Vector3[] oldButtonPos = new Vector3[buttons.Length];

        for(int i=0; i<buttons.Length; i++) {
            oldButtonPos[i] = buttons[i].transform.localPosition;
        }

        while (timer < 1 / speed) {
            timer += Time.deltaTime;
            for(int i=0; i<buttons.Length; i++) {
                buttons[i].transform.localPosition = Vector3.Lerp(oldButtonPos[i] , newButtonPositions[i], timer);
            }
            yield return null;
        }

        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].transform.localPosition = newButtonPositions[i];
        }
    }
}
