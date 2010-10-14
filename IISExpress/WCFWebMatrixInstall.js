// Configures Windows Communication Foundation 3.5 for WebMatrix
// 
// Usage: 
//    WCFWebMatrixInstall.js [un]install

var moduleName = "ServiceModel";
var handlerNames = ["svc-Integrated","rules-Integrated","xoml-Integrated","svc-ISAPI-2.0","rules-ISAPI-2.0","xoml-ISAPI-2.0"];

try { 
    var mode = ParseArguments();
    if (mode == "install")
    {
        UninstallWCF();
        InstallWCF(); 
        WScript.Echo("WCF 3.5 has been configured for WebMatrix.");
    }
    else if (mode == "uninstall")
    {
        UninstallWCF();
        WScript.Echo("WCF 3.5 has been uninstalled from WebMatrix.");
    }
    else
    {
        PrintUsage();
    }
} 
catch(e) { 
    WScript.Echo("An error occurred:\r\n " + e.description); 
} 

function InstallWCF() { 
    var adminManager = GetAdminManager(); 
 
    AddModule(adminManager);
    AddHandlers(adminManager);
 
    adminManager.CommitChanges(); 
} 

function UninstallWCF() {
    var adminManager = GetAdminManager(); 
    var moduleSection = adminManager.GetAdminSection("system.webServer/modules", "MACHINE/WEBROOT/APPHOST");

    var modulePosition = FindElement(moduleSection.Collection, "add", ["name", moduleName]); 
    if (modulePosition != -1) 
    {
      moduleSection.Collection.DeleteElement(modulePosition); 
    }
    
    var handlerSection = adminManager.GetAdminSection("system.webServer/handlers", "MACHINE/WEBROOT/APPHOST");
    for (i = 0 ; i < handlerNames.length; i++)
    {
        var svcPosition = FindElement(handlerSection.Collection, "add", ["name", handlerNames[i]]); 
        if (svcPosition != -1) 
        {
          handlerSection.Collection.DeleteElement(svcPosition); 
        }
    }
    
    adminManager.CommitChanges(); 
}

function AddModule(adminManager)
{
    var moduleSection = adminManager.GetAdminSection("system.webServer/modules", "MACHINE/WEBROOT/APPHOST"); 
    
    var element = moduleSection.Collection.CreateNewElement("add"); 
    element.Properties.Item("name").Value = moduleName; 
    element.Properties.Item("type").Value = "System.ServiceModel.Activation.HttpModule, System.ServiceModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
    element.Properties.Item("preCondition").Value = "managedHandler,runtimeVersionv2.0"; 
    moduleSection.Collection.AddElement(element, -1); 
}

function AddHandlers(adminManager)
{
    var handlerSection = adminManager.GetAdminSection("system.webServer/handlers", "MACHINE/WEBROOT/APPHOST");  

    AddIntegratedHandler(handlerSection, handlerNames[0], "*.svc");
    AddIntegratedHandler(handlerSection, handlerNames[1], "*.rules");
    AddIntegratedHandler(handlerSection, handlerNames[2], "*.xoml");
    
    AddISAPIHandler(handlerSection, handlerNames[3], "*.svc");
    AddISAPIHandler(handlerSection, handlerNames[4], "*.rules");
    AddISAPIHandler(handlerSection, handlerNames[5], "*.xoml");
}

function AddIntegratedHandler(section, name, path)
{
    var element = section.Collection.CreateNewElement("add"); 
    element.Properties.Item("name").Value = name; 
    element.Properties.Item("path").Value = path; 
    element.Properties.Item("verb").Value = "*"; 
    element.Properties.Item("type").Value = "System.ServiceModel.Activation.HttpHandler, System.ServiceModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
    element.Properties.Item("preCondition").Value = "integratedMode,runtimeVersionv2.0"; 
    section.Collection.AddElement(element, 0); 
}

function AddISAPIHandler(section, name, path)
{
    var element = section.Collection.CreateNewElement("add"); 
    element.Properties.Item("name").Value = name; 
    element.Properties.Item("path").Value = path; 
    element.Properties.Item("verb").Value = "*"; 
    element.Properties.Item("modules").Value = "IsapiModule"; 
    element.Properties.Item("scriptProcessor").Value = "%SystemRoot%\\Microsoft.NET\\Framework\\v2.0.50727\\aspnet_isapi.dll"; 
    element.Properties.Item("preCondition").Value = "classicMode,runtimeVersionv2.0,bitness32"; 
    section.Collection.AddElement(element, 0); 
}

function GetAdminManager()
{
    try
    {
        var vermg = new ActiveXObject("Microsoft.IIS.VersionManager");
        var exp = vermg.GetVersionObject("8.0", 1);
        return adminManager = exp.CreateObjectFromProgId("Microsoft.ApplicationHost.WritableAdminManager");
    }
    catch(e)
    {
        throw new Error("Unable to create WritableAdminManager.\r\n Please ensure that WebMatrix is installed properly.\r\n\r\n  " + e.description);
    }
}

function FindElement(collection, elementTagName, valuesToMatch) 
{ 
    for (var i = 0; i < collection.Count; i++) 
    { 
        var element = collection.Item(i); 
         
        if (element.Name == elementTagName) 
        { 
            var matches = true; 
            for (var iVal = 0; iVal < valuesToMatch.length; iVal += 2) 
            { 
                var property = element.GetPropertyByName(valuesToMatch[iVal]); 
                var value = property.Value; 
                if (value != null) 
                { 
                    value = value.toString(); 
                } 
                if (value != valuesToMatch[iVal + 1]) 
                { 
                    matches = false; 
                    break; 
                } 
            } 
            if (matches) 
            { 
                return i; 
            } 
        } 
    } 
     
    return -1; 
}

function ParseArguments()
{
    var mode = "";
    
    if (WScript.Arguments.Count() > 0)
    {
        if (WScript.Arguments.Item(0).toLowerCase() == "install")
        {
            mode="install";
        }
        else if (WScript.Arguments.Item(0).toLowerCase() == "uninstall")
        {
            mode="uninstall";
        }
    }
    
    return mode;
}

function PrintUsage()
{
    WScript.Echo("Usage:\r\n   WCFWebMatrixInstall.js <cmd>\r\n\r\nDescription:\r\nAdministration utility that enables configuation of WCF 3.5 for WebMatrix\r\n\r\nSupported Commands:\r\n install, uninstall\r\n\r\nSamples:\r\n WCFWebMatrixInstall.js install\r\n WCFWebMatrixInstall.js uninstall");
}


