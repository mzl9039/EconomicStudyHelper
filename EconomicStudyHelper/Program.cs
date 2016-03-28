/*
 * Created by SharpDevelop.
 * User: mzl
 * Date: 2015-10-16
 * Time: 18:54
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using DataHelper;
using LogHelper;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace EconomicStudyHelper
{
    /// <summary>
    /// Class with program entry point.
    /// </summary>
    internal sealed class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainSingle();
        }

        static void MainSingle()
        {
            LicenseInitializer m_AOLicenseInitialzer = null;
            try
            {
                if (!AoInitializeFirst(ref m_AOLicenseInitialzer))
                {
                    return;
                }

                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                //throw ex;
            }
            finally
            {
                m_AOLicenseInitialzer.ShutdownApplication();
            }
        }

        static bool AoInitializeFirst(ref LicenseInitializer m_AOLicenseInitialzer)
        {
            bool bInitialized = true;
            m_AOLicenseInitialzer = new LicenseInitializer();
            bInitialized = m_AOLicenseInitialzer.InitializeApplication(
                new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeEngine },
                new esriLicenseExtensionCode[] { esriLicenseExtensionCode.esriLicenseExtensionCode3DAnalyst,
                                                 esriLicenseExtensionCode.esriLicenseExtensionCodeSpatialAnalyst});

            if (bInitialized == false)
            {
                MessageBox.Show("ArcEngine 不能正常初始化，请确认是否正确安装ArcEngine并获得许可！", "提示信息");
                m_AOLicenseInitialzer.ShutdownApplication();
                return false;
            }
            return true;
        }
    }
}
