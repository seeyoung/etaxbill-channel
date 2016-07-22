﻿//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OpenETaxBill.Channel.WcfReporter {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07", ConfigurationName="WcfReporter.IReportService")]
    public interface IReportService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Write" +
            "Log", ReplyAction="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Write" +
            "LogResponse")]
        void WriteLog(System.Guid p_certapp, string p_exception, string p_message);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Write" +
            "Log", ReplyAction="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Write" +
            "LogResponse")]
        System.Threading.Tasks.Task WriteLogAsync(System.Guid p_certapp, string p_exception, string p_message);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Repor" +
            "tWithDateRange", ReplyAction="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Repor" +
            "tWithDateRangeResponse")]
        int ReportWithDateRange(System.Guid p_certapp, string p_invoicerId, System.DateTime p_fromDay, System.DateTime p_tillDay);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Repor" +
            "tWithDateRange", ReplyAction="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Repor" +
            "tWithDateRangeResponse")]
        System.Threading.Tasks.Task<int> ReportWithDateRangeAsync(System.Guid p_certapp, string p_invoicerId, System.DateTime p_fromDay, System.DateTime p_tillDay);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Repor" +
            "tWithIssueIDs", ReplyAction="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Repor" +
            "tWithIssueIDsResponse")]
        int ReportWithIssueIDs(System.Guid p_certapp, string p_invoicerId, string[] p_issueIds);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Repor" +
            "tWithIssueIDs", ReplyAction="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Repor" +
            "tWithIssueIDsResponse")]
        System.Threading.Tasks.Task<int> ReportWithIssueIDsAsync(System.Guid p_certapp, string p_invoicerId, string[] p_issueIds);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Reque" +
            "stResult", ReplyAction="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Reque" +
            "stResultResponse")]
        bool RequestResult(System.Guid p_certapp, string p_submitId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Reque" +
            "stResult", ReplyAction="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Reque" +
            "stResultResponse")]
        System.Threading.Tasks.Task<bool> RequestResultAsync(System.Guid p_certapp, string p_submitId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Clear" +
            "XFlag", ReplyAction="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Clear" +
            "XFlagResponse")]
        int ClearXFlag(System.Guid p_certapp, string p_invoicerId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Clear" +
            "XFlag", ReplyAction="http://www.odinsoftware.co.kr/open/etaxbill/reporter/2016/07/IReportService/Clear" +
            "XFlagResponse")]
        System.Threading.Tasks.Task<int> ClearXFlagAsync(System.Guid p_certapp, string p_invoicerId);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IReportServiceChannel : OpenETaxBill.Channel.WcfReporter.IReportService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ReportServiceClient : System.ServiceModel.ClientBase<OpenETaxBill.Channel.WcfReporter.IReportService>, OpenETaxBill.Channel.WcfReporter.IReportService {
        
        public ReportServiceClient() {
        }
        
        public ReportServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ReportServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ReportServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ReportServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void WriteLog(System.Guid p_certapp, string p_exception, string p_message) {
            base.Channel.WriteLog(p_certapp, p_exception, p_message);
        }
        
        public System.Threading.Tasks.Task WriteLogAsync(System.Guid p_certapp, string p_exception, string p_message) {
            return base.Channel.WriteLogAsync(p_certapp, p_exception, p_message);
        }
        
        public int ReportWithDateRange(System.Guid p_certapp, string p_invoicerId, System.DateTime p_fromDay, System.DateTime p_tillDay) {
            return base.Channel.ReportWithDateRange(p_certapp, p_invoicerId, p_fromDay, p_tillDay);
        }
        
        public System.Threading.Tasks.Task<int> ReportWithDateRangeAsync(System.Guid p_certapp, string p_invoicerId, System.DateTime p_fromDay, System.DateTime p_tillDay) {
            return base.Channel.ReportWithDateRangeAsync(p_certapp, p_invoicerId, p_fromDay, p_tillDay);
        }
        
        public int ReportWithIssueIDs(System.Guid p_certapp, string p_invoicerId, string[] p_issueIds) {
            return base.Channel.ReportWithIssueIDs(p_certapp, p_invoicerId, p_issueIds);
        }
        
        public System.Threading.Tasks.Task<int> ReportWithIssueIDsAsync(System.Guid p_certapp, string p_invoicerId, string[] p_issueIds) {
            return base.Channel.ReportWithIssueIDsAsync(p_certapp, p_invoicerId, p_issueIds);
        }
        
        public bool RequestResult(System.Guid p_certapp, string p_submitId) {
            return base.Channel.RequestResult(p_certapp, p_submitId);
        }
        
        public System.Threading.Tasks.Task<bool> RequestResultAsync(System.Guid p_certapp, string p_submitId) {
            return base.Channel.RequestResultAsync(p_certapp, p_submitId);
        }
        
        public int ClearXFlag(System.Guid p_certapp, string p_invoicerId) {
            return base.Channel.ClearXFlag(p_certapp, p_invoicerId);
        }
        
        public System.Threading.Tasks.Task<int> ClearXFlagAsync(System.Guid p_certapp, string p_invoicerId) {
            return base.Channel.ClearXFlagAsync(p_certapp, p_invoicerId);
        }
    }
}
