using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class DepthView : MonoBehaviour
{
    private Thread clientReceiveThread;
    private TcpClient socketConnection;
    private Texture2D tex;
    const int Pixels = 640 * 480;
    const int ByteCount = Pixels * BytesDepth;
    private const int BytesDepth = 4;
    private const float MinDepth = 300f;
    private const float MaxDepth = 1100f;
    private float[] heightMap = new float[Pixels];
    private byte[] receivedBytes = new Byte[ByteCount];
    private Queue<int[]> buffer = new Queue<int[]>();
    private int FramesToAverageTarget = 1;

    public float[] GetHeightMap()
    {
        if (buffer.Count == 0) return heightMap;
        var framesToAverage = Math.Min(FramesToAverageTarget, buffer.Count);

        var frames = buffer.Take(framesToAverage).ToArray();

        for (int i = 0; i < Pixels; i++)
        {
            double depth = 0;
            for (int f = 0; f < framesToAverage; f++)
            {
                depth += frames[f][i];
            }

            heightMap[i] = MaxDepth - ((float) (depth / (framesToAverage)) - MinDepth);
        }
        return heightMap;
    }

    private int[] HeightInts(byte[] bytesFromSteam)
    {
        int[] output = new int[Pixels];
        byte[] pixelBytes = new byte[BytesDepth];
        for (int i = 0; i < Pixels; i++)
        {
            Array.Copy(bytesFromSteam, i * BytesDepth, pixelBytes, 0, BytesDepth);

            int newHeight = BitConverter.ToInt32(pixelBytes, 0);
            if (newHeight == 0)
            {
                newHeight = (int)MaxDepth;
            }

            output[i] = newHeight;
        }
        return output;
    }

    /// <summary>
    /// Setup socket connection.
    /// </summary>
    public void ConnectToTcpServer()
    {
        Debug.Log("bytes per frame "+ByteCount);
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    /// <summary>
    /// Runs in background clientReceiveThread; Listens for incomming data.
    /// </summary>
    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient("192.168.1.30", 8888);

            using (NetworkStream stream = socketConnection.GetStream())
            {
                while (true) //  loop over frames forever
                {
                    // Get a stream object for reading
                    int length;
                    int offset = 0;
                    // Read one frame of bytes.length
                    while ((length = stream.Read(receivedBytes, offset, ByteCount - offset)) != 0)
                    {
                        offset += length;
                    }

                    buffer.Enqueue(HeightInts(receivedBytes));
                    while (buffer.Count > 10) buffer.Dequeue();
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    // Update is called once per frame

    private void OnDestroy()
    {
        clientReceiveThread.Abort();
        socketConnection.Close();
    }
}
