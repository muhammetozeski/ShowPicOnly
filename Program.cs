namespace ShowPicOnly;

static class Program
{
    /// <summary>
    /// Opens the picture window and returns when it is closed.
    /// </summary>
    /// <param name="arguments">Command line arguments. The first one, when given, is the path of the image file to show.</param>
    [STAThread]
    static void Main(string[] arguments)
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new PictureWindow(arguments is [var imagePath, ..] ? imagePath : null));
    }
}
