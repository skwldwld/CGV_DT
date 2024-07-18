using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class test : MonoBehaviour
{
    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("MeshFilter �Ǵ� �޽ð� �����ϴ�.");
            return;
        }

        Mesh originalMesh = meshFilter.sharedMesh;

        // �ݿ� ����� �޽� ����
        Mesh semicircleMesh = CreateSemicircleMesh();

        // �ݿ� ����� �������� ���� �޽ø� �ڸ���
        Mesh cutMesh = CutMeshBySemicircle(originalMesh, semicircleMesh);

        // ���ο� �޽� ����
        meshFilter.mesh = cutMesh;
    }

    // �ݿ� ����� �޽� ���� �Լ�
    Mesh CreateSemicircleMesh()
    {
        int segments = 32; // �ݿ��� ���׸�Ʈ ��
        float radius = 5f; // �ݿ��� ������
        float angleStep = Mathf.PI / segments; // ���� ����

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 2]; // �߽��� + �ݿ� ����
        int[] triangles = new int[segments * 3]; // �ﰢ�� �ε��� �迭

        // �߽��� �߰�
        vertices[0] = Vector3.zero;

        // �ݿ��� ���� ��ġ ���
        for (int i = 1; i <= segments + 1; i++)
        {
            float angle = angleStep * (i - 1);
            vertices[i] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        // �ﰢ�� �迭 ����
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0; // �߽���
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i < segments) ? i + 2 : 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    // �ݿ� ������� ���� �޽ø� �ڸ��� �Լ�
    Mesh CutMeshBySemicircle(Mesh originalMesh, Mesh semicircleMesh)
    {
        Vector3[] originalVertices = originalMesh.vertices;
        int[] originalTriangles = originalMesh.triangles;
        Vector3[] semicircleVertices = semicircleMesh.vertices;
        int[] semicircleTriangles = semicircleMesh.triangles;

        Vector3 semicircleNormal = Vector3.up; // �ݿ� �޽��� ��� normal
        Vector3 semicircleCenter = Vector3.zero; // �ݿ� �޽��� �߽�

        // �ݿ��� ��輱 ���� �ִ� ���� �޽��� ������ ����� ����Ʈ
        var cutVerticesList = new System.Collections.Generic.List<Vector3>();
        var cutTrianglesList = new System.Collections.Generic.List<int>();

        // ���� �޽��� �� ������ �ݿ� ������ ����� �˻��Ͽ� �ڸ���
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertexPosition = originalVertices[i];

            // �ݿ� ������ ���� �˻�
            bool isInsideSemicircle = Vector3.Dot(vertexPosition - semicircleCenter, semicircleNormal) >= 0f;

            if (isInsideSemicircle)
            {
                // �ݿ� ���ο� �ִ� ���� �޽��� ���� �߰�
                cutVerticesList.Add(vertexPosition);
            }
        }

        // ���� �޽��� ������ �ﰢ�� �迭�� �ٽ� ����
        Vector3[] cutVertices = cutVerticesList.ToArray();
        int[] cutTriangles = new int[originalTriangles.Length];

        // ���� �޽��� �ﰢ�� �迭�� �ݿ� �޽ÿ� �����Ͽ� �ڸ���
        int cutIndex = 0;
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int index1 = originalTriangles[i];
            int index2 = originalTriangles[i + 1];
            int index3 = originalTriangles[i + 2];

            bool inside1 = cutVerticesList.Contains(originalVertices[index1]);
            bool inside2 = cutVerticesList.Contains(originalVertices[index2]);
            bool inside3 = cutVerticesList.Contains(originalVertices[index3]);

            // �ݿ� ���ο� �ִ� �ﰢ���� ����
            if (inside1 && inside2 && inside3)
            {
                cutTriangles[cutIndex++] = ArrayUtility.IndexOf(cutVertices, originalVertices[index1]);
                cutTriangles[cutIndex++] = ArrayUtility.IndexOf(cutVertices, originalVertices[index2]);
                cutTriangles[cutIndex++] = ArrayUtility.IndexOf(cutVertices, originalVertices[index3]);
            }
        }

        // ���������� �ڸ� �޽� ����
        Mesh cutMesh = new Mesh();
        cutMesh.vertices = cutVertices;
        cutMesh.triangles = cutTriangles;
        cutMesh.RecalculateNormals();

        return cutMesh;
    }
}