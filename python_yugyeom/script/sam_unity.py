import os
import cv2
import numpy as np
from flask import Flask, request, jsonify
from segment_anything import SamPredictor, sam_model_registry

app = Flask(__name__)

# SAM 모델 로드
checkpoint_path = "C:/Users/max47/Desktop/Coding/python/checkpoint/sam_vit_h_4b8939.pth"
sam = sam_model_registry["vit_h"](checkpoint=checkpoint_path)
predictor = SamPredictor(sam)

# 저장할 이미지 경로 설정
upload_folder = "uploads"
if not os.path.exists(upload_folder):
    os.makedirs(upload_folder)

def load_image(image_path):
    if not os.path.exists(image_path):
        raise FileNotFoundError(f"이미지 파일을 찾을 수 없습니다: {image_path}")
    image = cv2.imread(image_path)
    if image is None:
        raise ValueError(f"이미지를 로드할 수 없습니다: {image_path}")
    return cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

def save_mask(mask, path):
    cv2.imwrite(path, mask.astype(np.uint8) * 255)

@app.route('/predict', methods=['POST'])
def predict():
    # 이미지 받기
    file = request.files['image']
    img_array = np.frombuffer(file.read(), np.uint8)
    image = cv2.imdecode(img_array, cv2.IMREAD_COLOR)
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    # 전체 이미지에 대해 세분화 수행
    input_points = None  # 전체 이미지를 처리하므로 포인트는 필요하지 않습니다.
    input_labels = None

    # 예측 수행
    predictor.set_image(image_rgb)
    masks, _, _ = predictor.predict(point_coords=input_points, point_labels=input_labels)

    # 원본 이미지 저장
    original_image_path = os.path.join(upload_folder, "original_image.png")
    cv2.imwrite(original_image_path, cv2.cvtColor(image_rgb, cv2.COLOR_RGB2BGR))
    print(f"Original image saved to {original_image_path}")

    # 마지막 마스크 저장
    last_mask = masks[-1]  # 마지막 생성된 마스크 선택
    last_mask_path = os.path.join(upload_folder, "last_mask.png")
    save_mask(last_mask, last_mask_path)
    print(f"Last mask saved to {last_mask_path}")

    # 응답 준비
    response = {
        "message": "세분화 및 이미지 저장 완료",
        "original_image_path": original_image_path,
        "last_mask_path": last_mask_path,
        "masks_count": len(masks)
    }

    return jsonify(response)

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
