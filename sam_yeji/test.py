import torch
from segment_anything import sam_model_registry
import cv2
from segment_anything import SamAutomaticMaskGenerator
import supervision as sv
import matplotlib.pyplot as plt

# 장치 설정
DEVICE = torch.device('cuda:0' if torch.cuda.is_available() else 'cpu')  # CUDA 사용 가능하면 GPU, 그렇지 않으면 CPU 사용
MODEL_TYPE = "vit_h"

# 모델 로드
sam = sam_model_registry[MODEL_TYPE](checkpoint=r"C:\Users\yejim\Desktop\cgv\sam\python\checkpoint\sam_vit_h_4b8939.pth")
sam.to(device=DEVICE)

# 마스크 생성기 초기화
mask_generator = SamAutomaticMaskGenerator(sam)

# 이미지 읽기 및 RGB 변환
image_bgr = cv2.imread(r"C:\Users\yejim\Desktop\cgv\github\CGV_DT\sam_yeji\img\testimg.jfif")  # 이미지 파일을 BGR 포맷으로 읽어옴
image_rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)  # BGR 이미지를 RGB 포맷으로 변환

# 입력 이미지 해상도 축소
image_rgb_resized = cv2.resize(image_rgb, (800, 600))  # 예시로 800x600으로 축소

# 마스크 생성
result = mask_generator.generate(image_rgb_resized)

# 마스크 주석기 초기화 및 주석 추가
mask_annotator = sv.MaskAnnotator()

# Detections 객체 생성
detections = sv.Detections.from_sam(result)
detections.class_id = list(range(len(detections)))  # 각 객체에 대해 고유한 class_id 할당

# 이미지에 주석 추가
annotated_image = mask_annotator.annotate(image_rgb_resized, detections)  # 주석 기능은 축소된 이미지에 적용

# 주석된 이미지 화면에 표시
annotated_image_rgb = cv2.cvtColor(annotated_image, cv2.COLOR_BGR2RGB)
plt.imshow(annotated_image_rgb)
plt.axis('off')
plt.show()

# 주석된 이미지 파일로 저장 - 파일 경로를 정확히 지정해야 합니다.
output_path = r"C:\Users\yejim\Desktop\cgv\github\CGV_DT\sam_yeji\new_img\test.jpg"
success = cv2.imwrite(output_path, annotated_image_rgb)

if success:
    print(f"주석된 이미지가 성공적으로 저장되었습니다: {output_path}")
else:
    print("이미지 저장에 실패했습니다.")
