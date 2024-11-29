using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using OpenCvSharp;
using Window = Avalonia.Controls.Window;

namespace Runner;

public partial class MainWindow : Window
{
    static Button runAnalysisButton;
    static Image previewImage;
    static Image overlayImage;

    static UpdatePreview updatePreview;

    delegate void UpdatePreview(Bitmap camera, List<Analyzer.Target> targets);

    public MainWindow()
    {
        InitializeComponent();

        runAnalysisButton = this.Find<Button>("RunVideoButton");
        runAnalysisButton.Click += delegate { RunAnalysis(); };

        previewImage = this.Find<Image>("PreviewImage");
        overlayImage = this.Find<Image>("OverlayImage");
        updatePreview = SetPreview;
    }

    void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    static void ResetRunButton()
    {
        Dispatcher.UIThread.InvokeAsync(() => { runAnalysisButton.IsEnabled = true; });
    }

    static void SetPreview(Bitmap camera, List<Analyzer.Target> targets)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (targets.Any())
            {
                DrawingImage drawingImage = DrawGeometry(targets);
                overlayImage.Source = drawingImage;
            }

            previewImage.Source = camera;
        }, DispatcherPriority.Render);

        Dispatcher.UIThread.RunJobs();
    }

    static DrawingImage DrawGeometry(List<Analyzer.Target> targets)
    {
        DrawingImage drawingImage = new DrawingImage();

        DrawingGroup drawingGroup = new DrawingGroup();
        
        Color penColor = Colors.Red;
        
        // HACK: add small dots to the corners of the image to force the overlay to not resize
        GeometryDrawing lineGeometryDrawingTopCorner = new GeometryDrawing
        {
            Pen = new Pen(new SolidColorBrush(penColor))
        };

        RectangleGeometry rectangleGeometryTopCorner = new RectangleGeometry
        {
            Rect = new Avalonia.Rect(0, 0, 1, 1)
        };

        lineGeometryDrawingTopCorner.Geometry = rectangleGeometryTopCorner;
        drawingGroup.Children.Add(lineGeometryDrawingTopCorner);
        
        GeometryDrawing lineGeometryDrawingBottomCorner = new GeometryDrawing
        {
            Pen = new Pen(new SolidColorBrush(penColor))
        };

        RectangleGeometry rectangleGeometryBottomCorner = new RectangleGeometry
        {
            Rect = new Avalonia.Rect(640, 480, 1, 1)
        };

        lineGeometryDrawingBottomCorner.Geometry = rectangleGeometryBottomCorner;
        drawingGroup.Children.Add(lineGeometryDrawingBottomCorner);

        foreach (Analyzer.Target target in targets)
        {
            GeometryDrawing lineGeometryDrawing = new GeometryDrawing
            {
                Pen = new Pen(new SolidColorBrush(penColor), 3)
            };

            RectangleGeometry rectangleGeometry = new RectangleGeometry
            {
                Rect = new Avalonia.Rect(target.X, target.Y, target.Width, target.Height)
            };

            lineGeometryDrawing.Geometry = rectangleGeometry;
            drawingGroup.Children.Add(lineGeometryDrawing);
        }

        drawingImage.Drawing = drawingGroup;
        return drawingImage;
    }

    static void RunAnalysis()
    {
        ResetRunButton();

        Analyzer analyzer = new Analyzer();

        const int deviceIndex = 0;
        const int frameWidth = 640;
        const int frameHeight = 480;
        const int fps = 30;

        DateTime lastHit = DateTime.Now;

        VideoCapture videoCapture = VideoCapture.FromCamera(deviceIndex, VideoCaptureAPIs.DSHOW);
        videoCapture.Open(deviceIndex, VideoCaptureAPIs.DSHOW);

        videoCapture.FrameWidth = frameWidth;
        videoCapture.FrameHeight = frameHeight;
        videoCapture.Fps = fps;

        Task<List<Analyzer.Target>> computerVisionTask = null;
        List<Analyzer.Target> lastTargets = new List<Analyzer.Target>();

        while (true)
        {
            // get video frame
            Mat frame = videoCapture.RetrieveMat();
            
            if (frame.Empty())
            {
                continue; // Skip empty frames
            }

            // Ensure the Mat is in the correct format
            if (frame.Type() != MatType.CV_8UC3)
            {
                Cv2.CvtColor(frame, frame, ColorConversionCodes.GRAY2BGR);
            }

            bool success = frame.GetArray(out Vec3b[] data);


            if (!success) continue;

            Bitmap bitmap = new Bitmap(frame.ToMemoryStream());

            if (computerVisionTask != null && computerVisionTask.IsCompleted)
            {
                // draw video frame with target overlay
                lastTargets = computerVisionTask.Result;
                if (lastTargets.Any())
                {
                    lastHit = DateTime.Now;
                }

                computerVisionTask = null;
            }
            else if (computerVisionTask == null)
            {
                // start the computer vision background task
                ReadOnlySpan<byte> span = MemoryMarshal.Cast<Vec3b, byte>(data);
                SixLabors.ImageSharp.Image image =
                    SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Bgr24>(span, frameWidth, frameHeight);
                computerVisionTask = Task.Run(() => analyzer.Run(image));
            }

            TimeSpan timeSinceLastHit = DateTime.Now - lastHit;
            if (timeSinceLastHit.Seconds > 2)
            {
                lastTargets.Clear();
            }

            // draw video frame
            updatePreview.Invoke(bitmap, lastTargets);
        }
    }
}