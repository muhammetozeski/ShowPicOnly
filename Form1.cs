using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Text;
namespace ShowPicOnly
{
    public partial class Form1 : Form
    {
        #region drag window variables
        private bool isDragging = false;
        private Point lastCursorPosition;
        #endregion

        const int ResizeBoxSize = 7;
        const int MinimumWindowLength = 20;
        const int MinimumWindowHeight = 20;
        readonly Color ResizeBoxColor = Color.Gray;
        const string Title = "Image";

        PictureBox pictureBox;

        Panel ResizeBox;

        Size ImageSize;
        Image image = null;
        Image Image { get => image; set { image = value; ImageSize = value.Size; } }

        NotifyIcon notifyIcon = new NotifyIcon { Visible = false, Text = Title, Icon = Properties.Resources.icon1 };

        public Form1()
        {
            InitializeComponent();
        }

        const string IdleText = "There is no any Image or text in the clipboard\nCopy an Image or text and then right click here to show it\nClick with middle mouse button to close the screen\nDrag the screen to change its location\nYou can also resize the Image";

        private void Form1_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(255, 250, 255);
            this.TransparencyKey = Color.FromArgb(255, 250, 255); //nobody would use this color instead of pure white. i hope...


            #region initialize picture box
            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            Controls.Add(pictureBox);
            #endregion


            #region signing drag window events
            MouseMove += Form1_MouseMove;
            #endregion

            MouseUp += Form1_MouseUp;

            #region assign events for picture box
            pictureBox.MouseDown += Form1_MouseDown;
            pictureBox.MouseMove += Form1_MouseMove;
            pictureBox.MouseUp += Form1_MouseUp;
            #endregion

            Paint += Form1_Paint;

            TopMost = true;

            //locate at right center
            int x = Screen.PrimaryScreen.Bounds.Width - Size.Width;
            int y = Screen.PrimaryScreen.Bounds.Height / 2 - Size.Height / 2;
            Location = new Point(x, y);

            RemoveAppBorders();

            Text = Title;

            SetBackgroundWithSameSize(CreateBitmapImage(IdleText));
            SetBackgroundImageFromClipboard();

            try
            {
                if (Program.args.Length != 0)
                {
                    string path = Program.args[0].Replace("\"", "");
                    if (File.Exists(path))
                    {
                        this.Image = Image.FromFile(path);
                        SetBackgroundWithSameSize(Image);
                    }
                }
            }
            catch (Exception) { }

            ResizeBox = CreateResizeBox(this, ResizeBoxSize);
            #region assign events for resize box
            ResizeBox.MouseDown += Form1_MouseDown;
            ResizeBox.MouseMove += Form1_MouseMove;
            ResizeBox.MouseUp += Form1_MouseUp;
            Resize += (obj, e) => { ResizeBox.Location = RelocateResizeBox(ResizeBoxSize); };
            #endregion
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        private void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Visible = !Visible;
            else if (e.Button == MouseButtons.Right)
                Application.Exit();
        }

        Point RelocateResizeBox(int size)
        {
            return new Point(Size.Width - size, Size.Height - size);
        }
        Panel CreateResizeBox(Form form, int size)
        {
            var blackBox = new Panel
            {
                BackColor = ResizeBoxColor,
                Width = size,
                Height = size,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = RelocateResizeBox(size),
            };

            form.Controls.Add(blackBox);
            blackBox.BringToFront(); // En üstte göstermek için
            return blackBox;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            using (var brush = new SolidBrush(Color.Gray))
            {
                e.Graphics.FillRectangle(brush, new Rectangle(this.ClientSize.Width - ResizeBoxSize, this.ClientSize.Height - ResizeBoxSize, ResizeBoxSize, ResizeBoxSize));
            }
        }

        bool MouseIsOverControl(Control control) => control.ClientRectangle.Contains(control.PointToClient(Cursor.Position));

