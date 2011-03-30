using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Configuration;
using System.Web;
using System.Threading;


namespace caneco
{
    class SubmitEffort
    {
	private static int workersParameterIdx = 8;
        private static string uri = "http://pms.siscog.com:8080/pms/tasks-ms/tasks/working-periods/tasks-effort-register.php";
	private static string findUri = "http://pms.siscog.com:8080/pms/tasks-ms/tasks/lists/list-tasks.asp";
	
	static void Dump(List <string> data)
	{
	    foreach(string s in data)
	    {
		Console.WriteLine(s);
	    }
	}

	static string EncodeString(string source)
	{
	    Encoding eISOLatin = Encoding.GetEncoding(28591);
	    return HttpUtility.UrlEncode(source, eISOLatin);
	}

        static int Main(string[] args)
        {
	    List <string> effortActivitiesList = new List <string> (ConfigurationManager.AppSettings["effortActivities"].Split('|'));
	    List <string> workersList = new List <string> (ConfigurationManager.AppSettings["workers"].Split('|'));
	    bool simulationMode = string.IsNullOrEmpty(ConfigurationManager.AppSettings["SimulationMode"]) ? true : bool.Parse(ConfigurationManager.AppSettings["SimulationMode"]);
	    int sleepTime = string.IsNullOrEmpty(ConfigurationManager.AppSettings["SleepTime"]) ? 500 : Int32.Parse(ConfigurationManager.AppSettings["SleepTime"]);

    	    // parameter count validation
	    if (args.Length < 9)
	    {
		Console.WriteLine("Syntax error");
		Console.WriteLine("Usage: submit-effort.exe <username> <password> <taskId> <date> <effortActivityInput> <internalEffortDuration> <externalEffortDuration> <remarks> <workerName> [<workerName>*]"); //TODO

		Console.WriteLine(" - username ex: hvieira");
		Console.WriteLine(" - password ex: abcd123)");
		Console.WriteLine(" - taskId ex: POA.18013.0");
		Console.WriteLine(" - date ex: 2010/01/14");
		Console.WriteLine(" - effortActivityInput ex: Development");
		Console.WriteLine(" - internalEffortDuration ex: 0:00");
		Console.WriteLine(" - externalEffortDuration ex: 1:00");
		Console.WriteLine(" - remarks ex: \"Proposal Update\"");
		Console.WriteLine(" - workerName ex: \"Hugo Vieira\"");

		return 1;
	    }

	    string username = args[0];
	    string password = args[1];
	    string taskId = args[2];
	    string date = args[3];
	    string effortActivityInput = args[4];
	    string internalEffortDuration = args[5];
	    string externalEffortDuration = args[6];
	    string remarks = EncodeString(args[7]);

	    // date validation
	    DateTime effortDate;
	    if (!DateTime.TryParse(date, out effortDate))
	    {
		Console.WriteLine("<date> parameter invalid format, try YYYY/MM/DD instead.");
		return 1;
	    }

    	    // effort activity validation
	    if (!effortActivitiesList.Contains(effortActivityInput))
	    {
		Console.WriteLine("Invalid effortActivityInput, please select one of the options listed below:");
		Dump(effortActivitiesList);
		return 1;
	    }

	    // *do not be a spammer*
	    Thread.Sleep(sleepTime);

	    // task validation
	    if (!TaskExists(username, password, findUri, taskId))
	    {
		Console.WriteLine("Invalid taskId: {0}", taskId);
		return 1;
	    }
	    
	    // iterate workers
       	    string allWorkers = string.Empty;
	    string allWorkersCombos = string.Empty;

	    List <string> workers = new List <string>();
	    string worker;
	    for (int i = workersParameterIdx; i < args.Length; i++)
	    {
		// workers validation
		if (!workersList.Contains(args[i]))
		{
		    Console.WriteLine(string.Format("Invalid worker '{0}', please select one of the options listed below:", args[i]));
		    Dump(workersList);
		    return 1;
		}

		// prepare string
		if (i != workersParameterIdx)
		{
		    allWorkers = allWorkers + ',';
		}

		worker = EncodeString(args[i]);
		allWorkers = allWorkers + worker;
		allWorkersCombos = allWorkersCombos + "&worker_combo=" + worker;
	    }

	    // calculate total effort
	    TimeSpan t1;
    	    TimeSpan t2;
 	    TimeSpan totalEffort;

	    try
	    {
		t1 = TimeSpan.Parse(internalEffortDuration);
		t2 = TimeSpan.Parse(externalEffortDuration);
		totalEffort = t1.Add(t2);
	    }
	    catch(Exception ex)
	    {
		Console.WriteLine(string.Format("Unable to parse effort durations - {0}", ex.Message));
		return 1;
	    }
	    
	    // build request
	    string request = string.Format("form_name=tasks_view_work&user_name={0}&task_code_input={1}&action_input=insert+one&last_page=tasks-view-work.php&poa_group_code_input=-1&affected_workers_input={2}&date_input={3}&tbSelMonth={4}&tbSelYear={5}{6}&effort_activity_input={7}&internal_effort_input={8}&external_effort_input={9}&duration_input={10}&remarks={11}&time_out_snwh=0%3A00&time_out_snwh_in_cnwh=0%3A00&time_out_snwh_out_cnwh=0%3A00",
					   username, 
					   taskId, 
					   allWorkers, 
					   string.Format("{0}/{1}/{2}", effortDate.Year, effortDate.Month, effortDate.Day),
					   effortDate.Month,
					   effortDate.Year,
					   allWorkersCombos,
					   effortActivityInput,
					   internalEffortDuration,
					   externalEffortDuration,
					   string.Format("{0}:{1}", totalEffort.Hours, totalEffort.Minutes),
					   remarks);

	    // simulation mode
	    if (simulationMode)
	    {
		Console.WriteLine("CurrentTime: {0}", DateTime.Now);
		Console.WriteLine("username: {0}", username);
		Console.WriteLine("password: {0}", password);
		Console.WriteLine("uri: {0}", uri);
		Console.WriteLine("request: {0}", request);

		return 0;
	    }

	    string response = null;
	    if (HttpPost(username, password, uri, request, out response))
	    {
		Console.WriteLine("Done!");
		return 0;
	    }
	    else
	    {
		Console.WriteLine("Error occurred...");
		return 1;
	    }

	}

	static bool HttpPost(string username, string password, string uri, string request, out string response)
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

	static bool TaskExists(string username, string password, string findUri, string taskId)
	{
	    string request = string.Format("user_name={0}&object_name=Task&action_id=Find+Criteria&session_poa_query=&session_task_query=SELECT+DISTINCT+Task.Responsible%2C+Task.Code%2C+Task.Scope%2C+Task.Name%2C+Task.Project%2C+Task.State%2C+Task.DueDate%2C+Task.Level%2C+Task.SpecificType+FROM+%28Task+INNER+JOIN+TaskAssignment+ON+TaskAssignment.TaskCode%3DTask.Code%29+WHERE+%28%28Task.Code+%3D+%27{1}%27%29%29&session_mus_query=&session_criteria_conditions=Task+Code+%3D+{1}+AND&effort_query=&form_name=&id_input=&life_input=&muss_id_input=&effort_id_input=&last_page=load_criteria_page&task_code_input=&task_type_input=&specific_task_type_input=&scheduler_input=&poa_type_input=&task_scope_input=&external_input=0&choosed_criteria_name=",
					   username,
					   taskId);


	    string response = null;
	    bool result = HttpPost(username, password, findUri, request, out response);

	    return (result && !response.Contains("no records found!"));
	}
    }
}
