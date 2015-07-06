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
namespace CalcServer.Contracts
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TaskData", Namespace="http://schemas.datacontract.org/2004/07/CalcServer.Contracts")]
    public partial class TaskData : object
    {
        
        private string ContentsField;
        
        private string NameField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Contents
        {
            get
            {
                return this.ContentsField;
            }
            set
            {
                this.ContentsField = value;
            }
        }
        
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
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ServiceFault", Namespace="http://schemas.datacontract.org/2004/07/CalcServer.Contracts")]
    public partial class ServiceFault : object
    {
        
        private CalcServer.Contracts.ServiceFaultCode CodeField;
        
        private string IdField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public CalcServer.Contracts.ServiceFaultCode Code
        {
            get
            {
                return this.CodeField;
            }
            set
            {
                this.CodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Id
        {
            get
            {
                return this.IdField;
            }
            set
            {
                this.IdField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ServiceFaultCode", Namespace="http://schemas.datacontract.org/2004/07/CalcServer.Contracts")]
    public enum ServiceFaultCode : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Unknown = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TaskGenerateRequestIdFailed = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ReceiveTaskDataFailed = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TaskDataFormatError = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ComponentUnavailable = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TaskResultsNotFound = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        SendTaskResultsFailed = 6,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        InternalError = 7,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TaskState", Namespace="http://schemas.datacontract.org/2004/07/CalcServer.Contracts")]
    public enum TaskState : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        None = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Ready = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Started = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Completed = 3,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TaskResults", Namespace="http://schemas.datacontract.org/2004/07/CalcServer.Contracts")]
    public partial class TaskResults : object
    {
        
        private string ContentsField;
        
        private System.TimeSpan ElapsedTimeField;
        
        private CalcServer.Contracts.TaskErrorInfo[] EncounteredErrorsField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Contents
        {
            get
            {
                return this.ContentsField;
            }
            set
            {
                this.ContentsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.TimeSpan ElapsedTime
        {
            get
            {
                return this.ElapsedTimeField;
            }
            set
            {
                this.ElapsedTimeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public CalcServer.Contracts.TaskErrorInfo[] EncounteredErrors
        {
            get
            {
                return this.EncounteredErrorsField;
            }
            set
            {
                this.EncounteredErrorsField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TaskErrorInfo", Namespace="http://schemas.datacontract.org/2004/07/CalcServer.Contracts")]
    public partial class TaskErrorInfo : object
    {
        
        private CalcServer.Contracts.TaskErrorCode CodeField;
        
        private string DetailsField;
        
        private string IdField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public CalcServer.Contracts.TaskErrorCode Code
        {
            get
            {
                return this.CodeField;
            }
            set
            {
                this.CodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Details
        {
            get
            {
                return this.DetailsField;
            }
            set
            {
                this.DetailsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Id
        {
            get
            {
                return this.IdField;
            }
            set
            {
                this.IdField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TaskErrorCode", Namespace="http://schemas.datacontract.org/2004/07/CalcServer.Contracts")]
    public enum TaskErrorCode : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Unknown = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ComponentNotFound = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ComponentReadDataFailed = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ComponentProcessingFailed = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ComponentWriteResultFailed = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ComponentUnknownError = 5,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        InternalError = 6,
    }
}


[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(ConfigurationName="IProcessingService")]
public interface IProcessingService
{
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IProcessingService/QueryForResource", ReplyAction="http://tempuri.org/IProcessingService/QueryForResourceResponse")]
    System.IAsyncResult BeginQueryForResource(string name, string version, System.AsyncCallback callback, object asyncState);
    
    [return: System.ServiceModel.MessageParameterAttribute(Name="enabled")]
    bool EndQueryForResource(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IProcessingService/QueryForEnabledResources", ReplyAction="http://tempuri.org/IProcessingService/QueryForEnabledResourcesResponse")]
    System.IAsyncResult BeginQueryForEnabledResources(System.AsyncCallback callback, object asyncState);
    
    [return: System.ServiceModel.MessageParameterAttribute(Name="resources")]
    string[] EndQueryForEnabledResources(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IProcessingService/SubmitData", ReplyAction="http://tempuri.org/IProcessingService/SubmitDataResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(CalcServer.Contracts.ServiceFault), Action="http://tempuri.org/IProcessingService/SubmitDataServiceFaultFault", Name="ServiceFault", Namespace="http://schemas.datacontract.org/2004/07/CalcServer.Contracts")]
    System.IAsyncResult BeginSubmitData(CalcServer.Contracts.TaskData data, System.AsyncCallback callback, object asyncState);
    
    [return: System.ServiceModel.MessageParameterAttribute(Name="id")]
    string EndSubmitData(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IProcessingService/GetState", ReplyAction="http://tempuri.org/IProcessingService/GetStateResponse")]
    System.IAsyncResult BeginGetState(string id, System.AsyncCallback callback, object asyncState);
    
    [return: System.ServiceModel.MessageParameterAttribute(Name="state")]
    CalcServer.Contracts.TaskState EndGetState(System.IAsyncResult result);
    
    [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IProcessingService/GetResults", ReplyAction="http://tempuri.org/IProcessingService/GetResultsResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(CalcServer.Contracts.ServiceFault), Action="http://tempuri.org/IProcessingService/GetResultsServiceFaultFault", Name="ServiceFault", Namespace="http://schemas.datacontract.org/2004/07/CalcServer.Contracts")]
    System.IAsyncResult BeginGetResults(string id, System.AsyncCallback callback, object asyncState);
    
    [return: System.ServiceModel.MessageParameterAttribute(Name="results")]
    CalcServer.Contracts.TaskResults EndGetResults(System.IAsyncResult result);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface IProcessingServiceChannel : IProcessingService, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class QueryForResourceCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
{
    
    private object[] results;
    
    public QueryForResourceCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState)
    {
        this.results = results;
    }
    
    public bool Result
    {
        get
        {
            base.RaiseExceptionIfNecessary();
            return ((bool)(this.results[0]));
        }
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class QueryForEnabledResourcesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
{
    
    private object[] results;
    
    public QueryForEnabledResourcesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState)
    {
        this.results = results;
    }
    
    public string[] Result
    {
        get
        {
            base.RaiseExceptionIfNecessary();
            return ((string[])(this.results[0]));
        }
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class SubmitDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
{
    
    private object[] results;
    
    public SubmitDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState)
    {
        this.results = results;
    }
    
    public string Result
    {
        get
        {
            base.RaiseExceptionIfNecessary();
            return ((string)(this.results[0]));
        }
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class GetStateCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
{
    
    private object[] results;
    
    public GetStateCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState)
    {
        this.results = results;
    }
    
    public CalcServer.Contracts.TaskState Result
    {
        get
        {
            base.RaiseExceptionIfNecessary();
            return ((CalcServer.Contracts.TaskState)(this.results[0]));
        }
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class GetResultsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
{
    
    private object[] results;
    
    public GetResultsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState)
    {
        this.results = results;
    }
    
    public CalcServer.Contracts.TaskResults Result
    {
        get
        {
            base.RaiseExceptionIfNecessary();
            return ((CalcServer.Contracts.TaskResults)(this.results[0]));
        }
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class ProcessingServiceClient : System.ServiceModel.ClientBase<IProcessingService>, IProcessingService
{
    
    private BeginOperationDelegate onBeginQueryForResourceDelegate;
    
    private EndOperationDelegate onEndQueryForResourceDelegate;
    
    private System.Threading.SendOrPostCallback onQueryForResourceCompletedDelegate;
    
    private BeginOperationDelegate onBeginQueryForEnabledResourcesDelegate;
    
    private EndOperationDelegate onEndQueryForEnabledResourcesDelegate;
    
    private System.Threading.SendOrPostCallback onQueryForEnabledResourcesCompletedDelegate;
    
    private BeginOperationDelegate onBeginSubmitDataDelegate;
    
    private EndOperationDelegate onEndSubmitDataDelegate;
    
    private System.Threading.SendOrPostCallback onSubmitDataCompletedDelegate;
    
    private BeginOperationDelegate onBeginGetStateDelegate;
    
    private EndOperationDelegate onEndGetStateDelegate;
    
    private System.Threading.SendOrPostCallback onGetStateCompletedDelegate;
    
    private BeginOperationDelegate onBeginGetResultsDelegate;
    
    private EndOperationDelegate onEndGetResultsDelegate;
    
    private System.Threading.SendOrPostCallback onGetResultsCompletedDelegate;
    
    private BeginOperationDelegate onBeginOpenDelegate;
    
    private EndOperationDelegate onEndOpenDelegate;
    
    private System.Threading.SendOrPostCallback onOpenCompletedDelegate;
    
    private BeginOperationDelegate onBeginCloseDelegate;
    
    private EndOperationDelegate onEndCloseDelegate;
    
    private System.Threading.SendOrPostCallback onCloseCompletedDelegate;
    
    public ProcessingServiceClient()
    {
    }
    
    public ProcessingServiceClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
    {
    }
    
    public ProcessingServiceClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public ProcessingServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public ProcessingServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
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
    
    public event System.EventHandler<QueryForResourceCompletedEventArgs> QueryForResourceCompleted;
    
    public event System.EventHandler<QueryForEnabledResourcesCompletedEventArgs> QueryForEnabledResourcesCompleted;
    
    public event System.EventHandler<SubmitDataCompletedEventArgs> SubmitDataCompleted;
    
    public event System.EventHandler<GetStateCompletedEventArgs> GetStateCompleted;
    
    public event System.EventHandler<GetResultsCompletedEventArgs> GetResultsCompleted;
    
    public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> OpenCompleted;
    
    public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> CloseCompleted;
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    System.IAsyncResult IProcessingService.BeginQueryForResource(string name, string version, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginQueryForResource(name, version, callback, asyncState);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    bool IProcessingService.EndQueryForResource(System.IAsyncResult result)
    {
        return base.Channel.EndQueryForResource(result);
    }
    
    private System.IAsyncResult OnBeginQueryForResource(object[] inValues, System.AsyncCallback callback, object asyncState)
    {
        string name = ((string)(inValues[0]));
        string version = ((string)(inValues[1]));
        return ((IProcessingService)(this)).BeginQueryForResource(name, version, callback, asyncState);
    }
    
    private object[] OnEndQueryForResource(System.IAsyncResult result)
    {
        bool retVal = ((IProcessingService)(this)).EndQueryForResource(result);
        return new object[] {
                retVal};
    }
    
    private void OnQueryForResourceCompleted(object state)
    {
        if ((this.QueryForResourceCompleted != null))
        {
            InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
            this.QueryForResourceCompleted(this, new QueryForResourceCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
        }
    }
    
    public void QueryForResourceAsync(string name, string version)
    {
        this.QueryForResourceAsync(name, version, null);
    }
    
    public void QueryForResourceAsync(string name, string version, object userState)
    {
        if ((this.onBeginQueryForResourceDelegate == null))
        {
            this.onBeginQueryForResourceDelegate = new BeginOperationDelegate(this.OnBeginQueryForResource);
        }
        if ((this.onEndQueryForResourceDelegate == null))
        {
            this.onEndQueryForResourceDelegate = new EndOperationDelegate(this.OnEndQueryForResource);
        }
        if ((this.onQueryForResourceCompletedDelegate == null))
        {
            this.onQueryForResourceCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnQueryForResourceCompleted);
        }
        base.InvokeAsync(this.onBeginQueryForResourceDelegate, new object[] {
                    name,
                    version}, this.onEndQueryForResourceDelegate, this.onQueryForResourceCompletedDelegate, userState);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    System.IAsyncResult IProcessingService.BeginQueryForEnabledResources(System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginQueryForEnabledResources(callback, asyncState);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    string[] IProcessingService.EndQueryForEnabledResources(System.IAsyncResult result)
    {
        return base.Channel.EndQueryForEnabledResources(result);
    }
    
    private System.IAsyncResult OnBeginQueryForEnabledResources(object[] inValues, System.AsyncCallback callback, object asyncState)
    {
        return ((IProcessingService)(this)).BeginQueryForEnabledResources(callback, asyncState);
    }
    
    private object[] OnEndQueryForEnabledResources(System.IAsyncResult result)
    {
        string[] retVal = ((IProcessingService)(this)).EndQueryForEnabledResources(result);
        return new object[] {
                retVal};
    }
    
    private void OnQueryForEnabledResourcesCompleted(object state)
    {
        if ((this.QueryForEnabledResourcesCompleted != null))
        {
            InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
            this.QueryForEnabledResourcesCompleted(this, new QueryForEnabledResourcesCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
        }
    }
    
    public void QueryForEnabledResourcesAsync()
    {
        this.QueryForEnabledResourcesAsync(null);
    }
    
    public void QueryForEnabledResourcesAsync(object userState)
    {
        if ((this.onBeginQueryForEnabledResourcesDelegate == null))
        {
            this.onBeginQueryForEnabledResourcesDelegate = new BeginOperationDelegate(this.OnBeginQueryForEnabledResources);
        }
        if ((this.onEndQueryForEnabledResourcesDelegate == null))
        {
            this.onEndQueryForEnabledResourcesDelegate = new EndOperationDelegate(this.OnEndQueryForEnabledResources);
        }
        if ((this.onQueryForEnabledResourcesCompletedDelegate == null))
        {
            this.onQueryForEnabledResourcesCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnQueryForEnabledResourcesCompleted);
        }
        base.InvokeAsync(this.onBeginQueryForEnabledResourcesDelegate, null, this.onEndQueryForEnabledResourcesDelegate, this.onQueryForEnabledResourcesCompletedDelegate, userState);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    System.IAsyncResult IProcessingService.BeginSubmitData(CalcServer.Contracts.TaskData data, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginSubmitData(data, callback, asyncState);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    string IProcessingService.EndSubmitData(System.IAsyncResult result)
    {
        return base.Channel.EndSubmitData(result);
    }
    
    private System.IAsyncResult OnBeginSubmitData(object[] inValues, System.AsyncCallback callback, object asyncState)
    {
        CalcServer.Contracts.TaskData data = ((CalcServer.Contracts.TaskData)(inValues[0]));
        return ((IProcessingService)(this)).BeginSubmitData(data, callback, asyncState);
    }
    
    private object[] OnEndSubmitData(System.IAsyncResult result)
    {
        string retVal = ((IProcessingService)(this)).EndSubmitData(result);
        return new object[] {
                retVal};
    }
    
    private void OnSubmitDataCompleted(object state)
    {
        if ((this.SubmitDataCompleted != null))
        {
            InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
            this.SubmitDataCompleted(this, new SubmitDataCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
        }
    }
    
    public void SubmitDataAsync(CalcServer.Contracts.TaskData data)
    {
        this.SubmitDataAsync(data, null);
    }
    
    public void SubmitDataAsync(CalcServer.Contracts.TaskData data, object userState)
    {
        if ((this.onBeginSubmitDataDelegate == null))
        {
            this.onBeginSubmitDataDelegate = new BeginOperationDelegate(this.OnBeginSubmitData);
        }
        if ((this.onEndSubmitDataDelegate == null))
        {
            this.onEndSubmitDataDelegate = new EndOperationDelegate(this.OnEndSubmitData);
        }
        if ((this.onSubmitDataCompletedDelegate == null))
        {
            this.onSubmitDataCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnSubmitDataCompleted);
        }
        base.InvokeAsync(this.onBeginSubmitDataDelegate, new object[] {
                    data}, this.onEndSubmitDataDelegate, this.onSubmitDataCompletedDelegate, userState);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    System.IAsyncResult IProcessingService.BeginGetState(string id, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginGetState(id, callback, asyncState);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    CalcServer.Contracts.TaskState IProcessingService.EndGetState(System.IAsyncResult result)
    {
        return base.Channel.EndGetState(result);
    }
    
    private System.IAsyncResult OnBeginGetState(object[] inValues, System.AsyncCallback callback, object asyncState)
    {
        string id = ((string)(inValues[0]));
        return ((IProcessingService)(this)).BeginGetState(id, callback, asyncState);
    }
    
    private object[] OnEndGetState(System.IAsyncResult result)
    {
        CalcServer.Contracts.TaskState retVal = ((IProcessingService)(this)).EndGetState(result);
        return new object[] {
                retVal};
    }
    
    private void OnGetStateCompleted(object state)
    {
        if ((this.GetStateCompleted != null))
        {
            InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
            this.GetStateCompleted(this, new GetStateCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
        }
    }
    
    public void GetStateAsync(string id)
    {
        this.GetStateAsync(id, null);
    }
    
    public void GetStateAsync(string id, object userState)
    {
        if ((this.onBeginGetStateDelegate == null))
        {
            this.onBeginGetStateDelegate = new BeginOperationDelegate(this.OnBeginGetState);
        }
        if ((this.onEndGetStateDelegate == null))
        {
            this.onEndGetStateDelegate = new EndOperationDelegate(this.OnEndGetState);
        }
        if ((this.onGetStateCompletedDelegate == null))
        {
            this.onGetStateCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetStateCompleted);
        }
        base.InvokeAsync(this.onBeginGetStateDelegate, new object[] {
                    id}, this.onEndGetStateDelegate, this.onGetStateCompletedDelegate, userState);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    System.IAsyncResult IProcessingService.BeginGetResults(string id, System.AsyncCallback callback, object asyncState)
    {
        return base.Channel.BeginGetResults(id, callback, asyncState);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    CalcServer.Contracts.TaskResults IProcessingService.EndGetResults(System.IAsyncResult result)
    {
        return base.Channel.EndGetResults(result);
    }
    
    private System.IAsyncResult OnBeginGetResults(object[] inValues, System.AsyncCallback callback, object asyncState)
    {
        string id = ((string)(inValues[0]));
        return ((IProcessingService)(this)).BeginGetResults(id, callback, asyncState);
    }
    
    private object[] OnEndGetResults(System.IAsyncResult result)
    {
        CalcServer.Contracts.TaskResults retVal = ((IProcessingService)(this)).EndGetResults(result);
        return new object[] {
                retVal};
    }
    
    private void OnGetResultsCompleted(object state)
    {
        if ((this.GetResultsCompleted != null))
        {
            InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
            this.GetResultsCompleted(this, new GetResultsCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
        }
    }
    
    public void GetResultsAsync(string id)
    {
        this.GetResultsAsync(id, null);
    }
    
    public void GetResultsAsync(string id, object userState)
    {
        if ((this.onBeginGetResultsDelegate == null))
        {
            this.onBeginGetResultsDelegate = new BeginOperationDelegate(this.OnBeginGetResults);
        }
        if ((this.onEndGetResultsDelegate == null))
        {
            this.onEndGetResultsDelegate = new EndOperationDelegate(this.OnEndGetResults);
        }
        if ((this.onGetResultsCompletedDelegate == null))
        {
            this.onGetResultsCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetResultsCompleted);
        }
        base.InvokeAsync(this.onBeginGetResultsDelegate, new object[] {
                    id}, this.onEndGetResultsDelegate, this.onGetResultsCompletedDelegate, userState);
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
    
    protected override IProcessingService CreateChannel()
    {
        return new ProcessingServiceClientChannel(this);
    }
    
    private class ProcessingServiceClientChannel : ChannelBase<IProcessingService>, IProcessingService
    {
        
        public ProcessingServiceClientChannel(System.ServiceModel.ClientBase<IProcessingService> client) : 
                base(client)
        {
        }
        
        public System.IAsyncResult BeginQueryForResource(string name, string version, System.AsyncCallback callback, object asyncState)
        {
            object[] _args = new object[2];
            _args[0] = name;
            _args[1] = version;
            System.IAsyncResult _result = base.BeginInvoke("QueryForResource", _args, callback, asyncState);
            return _result;
        }
        
        public bool EndQueryForResource(System.IAsyncResult result)
        {
            object[] _args = new object[0];
            bool _result = ((bool)(base.EndInvoke("QueryForResource", _args, result)));
            return _result;
        }
        
        public System.IAsyncResult BeginQueryForEnabledResources(System.AsyncCallback callback, object asyncState)
        {
            object[] _args = new object[0];
            System.IAsyncResult _result = base.BeginInvoke("QueryForEnabledResources", _args, callback, asyncState);
            return _result;
        }
        
        public string[] EndQueryForEnabledResources(System.IAsyncResult result)
        {
            object[] _args = new object[0];
            string[] _result = ((string[])(base.EndInvoke("QueryForEnabledResources", _args, result)));
            return _result;
        }
        
        public System.IAsyncResult BeginSubmitData(CalcServer.Contracts.TaskData data, System.AsyncCallback callback, object asyncState)
        {
            object[] _args = new object[1];
            _args[0] = data;
            System.IAsyncResult _result = base.BeginInvoke("SubmitData", _args, callback, asyncState);
            return _result;
        }
        
        public string EndSubmitData(System.IAsyncResult result)
        {
            object[] _args = new object[0];
            string _result = ((string)(base.EndInvoke("SubmitData", _args, result)));
            return _result;
        }
        
        public System.IAsyncResult BeginGetState(string id, System.AsyncCallback callback, object asyncState)
        {
            object[] _args = new object[1];
            _args[0] = id;
            System.IAsyncResult _result = base.BeginInvoke("GetState", _args, callback, asyncState);
            return _result;
        }
        
        public CalcServer.Contracts.TaskState EndGetState(System.IAsyncResult result)
        {
            object[] _args = new object[0];
            CalcServer.Contracts.TaskState _result = ((CalcServer.Contracts.TaskState)(base.EndInvoke("GetState", _args, result)));
            return _result;
        }
        
        public System.IAsyncResult BeginGetResults(string id, System.AsyncCallback callback, object asyncState)
        {
            object[] _args = new object[1];
            _args[0] = id;
            System.IAsyncResult _result = base.BeginInvoke("GetResults", _args, callback, asyncState);
            return _result;
        }
        
        public CalcServer.Contracts.TaskResults EndGetResults(System.IAsyncResult result)
        {
            object[] _args = new object[0];
            CalcServer.Contracts.TaskResults _result = ((CalcServer.Contracts.TaskResults)(base.EndInvoke("GetResults", _args, result)));
            return _result;
        }
    }
}
