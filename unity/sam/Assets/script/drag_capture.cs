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
    private Texture2D savedTexture; // 저장된 이미지의 텍스처

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 클릭 감지
        {
            if (!isDragging && !isSaving)
            {
                StartCoroutine(SaveImageAndCapture());
                Debug.Log("Saving and capturing started");
            }
        }

        if (Input.GetMouseButtonDown(1)) // 마우스 우클릭 감지
        {
            dragStartPosition = Input.mousePosition;
            isDragging = true;
            Debug.Log("Clicked");
        }

        if (Input.GetMouseButtonUp(1)) // 마우스 우클릭 해제 감지
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

        // 현재 화면을 캡처하여 저장
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

        // 저장된 이미지에서 선택 영역을 잘라내어 캡처
        if (savedTexture != null)
        {
            Texture2D capturedTexture = new Texture2D((int)selectionRect.width, (int)selectionRect.height, TextureFormat.RGB24, false);
            Color[] pixels = savedTexture.GetPixels((int)selectionRect.x, (int)selectionRect.y, (int)selectionRect.width, (int)selectionRect.height);
            capturedTexture.SetPixels(pixels);
            capturedTexture.Apply();

            // 텍스처를 PNG 바이트 배열로 변환
            byte[] bytes = capturedTexture.EncodeToPNG();
            Destroy(capturedTexture);

            // WWWForm 생성 및 이미지 데이터 추가
            WWWForm form = new WWWForm();
            form.AddBinaryData("image", bytes, "screenshot.png", "image/png");

            // 서버로 요청 보내기
            UnityWebRequest www = UnityWebRequest.Post(serverUrl, form);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("스크린샷 전송 중 오류 발생: " + www.error);
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
        Debug.Log("서버로부터의 응답: " + jsonResponse);
        // JSON 응답 파싱 및 결과 처리
        // 필요한 경우 추가 처리 수행
    }
}