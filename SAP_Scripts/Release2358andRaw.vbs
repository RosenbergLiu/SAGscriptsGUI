If Not IsObject(application) Then
   Set SapGuiAuto  = GetObject("SAPGUI")
   Set application = SapGuiAuto.GetScriptingEngine
End If
If Not IsObject(connection) Then
   Set connection = application.Children(0)
End If
If Not IsObject(session) Then
   Set session    = connection.Children(0)
End If
If IsObject(WScript) Then
   WScript.ConnectObject session,     "on"
   WScript.ConnectObject application, "on"
End If
session.findById("wnd[0]").maximize
session.findById("wnd[0]/tbar[0]/okcd").text = "vl10d"
session.findById("wnd[0]").sendVKey 0
session.findById("wnd[0]/usr/ctxtST_VSTEL-LOW").text = "2358"
session.findById("wnd[0]/usr/ctxtST_LEDAT-LOW").text = ""
session.findById("wnd[0]/usr/ctxtST_LEDAT-HIGH").text = ""
session.findById("wnd[0]/usr/ctxtST_LEDAT-HIGH").setFocus
session.findById("wnd[0]/usr/ctxtST_LEDAT-HIGH").caretPosition = 0
session.findById("wnd[0]/tbar[1]/btn[8]").press
session.findById("wnd[0]/tbar[1]/btn[5]").press
session.findById("wnd[0]/tbar[1]/btn[19]").press
session.findById("wnd[0]/tbar[0]/btn[3]").press
session.findById("wnd[0]/usr/ctxtST_VSTEL-LOW").text = "2355"
session.findById("wnd[0]/usr/ctxtST_VSTEL-HIGH").text = "2357"
session.findById("wnd[0]/usr/ctxtST_LEDAT-HIGH").text = ""
session.findById("wnd[0]/usr/ctxtST_LEDAT-HIGH").setFocus
session.findById("wnd[0]/usr/ctxtST_LEDAT-HIGH").caretPosition = 0
session.findById("wnd[0]/tbar[1]/btn[8]").press
session.findById("wnd[0]/tbar[1]/btn[5]").press
session.findById("wnd[0]/tbar[1]/btn[19]").press
session.findById("wnd[0]/tbar[1]/btn[45]").press
session.findById("wnd[1]/usr/subSUBSCREEN_STEPLOOP:SAPLSPO5:0150/sub:SAPLSPO5:0150/radSPOPLI-SELFLAG[1,0]").select
session.findById("wnd[1]/usr/subSUBSCREEN_STEPLOOP:SAPLSPO5:0150/sub:SAPLSPO5:0150/radSPOPLI-SELFLAG[1,0]").setFocus
session.findById("wnd[1]/tbar[0]/btn[0]").press
session.findById("wnd[1]/usr/ctxtDY_PATH").text = "C:\Users\roshan.liu\Documents\OneDrive - Gorenje d.o.o\Automation\SAP OUTPUT\"
session.findById("wnd[1]/usr/ctxtDY_FILENAME").text = "1.txt"
session.findById("wnd[1]/usr/ctxtDY_FILENAME").caretPosition = 5
session.findById("wnd[1]/tbar[0]/btn[0]").press
