using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
namespace ShowPicOnly
{
    public partial class Form1 : Form, IMessageFilter
    {
        #region drag without title - varaibles
        //source: https://stackoverflow.com/questions/23966253/moving-form-without-title-bar

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        public const int WM_LBUTTONDOWN = 0x0201;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private HashSet<Control> controlsToMove = new HashSet<Control>();
        #endregion
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetBackgroundWithSameSize(CreateBitmapImage("There is no any image or text in the clipboard\nCopy an image or text and then right click here to show it\nClick with middle mouse button to close the screen\nDrag the screen to change its location\nYou can also resize the image"));
            SetBackgroundImageFromClipboard();

            Application.AddMessageFilter(this);

            controlsToMove.Add(this);

            //FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.Text = String.Empty;

            TopMost = true;

            //locate at right center
            int x = Screen.PrimaryScreen.Bounds.Width - Size.Width;
            int y = Screen.PrimaryScreen.Bounds.Height / 2 - Size.Height / 2;
            Location = new Point(x, y);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                SetBackgroundImageFromClipboard();
            }
            if (e.Button == MouseButtons.Middle)
            {
                Application.Exit();
            }
        }

        void SetBackgroundImageFromClipboard()
        {
            Image? image = null;
            if (Clipboard.ContainsImage())
                image = Clipboard.GetImage();
            else if (Clipboard.ContainsFileDropList())
            {
                try
                {
                    var files = Clipboard.GetFileDropList();
                    image = Image.FromFile(files[0]);
                }
                catch
                {//ignore
                }
            }
            else if (Clipboard.ContainsText())
            {
                try
                {
                    image = CreateBitmapImage(Clipboard.GetText());
                }
                catch
                {//ignore
                }
            }

            if (image != null)
                SetBackgroundWithSameSize(image);
        }

        void SetBackgroundWithSameSize(Image image)
        {
            Size = image.Size;
            BackgroundImage = image;

            //locate at right center
            int x = Screen.PrimaryScreen.Bounds.Width - Size.Width;
            int y = Screen.PrimaryScreen.Bounds.Height / 2 - Size.Height / 2;
            Location = new Point(x, y);
        }

        #region drag without title
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN
                /* && controlsToMove.Contains(Control.FromHandle(m.HWnd))*/)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                return true;
            }
            return false;
        }
        #endregion

        private Bitmap CreateBitmapImage(string sImageText, int fontSize = 60, string fontName = "Arial")
        {
            Bitmap objBmpImage = new Bitmap(1, 1);

            int width = 0;
            int height = 0;

            // Create the Font object for the image text drawing.
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
            objGraphics.Clear(Color.White);
            objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            objGraphics.DrawString(sImageText, objFont, new SolidBrush(Color.FromArgb(102, 102, 102)), 0, 0);
            objGraphics.Flush();

            //it's being so wide. idk why. so i should reduce the width size like this:
            float widthDivideRatio = 1.5f;
            objBmpImage = new Bitmap(objBmpImage, new Size((int)(objBmpImage.Width / widthDivideRatio), objBmpImage.Height));

            return (objBmpImage);
        }
    }
}