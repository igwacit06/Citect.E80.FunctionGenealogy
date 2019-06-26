using DbfDataReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.IO;

namespace Citect.E80.FunctionGenealogy
{
    public class CitectGraphics
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly GraphicsBuilder.IGraphicsBuilder GraphicsBuilder;
        private List<CitectPages> CitectPages;
        private readonly List<ProjectPages> ProjectPages;

        public CitectGraphics()
        {
            GraphicsBuilder = new GraphicsBuilder.GraphicsBuilder();
            ProjectPages = new List<ProjectPages>();
            CitectPages = new List<CitectPages>();
        }


        /// <summary>
        /// get project pages (from pages.dbf)        
        /// </summary>
        /// <param name="path"></param>
        public void GetProjectPages(string path, bool retrievePageANs = true)
        {            
            using (var dbfTable = new DbfTable(path, System.Text.Encoding.UTF8))
            {
                var skipDeleted = true;
                var dbfRecord = new DbfRecord(dbfTable);
                while (dbfTable.Read(dbfRecord))
                {
                    if (skipDeleted && dbfRecord.IsDeleted)
                        continue;

                    CitectPages.Add(new CitectPages { PageName = dbfRecord.Values[0].ToString(), ObjectANs = new List<int>() });
                }
            }
            var subdir = path.Substring(0, path.LastIndexOf('\\'));
            if (retrievePageANs)
                GetPageAns(subdir + @"\pgdynobj.dbf");
        }

        /// <summary>
        /// Extract Pages from Project List
        /// </summary>
        /// <param name="ProjectPath"></param>
        /// <param name="Prefix"></param>
        public int GetAllProjectPages(string Prefix, string ProjectPath)
        {
            var path = Path.GetDirectoryName(ProjectPath);
            //get list of sub directories with project prefix
            var directories = Directory.GetDirectories(ProjectPath).Where(s => s.Contains(Prefix)).ToArray();            
            //var searchList = Directory.GetFiles(path, "pages.dbf", SearchOption.AllDirectories).Where(s => s.Contains(Prefix));            
            var errors = 0;

            //get list of pages in project
            foreach (var searchDir in directories)
            {
                try
                {
                    CitectPages.Clear();
                    GetProjectPages(string.Format("{0}\\pages.dbf", searchDir), false);
                    var pagelist = new List<CitectPages>();
                    var directory = searchDir.Split(new char[] { '\\' });
                    pagelist.AddRange(CitectPages);                    
                    ProjectPages.Add(new ProjectPages { ProjectName = directory[directory.Length-1], PageList = pagelist });
                }
                catch (Exception ex)
                {
                    log.Error("GetAllProjectPage:", ex);
                    errors++;
                }
            }

            var LogCsv = new LogCsv(@"C:\QR_PSCS\Logs\ProjectPages.csv", true);
            LogCsv.WriteToFile("Project,Citect Page,ANs");
            foreach (var projectPage in ProjectPages)
            {
                foreach (var citectPage in projectPage.PageList)
                {
                    LogCsv.WriteToFile("{0},{1},{2}", projectPage.ProjectName, citectPage.PageName, string.Empty);
                }
            }
            LogCsv.CloseFile();
            return errors;
        }

        /// <summary>
        /// open pgdynobject.dbf to get ANs
        /// </summary>
        /// <param name="path"></param>
        private void GetPageAns(string path)
        {
            using (var dbfTable = new DbfTable(path, System.Text.Encoding.UTF8))
            {
                var skipDeleted = true;
                var dbfRecord = new DbfRecord(dbfTable);

                var cols = dbfTable.Columns.ToList();
                var ANColIdx = 0;
                if (cols.FirstOrDefault(s => s.Name.Contains("AN")) != null)
                    ANColIdx = cols.FirstOrDefault(s => s.Name.Contains("AN")).Index;

                while (dbfTable.Read(dbfRecord))
                {
                    if (skipDeleted && dbfRecord.IsDeleted)
                        continue;

                    //get citectpage object from list
                    var citectPage = CitectPages.Find(s => s.PageName.Equals(dbfRecord.GetValue(0).ToString()));
                    citectPage.ObjectANs.Add(int.Parse(dbfRecord.GetValue(ANColIdx).ToString()));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="pageName"></param>
        public bool GetGenies(string projectName, string pageName)
        {
            GraphicsBuilder.ProjectSelect(projectName);
            GraphicsBuilder.PageOpen(projectName, pageName);
            GraphicsBuilder.PageSelectFirstObject();
            while (GraphicsBuilder.AttributeClass() != "")
            {
                try
                {
                    var attributes = GraphicsBuilder.AttributeClass();
                    if (!attributes.Contains("Genie"))
                    {
                        GraphicsBuilder.PageSelectNextObject();
                        continue;
                    }

                    var error = false;
                    GraphicsBuilder.LibraryObjectFirstProperty(out string propertyName, out string propertyVal);
                    while (!error)
                    {
                        try
                        {
                            GraphicsBuilder.LibraryObjectNextProperty(out propertyName, out propertyVal);
                            log.DebugFormat("propertyName:{0}|propertyvalue:{1}", propertyName, propertyVal);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error", ex);
                            error = true;
                        }
                    }
                    //GraphicsBuilder.LibraryObjectFirstProperty(out string propertyName, out string propertyVal);
                    //GraphicsBuilder.LibraryNextObject(out string value);

                    //log.DebugFormat("GetGenies:{0}", attributes);

                    GraphicsBuilder.PageSelectNextObject();
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            GraphicsBuilder.PageClose();
            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ProjectPages
    {
        public string ProjectName { get; set; }
        public List<CitectPages> PageList { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class CitectPages
    {
        public string PageName;
        public List<int> ObjectANs;
    }
}
