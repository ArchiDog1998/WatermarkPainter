using Grasshopper;
using SimpleGrasshopper.Attributes;
using System.Drawing;
using System.Drawing.Imaging;
using OpenFileDialog = Rhino.UI.OpenFileDialog;
using System.IO;
using System;

namespace WatermarkPainter;
internal static partial class Data
{
    [Range(0, 5, 3)]
    [Setting, Config("Image Ratio")]
    private static readonly float _ImageRatio = 1f;

    [Setting]
    private static readonly string _ImagePath = string.Empty;

    [Setting, Config("Alignment")]
    private static readonly ContentAlignment _Alignment = ContentAlignment.BottomRight;

    [Range(5, 60)]
    [Setting, Config("FPS")]
    private static readonly int _GifFps = 15;

    [Config("File Location")]
    private static bool ChangeFile
    {
        get => false;
        set
        {
            var open = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif",
                Title = "A Image File",
                FileName = ImagePath,
                MultiSelect = false
            };
            if (open.ShowOpenDialog())
            {
                ImagePath = open.FileName;
            };
        }
    }
    static readonly DateTime startTime = DateTime.Now;
    static int _frameCount;
    static Image _readImage;
    public static Image DrawImage
    {
        get
        {
            if (HasFrameCount)
            {
                var i = (int)((DateTime.Now - startTime).TotalSeconds * GifFps) % _frameCount;
                _readImage.SelectActiveFrame(FrameDimension.Time, i);
            }
            return _readImage;
        }
    }

    public static bool HasFrameCount => _frameCount > 1;

    static Data()
    {
        OnPropertyChanged += (s, e) =>
        {
            Instances.ActiveCanvas.ScheduleRegen(10);
        };

        OnImagePathChanged += LoadImage;
    }

    private static void LoadImage(string path)
    {
        if (!File.Exists(path)) return;

        _readImage = Image.FromFile(path);
        _frameCount = _readImage.GetFrameCount(new FrameDimension(_readImage.FrameDimensionsList[0]));
    }
}
