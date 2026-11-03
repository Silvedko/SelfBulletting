"""Synthesizes the SelfBulletting sound effects: one mono 16-bit WAV per SoundId.

Light hyper-casual style built from sines, filtered noise and simple envelopes.
Run from anywhere: python generate_sounds.py  (needs numpy). Output goes to the parent folder.
Unity ignores folders ending with "~", so this script is not imported as an asset.
"""

import wave
from pathlib import Path

import numpy as np

SAMPLE_RATE = 44100
PEAK = 0.9
EDGE_FADE_SECONDS = 0.003
HARMONIC_LIMIT_HZ = 10000  # clips are imported at 22050 Hz, so partials above this would only alias
OUTPUT_DIR = Path(__file__).resolve().parent.parent
RNG = np.random.default_rng(20261103)


# ---------- building blocks ----------

def timeline(seconds):
    return np.arange(int(round(seconds * SAMPLE_RATE))) / SAMPLE_RATE


def sine(frequency, t, phase=0.0):
    return np.sin(2 * np.pi * frequency * t + phase)


def sweep(start_hz, end_hz, t, sweep_seconds):
    """Sine whose pitch glides exponentially from start_hz to end_hz, then holds."""
    progress = np.clip(t / sweep_seconds, 0.0, 1.0)
    frequency = start_hz * (end_hz / start_hz) ** progress
    return np.sin(2 * np.pi * np.cumsum(frequency) / SAMPLE_RATE)


def decay(t, attack_seconds, tau_seconds):
    attack = np.clip(t / max(attack_seconds, 1e-6), 0.0, 1.0)
    return attack * np.exp(-np.maximum(t - attack_seconds, 0.0) / tau_seconds)


def note_envelope(t, attack_seconds, hold_seconds, release_seconds):
    attack = np.clip(t / attack_seconds, 0.0, 1.0)
    release = np.clip(1.0 - (t - attack_seconds - hold_seconds) / release_seconds, 0.0, 1.0)
    return attack * release


def band_noise(sample_count, low_hz, high_hz, rng=RNG):
    """White noise band-limited in the frequency domain. Circular, so it also loops seamlessly."""
    spectrum = np.fft.rfft(rng.standard_normal(sample_count))
    frequencies = np.fft.rfftfreq(sample_count, 1.0 / SAMPLE_RATE)
    spectrum[(frequencies < low_hz) | (frequencies > high_hz)] = 0
    noise = np.fft.irfft(spectrum, sample_count)
    return noise / np.max(np.abs(noise))


def sweeping_bandpass(signal, start_hz, end_hz, resonance=0.6):
    """Chamberlin state-variable band-pass whose centre glides from start_hz to end_hz."""
    centres = np.geomspace(start_hz, end_hz, len(signal))
    low = band = 0.0
    output = np.empty_like(signal)
    for index, (sample, centre) in enumerate(zip(signal, centres)):
        f = 2 * np.sin(np.pi * centre / SAMPLE_RATE)
        low += f * band
        high = sample - low - resonance * band
        band += f * high
        output[index] = band
    return output / np.max(np.abs(output))


def bell(frequency, t, tau_seconds, brightness=1.0):
    """Bell tone. Brightness above 1 lifts the upper partials and adds a fourth one."""
    partials = ((1.0, 1.0), (2.0, 0.35 * brightness), (3.01, 0.12 * brightness), (4.02, 0.1 * (brightness - 1)))
    tone = sum(gain * sine(ratio * frequency, t) for ratio, gain in partials
               if gain > 0 and ratio * frequency < HARMONIC_LIMIT_HZ)
    return tone * decay(t, 0.002, tau_seconds)


def release_tail(buffer, release_seconds):
    """Fades the end of the buffer out to silence."""
    length_seconds = len(buffer) / SAMPLE_RATE
    t = np.arange(len(buffer)) / SAMPLE_RATE
    return buffer * np.clip((length_seconds - t) / release_seconds, 0.0, 1.0)


def place(buffer, sound, start_seconds):
    start = int(round(start_seconds * SAMPLE_RATE))
    end = min(len(buffer), start + len(sound))
    buffer[start:end] += sound[:end - start]


def saturate(signal, drive):
    return np.tanh(drive * signal) / np.tanh(drive)


# ---------- sounds ----------

