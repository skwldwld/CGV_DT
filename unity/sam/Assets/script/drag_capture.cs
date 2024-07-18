using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class drag_capture : MonoBehaviour
{
    public Camera cam;
    public string serverUrl = "http://localhost:5000/predict";

    private Vector3 dragStartPosition;
    private Vector3 dragEndPosition;
    private bool isDragging = false;
    private bool isSaving = false;
    private Texture2D savedTexture; // ����� �̹����� �ؽ�ó

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // ���콺 ���� Ŭ�� ����
        {
            if (!isDragging && !isSaving)
            {
                StartCoroutine(SaveImageAndCapture());
                Debug.Log("Saving and capturing started");
            }
        }

        if (Input.GetMouseButtonDown(1)) // ���콺 ��Ŭ�� ����
        {
            dragStartPosition = Input.mousePosition;
            isDragging = true;
            Debug.Log("Clicked");
        }

        if (Input.GetMouseButtonUp(1)) // ���콺 ��Ŭ�� ���� ����
        {
            dragEndPosition = Input.mousePosition;
            isDragging = false;
            StartCoroutine(CaptureAndSendImage(dragStartPosition, dragEndPosition));
            Debug.Log("Non Clicked");
        }
    }

    IEnumerator SaveImageAndCapture()
    {
        isSaving = true;
        yield return new WaitForEndOfFrame();

        // ���� ȭ���� ĸó�Ͽ� ����
        savedTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        savedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        savedTexture.Apply();

        isSaving = false;
    }

    void OnGUI()
    {
        if (isDragging && savedTexture != null)
        {
            Rect selectionRect = GetScreenRect(dragStartPosition, Input.mousePosition);
            GUI.DrawTexture(selectionRect, savedTexture);
        }
    }

    Rect GetScreenRect(Vector3 start, Vector3 end)
    {
        float left = Mathf.Min(start.x, end.x);
        float top = Mathf.Min(Screen.height - start.y, Screen.height - end.y);
        float width = Mathf.Abs(end.x - start.x);
        float height = Mathf.Abs(end.y - start.y);

        return new Rect(left, top, width, height);
    }

    IEnumerator CaptureAndSendImage(Vector3 start, Vector3 end)
    {
        yield return new WaitForEndOfFrame();

        Rect selectionRect = GetScreenRect(start, end);

        // ����� �̹������� ���� ������ �߶󳻾� ĸó
        if (savedTexture != null)
        {
            Texture2D capturedTexture = new Texture2D((int)selectionRect.width, (int)selectionRect.height, TextureFormat.RGB24, false);
            Color[] pixels = savedTexture.GetPixels((int)selectionRect.x, (int)selectionRect.y, (int)selectionRect.width, (int)selectionRect.height);
            capturedTexture.SetPixels(pixels);
            capturedTexture.Apply();

            // �ؽ�ó�� PNG ����Ʈ �迭�� ��ȯ
            byte[] bytes = capturedTexture.EncodeToPNG();
            Destroy(capturedTexture);

            // WWWForm ���� �� �̹��� ������ �߰�
            WWWForm form = new WWWForm();
            form.AddBinaryData("image", bytes, "screenshot.png", "image/png");

            // ������ ��û ������
            UnityWebRequest www = UnityWebRequest.Post(serverUrl, form);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("��ũ���� ���� �� ���� �߻�: " + www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                ProcessResponse(jsonResponse);
            }
        }
    }

    void ProcessResponse(string jsonResponse)
    {
        Debug.Log("�����κ����� ����: " + jsonResponse);
        // JSON ���� �Ľ� �� ��� ó��
        // �ʿ��� ��� �߰� ó�� ����
    }
}