using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel;
using System.Reflection;
using System.Data.Services.Client;
using System.Web;

namespace LAMSFinishingDA.Infrastructure
{
    public static class ServiceContext
    {
        public static dynamic SetContext(Type ReadEntityType, string UriConfigKey)
        {
            string AssemblyName = ReadEntityType.Namespace;
            Type ContextType = (from t in Assembly.GetExecutingAssembly().GetTypes()
                                where t.IsClass && t.Namespace == AssemblyName && t.Name.EndsWith("Entities")
                                select t).Single();
            Uri uri = new Uri(GetUriFromKey(UriConfigKey));
            dynamic serviceContext = Activator.CreateInstance(ContextType, new object[] { uri });
            LAMSCommonDataFunctions commonFunctionsDA = new LAMSCommonDataFunctions();
            serviceContext.Credentials = commonFunctionsDA.GetNetworkCredentials();
            serviceContext.SendingRequest += new EventHandler<SendingRequestEventArgs>(OnSendingRequest);
            return serviceContext;
        }
        private static void OnSendingRequest(object sender, SendingRequestEventArgs e)
        {
            if (e.Request.Method != "GET")
            {
            char[] urlSplitter = { '/', '?' };
            string url = HttpContext.Current.Request.Url.ToString();
            url = url.Substring(url.IndexOf("//") + 2);
            url = url.Substring(0, url.IndexOf("?") == -1 ? url.Length : url.IndexOf("?"));
            List<string> urlSegments = new List<string>(url.ToUpper().Split(urlSplitter));
            // Since Modified By field in DB is only of size 30, removing Application name from the list
            urlSegments.RemoveAt(0);
            e.RequestHeaders.Add("MVC_Screen", string.Join("_", urlSegments));
            }
        }
        public static string GetUriFromKey(string UriConfigKey)
        {
            string Environment = ConfigurationManager.AppSettings["Environment"].ToString();
            string uri;
            switch (Environment)
            {
                case GlobalConstants.ENVIRONMENT_Development:
                    UriConfigKey = UriConfigKey + GlobalConstants.ENVIRONMENT_Development;
                    uri = ConfigurationManager.AppSettings[UriConfigKey].ToString();
                    break;
                case GlobalConstants.ENVIRONMENT_Testing:
                    UriConfigKey = UriConfigKey + GlobalConstants.ENVIRONMENT_Testing;
                    uri = ConfigurationManager.AppSettings[UriConfigKey].ToString();
                    break;
                case GlobalConstants.ENVIRONMENT_Production:
                    UriConfigKey = UriConfigKey + GlobalConstants.ENVIRONMENT_Production;
                    uri = ConfigurationManager.AppSettings[UriConfigKey].ToString();
                    break;
                default:
                    UriConfigKey = UriConfigKey + GlobalConstants.ENVIRONMENT_Development;
                    uri = ConfigurationManager.AppSettings[UriConfigKey].ToString();
                    break;
            }
            return uri;
        }
        
    }
    
}
