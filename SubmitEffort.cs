using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Configuration;
using System.Web;
using System.Threading;


namespace SubmitEffortNamespace
{
    class SubmitEffort
    {
        private static string uri = "http://pms.siscog.com:8080/pms/tasks-ms/tasks/working-periods/tasks-effort-register.php";
	private static string findUri = "http://pms.siscog.com:8080/pms/tasks-ms/tasks/lists/list-tasks.asp";
	

	static string EncodeString(string source)
	{
	    Encoding eISOLatin = Encoding.GetEncoding(28591);
	    return HttpUtility.UrlEncode(source, eISOLatin);
	}

	
	static int Main(string[] args)
	{
	    string value;

	    // app settings parameters
	    value = ConfigurationManager.AppSettings["SleepTime"];
	    int sleepTime = string.IsNullOrEmpty(value) ? 500 : Int32.Parse(value);

	    value = ConfigurationManager.AppSettings["SimulationMode"];
	    bool simulationMode = string.IsNullOrEmpty(value) ? true : bool.Parse(value);


	    // args structure definition
	    ParametersHandler parametersHandler = new ParametersHandler();

	    parametersHandler.AddDefinition("Username", new EmptyValidator());
	    parametersHandler.AddDefinition("Password", new EmptyValidator());
	    parametersHandler.AddDefinition("TaskId", new PmsTaskValidator(findUri, args.Length > 1 ? args[0] : "", args.Length > 2 ? args[1] : ""));
	    parametersHandler.AddDefinition("Date", new DateTimeValidator("yyyy/MM/dd"));
	    parametersHandler.AddDefinition("EffortActivityInput", new InListValidator(ConfigurationManager.AppSettings["effortActivities"], '|'));
	    parametersHandler.AddDefinition("InternalEffortDuration", new DateTimeValidator("hh:mm"));
	    parametersHandler.AddDefinition("ExternalEfforDuration", new DateTimeValidator("hh:mm"));
	    parametersHandler.AddDefinition("Remarks", new EmptyValidator());
	    parametersHandler.AddDefinition("WorkerName", new InListValidator(ConfigurationManager.AppSettings["workers"], '|'));

	    // args values
	    Dictionary<string, string> parameters = parametersHandler.GetParameters(args);
	    
	    if (parameters == null)
	    {
		Console.WriteLine(parametersHandler.GetErrorMessage());
		return 1;
	    }

	    // *do not be a spammer*
	    Thread.Sleep(sleepTime);

    	    // calculate needed values
	    string username = parameters["Username"];
	    string password = parameters["Password"];
	    string taskId = parameters["TaskId"];
	    string workerName = EncodeString(parameters["WorkerName"]);
	    string date = parameters["Date"];
	    string effortActivityInput = parameters["EffortActivityInput"];
    	    string internalEffortDuration = parameters["InternalEffortDuration"];
	    string externalEffortDuration = parameters["ExternalEffortDuration"];
	    string remarks = EncodeString(parameters["Remarks"]);
 	    TimeSpan totalEffort = TimeSpan.Parse(internalEffortDuration).Add(TimeSpan.Parse(externalEffortDuration));
	    DateTime effortDate = DateTime.Parse(parameters["Date"]);

	    // build request
	    string request = string.Format("form_name=tasks_view_work&user_name={0}&task_code_input={1}&action_input=insert+one&last_page=tasks-view-work.php&poa_group_code_input=-1&affected_workers_input={2}&date_input={3}&tbSelMonth={4}&tbSelYear={5}&worker_combo={6}&effort_activity_input={7}&internal_effort_input={8}&external_effort_input={9}&duration_input={10}&remarks={11}&time_out_snwh=0%3A00&time_out_snwh_in_cnwh=0%3A00&time_out_snwh_out_cnwh=0%3A00",
					   username, 
					   taskId, 
					   workerName, 
					   date,
					   effortDate.Month,
					   effortDate.Year,
					   workerName,
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

	    // post request
	    string response = null;
	    if (Http.HttpPost(username, password, uri, request, out response))
	    {
		Console.WriteLine("Done!");
		return 0;
	    }
	    
	    Console.WriteLine("Error occurred...");
	    return 1;
	}
    }
}
