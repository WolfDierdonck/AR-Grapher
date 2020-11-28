using UnityEngine;
using System;

public class Graph : MonoBehaviour
{

    Mesh mesh;

    GameObject meshGen;
    public Gradient test;

    Vector3[] vertices;
    int[] triangles;
    Color[] colors;

    Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    int boundMinX;
    int boundMaxX;
    int boundMinY;
    int boundMaxY;

    double minHeight = 0.0;
    double maxHeight = 0.0;

    UpdatedEqSolver equationSolver;

    public void GenerateMesh()
    {

        gradient = new Gradient();

        colorKey = new GradientColorKey[7];
        colorKey[0].color = new Color32(29, 61, 209, 255);
        colorKey[0].time = 0.0f;
        colorKey[1].color = new Color32(59, 171, 226, 255);
        colorKey[1].time = 0.25f;
        colorKey[2].color = new Color32(61, 236, 76, 255);
        colorKey[2].time = 0.37f;
        colorKey[3].color = new Color32(124, 241, 134, 255);
        colorKey[3].time = 0.56f;
        colorKey[4].color = new Color32(250, 255, 102, 255);
        colorKey[4].time = 0.65f;
        colorKey[5].color = new Color32(255, 54, 54, 255);
        colorKey[5].time = 0.8f;
        colorKey[6].color = new Color32(255, 50, 20, 255);
        colorKey[6].time = 1.0f;

        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);

        mesh = new Mesh();
        meshGen = GameObject.Find("Mesh Gen");
        meshGen.GetComponent<MeshFilter>().mesh = mesh;

        equationSolver = new UpdatedEqSolver(GraphData.equationString);
        equationSolver.ParseEquation();

        boundMinX = GraphData.boundMinX;
        boundMaxX = GraphData.boundMaxX;
        boundMinY = GraphData.boundMinY;
        boundMaxY = GraphData.boundMaxY;

        CreateShape();
        UpdateShape();

    }

    void CreateShape()
    {
        vertices = new Vector3[(GraphData.xSize + 1) * (GraphData.zSize + 1)];

        int i = 0;
        for (double z = boundMinY; z < boundMaxY; z += (double)(boundMaxY - boundMinY) / GraphData.zSize)
        {

            for (double x = boundMinX; x < boundMaxX; x += (double)(boundMaxX - boundMinX) / GraphData.xSize)
            {
                double y = equationSolver.CalculateFunction(Math.Round(x, 5), Math.Round(z, 5));

                vertices[i] = new Vector3((float)x / 100, (float)y / 100, (float)z / 100);
                i++;

                if ((y / 100) > maxHeight)
                {
                    maxHeight = (double)y / 100;
                }
                if ((y / 100) < minHeight)
                {
                    minHeight = (double)y / 100;
                }
            }
        }

        int vert = 0;
        int tris = 0;
        triangles = new int[GraphData.xSize * GraphData.zSize * 6];

        for (int z = 0; z < GraphData.zSize; z++)
        {
            for (int x = 0; x < GraphData.xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + GraphData.xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + GraphData.xSize + 1;
                triangles[tris + 5] = vert + GraphData.xSize + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }

        colors = new Color[vertices.Length];

        i = 0;

        maxHeight = (maxHeight > 0.25) ? 0.25 : maxHeight;
        minHeight = (minHeight < -0.25) ? -0.25 : minHeight;

        double totalHeight = maxHeight - minHeight;
        for (double z = boundMinY; z < boundMaxY; z += (double)(boundMaxY - boundMinY) / GraphData.zSize)
        {

            for (double x = boundMinX; x < boundMaxX; x += (double)(boundMaxX - boundMinX) / GraphData.xSize)
            {
                double height = (double)(vertices[i].y - minHeight) / totalHeight;
                if (height > 1) { height = 1.0; }
                if (height < 0) { height = 0.0; }
                colors[i] = gradient.Evaluate((float)height);
                i++;
            }
        }
    }

    void UpdateShape()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
    }

}