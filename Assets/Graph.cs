using UnityEngine;
using System;

public class Graph : MonoBehaviour
{

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    Color[] colors;

    public Gradient gradient;

    int boundMinX;
    int boundMaxX;
    int boundMinY;
    int boundMaxY;

    double minHeight = 0.0;
    double maxHeight = 0.0;

    EquationSolver equationSolver = new EquationSolver();

    void Start()
    {

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        boundMinX = GraphData.boundMinX;
        boundMaxX = GraphData.boundMaxX;
        boundMinY = GraphData.boundMinY;
        boundMaxY = GraphData.boundMaxY;

        CreateShape();
        UpdateShape();
        
    }

    void CreateShape() {
        vertices = new Vector3[(GraphData.xSize + 1) * (GraphData.zSize + 1)];

        int i = 0;
        for (double z = boundMinY; z < boundMaxY; z += (double) (boundMaxY-boundMinY)/GraphData.zSize) {

            for (double x = boundMinX; x < boundMaxX; x += (double) (boundMaxX-boundMinX)/GraphData.xSize) {
                    double y = GetHeight(Math.Round(x,5),Math.Round(z,5));
                    
                    vertices[i] = new Vector3 ((float)x/100,(float) y/100,(float) z/100);
                    i++;

                    if ((y/100) > maxHeight) {
                        maxHeight = (double) y/100;
                    }
                    if ((y/100) < minHeight) {
                        minHeight = (double) y/100;
                    }
            }
        }

        int vert = 0;
        int tris = 0;
        triangles = new int[GraphData.xSize * GraphData.zSize * 6];

        for (int z = 0; z < GraphData.zSize; z++) {
            for (int x = 0; x < GraphData.xSize; x++) {
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
        Debug.Log($"Max: {maxHeight}     Min: {minHeight}");

        double totalHeight = maxHeight-minHeight;
        for (double z = boundMinY; z < boundMaxY; z += (double) (boundMaxY-boundMinY)/GraphData.zSize) {

            for (double x = boundMinX; x < boundMaxX; x += (double) (boundMaxX-boundMinX)/GraphData.xSize) {
                    double height = (double) (vertices[i].y-minHeight)/totalHeight;
                    if (height > 1) { height = 1.0; }
                    if (height < 0) { height = 0.0; }
                    colors[i] = gradient.Evaluate((float)height);
                    i++;
            }
        }
    }

    void UpdateShape() {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
    }

    private double GetHeight(double x, double z) {
        string equation = GraphData.equationString;

        equation = equation.Replace("x", "(" + Convert.ToString(x) + ")") ;
        equation = equation.Replace("y", "(" + Convert.ToString(z) + ")");

        return equationSolver.SolveEquation(equation);
    }

}