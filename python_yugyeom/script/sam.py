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
    cv2.imwrite(path, mask.astype(np.uint8) * 255)

# Load the model
checkpoint_path = "C:/Users/max47/Desktop/Coding/python/checkpoint/sam_vit_h_4b8939.pth"
sam = sam_model_registry["vit_h"](checkpoint=checkpoint_path)
predictor = SamPredictor(sam)

# Load the image
image_path = "C:/Users/max47/Desktop/Coding/python/image/test1.png"
print(f"Loading image from path: {image_path}")
image = load_image(image_path)
predictor.set_image(image)

# Perform segmentation with points prompt
input_points = np.array([[150, 200], [300, 400]])  # Example points
input_labels = np.array([1, 0])  # 1: positive point, 0: negative point

# Predict
masks, _, _ = predictor.predict(point_coords=input_points, point_labels=input_labels)

# Save the last mask
last_mask = masks[-1]  # Selecting the last generated mask
last_mask_path = "C:/Users/max47/Desktop/Coding/python/last_mask.png"
save_mask(last_mask, last_mask_path)

# Visualize the last mask
plt.imshow(last_mask)
plt.title("Last Mask")
plt.show()