def ui_click():
    t = timeline(0.08)
    tone = sweep(1500, 900, t, 0.03) + 0.2 * sweep(3000, 1800, t, 0.03)
    click = band_noise(len(t), 2000, 8000) * decay(t, 0.0005, 0.003)
    return tone * decay(t, 0.001, 0.018) + 0.25 * click


def aim_charge():
    """Two-second seamless loop: every frequency makes a whole number of cycles in the loop."""
    t = timeline(2.0)
    tremolo = 0.75 + 0.25 * sine(6, t)
    shimmer = 0.5 + 0.5 * sine(1.5, t)
    hum = (0.5 * sine(330, t) + 0.35 * sine(330.5, t) + 0.25 * sine(495, t)
           + 0.12 * sine(660, t) + 0.06 * sine(990, t) * shimmer)
    fizz = 0.08 * band_noise(len(t), 1500, 5000) * shimmer
    return (hum + fizz) * tremolo


def shot_fire():
    t = timeline(0.28)
    zap = sweep(1100, 220, t, 0.2) * decay(t, 0.002, 0.07)
    hiss = band_noise(len(t), 1000, 6000) * decay(t, 0.001, 0.03)
    thump = sweep(120, 60, t, 0.1) * decay(t, 0.002, 0.05)
    return saturate(zap + 0.35 * hiss + 0.5 * thump, 1.5)


def infect():
    t = timeline(0.09)
    tone = sweep(700, 1300, t, 0.06) + 0.15 * sweep(1400, 2600, t, 0.06)
    return tone * decay(t, 0.002, 0.025)


def explode():
    t = timeline(0.3)
    thump = sweep(220, 55, t, 0.12) * decay(t, 0.001, 0.06)
    puff = band_noise(len(t), 80, 2500) * decay(t, 0.001, 0.07)
    pop = sine(2000, t) * decay(t, 0.0005, 0.005)
    return saturate(0.9 * thump + 0.6 * puff + 0.3 * pop, 1.5)


COMBO_RELEASE_SECONDS = 0.15
COMBO_HUGE_RELEASE_SECONDS = 0.2
COMBO_HUGE_SEED = 3
COMBO_HUGE_DRIVE = 2.0
SPARKLE_SPACING_SECONDS = 0.05
SPARKLE_DELAY_SECONDS = 0.03
CHORUS_DETUNE_CENTS = 6


def place_arpeggio(buffer, notes, note_spacing, tau_seconds, brightness, gain=1.0):
    """Rising bell arpeggio from the start of the buffer. Returns the time the last note starts after it."""
    note_time = timeline(0.6)
    for index, frequency in enumerate(notes):
        place(buffer, gain * bell(frequency, note_time, tau_seconds, brightness), index * note_spacing)
    return len(notes) * note_spacing


def place_sparkles(buffer, notes, start_seconds, count, spacing_seconds=SPARKLE_SPACING_SECONDS):
    """Pings one octave above the arpeggio, cascading down from its top note."""
    ping_time = timeline(0.2)
    for index in range(count):
        frequency = 2 * notes[-1 - index % len(notes)]
        ping = sine(frequency, ping_time) * decay(ping_time, 0.001, 0.06)
        place(buffer, 0.25 * 0.85 ** index * ping, start_seconds + index * spacing_seconds)


def chorus_sine(frequency, t):
    """Three slightly detuned copies of a sine: a fuller tone that slowly shimmers, even in mono."""
    ratio = 2 ** (CHORUS_DETUNE_CENTS / 1200)
    return (sine(frequency, t) + sine(frequency * ratio, t) + sine(frequency / ratio, t)) / 3


def combo_tier(notes, note_spacing, tau_seconds, brightness, sparkle_count, length_seconds):
    """One combo sound: a rising bell arpeggio and a cascade of sparkles.
    The tiers share this shape and grow higher, brighter and longer."""
    buffer = np.zeros(int(length_seconds * SAMPLE_RATE))
    arpeggio_end = place_arpeggio(buffer, notes, note_spacing, tau_seconds, brightness)
    place_sparkles(buffer, notes, arpeggio_end + SPARKLE_DELAY_SECONDS, sparkle_count)
    return release_tail(buffer, COMBO_RELEASE_SECONDS)


