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
    private readonly int _byteCount = 640 * 480 * 4;
    const int pixels = 640 * 480;
    private byte[] bytes = new byte[640 * 480 * 4];
    private int[] heightMap = new int[pixels];

    public int[] GetHeightMap()
    {
        return heightMap;
    }

    private int[] HeightInts(byte[] bytesFromSteam)
    {
        int[] output = new int[pixels];
        for (int i = 0; i < pixels; i++)
        {
            int newHeight = BitConverter.ToInt32(bytesFromSteam, i*4);
            output[i] = newHeight;
        }
        return output;
    }

    /// <summary>
    /// Setup socket connection.
    /// </summary>
    public void ConnectToTcpServer()
    {
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
            socketConnection = new TcpClient("192.168.1.199", 8888);
            var receivedBytes = new Byte[_byteCount];

            using (NetworkStream stream = socketConnection.GetStream())
            {
                while (true) //  loop over frames forever
                {
                    // Get a stream object for reading
                    int length;
                    int offset = 0;
                    // Read one frame of bytes.length
                    while ((length = stream.Read(receivedBytes, offset, bytes.Length - offset)) != 0)
                    {
                        offset += length;
                    }

                    // Debug.Log("server message received as: " + Encoding.ASCII.GetString(receivedBytes));
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
