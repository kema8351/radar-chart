using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarChart : BaseMeshEffect
{
    static List<UIVertex> tempVertexTriangleStream = new List<UIVertex>();
    static List<UIVertex> tempInnerVertices = new List<UIVertex>();
    static List<UIVertex> tempOuterVertices = new List<UIVertex>();

    [SerializeField]
    float[] parameters;
    public float[] Parameters
    {
        get { return parameters; }
        set { parameters = value; this.graphic.SetVerticesDirty(); }
    }

    // 0f = 12 o'clock in the watch board
    // plus direction = clockwise direction
    [SerializeField, Range(0f, 360f)]
    float startAngleDegree = 0f;
    public float StartAngleDegree
    {
        get { return startAngleDegree; }
        set { startAngleDegree = value; this.graphic.SetVerticesDirty(); }
    }

    [SerializeField]
    Color outerColor = Color.white;
    public Color OuterColor
    {
        get { return outerColor; }
        set { outerColor = value; this.graphic.SetVerticesDirty(); }
    }

    [SerializeField, Range(0f, 1f)]
    float outerRatio = 1f;
    public float OuterRatio
    {
        get { return outerRatio; }
        set { outerRatio = value; this.graphic.SetVerticesDirty(); }
    }

    [SerializeField]
    Color innerColor = Color.clear;
    public Color InnerColor
    {
        get { return innerColor; }
        set { innerColor = value; this.graphic.SetVerticesDirty(); }
    }

    [SerializeField, Range(0f, 1f)]
    float innerRatio = 0f;
    public float InnerRatio
    {
        get { return innerRatio; }
        set { innerRatio = value; this.graphic.SetVerticesDirty(); }
    }

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

        Color outerMultipliedColor = GetMultipliedColor(vertices[0].color, outerColor);
        Color innerMultipliedColor = GetMultipliedColor(vertices[0].color, innerColor);

        for (int i = 0; i < parameters.Length; i++)
        {
            float parameter = parameters[i];
            float cosine = cacheCosines[i];
            float sine = cacheSines[i];

            UIVertex outerVertex = vertices[0];
            float outerParameter = parameter * outerRatio;
            outerVertex.position = centerPosition + (xUnit * cosine + yUnit * sine) * outerParameter;
            outerVertex.uv0 = centerUv + (uUnit * cosine + vUnit * sine) * outerParameter;
            outerVertex.color = outerMultipliedColor;
            tempOuterVertices.Add(outerVertex);

            UIVertex innerVertex = vertices[0];
            float innerParameter = parameter * innerRatio;
            innerVertex.position = centerPosition + (xUnit * cosine + yUnit * sine) * innerParameter;
            innerVertex.uv0 = centerUv + (uUnit * cosine + vUnit * sine) * innerParameter;
            innerVertex.color = innerMultipliedColor;
            tempInnerVertices.Add(innerVertex);
        }

        if (parameters.Length > 0)
        {
            tempOuterVertices.Add(tempOuterVertices[0]);
            tempInnerVertices.Add(tempInnerVertices[0]);
        }

        vertices.Clear();
        if (outerRatio != 0f)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                vertices.Add(tempInnerVertices[i]);
                vertices.Add(tempOuterVertices[i]);
                vertices.Add(tempOuterVertices[i + 1]);
            }
        }
        if (innerRatio != 0f)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                vertices.Add(tempOuterVertices[i + 1]);
                vertices.Add(tempInnerVertices[i + 1]);
                vertices.Add(tempInnerVertices[i]);
            }
        }

        tempOuterVertices.Clear();
        tempInnerVertices.Clear();
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
