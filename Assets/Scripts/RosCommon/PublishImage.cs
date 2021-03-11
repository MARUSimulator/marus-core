using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Threading.Tasks;
using UnityEngine.Experimental.Rendering;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class PublishImage : MonoBehaviour
{

    public Camera StreamCamera;

    [EnumProperty(typeof(RosConfig), "Instance", "GetTopicList")]
    public string topicName;
    private GraphicsFormat format;
    public ROSConnection ros;

    
    private int frameIndex = 0;


    IEnumerator Start()
    {
        ros = ROSConnection.instance;
        yield return new WaitForSeconds(1);
        while (true)
        {
            yield return new WaitForEndOfFrame();

            AsyncGPUReadback.Request(StreamCamera.targetTexture, 0, TextureFormat.RGBA32, OnCompleteReadback);

            yield return new WaitForSeconds(5f);

            // TOO SLOW - CPU and GPU must sync and main thread waits for GPU to finish. Better to use AsyncGPUReadback!
            // var renderTexture = StreamCamera.targetTexture;
            // Texture2D tex2d = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

            // RenderTexture.active = renderTexture;
            // tex2d.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            // ROSConnection.instance.Send(topicName, new UInt8MultiArray() { data = tex2d.GetRawTextureData() });
        }
    }


    void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (!(request.done && !request.hasError))
        {
            Debug.Log("GPU readback error detected.");
            return;
        }

        byte[] array = request.GetData<byte>().ToArray();
        ROSConnection.instance.Send(topicName, new UInt8MultiArray() { data = array });
        // Task.Run(() =>
        // {
        //     // File.WriteAllBytes($"D:/Screenshots/Screenshot{frameIndex}.png",ImageConversion.EncodeArrayToPNG(array, format, (uint) Screen.width, (uint) Screen.height));
        // }).Wait();



        frameIndex++;
    }

    public static string[] GetTopicList() =>
        RosConfig.Instance.GetTopicList();

}
