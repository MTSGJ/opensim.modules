
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Xml;

using log4net;
using Nini.Config;
using Nwc.XmlRpc;
using Mono.Addins;
using OpenMetaverse;

using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Services.Interfaces;
using OpenSim.Data.MySQL;


[assembly: Addin("LocalMigrationModule", "1.0")]
[assembly: AddinDependency("OpenSim.Region.Framework", OpenSim.VersionInfo.VersionNumber)]


namespace OpenSim.Modules
{
	[Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "LocalMigrationModule")]

    public class LocalMigrationModule : ISharedRegionModule
	{
		//
		// Log module
		private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		//
		// Module vars
		private List<Scene> m_scenes = new List<Scene>();
		private bool m_Enabled = true;
		private string connectString = "";


		#region ISharedRegionModule Members

		public void PostInitialise()
		{
			if (!m_Enabled) return;
		}


		public string Name
		{
			get { return "LocalMigrationModule"; }
		}


        public Type ReplaceableInterface
		{
			get { return null; }
		}


		public void Initialise(IConfigSource config)
		{
			//m_log.Info("[LOCALMIGRATION] Initialise");
			if (!m_Enabled) return;

			IConfig dbconfig = config.Configs["DatabaseService"];

			if (m_scenes.Count==0) { 	// First time
				if (dbconfig==null) {
					m_Enabled = false;
					return;
				}
				//
				if (connectString=="") {
					connectString = dbconfig.GetString("ConnectionString", "");
				}
			}
			
			if (connectString!="") {
				m_log.Info("[LOCALMIGRATION] Initialising DB");
				LocalMigrationData data = new LocalMigrationData();
				data.init(connectString);
			}
		}


		public void Close()
		{
		}


		public void AddRegion(Scene scene)
		{
            //m_log.Info("[LOCALMIGRATION] AddRegion");
            if (!m_Enabled) return;

            if (!m_scenes.Contains(scene))
            {
                m_scenes.Add(scene);
            }
		}

       
        public void RemoveRegion(Scene scene) 
        {
        }


        public void RegionLoaded(Scene scene)
        {
        }

        #endregion
	}
}

