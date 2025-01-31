using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace InsertFeature
{
	/// <summary>Das Objekt für die Implementierung eines Add-Ins.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2
	{

        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private string ConfigName = null;
        private string ProjectName = null;
        private string ProjectPath = null;
        private string MSIFile = null;

        // API zum Einbau der Feature
        private MSI_API msiapi = null;

        #region Events

        private EnvDTE.BuildEvents buildEvents;

        public void OnBuildBegin(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
        {

        }
        public bool ExecInsert()
        {
            bool ok = false;
            //handled = false;
                msiapi = new MSI_API(ProjectPath + "\\" + MSIFile, _applicationObject);
                // Testen ob es sich bei der Installerdatei auch um eine Datei handelt
                // in der die Benutzeroberfläche "Selection Feature' enthalten ist
                if (msiapi.StartTest())
                {
                    // Nur die Installer-Datei anpassen
                    msiapi.StartInsert();

                }
                ok = false;
                msiapi = null;

                // *****************************************************************************************************
                // Die Carbage Collection muss erzwungen werden, da das Installer-Objekt mit
                // "Database = Installer.OpenDatabase(FileName, MsiOpenDatabaseMode.msiOpenDatabaseModeTransact);"
                // die Installer-Datei exclosiv öffnet aber keine Methode zur Verfügung stellt, sie wieder freizugeben.
                GC.Collect();
                // Nun kann in der IDE die Installation angestossen werden
                // *****************************************************************************************************
                return ok;
        }

        /// <summary>
        /// Wenn ein Projektbuild beendet ist und es sich um ein Setup-Projekt haldelt,
        /// wird die Installerdatei auf 'SelectionTree# untersucht und weiter bearbeitet.
        /// </summary>
        /// <param name="Scope"></param>
        /// <param name="Action"></param>
        public void OnBuildDone(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
        {
            if (TestOf_vdproj())
            {
                ExecInsert();
            }
        }

        #endregion

		/// <summary>Implementiert den Konstruktor für das Add-In-Objekt. Platzieren Sie Ihren Initialisierungscode innerhalb dieser Methode.</summary>
		public Connect()
		{
		}

		/// <summary>Implementiert die OnConnection-Methode der IDTExtensibility2-Schnittstelle. Empfängt eine Benachrichtigung, wenn das Add-In geladen wird.</summary>
		/// <param term='application'>Stammobjekt der Hostanwendung.</param>
		/// <param term='connectMode'>Beschreibt, wie das Add-In geladen wird.</param>
		/// <param term='addInInst'>Objekt, das dieses Add-In darstellt.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;

            try
            {

                // Event-Handling installieren
                EnvDTE.Events events = _applicationObject.Events;
                buildEvents = (EnvDTE.BuildEvents)events.BuildEvents;
                buildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(this.OnBuildBegin);
                buildEvents.OnBuildDone += new _dispBuildEvents_OnBuildDoneEventHandler(this.OnBuildDone);
            }
            catch (System.Exception excep)
            {
                MessageBox.Show("3 " + excep.Message);
            }
        }

		/// <summary>Implementiert die OnDisconnection-Methode der IDTExtensibility2-Schnittstelle. Empfängt eine Benachrichtigung, wenn das Add-In entladen wird.</summary>
		/// <param term='disconnectMode'>Beschreibt, wie das Add-In entladen wird.</param>
		/// <param term='custom'>Array von spezifischen Parametern für die Hostanwendung.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implementiert die OnAddInsUpdate-Methode der IDTExtensibility2-Schnittstelle. Empfängt eine Benachrichtigung, wenn die Auflistung von Add-Ins geändert wurde.</summary>
		/// <param term='custom'>Array von spezifischen Parametern für die Hostanwendung.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implementiert die OnStartupComplete-Methode der IDTExtensibility2-Schnittstelle. Empfängt eine Benachrichtigung, wenn der Ladevorgang der Hostanwendung abgeschlossen ist.</summary>
		/// <param term='custom'>Array von spezifischen Parametern für die Hostanwendung.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implementiert die OnBeginShutdown-Methode der IDTExtensibility2-Schnittstelle. Empfängt eine Benachrichtigung, wenn die Hostanwendung entladen wird.</summary>
		/// <param term='custom'>Array von spezifischen Parametern für die Hostanwendung.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
        #region Test auf Setup-Projekt

        /// <summary>
        /// Testet die Projektdatei und ermittelt daraus benötigte Daten
        /// </summary>
        /// <returns></returns>
        public bool TestOf_vdproj()
        {
            bool result = false;
            try
            {
                SolutionBuild2 SB = (SolutionBuild2)_applicationObject.Solution.SolutionBuild;
                SolutionContext SolCtx;
                FileStream fs = null;

                int pos;
                // In der Projektmappe nachsehen ob ein Setup-Projekt vorhanden ist,
                // dann 'result' = true
                for (int i = 1; i <= SB.SolutionConfigurations.Item(1).SolutionContexts.Count; i++)
                {
                    // Die einzelnen Projekte in der Solution lesen
                    SolCtx = SB.SolutionConfigurations.Item(1).SolutionContexts.Item(i);

                    // Der komplette Name des Projektes mit dem Pfad
                    // Eigenartigerweise bei 'csproj' mit Extension, aber
                    // bei 'vdproj' ohne Extension, deshalb den Dateinamen
                    // immer abschneiden und später den Namen mit Extension 
                    // aus SolCtx.ProjectName ermitteln

                    ProjectPath = _applicationObject.Solution.Projects.Item(i).FullName.
                                 Substring(0, _applicationObject.Solution.Projects.Item(i).FullName.LastIndexOf("\\"));

                    // ProjectName ermitteln, wenn Unterdirectory, dann 
                    // den Pfad entfernen
                    pos = SolCtx.ProjectName.LastIndexOf("\\");
                    ProjectName = SolCtx.ProjectName.Substring(pos + 1);

                    // Art der Configuration (Release/Debug) ermitteln
                    // ConfigName = SolCtx.ConfigurationName;

                    // Ist es ein Installer-Prokekt ?
                    if (ProjectName.Substring(ProjectName.IndexOf('.') + 1) == "vdproj")
                    {
                        fs = new FileStream(ProjectPath + "\\" + ProjectName, FileMode.Open, FileAccess.Read);
                        StreamReader sr = new StreamReader(fs);

                        // Art der Configuration (Release/Debug) ermitteln
                        // wird benötigt um den richtigen Zweig in der xxx.vdproj zur
                        // Feststellung des 'OutputFileName' unter Debug oder Release zu finden
                        ConfigName = SolCtx.ConfigurationName;

                        // Nun kann der 'OutputFilename' ermittelt werden
                        MSIFile = GetOutputFilename(sr, ConfigName);
                        Debug.Print(Path.GetExtension(MSIFile));
                        if (Path.GetExtension(MSIFile) == ".msi")
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("2 " + e.Message);
            }
            return result;
        }
        /// <summary>
        /// liest aus dem übergebenen Stream (nur xxx.vdproj - Datei)
        /// den Parameter (Dateinamen) von 'OutputFilename' z.B. 'Debug\\xxx.msi'
        /// </summary>
        /// <param name="sr"> StreamReader</param>
        /// <param name="release">'Debug' oder 'Release'</param>
        public string GetOutputFilename(StreamReader sr, string configuration)
        {
            string result = null;
            string Zeile;
            int pos = 0;

            while (null != (Zeile = sr.ReadLine())) // Zeilenweise bis Dateiende
            {
                // Bis "Debug" oder "Release" entsprechens 'configuration' gefunden ist
                pos = Zeile.IndexOf("\"" + configuration + "\"");
                if (pos > 0)
                {
                    while (null != (Zeile = sr.ReadLine())) // Zeilenweise bis Dateiende
                    {
                        // Bis "OutputFilename" = "8:" gefunden ist
                        pos = Zeile.IndexOf("\"OutputFilename\" = \"8:");
                        if (pos > 0)
                        {
                            // gefunden, auch noch 4 x Backslash in 2 x wandeln
                            result = Zeile.Substring(pos + 22, Zeile.Length - pos - 23).Replace("\\\\", "\\");
                            break;
                        }
                    }
                }
            }
            sr.Close();
            return result;
        }
        /// <summary>
        /// Prüft ob ein Projekt geladen ist und ob es ein Setup-Projekt ist
        /// </summary>
        /// <returns></returns>
        private bool IsvdprojLoaded()
        {
            bool result = false;
            if (_applicationObject.Solution.IsOpen)
            {
                SolutionBuild2 SB = (SolutionBuild2)_applicationObject.Solution.SolutionBuild;
                SolutionContext SolCtx = SB.SolutionConfigurations.Item(1).SolutionContexts.Item(1);
                int pos = ProjectName.IndexOf('.');
                // Ist es ein Setup-Projekt ?
                if (ProjectName.Substring(pos + 1) == "vdproj") result = true;
            }
            return result;
        }

        #endregion
	}
}