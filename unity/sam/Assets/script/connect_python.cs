using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class connect_python : MonoBehaviour
{
    public Camera cam;
    public string serverUrl = "http://localhost:5000/predict";

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 마우스 우클릭 감지
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // 물체 이미지 캡처 및 전송
                StartCoroutine(CaptureAndSendImage(hit.collider));
            }
        }
    }

    IEnumerator CaptureAndSendImage(Collider collider)
    {
        yield return new WaitForEndOfFrame();

        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cam.targetTexture = renderTexture;
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        byte[] bytes = screenShot.EncodeToPNG();
        Destroy(screenShot);

        WWWForm form = new WWWForm();
        form.AddBinaryData("image", bytes, "screenshot.png", "image/png");

        UnityWebRequest www = UnityWebRequest.Post(serverUrl, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string jsonResponse = www.downloadHandler.text;
            ProcessResponse(jsonResponse);
        }
    }

    void ProcessResponse(string jsonResponse)
    {
        Debug.Log("Response from server: " + jsonResponse);
        // JSON 파싱 및 객체 정보 처리
        // 필요한 경우 여기에 추가 작업을 수행
    }
}