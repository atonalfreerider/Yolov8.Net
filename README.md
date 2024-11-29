# Yolov8.Net

https://github.com/sstainba/Yolov8.Net

This is a .NET interface for using Yolov5 and Yolov8 models on the ONNX runtime.

NOTE:  If you want to use the GPU, you must have BOTH the CUDA drivers AND CUDNN installed!!!!!!
       This was tested with cuDNN 9.3 + CUDA 11.8
       Loading the model is time consuming, so initial predictions will be slow.  Subsequent
       predictions will be significantly faster.

FORK:

Runs from a webcam and displays to an Avalonia window


# References

https://github.com/ultralytics/yolov8

https://github.com/mentalstack/yolov5-net