def combo_pad(root_hz, length_seconds):
    """Warm body under the whole sound: root an octave down, root, fifth and octave, swelling into the chord."""
    t = timeline(length_seconds)
    tone = (0.5 * chorus_sine(root_hz / 2, t) + 0.6 * chorus_sine(root_hz, t)
            + 0.4 * chorus_sine(root_hz * 1.5, t) + 0.25 * chorus_sine(root_hz * 2, t))
    swell = np.clip(t / 0.25, 0.0, 1.0) * np.exp(-np.maximum(t - 0.25, 0.0) / 0.45)
    return 0.35 * tone * swell


def combo_chord(notes):
    """Chorused chord of the arpeggio, doubled an octave up, under a vibrato top note: the long shimmering finish."""
    t = timeline(1.1)
    vibrato = 1 + 0.004 * sine(6, t)
    top = np.sin(2 * np.pi * np.cumsum(notes[3] * vibrato) / SAMPLE_RATE)
    chord = sum(chorus_sine(frequency, t) + 0.3 * chorus_sine(2 * frequency, t) for frequency in notes[:3]) / 3
    return 0.8 * (top + 0.8 * chord) * decay(t, 0.005, 0.4)


def combo_impact(rng):
    """A short clap and thump that land together with the chord."""
    t = timeline(0.2)
    clap = band_noise(len(t), 800, 6000, rng) * decay(t, 0.001, 0.03)
    thump = sweep(200, 80, t, 0.08) * decay(t, 0.002, 0.05)
    return 0.35 * clap + 0.4 * thump


def combo_shimmer(rng):
    """Airy high noise ringing with the chord."""
    t = timeline(1.0)
    return 0.1 * band_noise(len(t), 6000, 9500, rng) * decay(t, 0.02, 0.3)


def combo():
    """x1.5: C major, three notes, plain bell."""
    return combo_tier((1046.5, 1318.5, 1568.0), 0.06, 0.16, 1.0, 0, 0.6)


def combo_big():
    """x2: a step higher (D major), four notes, brighter, a short sparkle tail."""
    return combo_tier((1174.7, 1480.0, 1760.0, 2349.3), 0.055, 0.2, 1.4, 3, 0.85)


def combo_huge():
    """x3 and above: E major, the brightest and fullest tier. A five-note arpeggio doubled an octave up,
    a warm pad underneath, a clap with a chorused chord, airy shimmer and a long sparkle cascade."""
    notes = (1318.5, 1661.2, 1975.5, 2637.0, 3322.4)
    length_seconds = 1.4
    rng = np.random.default_rng(COMBO_HUGE_SEED)
    buffer = np.zeros(int(length_seconds * SAMPLE_RATE))

    place(buffer, combo_pad(notes[0] / 2, length_seconds), 0.0)
    arpeggio_end = place_arpeggio(buffer, notes, 0.05, 0.24, 2.2)
    place_arpeggio(buffer, [2 * frequency for frequency in notes], 0.05, 0.12, 1.0, gain=0.3)
    place(buffer, combo_impact(rng), arpeggio_end)
    place(buffer, combo_chord(notes), arpeggio_end)
    place(buffer, combo_shimmer(rng), arpeggio_end)
    place_sparkles(buffer, notes, arpeggio_end + SPARKLE_DELAY_SECONDS, 9, spacing_seconds=0.045)
    # Soft saturation tames the peak of the clap, so the whole sound comes out denser and louder.
    return release_tail(saturate(buffer / np.max(np.abs(buffer)), COMBO_HUGE_DRIVE), COMBO_HUGE_RELEASE_SECONDS)


def warning():
    buffer = np.zeros(int(0.32 * SAMPLE_RATE))
    t = timeline(0.11)
    beep = (sine(988, t) + sine(2964, t) / 9 + sine(4940, t) / 25) * note_envelope(t, 0.005, 0.065, 0.02)
    place(buffer, beep, 0.0)
    place(buffer, beep, 0.15)
    return buffer


def last_shot():
    buffer = np.zeros(int(0.75 * SAMPLE_RATE))
    t = timeline(0.55)
    for start, frequency, gain in ((0.0, 587.3, 0.8), (0.18, 440.0, 1.0)):
        marimba = sine(frequency, t) * decay(t, 0.002, 0.25) + 0.25 * sine(4 * frequency, t) * decay(t, 0.001, 0.05)
        place(buffer, gain * marimba, start)
    return buffer


