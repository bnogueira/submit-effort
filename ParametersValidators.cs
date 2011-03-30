using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace SubmitEffortNamespace
{
    public interface IValidateParameters
    {
	bool IsValid(string value);
	string GetErrorMessage();
    }



    public class EmptyValidator : IValidateParameters
    {
	public bool IsValid(string value)
	{
	    return true;
	}

	public string GetErrorMessage()
	{
	    return null;
	}
    }

    public class PmsTaskValidator : IValidateParameters
    {
	public PmsTaskValidator(string username, string password, string findUri)
	{
	    this.username = username;
	    this.password = password;
	    this.findUri = findUri;
	}

	public bool IsValid(string taskId)
	{
	    string request = string.Format("user_name={0}&object_name=Task&action_id=Find+Criteria&session_poa_query=&session_task_query=SELECT+DISTINCT+Task.Responsible%2C+Task.Code%2C+Task.Scope%2C+Task.Name%2C+Task.Project%2C+Task.State%2C+Task.DueDate%2C+Task.Level%2C+Task.SpecificType+FROM+%28Task+INNER+JOIN+TaskAssignment+ON+TaskAssignment.TaskCode%3DTask.Code%29+WHERE+%28%28Task.Code+%3D+%27{1}%27%29%29&session_mus_query=&session_criteria_conditions=Task+Code+%3D+{1}+AND&effort_query=&form_name=&id_input=&life_input=&muss_id_input=&effort_id_input=&last_page=load_criteria_page&task_code_input=&task_type_input=&specific_task_type_input=&scheduler_input=&poa_type_input=&task_scope_input=&external_input=0&choosed_criteria_name=",
					   username,
					   taskId);


	    string response = null;
	    bool result = Http.HttpPost(username, password, findUri, request, out response);

	    isValid = (result && !response.Contains("no records found!"));
	    return isValid;
	}

	public string GetErrorMessage()
	{
	    if (isValid)
		return null;

	    return "task not found";
	}

	string username;
	string password;
	string findUri;
	bool isValid;
    }

    public class DateTimeValidator : IValidateParameters
    {
	public DateTimeValidator(string format)
	{
	    this.format = format;
	}

	public bool IsValid(string value)
	{
	    try
	    {
		DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
		error = null;
	    }
	    catch(Exception ex)
	    {
		error = ex.Message;
		return false;
	    }

	    return true;
	}

	public string GetErrorMessage()
	{
	    return error;
	}

	string error = null;
	string format = null;
    }

    public class InListValidator : IValidateParameters
    {
	public InListValidator(string valuesString, char separator)
	{
	    possibleValues = new List <string> (valuesString.Split(separator));
	}

	public bool IsValid(string value)
	{
	    isValid = false;

	    if (possibleValues.Contains(value))
	    {
		isValid = true;
	    }

	    return isValid;
	}

	public string GetErrorMessage()
	{
	    if (isValid)
		return null;

	    StringBuilder sb = new StringBuilder();

	    for(int i = 0; i < possibleValues.Count; i++)
	    {
		if (i != 0)
		{
		    sb.Append("|");
		}
		
		sb.Append(possibleValues[i]);
	    }

	    return sb.ToString();
	}

	bool isValid;
	List <string> possibleValues;
    }   
}