 Instructions for Using the "Adjust Gain on Selected Area" Script
Welcome! This script allows you to quickly adjust the volume, normalization, and FadeIn effect for selected audio events in VEGAS Pro. Follow these simple steps to take advantage of   
    all its features:
- Script Installation -   
Copy *Adjust Gain on Selected Area.cs* and *Adjust Gain on Selected Area.png* to the directory 
[My Documents]\Vegas Script Menu
If the *Vegas Script Menu* folder does not exist, create it. For quick script launching, assign it, for example, to the "8" key.
 Script Window with Labels:

Detailed Instructions
# 1. Selecting an Audio Event  
	- Ensure that at least one audio event is selected in the project. If no event is selected, the script will display a warning: "Please select an audio event."  
	- To work with a portion of an event, highlight the desired section on the timeline. If the *Split* option is enabled and an area within the event is selected, it will automatically split, creating 350-millisecond fades on both sides.  
	- The window will always appear to the right of the mouse cursor.  
# 2. Volume Control (Gain)  
	- Trackbar (left side): Move the slider up or down to adjust the volume from -100 dB to 100 dB.  
	- The value is displayed below the trackbar (e.g., "0.0 dB").  
	- Double-click the trackbar to reset the volume to 0 dB.  
	- Use the mouse wheel for precise adjustments (hold Ctrl for 0.1 dB increments).  
	- Changes are applied in real time.  

# 3. FadeIn Adjustment (top right)  
	- Input field (numbers): Enter the FadeIn volume value in dB (from -100 to 0) or use the mouse wheel to adjust it.  
	- The initial value is displayed in the top right corner.  
	- Changes are applied in real time as you type or scroll.  
	- If the value is ≥ 1, it automatically resets to 0 dB.  
# 4. Normalization  
	- "Normalize" button (bottom left): Click to apply normalization to the selected audio event.  
	- After normalization, the volume is automatically set to a level where the maximum amplitude reaches 0 dB.  
	- Clicking the button removes focus to prevent repeated triggering when pressing Enter. 
# 5. Playback
	Pressing the spacebar: Starts or stops playback without closing the script window.  
# 6. Splitting (Split)  
	- "Split" checkbox: Enable this option to split the selected audio event area and create fades. Splitting occurs immediately upon enabling. The state is saved between script runs. Disabled by default.
# 7. Closing the Script  
	- "OK" button (bottom right): Click to close the script and save changes.
	- Pressing Enter: An alternative way to close the script, with the same effect as the "OK" button.
	- Pressing Escape: Closes the script without additional actions.  
	- Right-clicking outside the controls: Closes the script when clicking on the form’s background.
# 8. Moving the Window  
	- Hold the left mouse button on the form and drag it to the desired location. Screen boundaries are automatically accounted for.
# Notes
	- All changes are applied in real time and saved upon closing.
	- The script was developed by Time VEGAS PRO.
	- If errors occur, a message with a description of the issue will be displayed. 
 - For added convenience, assign it to a key, such as the "8" key, for quick script launching. 
 




https://www.youtube.com/watch?v=63vRxymYl0M

# How-to-Amplify-Audio-in-VEGAS-Pro-with-One-Script
