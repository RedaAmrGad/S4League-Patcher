using S4Launcher.Security;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;

namespace S4Launcher.LoginAPI
{
    public class StateObject
    {
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public Socket WorkSocket;
    }

    internal static class LoginClient
    {
        private const short Magic = 2002;

        private static Socket _netSocket;

        private static bool _connected;

        //not actually needed 
        private static readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);

        private static readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);

        private static readonly ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);


        public static void UpdateLabel(string msg)
        {
            Constants.LoginWindow.UpdateLabel(msg);
        }

        public static void Connect(IPEndPoint localEndPoint)
        {
            Constants.LoginWindow.UpdateLabel("Connecting..");
            var sck =
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    LingerState = { Enabled = false }
                };
            _connected = false;
            try
            {
                sck.BeginConnect(localEndPoint, ConnectCallback, sck);
                _netSocket = sck;
                var timer = new Thread(() =>
                {
                    Thread.Sleep(15000);
                    if (!_connected)
                    {
                        sck.Close();
                        Constants.LoginWindow.UpdateLabel("Authentication Timeout.");
                    }
                });
                timer.Start();
            }
            catch (Exception)
            {
                Constants.LoginWindow.LoginError("A Connection Error Occured , Try Again");

            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;

                client.EndConnect(ar);
                ConnectDone.Set();
                var state = new StateObject { WorkSocket = client };

                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    ReceiveCallback, state);
            }

            catch (Exception)
            {
                Constants.LoginWindow.LoginError("A Connection Error Occured , Try Again");
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var state = (StateObject)ar.AsyncState;
                var client = state.WorkSocket;
                var bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    var msg = new EMessage(state.Buffer, bytesRead);
                    short magic = 0;
                    ByteArray packet = new EMessage();
                    if (msg.Read(ref magic)
                        && magic == Magic
                        && msg.Read(ref packet))
                    {
                        EMessage.MessageType coreId = 0;
                        var message = new EMessage(packet);
                        message.Read(ref coreId);

                        switch (coreId)
                        {
                            case EMessage.MessageType.Notify:
                                {
                                    string username = Constants.LoginWindow.GetUsername();
                                    string password = Constants.LoginWindow.GetPassword();
                                    string hwid = ThumbPrint.FingerPrint == string.Empty ? ThumbPrint.Value() : ThumbPrint.FingerPrint;
                                    string secretkey = "S4PV1I2TH3B32T";

                                    var sendmsg = new EMessage();

                                    sendmsg.Write(secretkey);
                                    sendmsg.Write(hwid);
                                    sendmsg.Write(username);
                                    sendmsg.Write(password);

                                    RmiSend(client, 15, sendmsg);
                                    Constants.LoginWindow.UpdateLabel("Authenticating..");
                                    break;
                                }
                            case EMessage.MessageType.Rmi:
                                {
                                    short rmiId = 0;
                                    if (!message.Read(ref rmiId))
                                        Constants.LoginWindow.LoginError("Error, corrupted RMI");
                                    else
                                        switch (rmiId)
                                        {
                                            case 16:
                                                {
                                                    var success = false;
                                                    if (message.Read(ref success) && success)
                                                    {
                                                        ReceiveDone.Set();
                                                        var code = "";
                                                        message.Read(ref code);
                                                        Constants.LoginWindow.UpdateLabel("Login success");
                                                        Constants.LoginWindow.Start(code);
                                                        _connected = true;
                                                        var sendmsg = new EMessage();
                                                        RmiSend(client, 17, sendmsg);
                                                        client.Disconnect(false);
                                                        client.Close();
                                                    }
                                                    else
                                                    {
                                                        ReceiveDone.Set();
                                                        var errcode = "";
                                                        message.Read(ref errcode);
                                                        Constants.LoginWindow.LoginError($"Error: {errcode}");
                                                        _connected = true;
                                                        client.Disconnect(false);
                                                        client.Close();
                                                    }
                                                }
                                                break;
                                            default:
                                                Constants.LoginWindow.LoginError($"Incorrect RMI: {rmiId}");
                                                break;
                                        }
                                    break;
                                }
                            case EMessage.MessageType.Encrypted:
                                {
                                    break;
                                }
                        }

                        if (!_connected)
                            client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                                ReceiveCallback, state);
                    }
                    else
                    {
                        ReceiveDone.Set();
                        client.Disconnect(true);
                    }
                }
            }
            catch (Exception)
            {
                Constants.LoginWindow.LoginError("A Connection Error Occured , Try Again");
            }
        }

        private static void RmiSend(Socket handler, short rmiId, EMessage msg)
        {
            var rmiframe = new EMessage();
            rmiframe.Write(EMessage.MessageType.Rmi);
            rmiframe.Write(rmiId);
            rmiframe.Write(msg);
            Send(handler, rmiframe);
        }

        private static void Send(Socket handler, EMessage data)
        {
            try
            {
                var coreframe = new EMessage();
                coreframe.Write(Magic);
                coreframe.WriteScalar(data.Length);
                coreframe.Write(data);
                handler.BeginSend(coreframe.Buffer, 0, coreframe.WriteOffset, 0,
                    SendCallback, handler);
            }
            catch (Exception)
            {
                Constants.LoginWindow.LoginError("A Connection Error Occured , Try Again");
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                SendDone.Set();
            }
            catch (Exception)
            {
                Constants.LoginWindow.LoginError("A Connection Error Occured , Try Again");
            }
        }
    }
}
