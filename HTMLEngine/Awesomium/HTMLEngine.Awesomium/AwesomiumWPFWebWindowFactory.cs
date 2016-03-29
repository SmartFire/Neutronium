﻿using System.Threading;
using Awesomium.Core;
using HTML_WPF.Component;

namespace HTMLEngine.Awesomium
{
    public  class AwesomiumWPFWebWindowFactory : IWPFWebWindowFactory
    {
        private static WebConfig _WebConfig = new WebConfig() { RemoteDebuggingPort = 8001, RemoteDebuggingHost = "127.0.0.1" };

        private static WebSession _Session = null;

        static AwesomiumWPFWebWindowFactory()
        {
            WebCore.Started += (o, e) => { WebCoreThread = Thread.CurrentThread; };

            if (!WebCore.IsInitialized)
                WebCore.Initialize(_WebConfig);
        }

        public static Thread WebCoreThread { get; internal set; }

        public AwesomiumWPFWebWindowFactory(string iWebSessionPath=null)
        {
            if (_Session == null)
            {
                _Session = (iWebSessionPath != null) ?
                            WebCore.CreateWebSession(iWebSessionPath, new WebPreferences()) :
                            WebCore.CreateWebSession(new WebPreferences());
            }
        }

        public string EngineName
        {
            get { return "Chromium 19"; }
        }

        public string Name
        {
            get { return "Awesomium"; }
        }

        public IWPFWebWindow Create()
        {
            return new AwesomiumWPFWebWindow(_Session);
        }

        public int? GetRemoteDebuggingPort()
        {
            return _WebConfig.RemoteDebuggingPort;
        }

        public void Dispose()
        {
            if (_Session != null)
                _Session.Dispose();

            WebCore.Shutdown();
        }
    }
}
