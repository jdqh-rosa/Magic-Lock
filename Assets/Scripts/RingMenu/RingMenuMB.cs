using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingMenuMB : MonoBehaviour
{
    public Ring Data;
    public RingCakePiece RingCakePiecePrefab;
    public float GapWidthDegree = 1f;
    public Action<string> callback;
    protected RingCakePiece[] Pieces;
    protected RingMenuMB Parent;
    [HideInInspector]
    public string Path;

    private Color _selectedColor = new Color(1f, 1f, 1f, 0.75f);
    private Color _unselectedColor = new Color(1f, 1f, 1f, 0.5f);
    void Start()
    {
        //determine size each element must take up
        var stepLength = 360f / Data.Elements.Length;

        //get distance between Icon and Cakepiece
        var iconDist = Vector3.Distance(RingCakePiecePrefab.Icon.transform.position, RingCakePiecePrefab.CakePiece.transform.position);

        //position the elements
        Pieces = new RingCakePiece[Data.Elements.Length];

        for (int i = 0; i < Data.Elements.Length; i++) {
            Pieces[i] = Instantiate(RingCakePiecePrefab, transform);
            //set root element
            Pieces[i].transform.localPosition = Vector3.zero;
            Pieces[i].transform.localRotation = Quaternion.identity;

            //set cake piece
            Pieces[i].CakePiece.fillAmount = 1f / Data.Elements.Length - GapWidthDegree / 360f;
            Pieces[i].CakePiece.transform.localPosition = Vector3.zero;
            Pieces[i].CakePiece.transform.localRotation = Quaternion.Euler(0, 0, -stepLength / 2f + GapWidthDegree / 2f + i * stepLength +90);
            Pieces[i].CakePiece.color = new Color(1f, 1f, 1f, 0.5f);

            //set icon
            Pieces[i].Icon.transform.localPosition = Pieces[i].CakePiece.transform.localPosition + Quaternion.AngleAxis(i * stepLength, Vector3.forward) * Vector3.up * iconDist;
            Pieces[i].Icon.sprite = Data.Elements[i].Icon;
            Pieces[i].Icon.sprite.name = Data.Elements[i].Icon.name;
        }

    }

    // Update is called once per frame
    void Update()
    {
        //recalculate the stepLength every frame
        var stepLength = 360f / Data.Elements.Length;

        //calculate the mouseAngle from the middle of the screen
        var mouseAngle = NormalizeAngle(Vector3.SignedAngle(Vector3.up, Input.mousePosition - transform.position, Vector3.forward) + stepLength / 2f);
        //var mouseAngle = NormalizeAngle(Vector3.SignedAngle(Vector3.up, Input.mousePosition - new Vector3(Screen.width /2 , Screen.height /2), Vector3.forward) + stepLength /2f);
        //Debug.Log(mouseAngle);

        //use the angle of the mouse and the size of each element to determine which element you're selecting 
        var activeElement = (int)(mouseAngle / stepLength);

        //differentiate the selected element
        for (int i = 0; i < Data.Elements.Length; i++) {
            if (i == activeElement) {
                Debug.Log(activeElement);
                Pieces[i].CakePiece.color = _selectedColor;
            }
            else {
                Pieces[i].CakePiece.color = _unselectedColor;
            }
        }

        if (Input.GetMouseButtonDown(0)) {
            Debug.Log(activeElement);

            var path = Path + "/" + Data.Elements[activeElement].Name;
            if (Data.Elements[activeElement].NextRing != null) {
                var newSubRing = Instantiate(gameObject, transform.parent).GetComponent<RingMenuMB>();
                newSubRing.Parent = this;

                for (var j = 0; j < newSubRing.transform.childCount; j++) {
                    Destroy(newSubRing.transform.GetChild(j).gameObject);
                }

                newSubRing.Data = Data.Elements[activeElement].NextRing;
                newSubRing.Path = path;
                newSubRing.callback = callback;
            }
            else {
                callback?.Invoke(path);
            }
            gameObject.SetActive(false);
        }
    }

    private float NormalizeAngle(float a) => (a + 360) % 360f;
}
