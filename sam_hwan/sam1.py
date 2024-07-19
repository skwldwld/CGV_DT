#import torch
#import torchvision

#print("PyTroch version: ", torch.__version__)
#print("Torchvision version: ", torchvision.__version__)
#print("CUDA is available: ", torch.cuda.is_available())

#  import sys

import numpy as np
import torch
import matplotlib.pyplot as plt
import cv2
import io
from PIL import Image
#from google.colab import files

def show_anns(anns):    # 입력 받은 anns 리스트를 이용하여 어노테이션을 표시하는 기능
    if len(anns) == 0:  # anns 리스트가 비었을 경우 return
        return
    sorted_anns = sorted(anns, key=(lambda x: x['area']), reverse=True) # anns 리스트를 areaa 값을 기준으로 내림차순 -> 가장 큰 영역의 어노테이션부터 표시
    ax = plt.gca()     # 현재 활성화된 Matplotlib 축(axis)를 가져옴
    ax.set_autoscale_on(False)  # 축의 자동 스케일링 비활성화 -> 어노테이션의 크기와 위치 변경 안되도록
    polygons = []
    color = []
    for ann in sorted_anns: # 정렬된 anns 리스트
        m = ann['segmentation']     # 현재 어노테이션의 segmentation mask를 가져옴
        img = np.ones((m.shape[0], m.shape[1], 3))  # segmentation mask와 같은 크기의 RGB 이미지를 생성 -> 이미지에 랜덤한 색 적용
        color_mask = np.random.random((1, 3)).tolist()[0]   # 랜덤 색
        for i in range(3):  # 랜덤 색상을 이미지의 각 채널에 적용 -> 여기서 채널이 뭐지? -> RGB를 말하는듯?
                img[:,:,i] = color_mask[i]
        ax.imshow(np.dstack((img, m*0.35))) # 이미지와 segmentation mask를 결합
        
image = cv2.imread(r"C:\Users\yejim\Desktop\cgv\github\CGV_DT\sam_hwan\img\test.png")      # 원하는 이미지 읽어오기

image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB) # OpenCV에서 읽은 이미지는 BGR 채널 순서 -> RGB 채널 순서로 변경
image_rgb_resized = cv2.resize(image, (800, 600))

import sys

sys.path.append("..")   # 상위 디렉토리를 Python 경로에 추가 -> Segment Anything Model 관련 모듈을 가져오기 위해 (아래에 있음)
from segment_anything import sam_model_registry, SamAutomaticMaskGenerator, SamPredictor

sam_checkpoint = r"C:\Users\yejim\Desktop\cgv\sam\python\checkpoint\sam_vit_h_4b8939.pth" # 모델 체크포인트 파일
model_type = "vit_h"    # 사용할 SAM 모델

device = "cuda" # GPU를 사용하도록 설정

sam = sam_model_registry[model_type](checkpoint=sam_checkpoint) # 지정된 몸델 유형과 체크포인트 파일을 사용해 SAM 모델 생성
sam.to(device=device)   # 모델을 GPU로 이동시킴

mask_generator = SamAutomaticMaskGenerator(sam) # SAM 모델을 이용하여 자동으로 객체 마스크를 생성 -> 마스크 생성기를 생성

masks = mask_generator.generate(image)  # 입력 이미지에 객체 마스크 생성

print(len(masks))   # 마스크 개수
print(masks[0].keys())  # 마스크의 키 값들

plt.figure(figsize=(10, 10))    # matplotlib figure 크기
plt.imshow(image)   # 원본 이미지
show_anns(masks)    # show_anns 함수를 이용하여 생성된 마스크를 이미지에 표시
plt.axis('off')     # 이미지에 축을 표시하지 않음
plt.show()          # 이미지 화면에 표시