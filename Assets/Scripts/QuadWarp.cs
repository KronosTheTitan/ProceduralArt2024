using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class QuadWarp : MeshCreator
{
    [SerializeField] private Mesh inputMesh;

    private void Start()
    {
        RecalculateMesh();
    }

    private Vector3 WarpVertexUsingQuad(Vector3 vertex, List<Vector3> points)
    {
        float c1 = (1 - vertex.x) * (1 - vertex.z);
        float c2 = (1 - vertex.x) * vertex.z;
        float c3 = vertex.x * vertex.z;
        float c4 = vertex.x * (1 - vertex.z);

        Vector3 output = c1 * points[0] + c2 * points[1] + c3 * points[2] + c4 * points[3];

        output.y = vertex.y;
        
        return output;
    }
    
    public override void RecalculateMesh()
    {
        Curve spline = GetComponent<Curve>();
        if (spline == null)
        {
            Debug.Log("QuadWarp: this game object needs to have a curve component");
            return;
        }

        List<Vector3> points = spline.points;
        if (points.Count != 4)
        {
            Debug.Log("QuadWarp: the curve component needs to have 4 points");
            return;
        }

        Vector3[] warpedVertices = new Vector3[inputMesh.vertices.Length];

        for (int i = 0; i < inputMesh.vertices.Length; i++)
        {
            warpedVertices[i] = WarpVertexUsingQuad(inputMesh.vertices[i], points);
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = warpedVertices;
        newMesh.uv = inputMesh.uv;
        newMesh.subMeshCount = inputMesh.subMeshCount;

        for (int i = 0; i < inputMesh.subMeshCount; i++)
        {
            newMesh.SetTriangles(inputMesh.GetTriangles(i), i);
        }
        
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();

        GetComponent<MeshFilter>().mesh = newMesh;
    }
}
