namespace ShowPicOnly;

/// <summary>
/// A borderless window that stays above every other window and shows one picture at its original size. Pixels of
/// <see cref="Palette.Transparent"/> are see-through. Dragging with the left button moves the window, dragging the box
/// in the bottom right corner resizes it (Shift keeps the picture's aspect ratio), a right click shows the clipboard
/// content, a middle click exits, and pressing the left and right buttons together moves the window between the
/// taskbar and the notification area.
/// </summary>
sealed class PictureWindow : Form
{
    #region Settings
    const string Title = "Image";
    const string IdleText = "There is no any Image or text in the clipboard\nCopy an Image or text and then right click here to show it\nClick with middle mouse button to close the screen\nDrag the screen to change its location\nYou can also resize the Image";
    const int ResizeBoxSize = 7;
    const int MinimumWidth = 20;
    const int MinimumHeight = 20;

    /// <summary>A right button release this soon after the left and right buttons were pressed together belongs to that press and does not read the clipboard.</summary>
    static readonly TimeSpan TrayToggleReleaseTime = TimeSpan.FromSeconds(1);
    #endregion

    readonly PictureBox PictureBox = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.StretchImage };
    readonly Panel ResizeBox = new() { BackColor = Palette.ResizeBox, Size = new(ResizeBoxSize, ResizeBoxSize) };
    Image? Picture;
    NotifyIcon? TrayIcon;
    DateTime TrayToggleTime;

    #region drag window variables
    bool IsDragging;
    Point LastCursorPosition;
    #endregion

    /// <summary>
    /// Creates the window with the first picture that is available: the image file at <paramref name="imagePath"/>,
    /// otherwise the clipboard content, otherwise <see cref="IdleText"/>.
    /// </summary>
    /// <param name="imagePath">Path of an image file to show. When it is null or cannot be loaded, the clipboard is used.</param>
    public PictureWindow(string? imagePath = null)
    {
        // Every window style is set before the handle is created. Changing Text between empty and non-empty while
        // ControlBox is false makes WinForms destroy and recreate the window, which took about 0.65 s each time.
        Text = Title;
        ControlBox = false;
        ShowIcon = false;
        TopMost = true;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        BackColor = Palette.Transparent;
        TransparencyKey = Palette.Transparent;

        // Controls earlier in the collection are drawn on top, so the resize box stays above the picture.
        Controls.AddRange([ResizeBox, PictureBox]);
        Control[] mouseTargets = [this, PictureBox, ResizeBox];
        foreach (Control mouseTarget in mouseTargets)
        {
            mouseTarget.MouseDown += HandleMouseDown;
            mouseTarget.MouseMove += HandleMouseMove;
            mouseTarget.MouseUp += HandleMouseUp;
        }

        ShowPicture(PictureLoader.FromFile(imagePath) ?? PictureLoader.FromClipboard() ?? PictureLoader.FromText(IdleText));

        //locate at right center
        // Uses the size of the picture, so the window is placed after the picture is set. The top left corner stays on the screen.
        Rectangle screen = Screen.PrimaryScreen?.Bounds ?? SystemInformation.VirtualScreen;
        Location = new(Math.Max(screen.Left, screen.Right - Width), Math.Max(screen.Top, screen.Top + screen.Height / 2 - Height / 2));
    }

    /// <summary>
    /// Shows a picture, sizes the window to the picture's original size and disposes the picture it replaces.
    /// </summary>
    /// <param name="picture">The picture to show. Null leaves the window unchanged.</param>
    void ShowPicture(Image? picture)
    {
        if (picture is null) return;

        Image? previousPicture = Picture;
        Picture = picture;
        Size = picture.Size;
        PictureBox.Image = picture;
        // A picture loaded with Image.FromFile keeps its file locked until it is disposed.
        if (previousPicture != picture) previousPicture?.Dispose();

        //locate at right center
        //int x = Screen.PrimaryScreen.Bounds.Width - Size.Width;
        //int y = Screen.PrimaryScreen.Bounds.Height / 2 - Size.Height / 2;
        //Location = new Point(x, y);
    }

    /// <summary>
    /// Creates the notification area icon. It is needed only after the left and right buttons are pressed together,
    /// so it is not created at startup.
    /// </summary>
    /// <returns>A hidden icon that shows or hides the window on a left click and exits on a right click.</returns>
    NotifyIcon CreateTrayIcon()
    {
        NotifyIcon trayIcon = new() { Text = Title, Icon = Icon.ExtractIcon(Application.ExecutablePath, 0, SystemInformation.SmallIconSize.Width) ?? SystemIcons.Application };
        trayIcon.MouseClick += (_, mouse) =>
        {
            switch (mouse.Button)
            {
                case MouseButtons.Left: Visible = !Visible; break;
                case MouseButtons.Right: Application.Exit(); break;
            }
        };
        return trayIcon;
    }

    /// <summary>
    /// Removes the notification area icon when the window closes.
    /// </summary>
    /// <param name="eventArgs">Data of the closed event.</param>
    protected override void OnFormClosed(FormClosedEventArgs eventArgs)
    {
        TrayIcon?.Dispose();
        base.OnFormClosed(eventArgs);
    }

    /// <summary>
    /// Keeps the resize box in the bottom right corner whenever the window size changes.
    /// </summary>
    /// <param name="eventArgs">Data of the resize event.</param>
    protected override void OnResize(EventArgs eventArgs)
    {
        base.OnResize(eventArgs);
        ResizeBox.Location = new(ClientSize.Width - ResizeBoxSize, ClientSize.Height - ResizeBoxSize);
    }

    #region Mouse
    /// <summary>
    /// Checks whether the cursor is over the resize box.
    /// </summary>
    /// <returns>True when the cursor is inside the resize box.</returns>
    bool IsCursorOverResizeBox() => ResizeBox.ClientRectangle.Contains(ResizeBox.PointToClient(MousePosition));

    /// <summary>
    /// With the left and right buttons pressed together, moves the window between the taskbar and the notification area.
    /// With the left button alone outside the resize box, starts moving the window.
    /// </summary>
    /// <param name="sender">The window or one of its controls.</param>
    /// <param name="mouse">Data of the mouse event.</param>
    void HandleMouseDown(object? sender, MouseEventArgs mouse)
    {
        if (!MouseButtons.HasFlag(MouseButtons.Left)) return;

        //hide from taskbar
        if (MouseButtons.HasFlag(MouseButtons.Right))
        {
            ShowInTaskbar = !ShowInTaskbar;
            TrayIcon ??= CreateTrayIcon();
            TrayIcon.Visible = !TrayIcon.Visible;
            TrayToggleTime = DateTime.Now;
        }
        //relocate the window
        else if (!IsCursorOverResizeBox())
        {
            IsDragging = true;
            LastCursorPosition = MousePosition;
        }
    }

    /// <summary>
    /// Right button: shows the clipboard content, or restores the size of the current picture when the clipboard has
    /// nothing to show. Middle button: exits. Left button: stops moving the window.
    /// </summary>
    /// <param name="sender">The window or one of its controls.</param>
    /// <param name="mouse">Data of the mouse event.</param>
    void HandleMouseUp(object? sender, MouseEventArgs mouse)
    {
        switch (mouse.Button)
        {
            case MouseButtons.Right when DateTime.Now - TrayToggleTime > TrayToggleReleaseTime:
                ShowPicture(PictureLoader.FromClipboard() ?? Picture);
                break;
            case MouseButtons.Middle:
                Application.Exit();
                break;
            case MouseButtons.Left:
                IsDragging = false;
                break;
        }
    }

    /// <summary>
    /// Moves the window while it is being dragged, resizes it while the resize box is being dragged, and otherwise shows
    /// the resize cursor over the resize box.
    /// </summary>
    /// <param name="sender">The window or one of its controls.</param>
    /// <param name="mouse">Data of the mouse event.</param>
    void HandleMouseMove(object? sender, MouseEventArgs mouse)
    {
        if (IsDragging && LastCursorPosition != MousePosition)
        {
            if (MouseButtons.HasFlag(MouseButtons.Right))
            {
                IsDragging = false;
                return;
            }

            Point cursorPosition = MousePosition;
            Location += new Size(cursorPosition.X - LastCursorPosition.X, cursorPosition.Y - LastCursorPosition.Y);
            LastCursorPosition = cursorPosition;
        }
        else if (Cursor == Cursors.SizeNWSE && mouse.Button == MouseButtons.Left)
            ResizeToCursor();
        else
            Cursor = IsCursorOverResizeBox() ? Cursors.SizeNWSE : Cursors.Default;
    }

    /// <summary>
    /// Moves the bottom right corner of the window to the cursor. While Shift is held the picture's aspect ratio is kept,
    /// and the distance of the cursor from the top left corner decides the scale.
    /// </summary>
    void ResizeToCursor()
    {
        Point cursor = PointToClient(MousePosition);
        Size size = (Size)cursor;
        if (ModifierKeys.HasFlag(Keys.Shift) && Picture is not null)
        {
            double scale = double.Hypot(cursor.X, cursor.Y) / double.Hypot(Picture.Width, Picture.Height);
            size = new((int)Math.Round(Picture.Width * scale), (int)Math.Round(Picture.Height * scale));
        }
        Size = new(Math.Max(MinimumWidth, size.Width + ResizeBoxSize), Math.Max(MinimumHeight, size.Height + ResizeBoxSize));
    }
    #endregion
}
