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
    private byte[] bytes;
    private Texture2D tex;
    private readonly int _byteCount = 640 * 480;

    public void Start()
    {
        // Create a 16x16 texture with PVRTC RGBA4 format
        // and fill it with raw PVRTC bytes.
        tex = new Texture2D(640, 480, TextureFormat.R8, false);
        // Raw PVRTC4 data for a 16x16 texture. This format is four bits
        // per pixel, so data should be 16*16/2=128 bytes in size.
        // Texture that is encoded here is mostly green with some angular
        // blue and red lines.

        // Load data into the texture and upload it to the GPU.

        // Assign texture to renderer's material.
        bytes = new byte[_byteCount];

        tex.LoadRawTextureData(bytes);
        tex.Apply();
        GetComponent<Renderer>().material.mainTexture = tex;
        ConnectToTcpServer();
    }

    /// <summary>
    /// Setup socket connection.
    /// </summary>
    private void ConnectToTcpServer()
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
            socketConnection = new TcpClient("192.168.1.30", 8888);
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
                    bytes = receivedBytes;
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    // Update is called once per frame
    void Update()
    {
        tex.LoadRawTextureData(bytes);
        tex.Apply();
    }

    private void OnDestroy()
    {
        clientReceiveThread.Abort();
        socketConnection.Close();
    }
}
