using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace KittyHelpYouOut
{
    public class KittyLoomUDPClient
    {
        private UdpClient udpMessageReceiver;
        private IPEndPoint iped;
        public string ip;
        public readonly string port;
        private bool isListening=false;
        public bool OnOff => isListening;
        private bool isInited = false;
        private bool enable = true;
        private Action<string> OnReceiveCallback;

        public KittyLoomUDPClient(string ip, string port)
        {
            this.ip = ip;
            this.port = port;
        }
        private KittyLoomUDPClient()
        {
        }

        public void Init(Action<string> callback)
        {
            OnReceiveCallback = callback;
            isInited = true;
            iped = new IPEndPoint(IPAddress.Any, int.Parse(port));
            udpMessageReceiver = new UdpClient(iped);
            Loom.RunAsync(ReceiveMessage);
        }
        
        public void StartListen()
        {
            if (isInited)
            {
                isListening = true;
            }
            else
            {
                Debug.LogError("KittyUDP not Inited!");
            }
        }
        public void StopListen()
        {
            if (isListening)
            {
                isListening = false;
            }
        }
        public void DiscardClient()
        {
            isListening = false;
            enable = false;
            udpMessageReceiver.Dispose();
        }
        private void ReceiveMessage()
        {
            while (enable)
            {
                if (isListening)
                {
                    byte[] receivedBytes = udpMessageReceiver.Receive(ref iped);
                    if (receivedBytes.Length>0)//如果接受信号长度不为0，进入信号内容判断
                    {
                        var newString= Encoding.UTF8.GetString(receivedBytes);
                        Loom.QueueOnMainThread(e =>
                        {
                            if(isListening)OnReceiveCallback?.Invoke((string)e);
                        },newString);
                    }
                }
            }
        }
    }
}