def door_open():
    t = timeline(0.8)
    swell = np.clip(t / 0.3, 0.0, 1.0) * np.exp(-np.maximum(t - 0.3, 0.0) / 0.15)
    whoosh = sweeping_bandpass(RNG.standard_normal(len(t)), 400, 3000) * swell
    glide = sweep(300, 900, t, 0.4) * swell
    sparkles = np.zeros(len(t))
    ping_time = timeline(0.15)
    for start in np.linspace(0.3, 0.6, 6):
        frequency = RNG.uniform(2000, 4000)
        place(sparkles, sine(frequency, ping_time) * decay(ping_time, 0.001, 0.05), start)
    return 0.5 * whoosh + 0.3 * glide + 0.2 * sparkles


def hop():
    t = timeline(0.14)
    boing = sweep(260, 520, t, 0.08) * decay(t, 0.003, 0.045)
    thump = sine(150, t) * decay(t, 0.001, 0.02)
    return boing + 0.4 * thump


def win():
    buffer = np.zeros(int(1.6 * SAMPLE_RATE))
    short_note = timeline(0.5)
    for index, frequency in enumerate((523.25, 659.25, 784.0)):
        place(buffer, bell(frequency, short_note, 0.12), index * 0.1)

    final = timeline(1.25)
    vibrato = 1 + 0.004 * sine(5.5, final)
    top = np.sin(2 * np.pi * np.cumsum(1046.5 * vibrato) / SAMPLE_RATE)
    chord = sum(sine(frequency, final) for frequency in (523.25, 659.25, 784.0)) / 3
    place(buffer, (top + 0.6 * chord) * decay(final, 0.004, 0.45), 0.32)

    ping_time = timeline(0.2)
    for start, frequency in ((0.45, 3136.0), (0.55, 2637.0), (0.65, 4186.0)):
        place(buffer, 0.15 * sine(frequency, ping_time) * decay(ping_time, 0.001, 0.06), start)
    return buffer


def lose():
    buffer = np.zeros(int(1.5 * SAMPLE_RATE))
    notes = ((0.0, 466.2, 0.26), (0.28, 440.0, 0.26), (0.56, 415.3, 0.26), (0.84, 392.0, 0.5))
    for start, frequency, length in notes:
        t = timeline(length + 0.08)
        vibrato_depth = 0.015 if length > 0.3 else 0.0
        vibrato = 1 + vibrato_depth * sine(6, t) * np.clip(t / 0.15, 0, 1)
        phase = 2 * np.pi * np.cumsum(frequency * vibrato) / SAMPLE_RATE
        brightness = np.clip(t / 0.08, 0.0, 1.0)
        brass = sum((brightness ** (harmonic - 1)) * np.sin(harmonic * phase) / harmonic for harmonic in range(1, 7))
        place(buffer, brass * note_envelope(t, 0.03, length - 0.03, 0.08), start)
    return buffer


SOUNDS = {
    "UiClick": (ui_click, False),
    "AimCharge": (aim_charge, True),
    "ShotFire": (shot_fire, False),
    "Infect": (infect, False),
    "Explode": (explode, False),
    "Combo": (combo, False),
    "Warning": (warning, False),
    "LastShot": (last_shot, False),
    "DoorOpen": (door_open, False),
    "Hop": (hop, False),
    "Win": (win, False),
    "Lose": (lose, False),
    "ComboBig": (combo_big, False),
    "ComboHuge": (combo_huge, False),
}


# ---------- output ----------

def finish(signal, is_loop):
    signal = signal - np.mean(signal) if is_loop else signal
    if not is_loop:
        fade = min(int(EDGE_FADE_SECONDS * SAMPLE_RATE), len(signal) // 2)
        ramp = np.linspace(0.0, 1.0, fade)
        signal[:fade] *= ramp
        signal[-fade:] *= ramp[::-1]
    return signal * (PEAK / np.max(np.abs(signal)))


def write_wav(path, signal):
    pcm = np.round(signal * 32767).astype("<i2")
    with wave.open(str(path), "wb") as file:
        file.setnchannels(1)
        file.setsampwidth(2)
        file.setframerate(SAMPLE_RATE)
        file.writeframes(pcm.tobytes())


def main():
    for name, (build, is_loop) in SOUNDS.items():
        signal = finish(build().astype(np.float64), is_loop)
        path = OUTPUT_DIR / f"{name}.wav"
        write_wav(path, signal)
        print(f"{path.name}: {len(signal) / SAMPLE_RATE:.2f} s{' (loop)' if is_loop else ''}")


if __name__ == "__main__":
    main()
