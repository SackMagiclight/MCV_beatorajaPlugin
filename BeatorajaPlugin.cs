using System;
using System.IO;
using Plugin;
using SitePlugin;
using System.ComponentModel.Composition;
using System.IO.Pipes;
using System.Text;

namespace MCV_beatorajaPlugin
{
    [Export(typeof(IPlugin))]
    public class BeatorajaPlugin : IPlugin, IDisposable
    {

        public string Name { get { return "beatoraja連携"; } }
        public string Description { get { return "beatoraja連携"; } }
        public IPluginHost Host { get; set; }

        // named pipe
        private NamedPipeServerStream pipeServer;

        public virtual void OnLoaded()
        {
            pipeServer = new NamedPipeServerStream("beatoraja", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            Console.WriteLine("[PIPE SERVER] thread created");
            pipeServer.BeginWaitForConnection(ConnectCallback, null);
        }

        public void OnClosing()
        {
            pipeServer.Close();
        }

        public void OnMessageReceived(ISiteMessage message, IMessageMetadata messageMetadata)
        {
            if (pipeServer.IsConnected)
            {
                var (name, text) = PluginCommon.Tools.GetData(message);
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(text))
                {
                    byte[] sendMessage = Encoding.UTF8.GetBytes(text.ToString() + "\n");
                    pipeServer.BeginWrite(sendMessage, 0, sendMessage.Length, WriteCallback, null);
                }
                
            }
        }

        public void OnTopmostChanged(bool isTopmost)
        {

        }

        public void ShowSettingView()
        {

        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

        }

        private void ConnectCallback(IAsyncResult result)
        {
            Console.WriteLine("[PIPE SERVER] Client Connected");
            pipeServer.EndWaitForConnection(result);
            if (pipeServer.IsConnected)
            {
                byte[] message = Encoding.UTF8.GetBytes("START\n");
                pipeServer.BeginWrite(message, 0, message.Length, WriteCallback, null);
            }
        }

        private void WriteCallback(IAsyncResult ar)
        {
            pipeServer.EndWrite(ar);
        }

    }
}
