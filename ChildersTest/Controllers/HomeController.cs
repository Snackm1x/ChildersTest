using System.Web.Mvc;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using System.IO;
using ChildersTest.Models;

namespace ChildersTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        //Supported browsers
        public ActionResult Browsers()
        {
            //Connect to server to get xml file
            string BrowserUrl = "https://jamesalexchilders.litmus.com/pages/clients.xml";
            WebResponse response = ConnectToServer(BrowserUrl);
            Stream responseStream = response.GetResponseStream();

            //Parse xml for browser client info and pass to the view
            XDocument xml = XDocument.Load(responseStream);
            var browsers = (from b in xml.Descendants("testing_application")
                            select new Browsers
                            {
                                BrowserName = b.Element("application_long_name").Value.ToString(),
                                AppCode = b.Element("application_code").Value.ToString(),
                                TestTime = b.Element("average_time_to_process").Value.ToString(),
                                CurrentStatus = b.Element("status").Value.ToString(),
                            }).ToList();
            return View(browsers);
        }

        //Supported email clients
        public ActionResult Clients()
        {
            //Connect to server to get xml file
            string ClientURL = "https://jamesalexchilders.litmus.com/emails/clients.xml";
            WebResponse response = ConnectToServer(ClientURL);
            Stream responseStream = response.GetResponseStream();

            //Parse xml for email client info and pass to the view
            XDocument xml = XDocument.Load(responseStream);
            var clients = (from c in xml.Descendants("testing_application")
                           orderby (string) c.Attribute("platform_name") descending
                           select new EmailClient
                           {
                               ClientName = c.Element("application_long_name").Value.ToString(),
                               AppCode = c.Element("application_code").Value.ToString(),
                               TestTime = c.Element("average_time_to_process").Value.ToString(),
                               CurrentStatus = c.Element("status").Value.ToString(),
                               PlatformName = c.Element("platform_name").Value.ToString()
                           }).ToList();

            clients.Sort((x, y) => x.PlatformName.CompareTo(y.PlatformName)); 
            ViewBag.Message = "List of email clients supported by Litmus.";

            return View(clients);
        }


        //Connects and authenticates on given Litmus url
        public WebResponse ConnectToServer(string url)
        {
            WebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/xml";
            request.Method = "GET";

            NetworkCredential creds = new NetworkCredential("jamesalexchilders", "theonly1");
            CredentialCache cache = new CredentialCache();

            cache.Add(new System.Uri(url), "Basic", creds);
            cache.Add(new System.Uri(url), "Negotiate", creds);
            request.Credentials = cache;
            WebResponse response = (HttpWebResponse)request.GetResponse();
            return response;

        }
    }
}