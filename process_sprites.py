import os
from PIL import Image

def make_background_transparent(file_path):
    if not os.path.exists(file_path):
        print(f"Warning: File not found {file_path}")
        return

    print(f"Processing transparency for: {file_path}")
    img = Image.open(file_path).convert("RGBA")
    datas = img.getdata()

    new_data = []
    for item in datas:
        r, g, b, a = item
        brightness = max(r, g, b)
        
        # Soft thresholding for alpha transparency
        if r == 0 and g == 0 and b == 0:
            new_a = 0
        elif brightness < 12:
            new_a = 0
        elif brightness < 45:
            # Map brightness from range [12, 45] to alpha [0, 255] for smooth edges
            new_a = int((brightness - 12) * (255.0 / 33.0))
        else:
            new_a = 255
            
        new_data.append((r, g, b, new_a))

    img.putdata(new_data)
    img.save(file_path, "PNG")
    print(f"Successfully saved transparent version of {file_path}")

sprites = [
    "Assets/Sprites/letreros/controls/movement_icon.png",
    "Assets/Sprites/letreros/controls/jump_icon.png",
    "Assets/Sprites/letreros/controls/crouch_icon.png",
    "Assets/Sprites/letreros/controls/interact_icon.png",
    "Assets/Sprites/letreros/controls/inventory_icon.png",
    "Assets/Sprites/letreros/controls/run_icon.png",
    "Assets/Sprites/letreros/controls/mouse.png",
    "Assets/Sprites/letreros/warning.png",
    "Assets/Sprites/letreros/footer.png"
]

for sprite in sprites:
    make_background_transparent(sprite)
