using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class LimsPayload { 
    
            public int id_property { get; set; }
            public string ssourceaccountnumber { get; set; }
            public object updatedsourceaccountnumber { get; set; }
            public string billingaccountnumber { get; set; }
            public int id_townshipext { get; set; }
            public int id_townshipname { get; set; }
            public object id_sectionalscheme { get; set; }
            public string regionname { get; set; }
            public string ward { get; set; }
            public string spropertytype { get; set; }
            public string propertytypecode { get; set; }
            public bool bssexclusiveusearea { get; set; }
            public string idpropertytypeclass { get; set; }
            public object ivirtualpropertynumber { get; set; }
            public string townshipextname { get; set; }
            public string townshipname { get; set; }
            public int townshipextensionnumber { get; set; }
            public string erfnumber { get; set; }
            public string portion { get; set; }
            public string re { get; set; }
            public object ssuburbname { get; set; }
            public object sestatename { get; set; }
            public object ssname { get; set; }
            public string unit { get; set; }
            public object sectionalschemenumber { get; set; }
            public double effectiveextents { get; set; }
            public double deedextents { get; set; }
            public double financialextents { get; set; }
            public double sgextents { get; set; }
            public double gisextents { get; set; }
            public object excludedextents { get; set; }
            public object constraintpercent { get; set; }
            public string currentmarketvalue { get; set; }
            public object proposedmarketvalue { get; set; }
            public string streetaddress { get; set; }
            public string townshiptypecode { get; set; }
            public string tariffname { get; set; }
            public string tariffcode { get; set; }
            public string categoryname { get; set; }
            public string categorycode { get; set; }
            public string landusename { get; set; }
            public string landusecode { get; set; }
            public string szonename { get; set; }
            public string szonecode { get; set; }
            public string registeredownernames { get; set; }
            public string registeredownerids { get; set; }
            public string titledeednumbers { get; set; }
            public string billingidentifier { get; set; }
            public string lpi { get; set; }
            public string spropertysource { get; set; }
            public string idpropertystate { get; set; }
            public bool binjurisdiction { get; set; }
            public object improvementscount { get; set; }
            public object improvementextents { get; set; }
            public double locationlatitude { get; set; }
            public double locationlongitude { get; set; }
            public object basepropertytype { get; set; }
            public string regioncode { get; set; }

        public string Status { get; set; }
        public string Title { get; set; }
        public string StatusKey { get; set; }
    }

 
        
    
    

public class SolarERP
{
    public Header Header { get; set; }
    public List<LimsPayload> Payload { get; set; }
}

public class Root
{
    public SolarERP SolarERP { get; set; }
}

public class Message
{
    public string Name { get; set; }
    public string Type { get; set; }
}

public class Result
{
    public string Status { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorDescription { get; set; }
}

//public class Property
//{
//    public string Name { get; set; }
//    public string Value { get; set; }
//}

//public class Context
//{
//    public Property Property { get; set; }
//}

public class Header
{
    public string Version { get; set; }
    public DateTime TimeStamp { get; set; }
    public string CorrelationID { get; set; }
    public string TransactionID { get; set; }
    public string MessageID { get; set; }
    public string SenderID { get; set; }
    public string RecipientID { get; set; }
    public string Action { get; set; }
    public string SolarUser { get; set; }
    public Message Message { get; set; }
    public Result Result { get; set; }
    //public List<Context> Context { get; set; }
}

}