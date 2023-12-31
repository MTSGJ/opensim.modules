
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
//using OpenSim.Data.MySQL;

using DirFindFlags = OpenMetaverse.DirectoryManager.DirFindFlags;


[assembly: Addin("OpenSimSearchModule", "1.0")]
[assembly: AddinDependency("OpenSim.Region.Framework", OpenSim.VersionInfo.VersionNumber)]


namespace OpenSim.Modules.OpenSimSearch
{
	[Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "OpenSimSearch")]

	public class OpenSimSearchModule : ISharedRegionModule
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
		//private IConfigSource m_gConfig;
		private List<Scene> m_Scenes = new List<Scene>();
		private string m_SearchServer = "";
		private bool m_Enabled = true;

		private IGroupsModule m_GroupsService = null;
		//private string connectString = "";


		#region ISharedRegionModule Members

		public void PostInitialise()
		{
			if (!m_Enabled) return;
		}


		public string Name
		{
			get { return "OpenSimSearchModule"; }
		}


		public Type ReplaceableInterface
		{
			get { return null; }
		}


		public void Initialise(IConfigSource config)
		{
			if (!m_Enabled) return;

			IConfig searchConfig = config.Configs["Search"];
			IConfig dataConfig   = config.Configs["DatabaseService"];

			if (m_Scenes.Count==0) { 	// First time
				if (searchConfig==null) {
					m_log.Info("[OSSEARCH] Not configured, disabling");
					m_Enabled = false;
					return;
				}

                string module_name = searchConfig.GetString("SearchModule", "None");
				if (module_name=="None") {
					IConfig modsConfig = config.Configs["Modules"];
                	module_name = modsConfig.GetString("SearchModule", module_name);
				}
                if (module_name!=Name) {
                    m_log.InfoFormat("[OSSEARCH] OpenSimSearch module is disable. Module name is mismacth. ({0})", module_name);
                    m_Enabled = false;
                    return;
                }

				m_SearchServer = searchConfig.GetString("SearchURL", "");
				if (m_SearchServer!="") {
					m_log.Info("[OSSEARCH] OpenSimSearch module is activated");
				}
				else {
					m_log.Info("[OSSEARCH] OpenSimSearch module is disable. There is no SearchURL");
					m_Enabled = false;
					return;
				}

				// DB
                /*
                if (dataConfig!=null && connectString=="") {
                    connectString = dataConfig.GetString("ConnectionString", "");
                }*/
			}
			//m_gConfig = config;

            /*
            if (connectString!="") {
                m_log.Info("[OSSEARCH] Initialising DB");
                OpenSimSearchesData srchdata = new OpenSimSearchesData();
                srchdata.init(connectString);
            }*/

			Enc = System.Text.Encoding.GetEncoding(encode);
		}


		public void Close()
		{
		}


		public void AddRegion(Scene scene)
		{
			if (!m_Enabled) return;

			if (!m_Scenes.Contains(scene)) m_Scenes.Add(scene);

			// Hook up events
			scene.EventManager.OnNewClient += OnNewClient;
		}


		public void RemoveRegion(Scene scene)
		{
		}


		public void RegionLoaded(Scene scene)
		{
			if (!m_Enabled) return;

			if (m_GroupsService==null) {
				m_GroupsService = scene.RequestModuleInterface<IGroupsModule>();
				if (m_GroupsService==null) m_log.Warn("[OSSEARCH]: Could not get IGroupsModule");
			}
   
		}

 		#endregion


		/// New Client Event Handler
		private void OnNewClient(IClientAPI client)
		{
			// Subscribe to messages
			client.OnDirPlacesQuery += DirPlacesQuery;
			client.OnDirFindQuery += DirFindQuery;
			client.OnDirPopularQuery += DirPopularQuery;
			client.OnDirLandQuery += DirLandQuery;
			client.OnDirClassifiedQuery += DirClassifiedQuery;
			// Response after Directory Queries
			client.OnEventInfoRequest += EventInfoRequest;
			//client.OnClassifiedInfoRequest += ClassifiedInfoRequest; 	// already defined by osprofile 
			client.OnMapItemRequest += HandleMapItemRequest;
		}


		//
		// Make external XMLRPC request
		//
		private Hashtable GenericXMLRPCRequest(Hashtable ReqParams, string method)
		{
			ArrayList SendParams = new ArrayList();
			SendParams.Add(ReqParams);

			// Send Request
			XmlRpcResponse Resp;
			try {
				XmlRpcRequest Req = new XmlRpcRequest(method, SendParams);
				Resp = Req.Send(m_SearchServer, 30000);
			}
			catch (WebException ex) {
				m_log.ErrorFormat("[OSSEARCH]: Unable to connect to Search Server {0}.  Exception {1}", m_SearchServer, ex);
				Hashtable ErrorHash = new Hashtable();
				ErrorHash["success"] = false;
				ErrorHash["errorMessage"] = "WEB Connection Error.";
				ErrorHash["errorURI"] = "";

				return ErrorHash;
			}
			catch (SocketException ex) {
				m_log.ErrorFormat("[OSSEARCH]: Unable to connect to Search Server {0}. Exception {1}", m_SearchServer, ex);
				Hashtable ErrorHash = new Hashtable();
				ErrorHash["success"] = false;
				ErrorHash["errorMessage"] = "Network Socket Error.";
				ErrorHash["errorURI"] = "";

				return ErrorHash;
			}
			catch (XmlException ex) {
				m_log.ErrorFormat("[OSSEARCH]: Unable to connect to Search Server {0}. Exception {1}", m_SearchServer, ex);
				Hashtable ErrorHash = new Hashtable();
				ErrorHash["success"] = false;
				ErrorHash["errorMessage"] = "XML Parse Error.";
				ErrorHash["errorURI"] = "";

				return ErrorHash;
			}

			if (Resp.IsFault) {
				Hashtable ErrorHash = new Hashtable();
				ErrorHash["success"] = false;
				ErrorHash["errorMessage"] = "Response Fault.";
				ErrorHash["errorURI"] = "";
				return ErrorHash;
			}
			Hashtable RespData = (Hashtable)Resp.Value;

			return RespData;
		}


		protected void DirPlacesQuery(IClientAPI remoteClient, UUID queryID, string queryText, int queryFlags, int category, string simName, int queryStart)
		{
			Hashtable ReqHash 		= new Hashtable();
			ReqHash["text"] 		= queryText;
			ReqHash["flags"] 		= queryFlags.ToString();
			ReqHash["category"] 	= category.ToString();
			ReqHash["sim_name"] 	= simName;
			ReqHash["query_start"] 	= queryStart.ToString();

			Hashtable result = GenericXMLRPCRequest(ReqHash, "dir_places_query");

			if (!Convert.ToBoolean(result["success"])) {
				remoteClient.SendAgentAlertMessage(result["errorMessage"].ToString(), false);
				return;
			}

			ArrayList dataArray = (ArrayList)result["data"];

			int count = dataArray.Count;
			if (count > 100) count = 101;

			DirPlacesReplyData[] data = new DirPlacesReplyData[count];

			int i = 0;
			foreach (Object o in dataArray) {
				Hashtable d = (Hashtable)o;

				data[i] 		 = new DirPlacesReplyData();
				data[i].parcelID = new UUID(d["parcel_id"].ToString());
				data[i].name 	 = d["name"].ToString();
				data[i].forSale  = Convert.ToBoolean(d["for_sale"]);
				data[i].auction  = Convert.ToBoolean(d["auction"]);
				data[i].dwell	 = Convert.ToSingle(d["dwell"]);
				i++;
				if (i >= count) break;
			}

			remoteClient.SendDirPlacesReply(queryID, data);
		}


		// not used ?
		public void DirPopularQuery(IClientAPI remoteClient, UUID queryID, uint queryFlags)
		{
			Hashtable ReqHash = new Hashtable();
			ReqHash["flags"] = queryFlags.ToString();

			Hashtable result = GenericXMLRPCRequest(ReqHash, "dir_popular_query");

			if (!Convert.ToBoolean(result["success"])) {
				remoteClient.SendAgentAlertMessage(result["errorMessage"].ToString(), false);
				return;
			}

			ArrayList dataArray = (ArrayList)result["data"];

			int count = dataArray.Count;
			if (count > 100) count = 101;

			DirPopularReplyData[] data = new DirPopularReplyData[count];

			int i = 0;
			foreach (Object o in dataArray) {
				Hashtable d = (Hashtable)o;

				string name = d["name"].ToString();
				if (Enc!=null) name = Enc.GetString(Convert.FromBase64String(name));

				data[i] 		 = new DirPopularReplyData();
				data[i].parcelID = new UUID(d["parcel_id"].ToString());
				data[i].name  	 = name;
				data[i].dwell 	 = Convert.ToSingle(d["dwell"]);
				i++;
				if (i >= count) break;
			}

			remoteClient.SendDirPopularReply(queryID, data);
		}


		public void DirLandQuery(IClientAPI remoteClient, UUID queryID, uint queryFlags, uint searchType, int price, int area, int queryStart)
		{
			Hashtable ReqHash 		= new Hashtable();
			ReqHash["flags"] 		= queryFlags.ToString();
			ReqHash["type"] 		= searchType.ToString();
			ReqHash["price"] 		= price.ToString();
			ReqHash["area"] 		= area.ToString();
			ReqHash["query_start"] 	= queryStart.ToString();

			Hashtable result = GenericXMLRPCRequest(ReqHash, "dir_land_query");

			if (!Convert.ToBoolean(result["success"])) {
				remoteClient.SendAgentAlertMessage(result["errorMessage"].ToString(), false);
				return;
			}

			ArrayList dataArray = (ArrayList)result["data"];

			int count = dataArray.Count;
			if (count > 100) count = 101;

			DirLandReplyData[] data = new DirLandReplyData[count];

			int i = 0;
			foreach (Object o in dataArray) {
				Hashtable d = (Hashtable)o;

				if (d["name"]==null) continue;

				data[i] 		   = new DirLandReplyData();
				data[i].parcelID   = new UUID(d["parcel_id"].ToString());
				data[i].name 	   = d["name"].ToString();
				data[i].auction	= Convert.ToBoolean(d["auction"]);
				data[i].forSale	= Convert.ToBoolean(d["for_sale"]);
				data[i].salePrice  = Convert.ToInt32(d["sale_price"]);
				data[i].actualArea = Convert.ToInt32(d["area"]);
				i++;
				if (i >= count) break;
			}

			remoteClient.SendDirLandReply(queryID, data);
		}


		public void DirFindQuery(IClientAPI remoteClient, UUID queryID, string queryText, uint queryFlags, int queryStart)
		{
			m_log.InfoFormat("[OSSEARCH]: DirFindQuery Flags = {0}", queryFlags);
			/*
			DirFindFlags
				People = 1,
				Online = 2,
				Events = 8,
				Groups = 16,
				DateEvents = 32,
				AgentOwned = 64,
				ForSale = 128,
				GroupOwned = 256,
				DwellSort = 1024,
				PgSimsOnly = 2048,
				PicturesOnly = 4096,
				PgEventsOnly = 8192,
				MatureSimsOnly = 16384,
				SortAsc = 32768,
				PricesSort = 65536,
				PerMeterSort = 131072,
				AreaSort = 262144,
				NameSort = 524288,
				LimitByPrice = 1048576,
				LimitByArea = 2097152,
				FilterMature = 4194304,
				PGOnly = 8388608,
				IncludePG = 16777216,
				IncludeMature = 33554432,
				IncludeAdult = 67108864,
				AdultOnly = 134217728,
			*/

			//
			if (((DirFindFlags)queryFlags & DirFindFlags.People)==DirFindFlags.People) {
				m_log.InfoFormat("[OSSEARCH]: DirFindQuery.People");
				DirPeopleQuery(remoteClient, queryID, queryText, queryFlags, queryStart);
				return;
			}
			else if (((DirFindFlags)queryFlags & DirFindFlags.Groups)==DirFindFlags.Groups) {
				m_log.InfoFormat("[OSSEARCH]: DirFindQuery.Groups");
				if (m_GroupsService==null) {
					m_log.Warn("[OSSEARCH]: Groups service is not available. Unable to search groups.");
					remoteClient.SendAlertMessage("Groups search is not enabled");
					return;
				}

				if (string.IsNullOrEmpty(queryText)) remoteClient.SendDirGroupsReply(queryID, new DirGroupsReplyData[0]);
				remoteClient.SendDirGroupsReply(queryID, m_GroupsService.FindGroups(remoteClient, queryText).ToArray());
				return;
			}
			else if (((DirFindFlags)queryFlags & DirFindFlags.DateEvents)==DirFindFlags.DateEvents) {
				m_log.InfoFormat("[OSSEARCH]: DirFindQuery.DateEvents");
				DirEventsQuery(remoteClient, queryID, queryText, queryFlags, queryStart);
				return;
			}
		}


		public void DirPeopleQuery(IClientAPI remoteClient, UUID queryID, string queryText, uint queryFlags, int queryStart)
		{
			List<UserAccount> accounts = m_Scenes[0].UserAccountService.GetUserAccounts(m_Scenes[0].RegionInfo.ScopeID, queryText);
			DirPeopleReplyData[] data = new DirPeopleReplyData[accounts.Count];

			int i = 0;
			foreach (UserAccount item in accounts) {
				//if ((item.UserFlags&0x01)==1) {
				data[i] 			= new DirPeopleReplyData();
				data[i].agentID 	= item.PrincipalID;
				data[i].firstName 	= item.FirstName;
				data[i].lastName  	= item.LastName;
				data[i].group 	 	= "";
				data[i].online 		= false;
				data[i].reputation 	= 0;
				i++;
				//}
			}

			remoteClient.SendDirPeopleReply(queryID, data);
		}


		public void DirEventsQuery(IClientAPI remoteClient, UUID queryID, string queryText, uint queryFlags, int queryStart)
		{
			Hashtable ReqHash 		= new Hashtable();
			ReqHash["text"] 		= queryText;
			ReqHash["flags"] 		= queryFlags.ToString();
			ReqHash["query_start"] 	= queryStart.ToString();

			Hashtable result = GenericXMLRPCRequest(ReqHash, "dir_events_query");

			if (!Convert.ToBoolean(result["success"])) {
				remoteClient.SendAgentAlertMessage(result["errorMessage"].ToString(), false);
				return;
			}

			ArrayList dataArray = (ArrayList)result["data"];

			int count = dataArray.Count;
			if (count > 100) count = 101;

			DirEventsReplyData[] data = new DirEventsReplyData[count];

			int i = 0;
			foreach (Object o in dataArray) {
				Hashtable d = (Hashtable)o;

				string name = d["name"].ToString();
				if (Enc!=null) name = Enc.GetString(Convert.FromBase64String(name));

				data[i] 			= new DirEventsReplyData();
				data[i].ownerID 	= new UUID(d["creator_id"].ToString());
				data[i].name 		= name;
				data[i].eventID 	= Convert.ToUInt32(d["event_id"]);
				data[i].date 		= d["date"].ToString();
				data[i].unixTime 	= Convert.ToUInt32(d["unix_time"]);
				data[i].eventFlags 	= Convert.ToUInt32(d["event_flags"]);
				i++;
				if (i >= count) break;
			}

			remoteClient.SendDirEventsReply(queryID, data);
		}


		public void DirClassifiedQuery(IClientAPI remoteClient, UUID queryID, string queryText, uint queryFlags, uint category, int queryStart)
		{
			Hashtable ReqHash 		= new Hashtable();
			ReqHash["text"] 		= queryText;
			ReqHash["flags"] 		= queryFlags.ToString();
			ReqHash["category"] 	= category.ToString();
			ReqHash["query_start"] 	= queryStart.ToString();

			Hashtable result = GenericXMLRPCRequest(ReqHash, "dir_classified_query");

			if (!Convert.ToBoolean(result["success"])) {
				remoteClient.SendAgentAlertMessage(result["errorMessage"].ToString(), false);
				return;
			}

			ArrayList dataArray = (ArrayList)result["data"];

			int count = dataArray.Count;
			if (count > 100) count = 101;

			DirClassifiedReplyData[] data = new DirClassifiedReplyData[count];

			int i = 0;
			foreach (Object o in dataArray) {
				Hashtable d = (Hashtable)o;

				string name = d["name"].ToString();
				if (Enc!=null) name = Enc.GetString(Convert.FromBase64String(name));

				data[i] 				= new DirClassifiedReplyData();
				data[i].classifiedID 	= new UUID(d["classifiedid"].ToString());
				data[i].name 			= name;
				data[i].classifiedFlags = Convert.ToByte(d["classifiedflags"]);
				data[i].creationDate 	= Convert.ToUInt32(d["creation_date"]);
				data[i].expirationDate 	= Convert.ToUInt32(d["expiration_date"]);
				data[i].price 			= Convert.ToInt32(d["priceforlisting"]);
				i++;
				if (i >= count) break;
			}

			remoteClient.SendDirClassifiedReply(queryID, data);
		}


		public void EventInfoRequest(IClientAPI remoteClient, uint queryEventID)
		{
			Hashtable ReqHash = new Hashtable();
			ReqHash["eventID"] = queryEventID.ToString();

			Hashtable result = GenericXMLRPCRequest(ReqHash, "event_info_query");

			if (!Convert.ToBoolean(result["success"])) {
				remoteClient.SendAgentAlertMessage(result["errorMessage"].ToString(), false);
				return;
			}

			ArrayList dataArray = (ArrayList)result["data"];
			if (dataArray.Count==0) {
				// something bad happened here, if we could return an
				// event after the search,
				// we should be able to find it here
				// TODO do some (more) sensible error-handling here
				remoteClient.SendAgentAlertMessage("Couldn't find this event.", false);
				return;
			}

			Hashtable d = (Hashtable)dataArray[0];

			string name = d["name"].ToString();
			string desc = d["description"].ToString();
			string cate = d["category"].ToString();
			if (Enc!=null) {
				name = Enc.GetString(Convert.FromBase64String(name));
				desc = Enc.GetString(Convert.FromBase64String(desc));
				cate = Enc.GetString(Convert.FromBase64String(cate));
			}

			EventData data 	 = new EventData();
			data.eventID 	 = Convert.ToUInt32(d["event_id"]);
			data.creator 	 = d["creator"].ToString();
			data.name 		 = name;
			data.category 	 = cate;
			data.description = desc;
			data.date 		 = d["date"].ToString();
			data.dateUTC 	 = Convert.ToUInt32(d["dateUTC"]);
			data.duration 	 = Convert.ToUInt32(d["duration"]);
			data.cover 		 = Convert.ToUInt32(d["covercharge"]);
			data.amount 	 = Convert.ToUInt32(d["coveramount"]);
			data.simName 	 = d["simname"].ToString();
			Vector3.TryParse(d["globalposition"].ToString(), out data.globalPos);
			data.eventFlags  = Convert.ToUInt32(d["eventflags"]);

			remoteClient.SendEventInfoReply(data);
		}


