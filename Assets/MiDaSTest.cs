using System.Linq;
using UnityEngine;
using Unity.Barracuda;
using UI = UnityEngine.UI; 

sealed class MiDaSTest : MonoBehaviour
{
    public NNModel _model;
    public Texture2D _image;
    public Texture2D _depth;
    public UI.RawImage _imageView;
    public UI.RawImage _depthView;

    void Start()
    {
        // Convert the input image into a 1x256x256x3 tensor.
        using var input = new Tensor(1, 256, 256, 3);

        for (var y = 0; y < 256; y++)
        {
            for (var x = 0; x < 256; x++)
            {
                var tx = x * _image.width  / 256;
                var ty = y * _image.height / 256;
                input[0, 255 - y, x, 0] = _image.GetPixel(tx, ty).r;
                input[0, 255 - y, x, 1] = _image.GetPixel(tx, ty).g;
                input[0, 255 - y, x, 2] = _image.GetPixel(tx, ty).b;
            }
        }

        // Run the MiDaS model.
        using var worker = ModelLoader.Load(_model).CreateWorker(WorkerFactory.Device.CPU);

        worker.Execute(input);

        // Inspect the output tensor.
        var output = worker.PeekOutput();

        // 1, 1, 256, 256
        Debug.Log(output);

        for (var y = 0; y < 256; y++){
            for (var x = 0; x < 256; x++){
                // Debug.Log(x+y*256);
                // Debug.Log(output[0,0,x,y]/1000.0f);
                var tx = x * _depth.width  / 256;
                var ty = y * _depth.height / 256;
                Color color = new Color(0.0f, 0.0f, output[0,0,x,255-y]/1000.0f);
                _depth.SetPixel(tx, ty, color);
            }
        }

        _depth.Apply();

        // Show the results on the UI.
        _imageView.texture = _image;
        _depthView.texture = _depth;

    }
}
