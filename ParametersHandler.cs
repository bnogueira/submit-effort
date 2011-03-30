using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SubmitEffortNamespace
{
    public class ParameterDefinition
    {
	public ParameterDefinition(string name, bool isOptional, IValidateParameters validator)
	{
	    this.name = name;
	    this.isOptional = isOptional;
	    this.validator = validator;
	}

	string name;
        public string Name 
	{
	    get
	    {
		return name;
	    }
	}

	bool isOptional;
	public bool IsOptional
	{
	    get
	    {
		return isOptional;
	    }
	}

	IValidateParameters validator;
	public IValidateParameters Validator
	{
	    get
	    {
		return validator;
	    }
	}
    }

    public class ParametersHandler : IValidateParameters
    {
	public bool AddDefinition(string name, IValidateParameters validator)
	{
	    return AddDefinition(name, false, validator);
	}

	public bool AddDefinition(string name, bool isOptional, IValidateParameters validator)
	{
	    // get last parameter
	    ParameterDefinition lastParameter = (parameters.Count != 0) ? parameters[parameters.Count - 1] : null;

	    // if last parameter is optional then mandatory parameters cannot be accepted
	    if ((lastParameter != null) && (lastParameter.IsOptional) && !isOptional)
	    {
		errorMessage = string.Format("Parameter '{0}' cannot be mandatory because parameter {1} is optional", name, lastParameter.Name);
		return false;
	    }

	    // keep parameter definition
	    parameters.Add(new ParameterDefinition(name, isOptional, validator));
	    return true;
	}

	public bool IsValid(string value)
	{
	    return string.IsNullOrEmpty(errorMessage);
	}

	public string GetErrorMessage()
	{
	    return errorMessage;
	}

	public Dictionary<string, string> GetParameters(string[] args)
	{
	    Dictionary<string, string> parametersValues = new Dictionary<string, string>();
	    ParameterDefinition parameterDefinition = null;

	    // count of args vs count parameters definitions
	    if (args.Length != parameters.Count)
	    {
		errorMessage = BuildSyntaxString();
		return null;
	    }

	    for(int i = 0; i < parameters.Count; i++)
	    {
		parameterDefinition = parameters[i];

		if (parameterDefinition.Validator.IsValid(args[i]))
		{
		    parametersValues.Add(parameterDefinition.Name, args[i]);
		}
		else
		{
		    errorMessage = parameterDefinition.Validator.GetErrorMessage();
		    parametersValues = null;
		    break;
		}
	    }

	    return parametersValues;
	}


	private string BuildSyntaxString()
	{
	    StringBuilder sb = new StringBuilder();
	    
	    foreach(ParameterDefinition parameter in parameters)
	    {
		string format = parameter.IsOptional ? "[<{0}>] " : "<{0}> ";
		sb.AppendFormat(format, parameter.Name);
	    }

	    return sb.ToString();
	}

	private string errorMessage = null;
	private List <ParameterDefinition> parameters = new List <ParameterDefinition>();
    }
}


