/*
		//
		// Already defined by osprofile 
		//
		public void ClassifiedInfoRequest(UUID queryClassifiedID, IClientAPI remoteClient)
		{
			Hashtable ReqHash = new Hashtable();
			ReqHash["classifiedID"] = queryClassifiedID.ToString();

			Hashtable result = GenericXMLRPCRequest(ReqHash, "classifieds_info_query");

			if (!Convert.ToBoolean(result["success"])) {
				remoteClient.SendAgentAlertMessage(result["errorMessage"].ToString(), false);
				return;
			}

			ArrayList dataArray = (ArrayList)result["data"];
			if (dataArray.Count==0) {
				// something bad happened here, if we could return an
				// event after the search,
				// we should be able to find it here
				// TODO do some (more) sensible error-handling here
				remoteClient.SendAgentAlertMessage("Couldn't find this classifieds.", false);
				return;
			}

			Hashtable d = (Hashtable)dataArray[0];

			Vector3 globalPos = new Vector3();
			Vector3.TryParse(d["posglobal"].ToString(), out globalPos);

			if (d["name"]==null) 		d["name"] = String.Empty;
			if (d["description"]==null) d["description"] = String.Empty;
			if (d["parcelname"]==null)	d["parcelname"] = String.Empty;

			string name = d["name"].ToString();
			string desc = d["description"].ToString();
			if (Enc!=null) {
				name = Enc.GetString(Convert.FromBase64String(name));
				desc = Enc.GetString(Convert.FromBase64String(desc));
			}

			remoteClient.SendClassifiedInfoReply(
					new UUID(d["classifieduuid"].ToString()),
					new UUID(d["creatoruuid"].ToString()),
					Convert.ToUInt32(d["creationdate"]),
					Convert.ToUInt32(d["expirationdate"]),
					Convert.ToUInt32(d["category"]),
					name,
					desc,
					new UUID(d["parceluuid"].ToString()),
					Convert.ToUInt32(d["parentestate"]),
					new UUID(d["snapshotuuid"].ToString()),
					d["simname"].ToString(),
					globalPos,
					d["parcelname"].ToString(),
					Convert.ToByte(d["classifiedflags"]),
					Convert.ToInt32(d["priceforlisting"]));
		}
		*/


		public void HandleMapItemRequest(IClientAPI remoteClient, uint flags, uint EstateID, bool godlike, uint itemtype, ulong regionhandle)
		{
			//The following constant appears to be from GridLayerType enum
			//defined in OpenMetaverse/GridManager.cs of libopenmetaverse.

			if (itemtype==7) { 	//(land sales)
				int tc = Environment.TickCount;
				Hashtable ReqHash = new Hashtable();

				//m_log.Info("[OSSEARCH] HandleMapItemRequest Start");

				//The flags are: SortAsc (1 << 15), PerMeterSort (1 << 17)
				ReqHash["flags"] 		= "163840";
				ReqHash["type"]  		= "4294967295"; //This is -1 in 32 bits
				ReqHash["price"] 		= "0";
				ReqHash["area"]	 		= "0";
				ReqHash["query_start"] 	= "0";

				Hashtable result = GenericXMLRPCRequest(ReqHash, "dir_land_query");

				if (!Convert.ToBoolean(result["success"])) {
					remoteClient.SendAgentAlertMessage(result["errorMessage"].ToString(), false);
					return;
				}

				ArrayList dataArray = (ArrayList)result["data"];

				int count = dataArray.Count;
				if (count > 100) count = 101;

				DirLandReplyData[] Landdata = new DirLandReplyData[count];

				int i = 0;
				string[] ParcelLandingPoint = new string[count];
				string[] ParcelRegionUUID = new string[count];

				foreach (Object o in dataArray) {
					Hashtable d = (Hashtable)o;

					if (d["name"]==null) continue;
					string name = d["name"].ToString();
					if (Enc!=null) name = Enc.GetString(Convert.FromBase64String(name));

					Landdata[i] 			= new DirLandReplyData();
					Landdata[i].parcelID 	= new UUID(d["parcel_id"].ToString());
					Landdata[i].name 		= name;
					Landdata[i].auction 	= Convert.ToBoolean(d["auction"]);
					Landdata[i].forSale 	= Convert.ToBoolean(d["for_sale"]);
					Landdata[i].salePrice	= Convert.ToInt32(d["sale_price"]);
					Landdata[i].actualArea 	= Convert.ToInt32(d["area"]);
					ParcelLandingPoint[i] 	= d["landing_point"].ToString();
					ParcelRegionUUID[i] 	= d["region_UUID"].ToString();
					i++;
					if (i >= count) break;
				}

				i = 0;
				uint locX = 0;
				uint locY = 0;

				List<mapItemReply> mapitems = new List<mapItemReply>();

				foreach (DirLandReplyData landDir in Landdata) {
					foreach(Scene scene in m_Scenes) {
						if(scene.RegionInfo.RegionID.ToString()==ParcelRegionUUID[i]) {
							locX = scene.RegionInfo.RegionLocX;
							locY = scene.RegionInfo.RegionLocY;
						}
					}
					string[] landingpoint = ParcelLandingPoint[i].Split('/');
					mapItemReply mapitem  = new mapItemReply();
					mapitem.x 			  = (uint)(locX + Convert.ToDecimal(landingpoint[0]));
					mapitem.y 			  = (uint)(locY + Convert.ToDecimal(landingpoint[1]));
					mapitem.id 			  = landDir.parcelID;
					mapitem.name 		  = landDir.name;
					mapitem.Extra 		  = landDir.actualArea;
					mapitem.Extra2 		  = landDir.salePrice;
					mapitems.Add(mapitem);
					i++;
				}
				remoteClient.SendMapItemReply(mapitems.ToArray(), itemtype, flags);
				mapitems.Clear();
			}
		}
	}
}
