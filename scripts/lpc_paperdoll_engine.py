import os
import glob
from PIL import Image

# LPC Standard 64x64 Grid Sheet Format
# Row 0 (Y: 512px): North (9 frames)
# Row 1 (Y: 576px): West (9 frames)
# Row 2 (Y: 640px): South (9 frames)
# Row 3 (Y: 704px): East (9 frames)

FRAME_SIZE = 64
WALK_ROW_Y = {
    "north": 512,
    "west": 576,
    "south": 640,
    "east": 704
}

# Diagonal mappings to cardinal nearest
DIR_MAPPING = {
    "south": "south",
    "south-east": "east",
    "east": "east",
    "north-east": "north",
    "north": "north",
    "north-west": "west",
    "west": "west",
    "south-west": "south"
}

def extract_lpc_walk_frames(sheet_path, output_dir, item_name):
    if not os.path.exists(sheet_path):
        print(f"Sheet path not found: {sheet_path}")
        return False

    img = Image.open(sheet_path).convert("RGBA")
    os.makedirs(output_dir, exist_ok=True)

    # Extract 4 cardinal walk directions (north, west, south, east)
    for dir_name, start_y in WALK_ROW_Y.items():
        if start_y + FRAME_SIZE > img.height:
            # Fallback if image has different Y offsets
            start_y = 0

        # Frame 0 is resting idle, Frame 1..8 are walk steps
        crop_box = (0, start_y, FRAME_SIZE, start_y + FRAME_SIZE)
        frame_img = img.crop(crop_box)
        frame_path = os.path.join(output_dir, f"{dir_name}.png")
        frame_img.save(frame_path, "PNG")
        print(f"Extracted LPC layer '{item_name}' ({dir_name}): {frame_path}")

    # Create diagonal direction aliases (south-east, north-east, north-west, south-west)
    for diag_dir, cardinal_target in DIR_MAPPING.items():
        if diag_dir in WALK_ROW_Y:
            continue
        src_path = os.path.join(output_dir, f"{cardinal_target}.png")
        dest_path = os.path.join(output_dir, f"{diag_dir}.png")
        if os.path.exists(src_path) and not os.path.exists(dest_path):
            img_cardinal = Image.open(src_path)
            img_cardinal.save(dest_path, "PNG")

    return True

def main():
    repo_base = "tools/lpc_generator/spritesheets"
    output_base = "src/MMORPG.GodotClient/Assets/Textures/Paperdoll/LPC"

    items_to_process = {
        "Head/IronHelm": glob.glob(os.path.join(repo_base, "head/helmets/**/walk/*.png"), recursive=True) + glob.glob(os.path.join(repo_base, "head/helmets/**/*.png"), recursive=True),
        "Armor/IronPlateChest": glob.glob(os.path.join(repo_base, "torso/armors/**/walk/*.png"), recursive=True) + glob.glob(os.path.join(repo_base, "torso/armors/**/*.png"), recursive=True) + glob.glob(os.path.join(repo_base, "torso/**/plate*.png"), recursive=True),
        "Legs/IronLeggings": glob.glob(os.path.join(repo_base, "legs/pants/**/walk/*.png"), recursive=True) + glob.glob(os.path.join(repo_base, "legs/**/*.png"), recursive=True),
        "Boots/IronBoots": glob.glob(os.path.join(repo_base, "feet/boots/**/walk/*.png"), recursive=True) + glob.glob(os.path.join(repo_base, "feet/**/*.png"), recursive=True),
        "Weapons/IronSword": glob.glob(os.path.join(repo_base, "weapon/sword/**/walk/*.png"), recursive=True) + glob.glob(os.path.join(repo_base, "weapon/sword/**/*.png"), recursive=True)
    }

    for item_key, matches in items_to_process.items():
        if matches:
            chosen_sheet = matches[0]
            out_dir = os.path.join(output_base, item_key)
            extract_lpc_walk_frames(chosen_sheet, out_dir, item_key)
        else:
            print(f"No LPC matching sheet found for {item_key}")

if __name__ == "__main__":
    main()
