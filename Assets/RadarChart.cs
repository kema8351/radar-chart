using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarChart : BaseMeshEffect
{
    static List<UIVertex> tempVertexTriangleStream = new List<UIVertex>();
    static List<UIVertex> tempVertices = new List<UIVertex>();

    [SerializeField]
    public float[] parameters;

    // 0f = 12 o'clock in the watch board
    // plus direction = clockwise direction
    [SerializeField, Range(0f, 360f)]
    public float startAngleDegree = 0f;

    [SerializeField]
    public Color32 outerColor = Color.white;

    [SerializeField]
    public Color32 centerColor = Color.white;

    float? cacheStartAngleDegree = null;
    List<float> cacheSines = new List<float>();
    List<float> cacheCosines = new List<float>();

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!this.IsActive())
            return;

        vh.GetUIVertexStream(tempVertexTriangleStream);

        ModifyVertices(tempVertexTriangleStream);

        vh.Clear();
        vh.AddUIVertexTriangleStream(tempVertexTriangleStream);

        tempVertexTriangleStream.Clear();
    }

    void ModifyVertices(List<UIVertex> vertices)
    {
        if (parameters == null)
            return;

        if (NeedsToUpdateCaches())
            CacheSinesAndCosines();

        Vector3 centerPosition = (vertices[0].position + vertices[2].position) / 2f;
        Vector3 xUnit = (centerPosition.x - vertices[0].position.x) * Vector3.right;
        Vector3 yUnit = (centerPosition.y - vertices[0].position.y) * Vector3.up;

        Vector2 centerUv = (vertices[0].uv0 + vertices[2].uv0) / 2f;
        Vector2 uUnit = (centerUv.x - vertices[0].uv0.x) * Vector3.right;
        Vector2 vUnit = (centerUv.y - vertices[0].uv0.y) * Vector3.up;

        Color32 outerMultipliedColor = GetMultipliedColor(vertices[0].color, outerColor);
        Color32 centerMultipliedColor = GetMultipliedColor(vertices[0].color, centerColor);

        UIVertex centerVertex = vertices[0];
        centerVertex.position = centerPosition;
        centerVertex.uv0 = centerUv;
        centerVertex.color = centerMultipliedColor;

        for (int i = 0; i < parameters.Length; i++)
        {
            float parameter = parameters[i];
            float cosine = cacheCosines[i];
            float sine = cacheSines[i];

            UIVertex vertex = vertices[0];
            vertex.position = centerPosition + (xUnit * cosine + yUnit * sine) * parameter;
            vertex.uv0 = centerUv + (uUnit * cosine + vUnit * sine) * parameter;
            vertex.color = outerMultipliedColor;

            tempVertices.Add(vertex);
        }

        tempVertices.Add(tempVertices[0]);

        vertices.Clear();
        for (int i = 0; i < parameters.Length; i++)
        {
            vertices.Add(centerVertex);
            vertices.Add(tempVertices[i]);
            vertices.Add(tempVertices[i + 1]);
        }

        tempVertices.Clear();
    }

    bool NeedsToUpdateCaches()
    {
        return
            !cacheStartAngleDegree.HasValue ||
            cacheStartAngleDegree.Value != startAngleDegree ||
            cacheSines.Count != parameters.Length;
    }

    void CacheSinesAndCosines()
    {
        cacheSines.Clear();
        cacheCosines.Clear();

        float startAngleRadian = (90f - startAngleDegree) / 180f * (float)Math.PI;
        float unitRadian = -2f * (float)Math.PI / (float)parameters.Length;

        for (int i = 0; i < parameters.Length; i++)
        {
            float radian = startAngleRadian + (float)i * unitRadian;
            cacheSines.Add(Mathf.Sin(radian));
            cacheCosines.Add(Mathf.Cos(radian));
        }

        cacheStartAngleDegree = startAngleDegree;
    }

    Color32 GetMultipliedColor(Color32 color1, Color32 color2)
    {
        return new Color32(
            (Byte)(color1.r * color2.r / 255),
            (Byte)(color1.g * color2.g / 255),
            (Byte)(color1.b * color2.b / 255),
            (Byte)(color1.a * color2.a / 255));
    }
}
