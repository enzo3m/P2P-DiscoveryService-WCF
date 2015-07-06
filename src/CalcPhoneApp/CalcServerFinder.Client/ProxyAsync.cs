﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This code was auto-generated by SlSvcUtil, version 5.0.61118.0
// 
namespace CalcServerFinder.Contracts
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SearchOptions", Namespace="http://schemas.datacontract.org/2004/07/CalcServerFinder.Contracts")]
    public partial class SearchOptions : object
    {
        
        private string NameField;
        
        private string VersionField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name
        {
            get
            {
                return this.NameField;
            }
            set
            {
                this.NameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Version
        {
            get
            {
                return this.VersionField;
            }
            set
            {
                this.VersionField = value;
            }
        }
    }
}


[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(ConfigurationName="IProcessingServiceFinder")]
public interface IProcessingServiceFinder
{
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IProcessingServiceFinder/Search", ReplyAction="http://tempuri.org/IProcessingServiceFinder/SearchResponse")]
    System.IAsyncResult BeginSearch(CalcServerFinder.Contracts.SearchOptions options, System.AsyncCallback callback, object asyncState);
    
    [return: System.ServiceModel.MessageParameterAttribute(Name="addresses")]
    System.Uri[] EndSearch(System.IAsyncResult result);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface IProcessingServiceFinderChannel : IProcessingServiceFinder, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class SearchCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
{
    
    private object[] results;
    
    public SearchCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState)
    {
        this.results = results;
    }
    
    public System.Uri[] Result
    {
        get
        {
            base.RaiseExceptionIfNecessary();
            return ((System.Uri[])(this.results[0]));
        }
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class ProcessingServiceFinderClient : System.ServiceModel.ClientBase<IProcessingServiceFinder>, IProcessingServiceFinder
{
    
    private BeginOperationDelegate onBeginSearchDelegate;
    
    private EndOperationDelegate onEndSearchDelegate;
    
    private System.Threading.SendOrPostCallback onSearchCompletedDelegate;
    
    private BeginOperationDelegate onBeginOpenDelegate;
    
    private EndOperationDelegate onEndOpenDelegate;
    
    private System.Threading.SendOrPostCallback onOpenCompletedDelegate;
    
    private BeginOperationDelegate onBeginCloseDelegate;
    
    private EndOperationDelegate onEndCloseDelegate;
    
    private System.Threading.SendOrPostCallback onCloseCompletedDelegate;
    
    public ProcessingServiceFinderClient()
    {
    }
    
    public ProcessingServiceFinderClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
    {
    }
    
    public ProcessingServiceFinderClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public ProcessingServiceFinderClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public ProcessingServiceFinderClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
    {
    }
    
    public System.Net.CookieContainer CookieContainer
    {
        get
        {
            System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
            if ((httpCookieContainerManager != null))
            {
                return httpCookieContainerManager.CookieContainer;
            }
            else
            {
                return null;
            }
        }
        set
        {
            System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
            if ((httpCookieContainerManager != null))
            {
                httpCookieContainerManager.CookieContainer = value;
            }
            else
            {
                throw new System.InvalidOperationException("Unable to set the CookieContainer. Please make sure the binding contains an HttpC" +
                        "ookieContainerBindingElement.");
            }
        }
    }
    
    public event System.EventHandler<SearchCompletedEventArgs> SearchCompleted;
    
    public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> OpenCompleted;
    
    public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> CloseCompleted;
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    System.IAsyncResult IProcessingServiceFinder.BeginSearch(CalcServerFinder.Contracts.SearchOptions options, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginSearch(options, callback, asyncState);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    System.Uri[] IProcessingServiceFinder.EndSearch(System.IAsyncResult result)
    {
        return base.Channel.EndSearch(result);
    }
    
    private System.IAsyncResult OnBeginSearch(object[] inValues, System.AsyncCallback callback, object asyncState)
    {
        CalcServerFinder.Contracts.SearchOptions options = ((CalcServerFinder.Contracts.SearchOptions)(inValues[0]));
        return ((IProcessingServiceFinder)(this)).BeginSearch(options, callback, asyncState);
    }
    
    private object[] OnEndSearch(System.IAsyncResult result)
    {
        System.Uri[] retVal = ((IProcessingServiceFinder)(this)).EndSearch(result);
        return new object[] {
                retVal};
    }
    
    private void OnSearchCompleted(object state)
    {
        if ((this.SearchCompleted != null))
        {
            InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
            this.SearchCompleted(this, new SearchCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
        }
    }
    
    public void SearchAsync(CalcServerFinder.Contracts.SearchOptions options)
    {
        this.SearchAsync(options, null);
    }
    
    public void SearchAsync(CalcServerFinder.Contracts.SearchOptions options, object userState)
    {
        if ((this.onBeginSearchDelegate == null))
        {
            this.onBeginSearchDelegate = new BeginOperationDelegate(this.OnBeginSearch);
        }
        if ((this.onEndSearchDelegate == null))
        {
            this.onEndSearchDelegate = new EndOperationDelegate(this.OnEndSearch);
        }
        if ((this.onSearchCompletedDelegate == null))
        {
            this.onSearchCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnSearchCompleted);
        }
        base.InvokeAsync(this.onBeginSearchDelegate, new object[] {
                    options}, this.onEndSearchDelegate, this.onSearchCompletedDelegate, userState);
    }
    
    private System.IAsyncResult OnBeginOpen(object[] inValues, System.AsyncCallback callback, object asyncState)
    {
        return ((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(callback, asyncState);
    }
    
    private object[] OnEndOpen(System.IAsyncResult result)
    {
        ((System.ServiceModel.ICommunicationObject)(this)).EndOpen(result);
        return null;
    }
    
    private void OnOpenCompleted(object state)
    {
        if ((this.OpenCompleted != null))
        {
            InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
            this.OpenCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
        }
    }
    
    public void OpenAsync()
    {
        this.OpenAsync(null);
    }
    
    public void OpenAsync(object userState)
    {
        if ((this.onBeginOpenDelegate == null))
        {
            this.onBeginOpenDelegate = new BeginOperationDelegate(this.OnBeginOpen);
        }
        if ((this.onEndOpenDelegate == null))
        {
            this.onEndOpenDelegate = new EndOperationDelegate(this.OnEndOpen);
        }
        if ((this.onOpenCompletedDelegate == null))
        {
            this.onOpenCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnOpenCompleted);
        }
        base.InvokeAsync(this.onBeginOpenDelegate, null, this.onEndOpenDelegate, this.onOpenCompletedDelegate, userState);
    }
    
    private System.IAsyncResult OnBeginClose(object[] inValues, System.AsyncCallback callback, object asyncState)
    {
        return ((System.ServiceModel.ICommunicationObject)(this)).BeginClose(callback, asyncState);
    }
    
    private object[] OnEndClose(System.IAsyncResult result)
    {
        ((System.ServiceModel.ICommunicationObject)(this)).EndClose(result);
        return null;
    }
    
    private void OnCloseCompleted(object state)
    {
        if ((this.CloseCompleted != null))
        {
            InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
            this.CloseCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
        }
    }
    
    public void CloseAsync()
    {
        this.CloseAsync(null);
    }
    
    public void CloseAsync(object userState)
    {
        if ((this.onBeginCloseDelegate == null))
        {
            this.onBeginCloseDelegate = new BeginOperationDelegate(this.OnBeginClose);
        }
        if ((this.onEndCloseDelegate == null))
        {
            this.onEndCloseDelegate = new EndOperationDelegate(this.OnEndClose);
        }
        if ((this.onCloseCompletedDelegate == null))
        {
            this.onCloseCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnCloseCompleted);
        }
        base.InvokeAsync(this.onBeginCloseDelegate, null, this.onEndCloseDelegate, this.onCloseCompletedDelegate, userState);
    }
    
    protected override IProcessingServiceFinder CreateChannel()
    {
        return new ProcessingServiceFinderClientChannel(this);
    }
    
    private class ProcessingServiceFinderClientChannel : ChannelBase<IProcessingServiceFinder>, IProcessingServiceFinder
    {
        
        public ProcessingServiceFinderClientChannel(System.ServiceModel.ClientBase<IProcessingServiceFinder> client) : 
                base(client)
        {
        }
        
        public System.IAsyncResult BeginSearch(CalcServerFinder.Contracts.SearchOptions options, System.AsyncCallback callback, object asyncState)
        {
            object[] _args = new object[1];
            _args[0] = options;
            System.IAsyncResult _result = base.BeginInvoke("Search", _args, callback, asyncState);
            return _result;
        }
        
        public System.Uri[] EndSearch(System.IAsyncResult result)
        {
            object[] _args = new object[0];
            System.Uri[] _result = ((System.Uri[])(base.EndInvoke("Search", _args, result)));
            return _result;
        }
    }
}