import os
import numpy as np
import cv2
import matplotlib.pyplot as plt
from segment_anything import SamPredictor, sam_model_registry

def load_image(image_path):
    if not os.path.exists(image_path):
        raise FileNotFoundError(f"Image file not found: {image_path}")
    image = cv2.imread(image_path)
    if image is None:
        raise ValueError(f"Failed to load image from path: {image_path}")
    return cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

def save_mask(mask, path):
    cv2.imwrite(path, (mask * 255).astype(np.uint8))

# Load the model
checkpoint_path = "C:/Users/user/Desktop/capstone24_1/sam/python/checkpoint/sam_vit_h_4b8939.pth"
sam = sam_model_registry["vit_h"](checkpoint=checkpoint_path)
predictor = SamPredictor(sam)

# Load the image
image_path = "C:/Users/user/Desktop/capstone24_1/sam/python/image/IMG_4279.JPG"
print(f"Loading image from path: {image_path}")
image = load_image(image_path)
predictor.set_image(image)

# Perform segmentation with points prompt
input_points = np.array([[150, 200], [300, 400], [400, 300]])  # Example points
input_labels = np.array([1, 0, 1])  # 1: positive point, 0: negative point

# Predict
masks, _, _ = predictor.predict(point_coords=input_points, point_labels=input_labels)

# Visualize and save the masks
for i, mask in enumerate(masks):
    mask_path = f"C:/Users/user/Desktop/capstone24_1/sam/python/results/mask_{i}.png"
    save_mask(mask, mask_path)

    # Visualize the mask
    plt.figure(figsize=(8, 8))
    plt.imshow(image)
    plt.imshow(mask, cmap='viridis', alpha=0.5)
    plt.title(f"Mask {i}")
    plt.axis('off')
    plt.savefig(mask_path.replace('.png', '.jpg'))
    plt.close()
