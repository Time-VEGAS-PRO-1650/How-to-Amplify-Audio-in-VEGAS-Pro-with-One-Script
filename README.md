# Adjust Gain on Selected Area — Script for VEGAS Pro

This script allows you to quickly adjust volume gain, normalization, and fade-in effects on selected audio events in VEGAS Pro.

---

## 📦 Installation

1. Copy `Adjust Gain on Selected Area.cs` and `Adjust Gain on Selected Area.png` to the folder:

[My Documents]\Vegas Script Menu

2. If the `Vegas Script Menu` folder does not exist — create it manually.
3. For quick script launching, assign the script to a hotkey (e.g. `8`).

---

## 🧰 Features Overview

- **Split** – splits selected audio events into two parts
- **Play** – continues playback when pressing `Space`
- **Normalize** – automatic normalization to 0 dB peak
- **FadeIn** – adjust fade-in in dB
- **Volume Trackbar** – adjust gain between -100 dB to +100 dB
- **Mouse Wheel Support** – fine-tune with Ctrl+Scroll
- **Double Click Trackbar** – resets gain to 0 dB
- **Real-Time Changes** – instant updates to audio

---

## 📝 Detailed Instructions

### 1. Selecting Audio Events

- Select one or more audio events in VEGAS.
- If no event is selected, a warning appears: _"Please select an audio event."_
- You can select only a portion of an event. If **Split** is enabled, the script will split the selection and add 350ms fades on both sides.
- The script window appears near the mouse cursor.

### 2. Volume Gain Control

- Adjust volume using the **left-side trackbar**.
- Range: `-100 dB` to `+100 dB`.  
- Current value is shown below the trackbar.
- **Double-click** to reset to `0 dB`.
- **Mouse wheel** adjusts gain; hold `Ctrl` for 0.1 dB steps.

### 3. Fade-In Adjustment

- Enter fade-in value in the top-right **input field** (`-100 dB` to `0 dB`).
- Mouse wheel also works here.
- Values ≥ 1 reset automatically to `0 dB`.

### 4. Normalize Button

- Click **Normalize** to bring selected audio to peak `0 dB`.
- Automatically removes focus after click (avoids repeated triggers on Enter).

### 5. Playback

- Press `Spacebar` to play/pause without closing the script window.

### 6. Split Option

- **Split Checkbox**: Enables auto-splitting and fade generation on selected audio area.
- Remembers its state between script launches. Default: Off.

### 7. Closing the Script

- Click **OK** or press `Enter` to apply changes and close.
- Press `Escape` or **right-click background** to close without applying.

### 8. Moving the Window

- Drag the window by holding the left mouse button anywhere on the form.
- Auto-adjusts to screen boundaries.

---

## ℹ️ Notes

- All changes apply in **real time**.
- Script by **Time VEGAS PRO**.
- Errors are shown with detailed messages.
- Consider assigning the script to a shortcut key for faster use (e.g., key `8`).

---




