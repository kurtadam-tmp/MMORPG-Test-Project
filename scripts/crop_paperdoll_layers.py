import os
from PIL import Image

def process_image(file_path, slot_type):
    if not os.path.exists(file_path):
        return

    try:
        img = Image.open(file_path).convert("RGBA")
        width, height = img.size
        pixels = img.load()

        for y in range(height):
            norm_y = y / float(height)
            for x in range(width):
                r, g, b, a = pixels[x, y]
                if a == 0:
                    continue

                keep = False
                if slot_type == "Head":
                    # Keep head region (top 38%)
                    if norm_y <= 0.38:
                        keep = True
                elif slot_type == "Chest":
                    # Keep chest/torso region (28% to 62%)
                    if 0.28 <= norm_y <= 0.62:
                        keep = True
                elif slot_type == "Legs":
                    # Keep legs region (52% to 80%)
                    if 0.52 <= norm_y <= 0.80:
                        keep = True
                elif slot_type == "Boots":
                    # Keep feet/boots region (bottom 25%, y >= 75%)
                    if norm_y >= 0.75:
                        keep = True
                elif slot_type == "Weapons":
                    keep = True

                if not keep:
                    pixels[x, y] = (0, 0, 0, 0)

        img.save(file_path, "PNG")
        print(f"Processed isolated {slot_type} layer: {file_path}")
    except Exception as e:
        print(f"Error processing {file_path}: {e}")

def main():
    base_dir = "src/MMORPG.GodotClient/Assets/Textures/Paperdoll"
    slots = {
        "Head/IronHelm": "Head",
        "Armor/IronPlateChest": "Chest",
        "Legs/IronLeggings": "Legs",
        "Boots/IronBoots": "Boots"
    }

    dirs = ["south", "south-east", "east", "north-east", "north", "north-west", "west", "south-west"]

    for slot_folder, slot_type in slots.items():
        folder_path = os.path.join(base_dir, slot_folder)
        for d in dirs:
            file_path = os.path.join(folder_path, f"{d}.png")
            process_image(file_path, slot_type)

if __name__ == "__main__":
    main()
