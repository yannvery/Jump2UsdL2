using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Globalization;
using System.IO; 
using USD_WS;

/// <summary>
/// Summary description for Jump2UsdL2
/// </summary>
[WebService(Namespace = "http://localhost/jump_ws/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class Jump2UsdL2 : System.Web.Services.WebService {
    private USD_WebService myUsdService;
    private wsTools myWsTools;
    private int mySID = 0;
    private string methodName = null;
    private string myHandle = null;
    private string myCreatorHandle = null;
    private string newStatusHandle = null;
    private string repositoryHandle = null;
    private string[] myHandleAttr = { "persistent_id" };
    UsdObject usdObject;
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

    public Jump2UsdL2 () {
        this.usdObject = new UsdObject();
    }

    public void initializeResource(string methodName)
    {
        this.methodName = methodName;
        try
        {
            this.myUsdService = new USD_WebService();
            this.myWsTools = new wsTools();
        }
        catch (Exception e)
        {
            this.usdObject.ReturnCode = "ERR";
            this.usdObject.ReturnMessage = e.Message;
            this.usdObject.Description = "Can't initialize resource";
            this.usdObject.Success = false;
        }
    }

    public int login(string username, string password)
    {
        int sid = 0;
        try
        {
            this.mySID = this.myUsdService.login(username, password);
            sid = this.myUsdService.login(username, password);
        }
        catch (Exception e)
        {
            setReturn("Log in failed", "-1", e.Message, false);
        }
        return sid;
    }

    public void setReturn(string description, string code, string message, bool success)
    {
        string logMessage = "";
        string logDescription = "";
        string level = "ERROR";
        if (code == "0")
        {
            level = "INFO";
        }
        if (!(String.IsNullOrEmpty(message)))
        {
            logMessage = " - ReturnMessage : " + message;
        }
        if (!(String.IsNullOrEmpty(description)))
        {
            logDescription = " - Description : " + description;
        }
        this.myWsTools.log(level, this.methodName + " - " + this.mySID + logDescription + logMessage);
        this.usdObject.ReturnCode = code;
        this.usdObject.ReturnMessage = message;
        this.usdObject.Description = description;
        this.usdObject.Success = success;
    }

    public Boolean validatePresence(string param_name, string param_value)
    {
        Boolean myReturn = true;
        if (param_value == null)
        {
            setReturn("Invalid parameter", "1", "parameter " + param_name + " is null", false);
            myReturn = false;
        }
        return myReturn;
    }

    public Boolean validatePresenceNotEmpty(string param_name, string param_value)
    {
        Boolean myReturn = true;
        if (String.IsNullOrEmpty(param_value))
        {
            setReturn("Invalid parameter", "2", "parameter " + param_name + " is null or empty", false);
            myReturn = false;
        }
        return myReturn;
    }

    [WebMethod]
    public UsdObject logComment(string username, string password, string ref_num, string activityType, string activityValue, string activityDate, string operatorName)
    {
        initializeResource("logComment");
        string myResult;

        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - username : " + username + " - password : " + password + " - ref_num : " + ref_num + " - activityType : " + activityType + " - activityValue : " + activityValue + " - activityDate : " + activityDate + " - operatorName : " + operatorName);

        // Validate parameters
        if (!(validatePresenceNotEmpty("username", username))) return this.usdObject;
        if (!(validatePresence("password", password))) return this.usdObject;
        if (!(validatePresence("ref_num", ref_num))) return this.usdObject;
        if (!(validatePresenceNotEmpty("activityType", activityType))) return this.usdObject;
        if (!(validatePresenceNotEmpty("activityValue", activityValue))) return this.usdObject;
        if (!(validatePresenceNotEmpty("operatorName", operatorName))) return this.usdObject;

        string comment = "Le champ Jump! '" + activityType + "' a été mis à jour par '" + operatorName + "' : '" + activityValue +"'.";
        
        // login
        this.mySID = login(username, password);
        if (this.mySID == 0)
        {
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - SID : " + this.mySID);

        // Get Handle
        try
        {
            myResult = this.myUsdService.doSelect(this.mySID, "cr", "ref_num = '" + ref_num + "' AND active = 1", -1, this.myHandleAttr);
            XmlDocument myValue = new XmlDocument();
            myValue.LoadXml(myResult);
            myHandle = myValue.SelectSingleNode("//UDSObject/Handle/text()").Value;
        }
        catch (Exception e)
        {
            setReturn("Can't find incident", "3", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - myHandle : " + myHandle);

        //Log Comment
        try
        {
            this.myUsdService.logComment(this.mySID, myHandle, comment, 1);
            setReturn("Log Comment", "0", "", true);
        }
        catch (Exception e)
        {
            setReturn("Log Comment failed", "5", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - logComment : " + comment);

        try
        {
            this.myUsdService.logout(this.mySID);
        }
        catch { }
        return this.usdObject;
    }

    [WebMethod]
    public UsdObject changeStatus(string username, string password, string ref_num, string status, string description, string activityDate, string operatorName)
    {
        initializeResource("changeStatus");

        Hashtable status_matching = new Hashtable();
        status_matching["Assigned"] = "OP";
        status_matching["Work in Progress"] = "RSCHP";
        status_matching["Pending"] = "ATT";
        status_matching["Solved"] = "SLT";
        status_matching["Restored"] = "RESU";
        status_matching["Closed"] = "CL";
        
        string myResult;

        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - username : " + username + " - password : " + password + " - ref_num : " + ref_num + " - status : " + status + " - description : " + description + " - activityDate : " + activityDate + " - operatorName : " + operatorName);

        // Validate parameters
        if (!(validatePresenceNotEmpty("username", username))) return this.usdObject;
        if (!(validatePresence("password", password))) return this.usdObject;
        if (!(validatePresence("ref_num", ref_num))) return this.usdObject;
        if (!(validatePresenceNotEmpty("status", status))) return this.usdObject;
        if (!(validatePresenceNotEmpty("description", description))) return this.usdObject;
        if (!(validatePresenceNotEmpty("operatorName", operatorName))) return this.usdObject;

        if (description == "")
        {
            description = "Le status Jump! a été mis à jour par '" + operatorName + "' : '" + status + "'.";
        }
        else
        {
            description = "Le status Jump! a été mis à jour par '" + operatorName + "' : '" + status + "'.\n" + description;
        }
        // login
        this.mySID = login(username, password);
        if (this.mySID == 0)
        {
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - SID : " + this.mySID);

        // Get Creator Handle
        try
        {
            myCreatorHandle = this.myUsdService.getHandleForUserid(this.mySID, username);
        }
        catch (Exception e)
        {

            setReturn("Can't get handle for userid", "6", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - myCreatorHandle : " + myCreatorHandle);

        // Get Ticket Handle
        try
        {
            XmlDocument myValue = new XmlDocument();
            myResult = this.myUsdService.doSelect(this.mySID, "cr", "ref_num = '" + ref_num + "' AND active = 1", -1, this.myHandleAttr);
            myValue.LoadXml(myResult);
            myHandle = myValue.SelectSingleNode("//UDSObject/Handle/text()").Value;
        }
        catch (Exception e)
        {
            setReturn("Can't find incident", "3", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - ticket handle : " + myHandle);

        // Get Status Handle
        try
        {
            XmlDocument myValue = new XmlDocument();
            myResult = this.myUsdService.doSelect(this.mySID, "crs", "code = '" + status_matching[status] + "' AND delete_flag = 0", -1, this.myHandleAttr);
            myValue.LoadXml(myResult);
            newStatusHandle = myValue.SelectSingleNode("//UDSObject/Handle/text()").Value;
        }
        catch (Exception e)
        {
            setReturn("Can't find incident status", "7", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - status handle : " + newStatusHandle);

        //Change Status
        try
        {
            this.myUsdService.changeStatus(this.mySID, myCreatorHandle, myHandle, description, newStatusHandle);
            setReturn("Change Status", "0", "", true);
        }
        catch (Exception e)
        {
            setReturn("Change Status failed", "8", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - Change Status OK");

        try
        {
            this.myUsdService.logout(this.mySID);
        }
        catch { }
        return this.usdObject;
    }

    [WebMethod]
    public UsdObject addSolution (string username, string password, string ref_num, string solution, string activityDate, string operatorName)
    {
        initializeResource("addSolution");
        string myResult;

        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - username : " + username + " - password : " + password + " - ref_num : " + ref_num + " - solution : " + solution + " - activityDate : " + activityDate + " - operatorName : " + operatorName);

        // Validate parameters
        if (!(validatePresenceNotEmpty("username", username))) return this.usdObject;
        if (!(validatePresence("password", password))) return this.usdObject;
        if (!(validatePresence("ref_num", ref_num))) return this.usdObject;
        if (!(validatePresenceNotEmpty("solution", solution))) return this.usdObject;
        if (!(validatePresenceNotEmpty("operatorName", operatorName))) return this.usdObject;

        // login
        this.mySID = login(username, password);
        if (this.mySID == 0)
        {
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - SID : " + this.mySID);

        // Get Creator Handle
        try
        {
            myCreatorHandle = this.myUsdService.getHandleForUserid(this.mySID, username);
        }
        catch (Exception e)
        {
            setReturn("Can't get handle for userid", "6", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - myCreatorHandle : " + myCreatorHandle);

        // Get Ticket Handle
        try
        {
            XmlDocument myValue = new XmlDocument();
            myResult = this.myUsdService.doSelect(this.mySID, "cr", "ref_num = '" + ref_num + "' AND active = 1", -1, this.myHandleAttr);
            myValue.LoadXml(myResult);
            myHandle = myValue.SelectSingleNode("//UDSObject/Handle/text()").Value;
        }
        catch (Exception e)
        {
            setReturn("Can't find incident", "3", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - ticket handle : " + myHandle);

        //Add activity
        solution = "La solution Jump! a été mise à jour par '" + operatorName + ".\n" + solution;
        try
        {
            this.myUsdService.createActivityLog(this.mySID, myCreatorHandle, myHandle, solution, "SOLN", 0, false);
            setReturn("Add solution", "0", "", true);
        }
        catch (Exception e)
        {
            setReturn("Can't create activity", "9", e.Message, false);
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - create SOLN activity OK");

        //logout
        try
        {
            this.myUsdService.logout(this.mySID);
        }
        catch { }
        return this.usdObject;
    }

    [WebMethod]
    public UsdObject changeIncidentStart(string username, string password, string ref_num, string incidentSart, string activityDate, string operatorName)
    {
        initializeResource("changeIncidentStart");
        String myResult;

        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - username : " + username + " - password : " + password + " - ref_num : " + ref_num + " - incidentSart : " + incidentSart + " - activityDate : " + activityDate + " - operatorName : " + operatorName);

        // Validate parameters
        if (!(validatePresenceNotEmpty("username", username))) return this.usdObject;
        if (!(validatePresence("password", password))) return this.usdObject;
        if (!(validatePresence("ref_num", ref_num))) return this.usdObject;
        if (!(validatePresenceNotEmpty("incidentSart", incidentSart))) return this.usdObject;
        if (!(validatePresenceNotEmpty("operatorName", operatorName))) return this.usdObject;

        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        int secondsSinceEpoch;
        String format = "dd/MM/yy hh:mm:ss";
        CultureInfo provider = CultureInfo.InvariantCulture;
        DateTime incidentSartDate;
        try
        {
            //String incidentStart To DateTime
            incidentSartDate = DateTime.ParseExact(incidentSart, format, provider);
            //Specify incidentStartDate is a local DateTime
            incidentSartDate = DateTime.SpecifyKind(incidentSartDate, DateTimeKind.Local);
            //Convert it to utc Date
            incidentSartDate = incidentSartDate.ToUniversalTime();
            //Transform into timestamp
            t = incidentSartDate - new DateTime(1970, 1, 1);
            secondsSinceEpoch = (int)t.TotalSeconds;
        }
        catch (Exception e)
        {
            setReturn("invalid format : incidentStart", "11", e.Message, false);
            return this.usdObject;
        }

        // login
        this.mySID = login(username, password);
        if (this.mySID == 0)
        {
            return this.usdObject;
        }

        // Get Ticket Handle
        try
        {
            XmlDocument myValue = new XmlDocument();
            myResult = this.myUsdService.doSelect(this.mySID, "cr", "ref_num = '" + ref_num + "' AND active = 1", -1, this.myHandleAttr);
            myValue.LoadXml(myResult);
            myHandle = myValue.SelectSingleNode("//UDSObject/Handle/text()").Value;
        }
        catch (Exception e)
        {
            setReturn("Can't find incident", "3", e.Message, false);
            return this.usdObject;
        }

        //Update Object
        try
        {
            myResult = this.myUsdService.updateObject(this.mySID, myHandle, new string[] { "zdate_debut", secondsSinceEpoch.ToString() }, new string[0]);
            setReturn("IncidentStart updated", "0", incidentSartDate.ToString(), true);
        }
        catch (Exception e)
        {
            setReturn("Incident can't be updated", "4", e.Message, false);
            return this.usdObject;
        }

        //logout
        try
        {
            this.myUsdService.logout(this.mySID);
        }
        catch { }
        return this.usdObject;
    }

    [WebMethod]
    public UsdObject changeIncidentEnd (string username, string password, string ref_num, string incidentEnd, string activityDate, string operatorName)
    {
        initializeResource("changeIncidentEnd");
        String myResult;

        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - username : " + username + " - password : " + password + " - ref_num : " + ref_num + " - incidentEnd : " + incidentEnd + " - activityDate : " + activityDate + " - operatorName : " + operatorName);

        // Validate parameters
        if (!(validatePresenceNotEmpty("username", username))) return this.usdObject;
        if (!(validatePresence("password", password))) return this.usdObject;
        if (!(validatePresence("ref_num", ref_num))) return this.usdObject;
        if (!(validatePresenceNotEmpty("incidentEnd", incidentEnd))) return this.usdObject;
        if (!(validatePresenceNotEmpty("operatorName", operatorName))) return this.usdObject;

        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        int secondsSinceEpoch;
        String format = "dd/MM/yy hh:mm:ss";
        CultureInfo provider = CultureInfo.InvariantCulture;
        DateTime incidentEndDate;
        try
        {
            //String incidentEnd To DateTime
            incidentEndDate = DateTime.ParseExact(incidentEnd, format, provider);
            //Specify incidentEndDate is a local DateTime
            incidentEndDate = DateTime.SpecifyKind(incidentEndDate, DateTimeKind.Local);
            //Convert it to utc Date
            incidentEndDate = incidentEndDate.ToUniversalTime();
            //Transform into timestamp
            t = incidentEndDate - new DateTime(1970, 1, 1);
            secondsSinceEpoch = (int)t.TotalSeconds;
        }
        catch (Exception e)
        {
            setReturn("invalid format : incidentEndDate", "11", e.Message, false);
            return this.usdObject;
        }
        // login
        this.mySID = login(username, password);
        if (this.mySID == 0)
        {
            return this.usdObject;
        }

        // Get Ticket Handle
        try
        {
            XmlDocument myValue = new XmlDocument();
            myResult = this.myUsdService.doSelect(this.mySID, "cr", "ref_num = '" + ref_num + "' AND active = 1", -1, this.myHandleAttr);
            myValue.LoadXml(myResult);
            myHandle = myValue.SelectSingleNode("//UDSObject/Handle/text()").Value;
        }
        catch (Exception e)
        {
            setReturn("Can't find incident", "3", e.Message, false);
            return this.usdObject;
        }

        //Update Object
        try
        {
            myResult = this.myUsdService.updateObject(this.mySID, myHandle, new string[] { "zdate_fin", secondsSinceEpoch.ToString() }, new string[0]);
            setReturn("IncidentEnd updated", "0", incidentEndDate.ToString(), true);
        }
        catch (Exception e)
        {
            setReturn("Incident can't be updated", "4", e.Message, false);
            return this.usdObject;
        }

        //logout
        try
        {
            this.myUsdService.logout(this.mySID);
        }
        catch { }
        return this.usdObject;
    }

    [WebMethod]
    public UsdObject changeGroup(string username, string password, string ref_num, string group, string activityDate, string operatorName)
    {
        initializeResource("changeGroup");
        string myResult;
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - username : " + username + " - password : " + password + " - ref_num : " + ref_num + " - group : " + group + " - activityDate : " + activityDate + " - operatorName : " + operatorName);

        // Validate parameters
        if (!(validatePresenceNotEmpty("username", username))) return this.usdObject;
        if (!(validatePresence("password", password))) return this.usdObject;
        if (!(validatePresence("ref_num", ref_num))) return this.usdObject;
        if (!(validatePresenceNotEmpty("group", group))) return this.usdObject;
        if (!(validatePresenceNotEmpty("operatorName", operatorName))) return this.usdObject;

        string comment = "Le groupe Jump! a été mis à jour par '" + operatorName + "' : '" + group + "'.";
        // login
        this.mySID = login(username, password);
        if (this.mySID == 0)
        {
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - SID : " + this.mySID);

        // Get Handle
        try
        {
            myResult = this.myUsdService.doSelect(this.mySID, "cr", "ref_num = '" + ref_num + "' AND active = 1", -1, this.myHandleAttr);
            XmlDocument myValue = new XmlDocument();
            myValue.LoadXml(myResult);
            myHandle = myValue.SelectSingleNode("//UDSObject/Handle/text()").Value;
        }
        catch (Exception e)
        {
            setReturn("Can't find incident", "3", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - myHandle : " + myHandle);

        //Log Comment
        try
        {
            this.myUsdService.logComment(this.mySID, myHandle, comment, 1);
            setReturn("Change group", "0", "", true);
        }
        catch (Exception e)
        {
            setReturn("Ghange group failed failed", "5", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - changeGroup : " + comment);

        try
        {
            this.myUsdService.logout(this.mySID);
        }
        catch { }
        return this.usdObject;
    }

    [WebMethod]
    public UsdObject addAttachment(string username, string password, string ref_num, string description, string activityDate, string operatorName, string filename, byte[] buffer)
    {
        initializeResource("addAttachment");
        string myResult;
        this.myWsTools.log("INFO", this.methodName + " - username : " + username + " - password : " + password + " - ref_num : " + ref_num + " - filename : " + filename + " - description : " + description + " - operatorName : " + operatorName);
        this.myWsTools.log("INFO", this.methodName + " - buffer length : " + buffer.Length);
        //this.myWsTools.log("INFO", this.methodName + " - buffer : " + System.Text.Encoding.UTF8.GetString(buffer));
        
        // Validate parameters
        if (!(validatePresenceNotEmpty("username", username))) return this.usdObject;
        if (!(validatePresence("password", password))) return this.usdObject;
        if (!(validatePresence("ref_num", ref_num))) return this.usdObject;
        if (!(validatePresence("filename", filename))) return this.usdObject;
        if (!(validatePresenceNotEmpty("description", description))) return this.usdObject;
        if (!(validatePresenceNotEmpty("operatorName", operatorName))) return this.usdObject;

        // Get Configurations variables about where files are stored
        string doc_rep = System.Configuration.ConfigurationManager.AppSettings["doc_rep"];
        string upload_path = System.Configuration.ConfigurationManager.AppSettings["upload_path"];
        //Modify physical filename with timestamp prefix
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        int secondsSinceEpoch = (int)t.TotalSeconds;
        string physicalFilename = secondsSinceEpoch.ToString() + "_" + filename;
        try
        {
            // setting the file location to be saved in the server. 
            // reading from the web.config file 
            this.myWsTools.log("INFO", this.methodName + " - " + upload_path);
            string FilePath =
                //Path.Combine(ConfigurationManager.AppSettings["upload_path"], FileName);
                //WebConfigurationManager.AppSettings["PFUserName"].ToString()
            Path.Combine(upload_path, physicalFilename);

            File.Create(FilePath).Close();
            // open a file stream and write the buffer. 
            // Don't open with FileMode.Append because the transfer may wish to 
            // start a different point
            using (FileStream fs = new FileStream(FilePath, FileMode.Open,
            FileAccess.ReadWrite, FileShare.Read))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(buffer, 0, buffer.Length);
            }
            this.myWsTools.log("INFO", this.methodName + " - Upload filename : " + filename + " done.");
        }
        catch (Exception e)
        {
            setReturn("Upload file failed", "14", e.Message, false);
        }

        // login
        this.mySID = login(username, password);
        if (this.mySID == 0)
        {
            return this.usdObject;
        }

        // Get Handle
        try
        {
            myResult = this.myUsdService.doSelect(this.mySID, "cr", "ref_num = '" + ref_num + "' AND active = 1", -1, this.myHandleAttr);
            XmlDocument myValue = new XmlDocument();
            myValue.LoadXml(myResult);
            myHandle = myValue.SelectSingleNode("//UDSObject/Handle/text()").Value;
        }
        catch (Exception e)
        {
            setReturn("Can't find incident", "3", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - myHandle : " + myHandle);

        //Repository Handle
        try
        {
            myResult = this.myUsdService.doSelect(this.mySID, "doc_rep", "sym = '" + doc_rep + "'", -1, this.myHandleAttr);
            XmlDocument myValue = new XmlDocument();
            myValue.LoadXml(myResult);
            repositoryHandle = myValue.SelectSingleNode("//UDSObject/Handle/text()").Value;
        }
        catch (Exception e)
        {
            setReturn("Can't find incident", "3", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - repositoryHandle : " + repositoryHandle);

        //create a custom attachment
        string[] myAttrVals = {
            "orig_file_name",
            filename,
            "attmnt_name",
            filename,
            "link_only",
            "0",
            "file_name",
            physicalFilename,
            //"rel_file_path", 
            //"rep_folder_00001",
            "repository", 
            repositoryHandle,
            "status",
            "INSTALLED"
        };
        string[] myAttributes = {
            "id"
        };
        string createObjectReturn = "";
        string newHandle = "";
        try
        {
            this.myUsdService.createObject(this.mySID, "attmnt", myAttrVals, myAttributes, ref createObjectReturn, ref newHandle);
        }
        catch (Exception e)
        {
            setReturn("Can't create attachment", "-1", e.Message, false);
            return this.usdObject;
        }
        this.myWsTools.log("INFO", this.methodName + " - " + this.mySID + " - Create Attachment done : " + newHandle);

        //create lrel between custom attachment and call_req
        string[] myAttrValsLrel = {
            "cr",
            myHandle,
            "attmnt",
            newHandle,
        };
        string[] myAttributesLrel = {
            "persistent_id"
        };
        string createLrelReturn = "";
        string newLrelHandle = "";
        try
        {
            this.myUsdService.createObject(this.mySID, "lrel_attachments_requests", myAttrValsLrel, myAttributesLrel, ref createLrelReturn, ref newLrelHandle);
            setReturn("Create Lrel done", "0", newLrelHandle, true);
        }
        catch (Exception e)
        {
            setReturn("Can't create Lrel", "-1", e.Message, false);
            return this.usdObject;
        }

        try
        {
            this.myUsdService.logout(this.mySID);
        }
        catch { }
        return this.usdObject;
    }
}
