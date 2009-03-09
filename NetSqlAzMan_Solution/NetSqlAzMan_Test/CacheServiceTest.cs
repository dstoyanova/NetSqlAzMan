﻿using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetSqlAzMan.Cache;

namespace NetSqlAzMan_Test
{
    /// <summary>
    /// Summary description for CacheServiceTest
    /// </summary>
    [TestClass]
    public class CacheServiceTest
    {
        public CacheServiceTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethod1()
        {
            using (sr.CacheServiceClient csc = new NetSqlAzMan_Test.sr.CacheServiceClient())
            {
                try
                {
                    KeyValuePair<string, string>[] kvp;
                    sr.AuthorizationType result = csc.CheckAccessForWindowsUsersWithAttributesRetrieve(out kvp, "Italferr", "CartaDeiServizi", "Visualizza Dettagli Richiesta", WindowsIdentity.GetCurrent().GetUserBinarySSid(), WindowsIdentity.GetCurrent().GetGroupsBinarySSid(), DateTime.Now, false, null);
                    Debug.WriteLine(result);
                }
                finally
                {
                    csc.Close();
                }
            }
        }
    }
}