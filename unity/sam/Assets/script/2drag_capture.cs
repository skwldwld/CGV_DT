using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DragCapture : MonoBehaviour
{
    public Camera cam;
    public string serverUrl = "http://localhost:5000/predict";

    private Vector3 dragStartPosition;
    private Vector3 dragEndPosition;
    private bool isDragging = false;

    void Update()
    {
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

    void OnGUI()
    {
        if (isDragging)
        {
            Rect selectionRect = GetScreenRect(dragStartPosition, Input.mousePosition);
            GUI.Box(selectionRect, "");
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

        // 캡처 영역을 텍스처로 변환
        Texture2D screenShot = new Texture2D((int)selectionRect.width, (int)selectionRect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(selectionRect, 0, 0);
        screenShot.Apply();

        // 텍스처를 PNG 바이트 배열로 변환
        byte[] bytes = screenShot.EncodeToPNG();
        Destroy(screenShot);

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

    void ProcessResponse(string jsonResponse)
    {
        Debug.Log("서버로부터의 응답: " + jsonResponse);
        // JSON 응답 파싱 및 결과 처리
        // 필요한 경우 추가 처리 수행
    }
}
