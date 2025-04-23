/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *	 * Redistributions of source code must retain the above copyright
 *	   notice, this list of conditions and the following disclaimer.
 *	 * Redistributions in binary form must reproduce the above copyright
 *	   notice, this list of conditions and the following disclaimer in the
 *	   documentation and/or other materials provided with the distribution.
 *	 * Neither the name of the OpenSimulator Project nor the
 *	   names of its contributors may be used to endorse or promote products
 *	   derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * MuteListModule.cs
 *								Modified by Fumi.Iseki
*/

 
using System;
using System.Collections.Generic;
using System.Reflection;

using log4net;
using Nini.Config;
using Mono.Addins;
using OpenMetaverse;

using OpenSim.Framework;
using OpenSim.Framework.Servers;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
//using OpenSim.Data.MySQL;



[assembly: Addin("MuteListModule", "1.0")]
[assembly: AddinDependency("OpenSim.Region.Framework", OpenSim.VersionInfo.VersionNumber)]


namespace OpenSim.Modules.Messaging
{
	[Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "MuteListModule")]

	public class MuteListModule : ISharedRegionModule
	{
        private string encode = "UTF-8";
        private System.Text.Encoding Enc = null;
   
        //
        // Log module
        //
		private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //
        // Module vars
        //
		//private IConfigSource m_config;
		private bool m_enabled = true;
		private List<Scene> m_SceneList = new List<Scene>();
		private string m_RestURL = String.Empty;

		//private string connectString = "";

        #region ISharedRegionModule Members

		public void PostInitialise()
		{
			if (!m_enabled) return;
		}


		public string Name
		{
			get { return "NSLMuteListModule"; }
		}


        public Type ReplaceableInterface
        {
            get { return null; }
        }


		public void Initialise(IConfigSource config)
		{
			if (!m_enabled) return;					 

			IConfig cnf = config.Configs["Messaging"];
			IConfig dat = config.Configs["DatabaseService"];

			if (m_SceneList.Count==0) {
				if (cnf==null) {
					m_enabled = false;
					return;
				}
                string module_name = cnf.GetString("MuteListModule", "None");
				if (module_name!=Name) {
                    m_log.InfoFormat("[NSL MUTELIST] NSL MuteList Module is disable. Module name is mismacth", module_name);
					m_enabled = false;
					return;
				}

				m_RestURL = cnf.GetString("MuteListURL", "");
				if (m_RestURL!="") {
					m_log.Info("[NSL MUTELIST] NSL MuteList Module is activated");
				}
				else {
					m_log.Info("[NSL MUTELIST] NSL MuteList Module is disable. There is no MuteListURL");
					m_enabled = false;
					return;
				}

                // DB
                /*
                if (dat!=null && connectString=="") {
                    connectString = dat.GetString("ConnectionString", "");
                }*/
			}
			//m_config = config;
   
            /*
            if (connectString!="") {
                m_log.Info("[NSL MUTELIST] Initialising DB");
                NSLMuteListsData data = new NSLMuteListsData();
                data.init(connectString);
            }*/
   
   			Enc = System.Text.Encoding.GetEncoding(encode);
		}


        public void Close()
        {
        }


		public void AddRegion(Scene scene)
		{
			if (!m_enabled) return;					 

			if (!m_SceneList.Contains(scene)) m_SceneList.Add(scene);

			scene.EventManager.OnNewClient += OnNewClient;
		}


        public void RemoveRegion(Scene scene)
        {
        }


        public void RegionLoaded(Scene scene)
        {
        }

        #endregion
	   

		ScenePresence FindPresence(UUID clientID)
		{
			ScenePresence p;

			foreach (Scene s in m_SceneList)
			{
				p = s.GetScenePresence(clientID);
				if (!p.IsChildAgent) return p;
			}
			return null;
		}  


		private void OnNewClient(IClientAPI client)
		{
			client.OnMuteListRequest 	 += OnMuteListRequest;
			client.OnUpdateMuteListEntry += OnUpdateMuteListEntry; 
			client.OnRemoveMuteListEntry += OnRemoveMuteListEntry;
		}



		//////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		//

	  	public void OnUpdateMuteListEntry(IClientAPI client, UUID MuteID, string Name, int Type, uint Flags) 
	   	{
			//m_log.DebugFormat("[NSL MUTELIST] OnUpdateMuteListEntry {0}, {1}, {2}, {3}", MuteID.ToString(), Name, Type.ToString(), client.AgentId.ToString());

	   		GridMuteList ml = new GridMuteList(client.AgentId, MuteID, Name, Type, Flags);
			bool success = SynchronousRestObjectRequester.MakeRequest<GridMuteList, bool>("POST", m_RestURL+"/UpdateList/", ml);
		}


	   	public void OnRemoveMuteListEntry(IClientAPI client, UUID MuteID, string Name)
	   	{
			//m_log.DebugFormat("[NSL MUTELIST] OnRemoveMuteListEntry {0}, {1}, {2}", MuteID.ToString(), Name, client.AgentId.ToString());

	   		GridMuteList ml = new GridMuteList(client.AgentId, MuteID, Name, 0, (uint)0);
			bool success = SynchronousRestObjectRequester.MakeRequest<GridMuteList, bool>("POST", m_RestURL+"/DeleteList/", ml);
		}


		private void OnMuteListRequest(IClientAPI client, uint crc)
		{
			//m_log.DebugFormat("[NSL MUTELIST] Got MUTELIST request for crc {0}", crc);

			string str = "";
			string url = m_RestURL + "/RequestList/";

			List<GridMuteList> mllist = SynchronousRestObjectRequester.MakeRequest<UUID, List<GridMuteList>>("POST", url, client.AgentId);

			//int cnt = 0;
			//while (mllist==null && cnt<10) {		// retry
			//	mllist = SynchronousRestObjectRequester.MakeRequest<UUID, List<GridMuteList>>("POST", url, client.AgentId);
			//	cnt++;
			//}

			if (mllist!=null) {
				foreach (GridMuteList ml in mllist)
				{
					str += ml.muteType.ToString()+" "+ml.muteID.ToString()+" "+ml.muteName+"|"+ml.muteFlags.ToString()+"\n";
				}
			}
			else {
				m_log.ErrorFormat("[NSL MUTELIST] Not response from mute.php");
				return;
			}

			string filename = "mutes" + client.AgentId.ToString();
			IXfer xfer = client.Scene.RequestModuleInterface<IXfer>();
			if (xfer != null)
			{
 				byte[] byteArray = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(str);
				xfer.AddNewFile(filename, byteArray);
				client.SendMuteListUpdate(filename);
			}
		}
	}



	public class GridMuteList
	{
		public Guid agentID;
		public Guid muteID;
		public string muteName;
		public int  muteType;
		public uint muteFlags;
		public uint timestamp;


		public GridMuteList()
		{ 
		}


		public GridMuteList(UUID _uuid, UUID _mute, string _name, int _type, uint _flags)
		{
			agentID	  = _uuid.Guid;
			muteID	  = _mute.Guid;
			muteName  = _name;
			muteType  = _type;
			muteFlags = _flags;
			timestamp = (uint)Util.UnixTimeSinceEpoch();
		}
	}

}
