using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class EDRMSViewModel
    {
    }
    public class EDRMData
    {
        public string accessToken { get; set; }
    }

    //public class EDRMAuth
    //{
    //    public int status { get; set; }
    //    public string message { get; set; }
    //    public Data data { get; set; }
    //}
    public class Datum
    {
        public string id { get; set; }
        public string documentType { get; set; }
        public string documentTypeMetaData { get; set; }
    }

    public class Documenttypes
    {
        public int status { get; set; }
        public string message { get; set; }
        public List<Datum> data { get; set; }
    }



    public class DocumentVM
    {
        public string DocumentName { get; set; }
        public string DocumentTypeID { get; set; }
        public byte[] File { get; set; }
        public string Filename { get; set; }
        public string MimeDocType { get; set; }
        public string Extension { get; set; }
        public string SystemName { get; set; }
        public string Department { get; set; }
        public string Author { get; set; }
        public string Organisation { get; set; }
        public string SubmittedBy { get; set; }
        public string DateSubmission { get; set; }
        public string DateReceived { get; set; }
        public string DateCreation { get; set; }
        public string DateTimeStamp { get; set; }
        public string DocumentTypeExtensionForm { get; set; }
    }

    public class UploadDoc
    {
        public string AccessToken { get; set; }
        public List<DocumentVM> DocumentsUpload { get; set; }
    }

    public class ResultDocs
    {
        public int status { get; set; }
        public string message { get; set; }
        public string data { get; set; }
    }

    public class GetDocument
    {
        public string AccessToken { get; set; }
        public string SystemName { get; set; }
        public string SearchByField { get; set; }
        public string SearchByValue { get; set; }
    }

    public class GetDocumentResult
    {
        public string documentID { get; set; }
        public string fileName { get; set; }
        public string documentName { get; set; }
        public int documentSize { get; set; }
        public string mimeDocType { get; set; }
        public byte[] content { get; set; }
        public string extension { get; set; }
        public string createdBy { get; set; }
        public DateTime createDate { get; set; }
        public string fullNames { get; set; }
        public string systemName { get; set; }
        public string department { get; set; }
        public string author { get; set; }
        public string organisation { get; set; }
        public string submittedBy { get; set; }
        public string dateSubmission { get; set; }
        public string dateReceived { get; set; }
        public string dateCreation { get; set; }
        public string dateTimeStamp { get; set; }
    }
    public class ResultData
    {
        public List<GetDocumentResult> getDocumentResults { get; set; }
    }
}