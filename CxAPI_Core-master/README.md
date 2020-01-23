Go into the new directory and open appsettings.json in a test editor. You will see the following:
```javascript
{ "Logging": { "LogLevel": { "Default": "Warning" } },
 "CxRest":
	 { "CxUrl": "http://192.168.250.4",
		 "CxAPIResolver": "/CxWebInterface/CxWSResolver.asmx", 
		 "CxSDKWebService": "",
		 "CxFilePath": "C:\",
		 "CxDefaultFileName": "results.csv",
		 "CxDataFilePath": ".\", 
		 "CxDataFileName": "save.data",
		 "grant_type": "password",
		 "scope": "sast_rest_api",
		 "client_id":
		 "resource_owner_client", 
		 "client_secret": "014DF517-39D1-4453-B7B3-9930C563627C",
		 "project": "cxrestapi/projects", 
		 "scan": "cxrestapi/scans", 
		 "token": "cxrestapi/auth/identity/connect/token"
	 }
}
```
Change the CxUrl settings to point at your CxManager server. Set the CxFilePath to where you want to save your results. Set this to “.\” to use your current directory. Save this file.

Then open a cmd window and type “cxAPI_Core”:

C:\Users\epasanen>cxAPI_core With no arguments, or with the -? Or /? argument, you can see all your options below:

Usage: CxApi action arguments
```
Options:
 -t, --get_token Fetch the bearer token from the CxSAST service 
 -c, --store_credentials Store username and password in an encrypted file 
 -s, --scan_results Get scan results, filtered by time and project 
 --pn, --project_name=VALUE Filter with project name, Will return project if any portion of the project name is a match 
 --pi, --pipe Do not write to file but pipe output to stdout. Useful when using other API's 
 --path, --file_path=VALUE Override file path in configuration 
 --file, --file_name=VALUE Override filename in configuration 
 -u, --user_name=VALUE The username to use to retreive the token (REST) or session (SOAP) 
 -p, --password=VALUE The password needed to retreive the token (REST) or session (SOAP) 
 --st, --start_time=VALUE Last scan start time 
 --et, --end_time=VALUE Last scan end time 
 -v, --verbose Change degrees of debugging info 
 -d, --debug Output debugging info 
 -?, -h, --help show you your options
```
You will want to save your credentials first. Enter: cxAPI_Core –save_credentials –user_name=”Cxuser” –password=”Cxpassword”

Your credentials will be encrypted and saved on your system. You can now run the the –scan_results action without including the credentials in your scripts.

cxAPI_Core –get_token will create and store a token for CxREST services.

scAPI_Core –scan_results –start_time=”06/21/2019 0:0:0” will fetch projects that the last scan date is later than the start time used.
Use to get the current days scan summary.

scAPI_Core –scan_results” –start_time=”01/01/2019 0:0:0 –end_time=”06/01/2019 0:0:0 will fetch projects that the last scan date is later than the start time used. The end time argument will filter out any projects with scan dates later than that date.

cxAPI_Core –scan_results –project_name=”CxProject” will filter for projects containing part of this name. So using “Web” will return all projects containing that substring.

Other flags
```
--pipe turns off file creation and sends output to stdout. 
--debug puts error info in the console or log file.
--file_path and –file_name will override the settings in appsettings.json
```
