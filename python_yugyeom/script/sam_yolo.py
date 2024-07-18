import os
import numpy as np
import cv2
import matplotlib.pyplot as plt
import trimesh
import torch
from segment_anything import SamPredictor, sam_model_registry
from yolov5 import YOLOv5

def load_image(image_path):
    if not os.path.exists(image_path):
        raise FileNotFoundError(f"Image file not found: {image_path}")
    image = cv2.imread(image_path)
    if image is None:
        raise ValueError(f"Failed to load image from path: {image_path}")
    return cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

def save_mask(mask, path):
    cv2.imwrite(path, mask.astype(np.uint8) * 255)

# Load the SAM model
checkpoint_path = "C:/Users/max47/Desktop/Coding/python/checkpoint/sam_vit_h_4b8939.pth"
sam = sam_model_registry["vit_h"](checkpoint=checkpoint_path)
predictor = SamPredictor(sam)

# Load the YOLOv5 model
yolo_model_path = "yolov5s.pt"  # Pretrained YOLOv5 model path
yolo = YOLOv5(yolo_model_path, device='cpu')

# Load the image
image_path = "C:/Users/max47/Desktop/Coding/python/image/test1.png"
print(f"Loading image from path: {image_path}")
image = load_image(image_path)
predictor.set_image(image)

# Perform object detection with YOLOv5
results = yolo.predict(image)

# Check if any objects were detected
if len(results.pred[0]) == 0:
    raise ValueError("No objects detected in the image.")

# Extract the object with the highest confidence score
max_conf = -1
best_det = None

for det in results.pred[0]:
    x1, y1, x2, y2, conf, cls = det
    if conf > max_conf:
        max_conf = conf
        best_det = det

# Use the highest confidence detection for SAM input
if best_det is not None:
    x1, y1, x2, y2, conf, cls = best_det
    cx, cy = int((x1 + x2) / 2), int((y1 + y2) / 2)
    input_points = np.array([[cx, cy]])
    input_labels = np.array([1])  # Positive point

    # Predict segmentation masks
    masks, _, _ = predictor.predict(point_coords=input_points, point_labels=input_labels)

    # Save masks and generate meshes
    for i, mask in enumerate(masks):
        mask_path = f"mask_{i}.png"
        save_mask(mask, mask_path)
        
        # Generate 3D mesh
        mask_height, mask_width = mask.shape
        vertices = []
        vertex_indices = {}
        faces = []

        for y in range(mask_height):
            for x in range(mask_width):
                if mask[y, x]:
                    z = mask[y, x] * 10
                    vertex_indices[(x, y)] = len(vertices)
                    vertices.append([x, y, z])
        
        for y in range(mask_height - 1):
            for x in range(mask_width - 1):
                if mask[y, x] and mask[y + 1, x] and mask[y, x + 1] and mask[y + 1, x + 1]:
                    v0 = vertex_indices[(x, y)]
                    v1 = vertex_indices[(x + 1, y)]
                    v2 = vertex_indices[(x, y + 1)]
                    v3 = vertex_indices[(x + 1, y + 1)]
                    faces.append([v0, v1, v2])
                    faces.append([v1, v3, v2])

        mesh = trimesh.Trimesh(vertices=vertices, faces=faces)
        mesh.export(f"mesh_{i}.obj")

    # Visualize saved masks (optional)
    for i, mask in enumerate(masks):
        plt.subplot(1, len(masks), i+1)
        plt.imshow(mask)
        plt.title(f"Mask {i}")
    plt.show()
else:
    print("No high-confidence object detected.")
