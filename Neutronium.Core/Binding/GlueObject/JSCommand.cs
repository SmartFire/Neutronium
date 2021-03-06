﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Neutronium.Core.Extension;
using Neutronium.Core.JavascriptFramework;
using Neutronium.Core.WebBrowserEngine.JavascriptObject;
using Neutronium.Core.WebBrowserEngine.Window;

namespace Neutronium.Core.Binding.GlueObject
{
    public class JSCommand : GlueBase, IJSObservableBridge
    {
        private readonly HTMLViewContext _HTMLViewContext;       
        private readonly IJavascriptToCSharpConverter _JavascriptToCSharpConverter;
        private readonly ICommand _Command;
        private IJavascriptObject _MappedJSValue;
        private int _Count = 1;

        public IJavascriptObject JSValue { get; }
        public IJavascriptObject MappedJSValue => _MappedJSValue;
        public object CValue => _Command;
        public JsCsGlueType Type => JsCsGlueType.Command;
        private IWebView WebView => _HTMLViewContext.WebView;
        private IDispatcher UIDispatcher => _HTMLViewContext.UIDispatcher;
        private IJavascriptViewModelUpdater ViewModelUpdater => _HTMLViewContext.ViewModelUpdater;

        public JSCommand(HTMLViewContext context, IJavascriptToCSharpConverter converter, ICommand command)
        {
            _JavascriptToCSharpConverter = converter;
            _HTMLViewContext = context;
            _Command = command;
       
            var canexecute = true;
            try
            {
                canexecute = _Command.CanExecute(null);
            }
            catch { }

            JSValue = WebView.Evaluate(() =>
                {
                    var res = WebView.Factory.CreateObject(true);
                    res.SetValue("CanExecuteValue", WebView.Factory.CreateBool(canexecute));
                    res.SetValue("CanExecuteCount", WebView.Factory.CreateInt(_Count)); 
                    return res;       
                });
        }

        public void ListenChanges()
        {
            _Command.CanExecuteChanged += Command_CanExecuteChanged;
        }

        public void UnListenChanges()
        {
            _Command.CanExecuteChanged -= Command_CanExecuteChanged;
        }

        private void ExecuteCommand(IJavascriptObject[] e)
        {
            UIDispatcher.RunAsync(() => _Command.Execute(_JavascriptToCSharpConverter.GetFirstArgumentOrNull(e)));
        }

        private void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            _Count = (_Count == 1) ? 2 : 1;
            WebView.RunAsync(() =>
            {
                UpdateProperty("CanExecuteCount", (f) => f.CreateInt(_Count));
            });
        }

        private void CanExecuteCommand(IJavascriptObject[] e)
        {
            var res = _Command.CanExecute(_JavascriptToCSharpConverter.GetFirstArgumentOrNull(e));
            UpdateProperty("CanExecuteValue", (f) => f.CreateBool(res));
        }

        private void UpdateProperty(string propertyName, Func<IJavascriptObjectFactory, IJavascriptObject> factory)
        {
            ViewModelUpdater.UpdateProperty(_MappedJSValue, propertyName, factory(WebView.Factory));
        }

        public void SetMappedJSValue(IJavascriptObject ijsobject)
        {
            _MappedJSValue = ijsobject;
            _MappedJSValue.Bind("Execute", WebView, ExecuteCommand);
            _MappedJSValue.Bind("CanExecute", WebView, CanExecuteCommand);
        }

        public IEnumerable<IJSCSGlue> GetChildren()
        {
            return Enumerable.Empty<IJSCSGlue>();
        }

        protected override void ComputeString(StringBuilder sb, HashSet<IJSCSGlue> alreadyComputed)
        {
            sb.Append("{}");
        }
    }
}
