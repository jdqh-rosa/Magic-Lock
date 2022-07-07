using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [System.Serializable]
    public class Action
    {
        public Color color;
        public Sprite sprite;
        public string title;
    }

    public string title;
    public Action[] options;
    protected private RadialMenuDepr radialMenu;

    private void Start()
    {
        if (title == "" || title == null) {
            title = gameObject.name;
        }
    }

    virtual public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            radialMenu = RadialMenuSpawnerDepr.inst.SpawnMenu(this);
        }
    }
}