        private bool IsInResizeArea(Point location)
        {
            //return location.X >= this.ClientSize.Width - ResizeBoxSize && location.Y >= this.ClientSize.Height - ResizeBoxSize;
            return MouseIsOverControl(ResizeBox);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((MouseButtons & MouseButtons.Left) == MouseButtons.Left)
            {
                //hide from taskbar
                if ((MouseButtons & MouseButtons.Right) == MouseButtons.Right)
                {
                    ShowInTaskbar = !ShowInTaskbar;
                    notifyIcon.Visible = !notifyIcon.Visible;
                }

                //relocate the window
                else if (!IsInResizeArea(e.Location))
                {
                    isDragging = true;
                    lastCursorPosition = MousePosition;
                }
            }
        }
        private void Form1_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && (MouseButtons & MouseButtons.Left) != MouseButtons.Left)
            {
                SetBackgroundImageFromClipboard();
            }
            else
            if (e.Button == MouseButtons.Middle)
            {
                Application.Exit();
            }
            else
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }

        }
        private void Form1_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging && lastCursorPosition != MousePosition)
            {
                if ((MouseButtons & MouseButtons.Right) == MouseButtons.Right)
                {
                    isDragging = false;
                    return;
                }
                // Yeni fare konumu
                Point newCursorPosition = MousePosition;

                // Fare hareketini iþleyerek pencerenin konumunu güncelle
                int deltaX = newCursorPosition.X - lastCursorPosition.X;
                int deltaY = newCursorPosition.Y - lastCursorPosition.Y;

                // Konumu güncelle
                this.Location = new Point(this.Location.X + deltaX, this.Location.Y + deltaY);

                // Önceki konumu güncelle
                lastCursorPosition = newCursorPosition;
            }
            else if (this.Cursor == Cursors.SizeNWSE && e.Button == MouseButtons.Left) // Pencereyi boyutlandýrma
            {
                Point mouse = this.PointToClient(Cursor.Position);
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    double magnitude = GetMagnitude(mouse) / GetMagnitude(new Point(ImageSize.Width, ImageSize.Height));
                    var size = new Size((int)Math.Round(ImageSize.Width * magnitude), (int)Math.Round(ImageSize.Height * magnitude));

                    size.Width = Math.Max(MinimumWindowLength, size.Width + ResizeBoxSize);
                    size.Height = Math.Max(MinimumWindowHeight, size.Height + ResizeBoxSize);
                    Size = size;
                }
                else
                {
                    int newWidth = Math.Max(MinimumWindowLength, mouse.X + ResizeBoxSize);
                    int newHeight = Math.Max(MinimumWindowHeight, mouse.Y + ResizeBoxSize);
                    this.Size = new Size(newWidth, newHeight);
                }
            }
            else
            {
                this.Cursor = IsInResizeArea(MousePosition) ? Cursors.SizeNWSE : Cursors.Default; // Ýmleç türünü belirle
            }
        }

        void RemoveAppBorders()
        {
            // Windows API fonksiyonlarýný tek bir fonksiyon içinde tanýmlýyoruz
            [DllImport("user32.dll")]
            static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

            [DllImport("user32.dll")]
            static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            const int GWL_EXSTYLE = -20;
            const int WS_EX_LAYERED = 0x80000;
            const int WS_EX_TRANSPARENT = 0x20;
            IntPtr HWND_TOPMOST = new IntPtr(-1);
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOMOVE = 0x0002;
            const uint SWP_NOACTIVATE = 0x0010;

            // Formun baþlangýç ayarlarýný yap
            this.FormBorderStyle = FormBorderStyle.None; // Çerçeveyi kaldýrýr
            this.ControlBox = false; // Kapatma ve küçültme düðmelerini kaldýrýr
            this.Text = String.Empty; // Baþlýk çubuðunu kaldýrýr
            //this.ShowInTaskbar = false; // Görev çubuðunda görünmesini engeller

            // Geniþletilmiþ stil bayraklarýný al
            int extendedStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);

            // Katmanlý ve þeffaf stil bayraklarýný ayarla
            SetWindowLong(this.Handle, GWL_EXSTYLE, extendedStyle | WS_EX_LAYERED);

            // Pencereyi en üstte tutma (isteðe baðlý)
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }


        void SetBackgroundImageFromClipboard()
        {
            if (Clipboard.ContainsImage())
                Image = Clipboard.GetImage();
            else if (Clipboard.ContainsFileDropList())
            {
                try
                {
                    var files = Clipboard.GetFileDropList();
                    Image = Image.FromFile(files[0]);
                }
                catch
                {//ignore
                }
            }
            else if (Clipboard.ContainsText())
            {
                try
                {
                    Image = CreateBitmapImage(Clipboard.GetText());
                }
                catch
                {//ignore
                }
            }

            if (Image != null)
                SetBackgroundWithSameSize(Image);
        }

        void SetBackgroundWithSameSize(Image image)
        {
            Size = image.Size;
            pictureBox.Size = Size;
            pictureBox.Image = image;


            //locate at right center
            //int x = Screen.PrimaryScreen.Bounds.Width - Size.Width;
            //int y = Screen.PrimaryScreen.Bounds.Height / 2 - Size.Height / 2;
            //Location = new Point(x, y);
        }

        private Bitmap CreateBitmapImage(string sImageText, int fontSize = 60, string fontName = "Arial")
        {
            Bitmap objBmpImage = new Bitmap(1, 1);

            int width = 0;
            int height = 0;

            // Create the Font object for the Image text drawing.
            Font objFont = new Font(fontName, fontSize, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);

            // Create a graphics object to measure the text's width and height.
            Graphics objGraphics = Graphics.FromImage(objBmpImage);

            // This is where the bitmap size is determined.
            width = (int)objGraphics.MeasureString(sImageText, objFont).Width;
            height = (int)objGraphics.MeasureString(sImageText, objFont).Height;

            // Create the bmpImage again with the correct size for the text and fontName.
            objBmpImage = new Bitmap(objBmpImage, new Size(width, height));

            // Add the colors to the new bitmap.
            objGraphics = Graphics.FromImage(objBmpImage);

            // Set Background color
            objGraphics.Clear(Color.FromArgb(255, 255, 254));
            objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            objGraphics.DrawString(sImageText, objFont, new SolidBrush(Color.FromArgb(102, 102, 102)), 0, 0);
            objGraphics.Flush();

            //it's being so wide. idk why. so i should reduce the width size like this:
            float widthDivideRatio = 1.5f;
            objBmpImage = new Bitmap(objBmpImage, new Size((int)(objBmpImage.Width / widthDivideRatio), objBmpImage.Height));

            return (objBmpImage);
        }
        void UpdateNotifyIconsIcon()
        {
            notifyIcon.Icon = Icon.FromHandle(((Bitmap)Image).GetHicon());
        }

        double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
        double GetMagnitude(Point p)
        {
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }
    }
}