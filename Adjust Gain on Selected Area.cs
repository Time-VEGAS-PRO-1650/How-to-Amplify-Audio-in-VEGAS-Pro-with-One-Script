using System;
using System.Windows.Forms;
using System.Drawing;
using ScriptPortal.Vegas;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Collections.Generic;

public class EntryPoint
{
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    private float dbLevel = 0;
    private AudioEvent middleEvent = null;
    private bool isUpdatingGain = false;
    private Form inputForm;
    private float fadeInGainLevelLinear = 1.0f;
    private TextBox fadeInGainInput;
    private Vegas vegasInstance;
    private Timer playbackTimer;
    private CheckBox splitCheckBox;
    private AudioEvent originalEvent;
    private Timecode selectionStart;
    private Timecode selectionLength;
    private Timecode eventStart;
    private Timecode eventEnd;
    private List<AudioEvent> splitEvents = new List<AudioEvent>();

    public void FromVegas(Vegas vegas)
    {
        try
        {
            vegasInstance = vegas;
            bool wasPlayingInitially = vegas.Transport.IsPlaying;

            AudioEvent selectedEvent = null;
            foreach (Track track in vegas.Project.Tracks)
            {
                if (!track.IsAudio()) continue;
                foreach (TrackEvent ev in track.Events)
                {
                    if (ev.Selected && ev is AudioEvent)
                    {
                        selectedEvent = ev as AudioEvent;
                        break;
                    }
                }
                if (selectedEvent != null) break;
            }

            if (selectedEvent == null)
            {
                MessageBox.Show("Please select an audio event.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            originalEvent = selectedEvent;
            eventStart = selectedEvent.Start;
            eventEnd = eventStart + selectedEvent.Length;
            selectionStart = vegas.SelectionStart;
            selectionLength = vegas.SelectionLength;
            middleEvent = selectedEvent;

            bool hasValidSelection = selectionLength.ToMilliseconds() > 0 &&
                                    ((selectionStart >= eventStart && (selectionStart + selectionLength) <= eventEnd) ||
                                     (selectionStart < eventEnd && (selectionStart + selectionLength) > eventStart));

            float initialGainLinear = (float)(middleEvent.NormalizeGain > 0 ? middleEvent.NormalizeGain : 1.0);
            dbLevel = LinearToDB(initialGainLinear);
            fadeInGainLevelLinear = (float)middleEvent.FadeIn.Gain;

            inputForm = new Form
            {
                Text = "Gain",
                Size = new Size(130, 230),
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(40, 40, 40),
                Opacity = 0.8,
                ShowInTaskbar = false,
                KeyPreview = true,
                TopMost = true
            };

            inputForm.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, inputForm.Width, inputForm.Height, 20, 20));

            Point cursorPosition = Cursor.Position;
            int formWidth = inputForm.Width;
            int formHeight = inputForm.Height;
            Screen screen = Screen.FromPoint(cursorPosition);

            int formX = cursorPosition.X - (formWidth - 150);
            int formY = cursorPosition.Y - (formHeight / 2);

            formX = Math.Max(screen.WorkingArea.X, Math.Min(formX, screen.WorkingArea.Right - formWidth));
            formY = Math.Max(screen.WorkingArea.Y, Math.Min(formY, screen.WorkingArea.Bottom - formHeight));

            inputForm.Location = new Point(formX, formY);

            bool isDragging = false;
            Point lastCursorPosition = Point.Empty;
            inputForm.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isDragging = true;
                    lastCursorPosition = e.Location;
                }
            };
            inputForm.MouseMove += (s, e) =>
            {
                if (isDragging)
                {
                    int deltaX = e.Location.X - lastCursorPosition.X;
                    int deltaY = e.Location.Y - lastCursorPosition.Y;
                    Point newPosition = new Point(inputForm.Location.X + deltaX, inputForm.Location.Y + deltaY);
                    Rectangle screenBounds = screen.WorkingArea;
                    newPosition.X = Math.Max(screenBounds.X, Math.Min(newPosition.X, screenBounds.Right - inputForm.Width));
                    newPosition.Y = Math.Max(screenBounds.Y, Math.Min(newPosition.Y, screenBounds.Bottom - inputForm.Height));
                    inputForm.Location = new Point(newPosition.X, newPosition.Y);
                }
            };
            inputForm.MouseUp += (s, e) => { if (e.Button == MouseButtons.Left) isDragging = false; };

