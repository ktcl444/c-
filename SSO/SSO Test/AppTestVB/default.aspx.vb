Imports AppLauncherDataVB
Imports System.Configuration

Public Class _default
  Inherits System.Web.UI.Page

#Region " Web Form Designer Generated Code "

  'This call is required by the Web Form Designer.
  <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

  End Sub
  Protected WithEvents lblAppName As System.Web.UI.WebControls.Label
  Protected WithEvents Label2 As System.Web.UI.WebControls.Label
  Protected WithEvents lblLoginID As System.Web.UI.WebControls.Label
  Protected WithEvents Label3 As System.Web.UI.WebControls.Label

  'NOTE: The following placeholder declaration is required by the Web Form Designer.
  'Do not delete or move it.
  Private designerPlaceholderDeclaration As System.Object

  Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
    'CODEGEN: This method call is required by the Web Form Designer
    'Do not modify it using the code editor.
    InitializeComponent()
  End Sub

#End Region

  Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    lblLoginID.Text = User.Identity.Name
    lblAppName.Text = DirectCast( _
     Application("AppToken"), AppToken).AppName

    If User.IsInRole("Admin") Then
      lblLoginID.Visible = False
    End If
  End Sub
End Class
