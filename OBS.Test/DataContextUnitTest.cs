using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;

namespace OBS.Test
{
    public abstract class DataContextUnitTest<T> where T : DataContext
    {
        const string TestDBPath = @"C:UsersswaltherDocumentsCommon ContentBlogTip20 Linq to SQL CreateDatabaseCSTip20TestsApp_DataTest.mdf";
        protected T TestDataContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            this.CreateTestDB();
        }
        public void CreateTestDB()
        {
            var testConnectionString = GetTestConnectionString();
            // Need to use reflection here since you 
            // cannot use Generics with a contructors that require params
            Type[] types = { typeof(string) };
            Object[] typeValues = { testConnectionString };
            this.TestDataContext = (T)typeof(T).GetConstructor(types).Invoke(typeValues);
            this.RemoveTestDB();
            this.TestDataContext.CreateDatabase();
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.RemoveTestDB();
        }
        protected void RemoveTestDB()
        {
            if (this.TestDataContext.DatabaseExists())
                this.TestDataContext.DeleteDatabase();
        }
        private static string GetTestConnectionString()
        {
            var conBuilder = new SqlConnectionStringBuilder();
            conBuilder.AttachDBFilename = TestDBPath;
            conBuilder.DataSource = @".SQLExpress";
            conBuilder.IntegratedSecurity = true;
            conBuilder.UserInstance = true;
            return conBuilder.ConnectionString;
        }
    }
}
