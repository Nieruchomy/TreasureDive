using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BezierSpline))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RopeCreator : MonoBehaviour
{
    public float width = 1f;
    [Range(0.5f, 1.5f)] public float spacing = 1;
    public bool autoUpdate = true;
    public float tiling = 1;

    public void UpdateRope()
    {
        BezierSplinePath path = GetComponent<BezierSpline>().path;
        Vector3[] points = path.CalculateEvenlySpacedPoints(spacing);
        GetComponent<MeshFilter>().mesh = CreateRopeMesh(points, path.IsClosed);

        int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * 0.5f);
        GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1f, textureRepeat);
    }

    Mesh CreateRopeMesh(Vector3[] points, bool isClosed)
    {
        Vector3[] vertices = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int numTriangles = 2 * (points.Length - 1) + ((isClosed) ? 2 : 0);
        int[] triangles = new int[numTriangles * 3];
 
        int vertexIndex = 0;
        int triangleIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if(i < points.Length - 1)
            {
                forward += points[(i + 1) % points.Length]  - points[i];
            }
            if(i > 0 || isClosed)
            {
                forward += points[i] - points[(i - 1 + points.Length) % points.Length];
            }
            forward.Normalize();

            //TODO: FIX LEFT
            Vector3 left = new Vector3(-forward.y, forward.x);

            vertices[vertexIndex] = points[i] + left * width * 0.5f;
            vertices[vertexIndex + 1] = points[i] - left * width * 0.5f;

            float completionPercent = i / (float)(points.Length - 1);
            uvs[vertexIndex] = new Vector2(0, completionPercent);
            uvs[vertexIndex + 1] = new Vector2(1, completionPercent);

            if (i < points.Length - 1 || isClosed)
            {
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = (vertexIndex + 2) % vertices.Length;
                triangles[triangleIndex + 2] = vertexIndex + 1;

                triangles[triangleIndex + 3] = vertexIndex + 1;
                triangles[triangleIndex + 4] = (vertexIndex + 2) % vertices.Length;
                triangles[triangleIndex + 5] = (vertexIndex + 3) % vertices.Length;
            }

            vertexIndex += 2;
            triangleIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh; 
    }
	

}
