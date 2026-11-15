"""Generates the effect textures of the water-and-fire look.

- FlameNoise.png: tileable smooth noise, 128x128. R, G and B are independent noise fields of rising detail;
  the fire shader scrolls them upwards to wobble the flame outline.
- SteamPuff.png: soft cloudy puff, 64x64, white with the shape in alpha; the steam and ember particles use it.
- WaterRipple.png: two thin rings, 128x128, white with the shape in alpha; ripples running over the ground.
- WaterPuddle.png: a lumpy puddle, 128x128, with a brighter rim; spreads under every fire the water reaches.

Run from anywhere: python generate_fx_textures.py  (needs numpy and Pillow). Output goes to the parent folder.
Unity ignores folders ending with "~", so this script is not imported as an asset.
"""

from pathlib import Path

import numpy as np
from PIL import Image

OUTPUT_DIR = Path(__file__).resolve().parent.parent
RNG = np.random.default_rng(20261115)

NOISE_SIZE = 128
PUFF_SIZE = 64
WATER_SIZE = 128


def tileable_noise(size, cutoff_cycles):
    """White noise low-passed in the frequency domain, so it wraps seamlessly on both axes."""
    white = RNG.standard_normal((size, size))
    frequencies = np.fft.fftfreq(size) * size
    fx, fy = np.meshgrid(frequencies, frequencies)
    radius = np.sqrt(fx * fx + fy * fy)
    gain = np.exp(-(radius / cutoff_cycles) ** 2)
    smooth = np.real(np.fft.ifft2(np.fft.fft2(white) * gain))
    return equalize(smooth)


def equalize(values):
    """Spreads the values evenly over 0..1, so every channel uses the whole range."""
    ranks = values.ravel().argsort().argsort()
    return (ranks / (ranks.size - 1)).reshape(values.shape)


def flame_noise():
    channels = [tileable_noise(NOISE_SIZE, cycles) for cycles in (3.0, 6.0, 12.0)]
    rgb = np.stack(channels, axis=-1)
    return Image.fromarray(np.uint8(np.round(rgb * 255)), mode="RGB")


def smoothstep(edge0, edge1, x):
    t = np.clip((x - edge0) / (edge1 - edge0), 0.0, 1.0)
    return t * t * (3.0 - 2.0 * t)


def distance_from_centre(size):
    """Distance of every pixel from the image centre, 1 at the middle of each edge."""
    coordinates = (np.arange(size) + 0.5) / size * 2.0 - 1.0
    x, y = np.meshgrid(coordinates, coordinates)
    return np.sqrt(x * x + y * y)


def ring(distance, radius, half_width):
    return 1.0 - smoothstep(0.0, half_width, np.abs(distance - radius))


def steam_puff():
    distance = distance_from_centre(PUFF_SIZE)

    # A few lumps on the edge make it read as a cloud rather than a disc.
    clouds = tileable_noise(PUFF_SIZE, 4.0)
    edge = distance + (clouds - 0.5) * 0.35
    alpha = 1.0 - smoothstep(0.35, 0.95, edge)
    alpha *= 1.0 - smoothstep(0.9, 1.0, distance)  # never touch the border

    rgba = np.ones((PUFF_SIZE, PUFF_SIZE, 4))
    rgba[..., 3] = alpha
    return Image.fromarray(np.uint8(np.round(rgba * 255)), mode="RGBA")


def water_ripple():
    distance = distance_from_centre(WATER_SIZE)
    alpha = np.maximum(ring(distance, 0.84, 0.09), 0.55 * ring(distance, 0.6, 0.06))

    rgba = np.ones((WATER_SIZE, WATER_SIZE, 4))
    rgba[..., 3] = alpha
    return Image.fromarray(np.uint8(np.round(rgba * 255)), mode="RGBA")


def water_puddle():
    distance = distance_from_centre(WATER_SIZE)

    # Lumpy outline; the water is a little brighter and denser along its edge.
    lumps = tileable_noise(WATER_SIZE, 4.0)
    edge = distance + (lumps - 0.5) * 0.12
    inside = 1.0 - smoothstep(0.74, 0.8, edge)
    rim = ring(edge, 0.72, 0.08)
    alpha = inside * (0.72 + 0.28 * rim)

    rgba = np.ones((WATER_SIZE, WATER_SIZE, 4))
    rgba[..., :3] = (0.8 + 0.2 * rim)[..., None]
    rgba[..., 3] = alpha
    return Image.fromarray(np.uint8(np.round(rgba * 255)), mode="RGBA")


def main():
    # The order matters: the shared RNG feeds the textures in turn, so new ones go last.
    flame_noise().save(OUTPUT_DIR / "FlameNoise.png")
    steam_puff().save(OUTPUT_DIR / "SteamPuff.png")
    water_ripple().save(OUTPUT_DIR / "WaterRipple.png")
    water_puddle().save(OUTPUT_DIR / "WaterPuddle.png")
    print(f"Written to {OUTPUT_DIR}")


if __name__ == "__main__":
    main()
