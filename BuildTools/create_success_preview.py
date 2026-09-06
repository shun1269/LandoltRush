from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parents[1]
source = root / "BuildTools/Verification~/Builds/UIPreviews/SuccessDive"
frames = [Image.open(p).convert("RGB").resize((960, 540), Image.Resampling.LANCZOS)
          for p in sorted(source.glob("frame-*.png"))]
assert len(frames) == 31, "Render the 31 Unity success frames first"
frames[0].save(root / "Documentation/Previews/05-success.gif", save_all=True,
               append_images=frames[1:], duration=[600] + [20] * 29 + [1000], loop=0)

source = root / "BuildTools/Verification~/Builds/UIPreviews"
combo = [Image.open(source / "02-playing.png").convert("RGB").resize((960, 540), Image.Resampling.LANCZOS)]
combo += [Image.open(p).convert("RGB").resize((960, 540), Image.Resampling.LANCZOS)
          for p in sorted((source / "ComboPulse").glob("frame-*.png"))]
assert len(combo) == 16, "Render the 15 Unity combo frames first"
combo[0].save(root / "Documentation/Previews/06-combo.gif", save_all=True,
              append_images=combo[1:], duration=[600] + [20] * 14 + [1000], loop=0)
