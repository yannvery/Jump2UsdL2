# Jump2UsdL2
> This is the second stage of Jump2Usd project
----
## Pre required
* .NET Framework 2.0
* IIS 6.0
* Microsoft WSE 3.0.msi
* CA ServiceDesk Manager 12.5

##Installation
1. Add a virtual directory on IIS and link the directory which contains the project
2. Configure the virtual directory with permissions / default app pool and an application name
3. Enable anonymous access can help to avoid some permissions access problems

## Usage
Open browser and call Jump2UsdL1.asmx file to list all availables web methods.

## Notes
In this version, ServiceDesk Manager uses some customs fields. Errors can appears if one of this fields miss

## WebMethods availables
* logComment
* changeStatus
* changeIncidentStart
* changeIncidentEnd
* changeGroup
* addAttachment
