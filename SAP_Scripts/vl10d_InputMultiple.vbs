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
session.findById("wnd[0]/usr/ctxtST_VSTEL-LOW").text = ""
session.findById("wnd[0]/usr/ctxtST_VSTEL-HIGH").text = ""
session.findById("wnd[0]/usr/ctxtST_LEDAT-LOW").text = ""
session.findById("wnd[0]/usr/ctxtST_LEDAT-HIGH").text = ""
session.findById("wnd[0]/usr/tabsTABSTRIP_ORDER_CRITERIA/tabpS0S_TAB5").select
session.findById("wnd[0]/usr/tabsTABSTRIP_ORDER_CRITERIA/tabpS0S_TAB5/ssub%_SUBSCREEN_ORDER_CRITERIA:RVV50R10C:1030/btn%_ST_EBELN_%_APP_%-VALU_PUSH").press
