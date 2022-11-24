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
session.findById("wnd[0]/usr/ctxtST_VSTEL-LOW").text = ""
session.findById("wnd[0]/usr/ctxtST_VSTEL-HIGH").text = ""
session.findById("wnd[0]/usr/ctxtST_LEDAT-LOW").text = ""
session.findById("wnd[0]/usr/ctxtST_LEDAT-HIGH").text = ""
session.findById("wnd[0]/usr/ctxtST_VSTEL-HIGH").setFocus
session.findById("wnd[0]/usr/ctxtST_VSTEL-HIGH").caretPosition = 0
