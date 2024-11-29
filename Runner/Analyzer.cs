using Yolov8Net;
using Image = SixLabors.ImageSharp.Image;

namespace Runner;

public class Analyzer
{
    public class Target
    {
        public string Name { get; set; }
        public float Score { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Target(string name, float score, int x, int y, int width, int height)
        {
            Name = name;
            Score = score;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    readonly IPredictor yolo;
    
    public Analyzer()
    {
        // Create new Yolov8 predictor, specifying the model (in ONNX format)
        // If you are using a custom trained model, you can provide an array of labels. Otherwise, the standard Coco labels are used.
        string[] labels = { "item" };
        yolo = YoloV8Predictor.Create("./assets/best.onnx", labels, true);
    }

    public List<Target> Run(Image image)
    {
        Prediction[] predictions = yolo.Predict(image);

        List<Target> targets = new List<Target>();
        
        // Draw your boxes
        foreach (Prediction pred in predictions)
        {
            int originalImageHeight = image.Height;
            int originalImageWidth = image.Width;

            int x = Convert.ToInt32(Math.Max(pred.Rectangle.X, 0));
            int y = Convert.ToInt32(Math.Max(pred.Rectangle.Y, 0));
            int width = Convert.ToInt32(Math.Min(originalImageWidth - x, pred.Rectangle.Width));
            int height = Convert.ToInt32(Math.Min(originalImageHeight - y, pred.Rectangle.Height));

            targets.Add(new Target(pred.Label.Name, pred.Score, x, y, width, height));
        }

        return targets;
    }
}