            playbackTimer = new Timer();
            playbackTimer.Interval = 100;
            playbackTimer.Tick += (s, e) =>
            {
                try { vegas.UpdateUI(); } catch { }
            };
            playbackTimer.Start();

            inputForm.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    playbackTimer.Stop();
                    inputForm.Close();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    playbackTimer.Stop();
                    inputForm.Close();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Space)
                {
                    try
                    {
                        if (vegas.Transport.IsPlaying)
                            vegas.Transport.Stop();
                        else
                            vegas.Transport.Play();
                        e.SuppressKeyPress = true;
                    }
                    catch { }
                }
            };

            inputForm.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    Point mousePos = inputForm.PointToClient(Control.MousePosition);
                    bool clickedControl = false;
                    foreach (Control control in inputForm.Controls)
                    {
                        if (control.Bounds.Contains(mousePos))
                        {
                            clickedControl = true;
                            break;
                        }
                    }
                    if (!clickedControl)
                    {
                        playbackTimer.Stop();
                        inputForm.Close();
                    }
                }
            };

            Label label = new Label
            {
                Text = "Gain:",
                Location = new Point(5, 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.White
            };
            label.Click += (s, e) =>
            {
                string helpText = @"### Instructions for Using the 'Adjust Gain on Selected Area' Script

Welcome! This script allows you to quickly adjust the volume and FadeIn effect for selected audio events in VEGAS Pro. Follow these simple steps to take advantage of all its features:

Time VEGAS PRO
https://t.me/time_vegas_pro

#### 1. Selecting an Audio Event
- Ensure that at least one audio event is selected in the project. If no event is selected, the script will display a warning: 'Please select an audio event.'
- To work with a portion of an event, select the desired section on the timeline. If the Split option is enabled and an area within the event is selected, it will automatically split, and fades of 350 milliseconds will be created on both sides.
- The window will always appear to the right of the mouse.

#### 2. Volume Control (Gain)
- **Trackbar (left)**: Move the slider up or down to adjust the volume from -100 dB to 100 dB.
  - The value is displayed below the trackbar (e.g., '0.0 dB').
  - Double-click the trackbar to reset the volume to 0 dB.
  - Use the mouse wheel for fine-tuning (Ctrl for a 0.1 dB step).
- Changes are applied in real-time.

#### 3. FadeIn Adjustment (top right)
- **Input field (numbers)**: Enter the FadeIn volume value in dB (from -100 to 0) or use the mouse wheel to adjust.
  - The initial value is displayed in the top right corner.
  - Changes are applied in real-time as you type or scroll.
  - If the value is ≥ 1, it automatically resets to 0 dB.

#### 4. Normalization
- **'Normalize' button (bottom left)**: Click to apply normalization to the selected audio event.
  - After normalization, the volume is automatically set to a level where the maximum amplitude reaches 0 dB.
  - Clicking the button removes focus to prevent re-triggering with Enter.

#### 5. Playback
- **Pressing Space**: Starts or stops playback without closing the script window.

#### 6. Splitting (Split)
- **'Split' Checkbox**: Enable this option to split the selected audio event area with fade creation. Splitting occurs immediately when enabled. The state is saved between script runs. Disabled by default.

#### 7. Closing the Script
- **'OK' button (bottom right)**: Click to close the script and save changes.
- **Pressing Enter**: An alternative way to close the script with the same effect as the 'OK' button.
- **Pressing Escape**: Closes the script without additional actions.
- **Right-click outside controls**: Closes the script when clicking the form background.

#### 8. Moving the Window
- Hold the left mouse button on the form and drag it to the desired location. Screen boundaries are automatically respected.

#### Notes
- All changes are applied in real-time and saved upon closing.
- The script was developed by Time VEGAS PRO.
- If errors occur, a message with the problem description will be displayed.";
                MessageBox.Show(helpText, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            LinearTrackBar gainTrackBar = new LinearTrackBar
            {
                Location = new Point(40, 5),
                Minimum = -100,
                Maximum = 100,
                Value = (float)Math.Round(dbLevel, 1)
            };

            Label gainValueLabel = new Label
            {
                Text = dbLevel.ToString("F1") + " dB",
                Location = new Point(50, 160),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            splitCheckBox = new CheckBox
            {
                Text = "Split",
                Location = new Point(5, 25),
                Size = new Size(50, 20),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                Checked = false,
                Font = new Font("Segoe UI", 8)
            };

            splitCheckBox.CheckedChanged += (s, e) =>
            {
                Properties.Settings.Default.SplitEnabled = splitCheckBox.Checked;
                Properties.Settings.Default.Reset();

                if (splitCheckBox.Checked && hasValidSelection)
                {
                    SplitAudioEvent();
                }
                else if (!splitCheckBox.Checked && splitEvents.Count > 0)
                {
                    EventHeal();
                }
            };

            if (hasValidSelection && splitCheckBox.Checked)
            {
                SplitAudioEvent();
            }

            fadeInGainInput = new TextBox
            {
                Location = new Point(90, 5),
                Width = 40,
                Text = LinearToDB(fadeInGainLevelLinear).ToString("F1"),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Center
            };
            fadeInGainInput.TextChanged += (sender, e) =>
            {
                if (isUpdatingGain) return;
                isUpdatingGain = true;

                float fadeInValue;
                if (float.TryParse(fadeInGainInput.Text, out fadeInValue))
                {
                    if (fadeInValue >= 1)
                    {
                        fadeInGainInput.Text = "0.0";
                        fadeInGainLevelLinear = DBToLinear(0);
                    }
                    else
                    {
                        fadeInGainLevelLinear = DBToLinear(fadeInValue);
                    }
                    if (middleEvent != null)
                    {
                        middleEvent.FadeIn.Gain = fadeInGainLevelLinear;
                        try { vegas.UpdateUI(); } catch { }
                    }
                }
                isUpdatingGain = false;
            };
            fadeInGainInput.MouseWheel += (sender, e) =>
            {
                if (isUpdatingGain) return;
                isUpdatingGain = true;

                float step = (Control.ModifierKeys == Keys.Control) ? 0.1f : 1.0f;
                float currentValue;
                float.TryParse(fadeInGainInput.Text, out currentValue);

                if (e.Delta > 0) currentValue += step;
                else if (e.Delta < 0) currentValue -= step;

                if (currentValue >= 1) currentValue = 0;
                if (currentValue < -100) currentValue = -100;

                fadeInGainInput.Text = currentValue.ToString("F1");
                fadeInGainLevelLinear = DBToLinear(currentValue);
                if (middleEvent != null)
                {
                    middleEvent.FadeIn.Gain = fadeInGainLevelLinear;
                    try { vegas.UpdateUI(); } catch { }
                }

                isUpdatingGain = false;
            };

            gainTrackBar.ValueChanged += (s, e) =>
            {
                if (!isUpdatingGain)
                {
                    isUpdatingGain = true;
                    dbLevel = gainTrackBar.Value;
                    gainValueLabel.Text = dbLevel.ToString("F1") + " dB";
                    if (middleEvent != null && middleEvent.ActiveTake != null)
                    {
                        middleEvent.NormalizeGain = DBToLinear(dbLevel);
                        try { vegas.UpdateUI(); } catch { }
                    }
                    isUpdatingGain = false;
                }
            };

            gainTrackBar.MouseWheel += (s, e) =>
            {
                float step = (Control.ModifierKeys == Keys.Control) ? 0.1f : 1.0f;
                float newValue = gainTrackBar.Value + (e.Delta > 0 ? step : -step);
                newValue = Math.Max(-100, Math.Min(100, newValue));
                gainTrackBar.Value = (float)Math.Round(newValue, 1);
            };

            gainTrackBar.DoubleClick += (s, e) => { gainTrackBar.Value = 0; };

            Button playButton = new Button
            {
                Text = "▶",
                Location = new Point(5, 160),
                Size = new Size(30, 25),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 8)
            };
            playButton.Click += (s, e) =>
            {
                try
                {
                    if (vegas.Transport.IsPlaying)
                    {
                        vegas.Transport.Stop();
                        playButton.Text = "▶";
                    }
                    else
                    {
                        vegas.Transport.Play();
                        playButton.Text = "⏸";
                    }
                }
                catch { }
                inputForm.Focus();
            };

            Button applyButton = new Button
            {
                Text = "OK",
                Location = new Point(85, 190),
                Size = new Size(40, 30),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 8)
            };
            applyButton.Click += (s, e) =>
            {
                playbackTimer.Stop();
                inputForm.Close();
            };

            Button normalizeButton = new Button
            {
                Text = "Normalize",
                Location = new Point(5, 190),
                Size = new Size(65, 30),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 8)
            };
            normalizeButton.Click += (s, e) =>
            {
                if (middleEvent != null)
                {
                    middleEvent.Normalize = true;
                    float linearGain = (float)middleEvent.NormalizeGain;
                    dbLevel = LinearToDB(linearGain);
                    gainTrackBar.Value = (float)Math.Round(dbLevel, 1);
                    gainValueLabel.Text = dbLevel.ToString("F1") + " dB";
                    try { vegas.UpdateUI(); } catch { }
                    inputForm.Focus();
                }
            };

            inputForm.Controls.Add(label);
            inputForm.Controls.Add(splitCheckBox);
            inputForm.Controls.Add(gainTrackBar);
            inputForm.Controls.Add(gainValueLabel);
            inputForm.Controls.Add(applyButton);
            inputForm.Controls.Add(normalizeButton);
            inputForm.Controls.Add(fadeInGainInput);
            inputForm.Controls.Add(playButton);

            inputForm.FormClosing += (s, e) =>
            {
                playbackTimer.Stop();
            };

            inputForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SplitAudioEvent()
    {
        Timecode selectionEnd = selectionStart + selectionLength;

        if (selectionStart < eventStart)
            selectionStart = eventStart;
        if (selectionEnd > eventEnd)
            selectionEnd = eventEnd;

        Timecode overlap = Timecode.FromMilliseconds(350);
        Timecode overlap2 = Timecode.FromMilliseconds(overlap.ToMilliseconds() / 2);

        AudioEvent rightEvent = null;
        middleEvent = originalEvent;

        splitEvents.Clear();
        splitEvents.Add(middleEvent);

        Timecode leftCutPoint = selectionStart + overlap2;
        Timecode rightCutPoint = selectionEnd + overlap2;

        if (leftCutPoint > eventStart && leftCutPoint < eventEnd)
        {
            AudioEvent leftEvent = (AudioEvent)middleEvent.Split(leftCutPoint - eventStart);
            middleEvent = leftEvent;
            splitEvents.Add(leftEvent);
            if (middleEvent.ActiveTake != null && middleEvent.ActiveTake.Offset > Timecode.FromMilliseconds(0))
            {
                middleEvent.ActiveTake.Offset -= overlap;
                middleEvent.Start -= overlap;
                middleEvent.Length += overlap;
                middleEvent.FadeOut.Length = overlap;
            }
        }

        if (rightCutPoint > eventStart && rightCutPoint < eventEnd)
        {
            rightEvent = (AudioEvent)middleEvent.Split(rightCutPoint - middleEvent.Start);
            if (rightEvent != null)
            {
                splitEvents.Add(rightEvent);
                if (rightEvent.ActiveTake != null && rightEvent.ActiveTake.Offset > Timecode.FromMilliseconds(0))
                {
                    rightEvent.ActiveTake.Offset -= overlap;
                    rightEvent.Start -= overlap;
                    rightEvent.Length += overlap;
                    rightEvent.FadeIn.Length = overlap;
                }
            }
        }

        try { vegasInstance.UpdateUI(); } catch { }
    }

    private void EventHeal()
    {
        try
        {
            if (splitEvents.Count > 0)
            {
                AudioTrack track = splitEvents[0].Track as AudioTrack;
                if (track == null) return;

                splitEvents.Sort((a, b) => a.Start.CompareTo(b.Start));

                AudioEvent firstEvent = splitEvents[0];
                Timecode newEnd = splitEvents[splitEvents.Count - 1].End;

                firstEvent.Length = newEnd - firstEvent.Start;

                for (int i = 1; i < splitEvents.Count; i++)
                {
                    track.Events.Remove(splitEvents[i]);
                }

                middleEvent = firstEvent;
                splitEvents.Clear();

                try { vegasInstance.UpdateUI(); } catch { }
            }
            else
            {
                middleEvent = originalEvent;
                try { vegasInstance.UpdateUI(); } catch { }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error while restoring the event: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private float LinearToDB(float linear)
    {
        return (linear <= 0) ? -100 : 20 * (float)Math.Log10(linear);
    }

    private float DBToLinear(float db)
    {
        if (db > 100) return float.MaxValue;
        if (db < -100) return 0.0f;
        return (float)Math.Pow(10, db / 20.0);
    }
}

public class LinearTrackBar : Panel
{
    private float minValue = -100;
    private float maxValue = 100;
    private float currentValue = 0;
    private int trackHeight = 150;
    private int trackWidth = 3;
    private int knobRadius = 8;
    private Color knobColor = Color.FromArgb(200, 200, 200);

    public event EventHandler ValueChanged;

    public LinearTrackBar()
    {
        this.Size = new Size(50, trackHeight + 2);
        this.DoubleBuffered = true;
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        this.Cursor = Cursors.Hand;
    }

    public float Minimum
    {
        get { return minValue; }
        set { if (value < maxValue) { minValue = value; UpdateKnobPosition(); } }
    }

    public float Maximum
    {
        get { return maxValue; }
        set { if (value > minValue) { maxValue = value; UpdateKnobPosition(); } }
    }

    public float Value
    {
        get { return currentValue; }
        set
        {
            value = Math.Max(minValue, Math.Min(maxValue, value));
            if (currentValue != value)
            {
                currentValue = value;
                UpdateKnobPosition();
                if (ValueChanged != null)
                {
                    ValueChanged(this, EventArgs.Empty);
                }
                Invalidate();
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using (Pen pen = new Pen(knobColor, trackWidth))
        {
            e.Graphics.DrawLine(pen, this.Width / 2, 5, this.Width / 2, this.Height - 5);
        }
        RectangleF thumbRect = GetThumbBounds(currentValue);
        e.Graphics.FillEllipse(new SolidBrush(knobColor), thumbRect);
    }

    private RectangleF GetThumbBounds(float value)
    {
        float range = maxValue - minValue;
        float normalizedValue = (value - minValue) / range;
        float y = (this.Height - 2 * knobRadius - 10) * (1 - normalizedValue) + 5 + knobRadius;
        float x = (this.Width - 2 * knobRadius) / 2;
        return new RectangleF(x, y - knobRadius, 2 * knobRadius, 2 * knobRadius);
    }

    private void UpdateKnobPosition()
    {
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left) MoveKnobDirectly(e.Location);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (e.Button == MouseButtons.Left) MoveKnobDirectly(e.Location);
    }

    private void MoveKnobDirectly(Point location)
    {
        float y = Math.Max(5 + knobRadius, Math.Min(this.Height - 5 - knobRadius, location.Y));
        float range = this.Height - 2 * knobRadius - 10;
        float normalizedValue = 1 - (y - (5 + knobRadius)) / range;
        Value = (float)Math.Round(minValue + normalizedValue * (maxValue - minValue), 1);
    }
}

namespace Properties
{
    public sealed partial class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(Synchronized(new Settings())));

        public static Settings Default
        {
            get { return defaultInstance; }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("false")]
        public bool SplitEnabled
        {
            get { return ((bool)(this["SplitEnabled"])); }
            set { this["SplitEnabled"] = value; }
        }
    }
}