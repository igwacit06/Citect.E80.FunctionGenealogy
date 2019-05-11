using System;
using System.Data;
using System.Data.OleDb;
using log4net;

namespace Citect.E80.FunctionGenealogy
{
    /// <summary>
    /// Read DBF data file
    /// </summary>
    public static class ReadDbf
    {
        //static readonly ILog log = LogManager.GetLogger(typeof(ReadDbf));
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DataTable GetEquipmentData()
        {
            var resultSet = new DataTable();
            try
            {
                using (var connectionHandler =
                    new OleDbConnection(@"Provider=VFPOLEDB.1;Data Source=C:\\QR_PSCS\\colour_DBase\\EQUIP.DBF"))
                {
                    connectionHandler.Open();
                    if (connectionHandler.State != ConnectionState.Open) return resultSet;
                    var mySql = "select * from EQUIP"; // dbf table name
                    var query = new OleDbCommand(mySql, connectionHandler);
                    var da = new OleDbDataAdapter(query);
                    da.Fill(resultSet);
                }
            }
            catch (Exception ex)
            {
                //Logger.ErrorLog("GetEquipmentData: error {0}",ex.Message);
                log.Error("GetEquipmentData: error", ex);
            }

            return resultSet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathname"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static DataTable GetTable(string pathname, string table)
        {
            var resultSet = new DataTable();
            try
            {
                using (var connectionHandler =
                    new OleDbConnection(@"Provider=VFPOLEDB.1;Data Source=" + pathname))
                {
                    connectionHandler.Open();

                    if (connectionHandler.State != ConnectionState.Open) return resultSet;
                    var mySql = string.Format("select * from {0}", table); // dbf table name

                    var query = new OleDbCommand(mySql, connectionHandler);
                    var da = new OleDbDataAdapter(query);

                    da.Fill(resultSet);
                }
            }
            catch (Exception ex)
            {
                //Logger.ErrorLog("GetEquipmentData: error {0}",ex.Message);
                log.Error("GetEquipmentData: error", ex);
            }

            return resultSet;
        }
    }
}
