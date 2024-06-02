using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KittyHelpYouOut
{
    public class KittyCortUDPListener
    {
        private bool connected;
        private readonly IPEndPoint ipep;
        private readonly UdpClient client;
        private readonly Action<string> onReceiveCallback;
        private Coroutine listenCort;
        private readonly bool inited;
        
        private KittyCortUDPListener(){}

        public KittyCortUDPListener(IPEndPoint ipep, Action<string> callback)
        {
            
            inited = true;
            try
            {
                onReceiveCallback = callback;
                client = new UdpClient(ipep);
                this.ipep = ipep;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                inited = false;
            }
        }

        
    
        public KittyCortUDPListener StartListen()
        {
            if (inited)
            {
                connected = true;
                listenCort = KittyCoroutine.Instance.StartCoroutine(Listen(client,ipep,onReceiveCallback));
                Debug.Log($"{ipep.Address}:{ipep.Port} connected");
            }
            else
            {
                Debug.Log($"Listener not init");
            }
            return this;
        }

        public KittyCortUDPListener StopListen()
        {
            if (inited)
            {
                connected = false;
                KittyCoroutine.Instance.StopCoroutine(listenCort);
                client?.Close();
                Debug.Log($"{ipep.Address}:{ipep.Port} disconnected");
            }
            else
            {
                Debug.Log($"Listener not init");
            }
            return this;
        }


        private byte[] receive;
        private string convert;
        private IEnumerator Listen(UdpClient client, IPEndPoint ipep, Action<string> callback)
        {
            while (connected)
            {
                if (client.Available>0)
                {
                    receive = client.Receive(ref ipep);
                    convert = Encoding.UTF8.GetString(receive);
                    callback.Invoke(convert);
                }
                yield return null;
            }
        }
    }
}


