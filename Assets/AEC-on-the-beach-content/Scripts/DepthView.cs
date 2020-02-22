using System;
using System.Collections;
using System.Collections.Generic;
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
    private const int BytesDepth = 3;
    private int[] heightMap = new int[Pixels];
    private byte[] receivedBytes = new Byte[ByteCount +1];

    public int[] GetHeightMap()
    {
        return heightMap;
    }

    private int[] HeightInts(byte[] bytesFromSteam)
    {
        int[] output = new int[Pixels];
        for (int i = 0; i < Pixels; i++)
        {
            int newHeight = BitConverter.ToInt32(bytesFromSteam, i*BytesDepth);
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

                    //Debug.Log("received " + receivedBytes.Length);
                    heightMap = HeightInts(receivedBytes);
                    //Debug.Log("got frame: "+ offset);
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
