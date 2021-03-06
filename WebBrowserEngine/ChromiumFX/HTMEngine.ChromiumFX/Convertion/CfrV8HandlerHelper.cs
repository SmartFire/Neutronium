﻿using System;
using Chromium.Remote;
using Neutronium.Core.WebBrowserEngine.JavascriptObject;

namespace Neutronium.WebBrowserEngine.ChromiumFx.Convertion 
{
    public static class CfrV8HandlerHelper 
    {
        public static CfrV8Handler Convert(this Action<string, IJavascriptObject, IJavascriptObject[]> function, string name)
        {
            var res = new CfrV8Handler();
            res.Execute += (o, e) => 
            {
                function(name, e.Object.Convert(), e.Arguments.Convert());    
            };
            return res;
        }
    }
}
