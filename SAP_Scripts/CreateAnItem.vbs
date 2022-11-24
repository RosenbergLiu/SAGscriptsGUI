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
session.findById("wnd[0]/tbar[0]/okcd").text = "mb1a"
session.findById("wnd[0]").sendVKey 0
session.findById("wnd[0]/usr/ctxtRM07M-BWARTWA").text = "262"
session.findById("wnd[0]/usr/ctxtRM07M-WERKS").text = "2350"
session.findById("wnd[0]/usr/ctxtRM07M-LGORT").text = "0801"
session.findById("wnd[0]/usr/ctxtRM07M-LGORT").setFocus
session.findById("wnd[0]/usr/ctxtRM07M-LGORT").caretPosition = 4
session.findById("wnd[0]").sendVKey 0
session.findById("wnd[0]/usr/subBLOCK:SAPLKACB:1003/ctxtCOBL-AUFNR").text = "20308688"
materialN = InputBox("Material Number")
session.findById("wnd[0]/usr/sub:SAPMM07M:0421/ctxtMSEG-MATNR[0,7]").text = materialN
quantity = InputBox("Quantity")
session.findById("wnd[0]/usr/sub:SAPMM07M:0421/txtMSEG-ERFMG[0,26]").text = quantity
session.findById("wnd[0]/usr/sub:SAPMM07M:0421/txtMSEG-ERFMG[0,26]").setFocus
session.findById("wnd[0]/usr/sub:SAPMM07M:0421/txtMSEG-ERFMG[0,26]").caretPosition = 1
session.findById("wnd[0]").sendVKey 0
session.findById("wnd[0]/tbar[0]/btn[11]").press
session.findById("wnd[0]/tbar[0]/btn[15]").press
