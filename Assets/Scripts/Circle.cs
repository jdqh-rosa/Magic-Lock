using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    public int vertexCount = 40;
    public float lineWidth = -.2f;
    public float radius;
    public bool circleFillScreen;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        SetupCircle();    
    }

    private void SetupCircle()
    {
        lineRenderer.widthMultiplier = lineWidth;

        float deltaTheta = (2f * Mathf.PI) / vertexCount;
        float theta = 0f;

        lineRenderer.positionCount = vertexCount;
        for(int i=0; i < lineRenderer.positionCount; i++)
        {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta), 0);
            lineRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }

    //#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        float deltaTheta = (2f * Mathf.PI) / 40f;
        float theta = 0f;

        Vector3 oldPos = Vector3.zero;
        for (int i = 0; i < vertexCount + 1; i++)
        {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta), 0f);
            Gizmos.DrawLine(oldPos, transform.position + pos);
            oldPos = transform.position + pos;

            theta += deltaTheta;
        }
    }
    //#endif
}
