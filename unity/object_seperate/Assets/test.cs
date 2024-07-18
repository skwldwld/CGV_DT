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
            Debug.LogError("MeshFilter 또는 메시가 없습니다.");
            return;
        }

        Mesh originalMesh = meshFilter.sharedMesh;

        // 반원 모양의 메시 생성
        Mesh semicircleMesh = CreateSemicircleMesh();

        // 반원 모양을 기준으로 기존 메시를 자르기
        Mesh cutMesh = CutMeshBySemicircle(originalMesh, semicircleMesh);

        // 새로운 메시 적용
        meshFilter.mesh = cutMesh;
    }

    // 반원 모양의 메시 생성 함수
    Mesh CreateSemicircleMesh()
    {
        int segments = 32; // 반원의 세그먼트 수
        float radius = 5f; // 반원의 반지름
        float angleStep = Mathf.PI / segments; // 각도 간격

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 2]; // 중심점 + 반원 정점
        int[] triangles = new int[segments * 3]; // 삼각형 인덱스 배열

        // 중심점 추가
        vertices[0] = Vector3.zero;

        // 반원의 정점 위치 계산
        for (int i = 1; i <= segments + 1; i++)
        {
            float angle = angleStep * (i - 1);
            vertices[i] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        // 삼각형 배열 생성
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0; // 중심점
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i < segments) ? i + 2 : 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    // 반원 모양으로 기존 메시를 자르는 함수
    Mesh CutMeshBySemicircle(Mesh originalMesh, Mesh semicircleMesh)
    {
        Vector3[] originalVertices = originalMesh.vertices;
        int[] originalTriangles = originalMesh.triangles;
        Vector3[] semicircleVertices = semicircleMesh.vertices;
        int[] semicircleTriangles = semicircleMesh.triangles;

        Vector3 semicircleNormal = Vector3.up; // 반원 메시의 평면 normal
        Vector3 semicircleCenter = Vector3.zero; // 반원 메시의 중심

        // 반원의 경계선 위에 있는 원래 메시의 정점을 기록할 리스트
        var cutVerticesList = new System.Collections.Generic.List<Vector3>();
        var cutTrianglesList = new System.Collections.Generic.List<int>();

        // 원래 메시의 각 정점을 반원 평면과의 관계로 검사하여 자르기
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertexPosition = originalVertices[i];

            // 반원 평면과의 관계 검사
            bool isInsideSemicircle = Vector3.Dot(vertexPosition - semicircleCenter, semicircleNormal) >= 0f;

            if (isInsideSemicircle)
            {
                // 반원 내부에 있는 원래 메시의 정점 추가
                cutVerticesList.Add(vertexPosition);
            }
        }

        // 원래 메시의 정점과 삼각형 배열을 다시 구성
        Vector3[] cutVertices = cutVerticesList.ToArray();
        int[] cutTriangles = new int[originalTriangles.Length];

        // 원래 메시의 삼각형 배열을 반원 메시와 매핑하여 자르기
        int cutIndex = 0;
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int index1 = originalTriangles[i];
            int index2 = originalTriangles[i + 1];
            int index3 = originalTriangles[i + 2];

            bool inside1 = cutVerticesList.Contains(originalVertices[index1]);
            bool inside2 = cutVerticesList.Contains(originalVertices[index2]);
            bool inside3 = cutVerticesList.Contains(originalVertices[index3]);

            // 반원 내부에 있는 삼각형만 유지
            if (inside1 && inside2 && inside3)
            {
                cutTriangles[cutIndex++] = ArrayUtility.IndexOf(cutVertices, originalVertices[index1]);
                cutTriangles[cutIndex++] = ArrayUtility.IndexOf(cutVertices, originalVertices[index2]);
                cutTriangles[cutIndex++] = ArrayUtility.IndexOf(cutVertices, originalVertices[index3]);
            }
        }

        // 최종적으로 자른 메시 생성
        Mesh cutMesh = new Mesh();
        cutMesh.vertices = cutVertices;
        cutMesh.triangles = cutTriangles;
        cutMesh.RecalculateNormals();

        return cutMesh;
    }
}