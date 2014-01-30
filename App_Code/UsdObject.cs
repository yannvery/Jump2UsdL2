using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// This class is used to format SOAP return of Jump2Usd web services
/// </summary>
public class UsdObject
{
    private String returnCode;
    private String returnMessage;
    private String description;
    private Boolean success;

	public UsdObject()
	{
		//
		// No need to a constructor
		//
	}
    public string ReturnCode {
        get { return returnCode; }
        set { returnCode = value; }
    }
    public string ReturnMessage
    {
        get { return returnMessage; }
        set { returnMessage = value; }
    }
    public string Description
    {
        get { return description; }
        set { description = value; }
    }
    public bool Success
    {
        get { return success; }
        set { success = value; }
    }
}