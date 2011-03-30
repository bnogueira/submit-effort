using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Net;

namespace SubmitEffortNamespace
{
    public class Http
    {
	public static bool HttpPost(string username, string password, string uri, string request, out string response)
	{
	    WebRequest webRequest = WebRequest.Create (uri);
	    webRequest.Credentials = new NetworkCredential(username, password); 
	    webRequest.ContentType = "application/x-www-form-urlencoded";
	    webRequest.Method = "POST";
	    byte[] bytes = Encoding.ASCII.GetBytes (request);
	    Stream os = null;
	    response = null;

	    try
	    { 
		// send the Post
		webRequest.ContentLength = bytes.Length;   //Count bytes to send
		os = webRequest.GetRequestStream();
		os.Write (bytes, 0, bytes.Length);         //Send it
	    }
	    catch (WebException ex)
	    {
		Console.WriteLine("Error posting the request:");
		Console.WriteLine(ex.Message);
		return false;
	    }
	    finally
	    {
		if (os != null)
		{
		    os.Close();
		}
	    }
 
	    try
	    { 
		// get the response
		WebResponse webResponse = webRequest.GetResponse();
		if (webResponse == null) 
		{ 
		    return true; 
		}

		// read response
		StreamReader sr = new StreamReader (webResponse.GetResponseStream());
		response =  sr.ReadToEnd().Trim();
	    }
	    catch (WebException ex)
	    {
		Console.WriteLine("PMS Error:");
		Console.WriteLine(ex.Message);
		return false;
	    }

	    return true;
	}
    }
}


















