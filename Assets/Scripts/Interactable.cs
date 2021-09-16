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

    private void Start()
    {
        if (title == "" || title == null) {
            title = gameObject.name;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            RadialMenuSpawner.inst.SpawnMenu(this);
        }
    }
}
