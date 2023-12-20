
using System;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using MySql.Data.MySqlClient;
using OpenMetaverse;


namespace OpenSim.Data.MySQL
{
	public class LocalMigrationData
	{
		private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		// to Offline Message Module
		private string Table_of_OfflineMessage 	= "offline_message";

		// to Flotsam Group Module
		private string Table_of_GroupActive	 	= "group_active";
		private string Table_of_GroupInvite	 	= "group_invite";
		private string Table_of_GroupList	 	= "group_list";
		private string Table_of_GroupMembership = "group_membership";
		private string Table_of_GroupNotice	 	= "group_notice";
		private string Table_of_GroupRole	 	= "group_role";
		private string Table_of_GroupRoleMembership = "group_rolemembership";


		// for MySQL
		private string connectString;
		private MySqlConnection dbcon;
  

		public LocalMigrationData()
		{
		}


		public LocalMigrationData(string hostname,string database,string username ,string password,string cpooling, string port)
		{
			string s = "Server=" + hostname + ";Port=" + port + ";Database=" + database + 
											  ";User ID=" + username + ";Password=" + password + ";Pooling=" + cpooling + ";";
			Initialise(s);
		}


		public LocalMigrationData(string connect)
		{
			Initialise(connect);
		}


		public void init(string connect)
		{
			Initialise(connect);
		}


		private void Initialise(string connect)
		{
			try {
				connectString = connect;
				dbcon = new MySqlConnection(connectString);
				try {
					dbcon.Open();
				}
				catch (Exception e) {
					throw new Exception("[LOCALMIGRATION DATA]: Connection error while using connection string ["+connectString+"]", e);
				}
				//m_log.Info("[LOCALMIGRATION DATA]: Connection established");
			}

			catch(Exception e) {
				throw new Exception("[LOCALMIGRATION DATA]: Error initialising MySql Database: " + e.ToString());
			}

			//
			try {
				Dictionary<string,string> tableList = new Dictionary<string,string>();
				tableList = CheckTables();

				////////////////////////////////////////////////////////////////////////////////////////
				// Offline Message Table

				if (!tableList.ContainsKey(Table_of_OfflineMessage)) {
					try {
						CreateOfflineMessageTable();
					}
					catch (Exception e) {
						throw new Exception("[LOCALMIGRATION DATA]: Error creating offline message table: " + e.ToString());
					}
				}
				else {
					string version = tableList[Table_of_OfflineMessage].Trim();
					int nVer = getTableVersionNum(version);
					switch (nVer) {
					  case 1: //Rev.1
						//UpdateOfflineMessageTable1();
						break;
					}
				}

				////////////////////////////////////////////////////////////////////////////////////////
				// Flotsam Group Table

				// GroupActive Table
				if (!tableList.ContainsKey(Table_of_GroupActive)) {
					try {
						CreateGroupActiveTable();
					}
					catch (Exception e) {
						throw new Exception("[LOCALMIGRATION DATA]: Error creating group active table: " + e.ToString());
					}
				}
				else {
					string version = tableList[Table_of_GroupActive].Trim();
					int nVer = getTableVersionNum(version);
					switch (nVer) {
					  case 1: //Rev.1
						//UpdateGroupActiveTable1();
						break;
					}
				}

				//
				// GroupInvite Table
				if (!tableList.ContainsKey(Table_of_GroupInvite)) {
					try {
						CreateGroupInviteTable();
					}
					catch (Exception e) {
						throw new Exception("[LOCALMIGRATION DATA]: Error creating group invite table: " + e.ToString());
					}
				}
				else {
					string version = tableList[Table_of_GroupInvite].Trim();
					int nVer = getTableVersionNum(version);
					switch (nVer) {
					  case 1: //Rev.1
						//UpdateGroupInviteTable1();
						break;
					}
				}

				//
				// GroupList Table
				if (!tableList.ContainsKey(Table_of_GroupList)) {
					try {
						CreateGroupListTable();
					}
					catch (Exception e) {
						throw new Exception("[LOCALMIGRATION DATA]: Error creating group list table: " + e.ToString());
					}
				}
				else {
					string version = tableList[Table_of_GroupList].Trim();
					int nVer = getTableVersionNum(version);
					switch (nVer) {
					  case 1: //Rev.1
						//UpdateGroupListTable1();
						break;
					}
				}

				//
				// GroupMembership Table
				if (!tableList.ContainsKey(Table_of_GroupMembership)) {
					try {
						CreateGroupMembershipTable();
					}
					catch (Exception e) {
						throw new Exception("[LOCALMIGRATION DATA]: Error creating group membership table: " + e.ToString());
					}
				}
				else {
					string version = tableList[Table_of_GroupMembership].Trim();
					int nVer = getTableVersionNum(version);
					switch (nVer) {
					  case 1: //Rev.1
						//UpdateGroupMembershipTable1();
						break;
					}
				}

				//
				// GroupNotice Table
				if (!tableList.ContainsKey(Table_of_GroupNotice)) {
					try {
						CreateGroupNoticeTable();
					}
					catch (Exception e) {
						throw new Exception("[LOCALMIGRATION DATA]: Error creating group notice table: " + e.ToString());
					}
				}
				else {
					string version = tableList[Table_of_GroupNotice].Trim();
					int nVer = getTableVersionNum(version);
					switch (nVer) {
					  case 1: //Rev.1
						//UpdateGroupNoticeTable1();
						break;
					}
				}

				//
				// GroupRole Table
				if (!tableList.ContainsKey(Table_of_GroupRole)) {
					try {
						CreateGroupRoleTable();
					}
					catch (Exception e) {
						throw new Exception("[LOCALMIGRATION DATA]: Error creating group role table: " + e.ToString());
					}
				}
				else {
					string version = tableList[Table_of_GroupRole].Trim();
					int nVer = getTableVersionNum(version);
					switch (nVer) {
					  case 1: //Rev.1
						//UpdateGroupRoleTable1();
						break;
					}
				}

				//
				// GroupRoleMembership Table
				if (!tableList.ContainsKey(Table_of_GroupRoleMembership)) {
					try {
						CreateGroupRoleMembershipTable();
					}
					catch (Exception e) {
						throw new Exception("[LOCALMIGRATION DATA]: Error creating group role membership table: " + e.ToString());
					}
				}
				else {
					string version = tableList[Table_of_GroupRoleMembership].Trim();
					int nVer = getTableVersionNum(version);
					switch (nVer) {
					  case 1: //Rev.1
						//UpdateGroupRoleMembershipTable1();
						break;
					}
				}

			}
			catch (Exception e) {
				m_log.Error("[LOCALMIGRATION DATA]: Error checking or creating tables: " + e.ToString());
				throw new Exception("[LOCALMIGRATION DATA]: Error checking or creating tables: " + e.ToString());
			}
		}


		private int getTableVersionNum(string version)
		{
			int nVer = 0;

			Regex _commentPattenRegex = new Regex(@"\w+\.(?<ver>\d+)");
			Match m = _commentPattenRegex.Match(version);
			if (m.Success) {
				string ver = m.Groups["ver"].Value;
				nVer = Convert.ToInt32(ver);
			}
			return nVer;
		}



		///////////////////////////////////////////////////////////////////////
		// create Offline Message Table

		private void CreateOfflineMessageTable()
		{
			string sql = string.Empty;

			sql  = "CREATE TABLE `" + Table_of_OfflineMessage + "` (";
  			sql += "`messageid` bigint(11)  NOT NULL AUTO_INCREMENT,";
  			sql += "`to_uuid`   varchar(36) NOT NULL DEFAULT '',";
  			sql += "`from_uuid` varchar(36) NOT NULL DEFAULT '',";
  			sql += "`message`   longtext    NOT NULL,";
  			sql += "PRIMARY KEY (`messageid`),";
  			sql += "KEY `to_uuid` (`to_uuid`)";
			sql += ") ENGINE=InnoDB DEFAULT CHARSET=utf8 ";
			///////////////////////////////////////////////
			sql += "COMMENT='Rev.1';";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}



		///////////////////////////////////////////////////////////////////////
		// create Flotsam Group Table

		private void CreateGroupActiveTable()
		{
			string sql = string.Empty;

			sql  = "CREATE TABLE `" + Table_of_GroupActive + "` (";
  			sql += "`agentid`       varchar(64) NOT NULL DEFAULT '',";
  			sql += "`activegroupid` varchar(64) NOT NULL DEFAULT '',";
  			sql += "PRIMARY KEY (`agentid`)";
			sql += ") Engine=InnoDB DEFAULT CHARSET=utf8 ";
			///////////////////////////////////////////////
			sql += "COMMENT='Rev.1';";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		private void CreateGroupInviteTable()
		{
			string sql = string.Empty;

			sql  = "CREATE TABLE `" + Table_of_GroupInvite + "`(";
  			sql += "`inviteid` varchar(64) NOT NULL DEFAULT '',";
  			sql += "`groupid`  varchar(64) NOT NULL DEFAULT '',";
  			sql += "`roleid`   varchar(64)          DEFAULT NULL,";
  			sql += "`agentid`  varchar(64) NOT NULL DEFAULT '',";
  			sql += "`tmstamp`  bigint(11)  NOT NULL,";
  			sql += "PRIMARY KEY (`inviteid`),";
  			sql += "UNIQUE KEY `groupid` (`groupid`,`roleid`,`agentid`)";
			sql += ") Engine=InnoDB DEFAULT CHARSET=utf8 ";
			///////////////////////////////////////////////
			sql += "COMMENT='Rev.1';";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		private void CreateGroupListTable()
		{
			string sql = string.Empty;

			sql  = "CREATE TABLE `" + Table_of_GroupList + "` (";
  			sql += "`groupid`        varchar(64)  NOT NULL DEFAULT '',";
  			sql += "`name`           varchar(255) NOT NULL DEFAULT '',";
  			sql += "`charter`        longtext     NOT NULL,";
  			sql += "`insigniaid`     varchar(64)  NOT NULL DEFAULT '',";
  			sql += "`founderid`      varchar(64)  NOT NULL DEFAULT '',";
  			sql += "`membershipfee`  bigint(11)   NOT NULL DEFAULT '0',";
  			sql += "`openenrollment` varchar(255) NOT NULL DEFAULT '',";
  			sql += "`showinlist`     tinyint(1)   NOT NULL DEFAULT '0',";
  			sql += "`allowpublish`   tinyint(1)   NOT NULL DEFAULT '0',";
  			sql += "`maturepublish`  tinyint(1)   NOT NULL DEFAULT '0',";
  			sql += "`ownerroleid`    varchar(128) NOT NULL DEFAULT '',";
  			sql += "PRIMARY KEY (`groupid`),";
  			sql += "UNIQUE KEY `name` (`name`)";
			sql += ") Engine=InnoDB DEFAULT CHARSET=utf8 ";
			///////////////////////////////////////////////
			sql += "COMMENT='Rev.1';";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		private void CreateGroupMembershipTable()
		{
			string sql = string.Empty;

			sql  = "CREATE TABLE `" + Table_of_GroupMembership + "` (";
  			sql += "`groupid`        varchar(64) NOT NULL DEFAULT '',";
  			sql += "`agentid`        varchar(64) NOT NULL DEFAULT '',";
  			sql += "`selectedroleid` varchar(64) NOT NULL DEFAULT '',";
  			sql += "`contribution`   bigint(11)  NOT NULL DEFAULT '0',";
  			sql += "`listinprofile`  bigint(11)  NOT NULL DEFAULT '1',";
  			sql += "`acceptnotices`  bigint(11)  NOT NULL DEFAULT '1',";
  			sql += "PRIMARY KEY (`groupid`,`agentid`)";
			sql += ") Engine=InnoDB DEFAULT CHARSET=utf8 ";
			///////////////////////////////////////////////
			sql += "COMMENT='Rev.1';";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		private void CreateGroupNoticeTable()
		{
			string sql = string.Empty;

			sql  = "CREATE TABLE `" + Table_of_GroupNotice + "` (";
  			sql += "`groupid`   varchar(64)  NOT NULL DEFAULT '',";
  			sql += "`noticeid`  varchar(64)  NOT NULL DEFAULT '',";
  			sql += "`timestamp` bigint(11)   NOT NULL DEFAULT '0',";
  			sql += "`fromname`  varchar(255) NOT NULL DEFAULT '',";
  			sql += "`subject`   varchar(255) NOT NULL DEFAULT '',";
  			sql += "`message`   longtext     NOT NULL,";
  			sql += "`binarybucket` longblob NOT NULL,";
  			sql += "PRIMARY KEY (`groupid`,`noticeid`),";
  			sql += "KEY `timestamp` (`timestamp`)";
			sql += ") Engine=InnoDB DEFAULT CHARSET=utf8 ";
			///////////////////////////////////////////////
			sql += "COMMENT='Rev.1';";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		private void CreateGroupRoleTable()
		{
			string sql = string.Empty;

			sql  = "CREATE TABLE `" + Table_of_GroupRole + "` (";
  			sql += "`groupid`     varchar(64)  NOT NULL DEFAULT '',";
  			sql += "`roleid`      varchar(64)  NOT NULL DEFAULT '',";
  			sql += "`name`        varchar(255) NOT NULL DEFAULT '',";
  			sql += "`description` varchar(255) NOT NULL DEFAULT '',";
  			sql += "`title`       varchar(255) NOT NULL DEFAULT '',";
  			sql += "`powers`      bigint(20)   NOT NULL DEFAULT '0',";
  			sql += "PRIMARY KEY (`groupid`,`roleid`)";
			sql += ") Engine=InnoDB DEFAULT CHARSET=utf8 ";
			///////////////////////////////////////////////
			sql += "COMMENT='Rev.1';";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		private void CreateGroupRoleMembershipTable()
		{
			string sql = string.Empty;

			sql  = "CREATE TABLE `" + Table_of_GroupRoleMembership + "` (";
  			sql += "`groupid` varchar(64) NOT NULL DEFAULT '',";
  			sql += "`roleid`  varchar(64) NOT NULL DEFAULT '',";
  			sql += "`agentid` varchar(64) NOT NULL DEFAULT '',";
  			sql += "PRIMARY KEY (`groupid`,`roleid`,`agentid`)";
			sql += ") Engine=InnoDB DEFAULT CHARSET=utf8 ";
			///////////////////////////////////////////////
			sql += "COMMENT='Rev.1';";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}



		///////////////////////////////////////////////////////////////////////
		// update Offline Message Table

		private void UpdateOfflineMessageTable1()
		{
			string sql = string.Empty;

			sql  = "BEGIN;";
			sql += "ALTER TABLE `" + Table_of_OfflineMessage + "` ";
			sql += "COMMENT = 'Rev.2';";
			sql += "COMMIT;";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}



		///////////////////////////////////////////////////////////////////////
		// update Flotsam Group Table

		// GroupActive Table
		private void UpdateGroupActiveTable1()
		{
			string sql = string.Empty;

			sql  = "BEGIN;";
			sql += "ALTER TABLE `" + Table_of_GroupActive + "` ";
			sql += "COMMENT = 'Rev.2';";
			sql += "COMMIT;";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		// GroupInvite Table
		private void UpdateGroupInviteTable1()
		{
			string sql = string.Empty;

			sql  = "BEGIN;";
			sql += "ALTER TABLE `" + Table_of_GroupInvite + "` ";
			sql += "COMMENT = 'Rev.2';";
			sql += "COMMIT;";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		// GroupList Table
		private void UpdateGroupListTable1()
 		{
			string sql = string.Empty;

			sql  = "BEGIN;";
			sql += "ALTER TABLE `" + Table_of_GroupList + "` ";
			sql += "COMMENT = 'Rev.2';";
			sql += "COMMIT;";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		// GroupMembership Table
		private void UpdateGroupMembershipTable1()
 		{
			string sql = string.Empty;

			sql  = "BEGIN;";
			sql += "ALTER TABLE `" + Table_of_GroupMembership + "` ";
			sql += "COMMENT = 'Rev.2';";
			sql += "COMMIT;";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		// GroupNotice Table
		private void UpdateGroupNoticeTable1()
 		{
			string sql = string.Empty;

			sql  = "BEGIN;";
			sql += "ALTER TABLE `" + Table_of_GroupNotice + "` ";
			sql += "COMMENT = 'Rev.2';";
			sql += "COMMIT;";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		// GroupRole Table
		private void UpdateGroupRoleTable1()
 		{
			string sql = string.Empty;

			sql  = "BEGIN;";
			sql += "ALTER TABLE `" + Table_of_GroupRole + "` ";
			sql += "COMMENT = 'Rev.2';";
			sql += "COMMIT;";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}


		// GroupRoleMembership Table
		private void UpdateGroupRoleMembershipTable1()
 		{
			string sql = string.Empty;

			sql  = "BEGIN;";
			sql += "ALTER TABLE `" + Table_of_GroupRoleMembership + "` ";
			sql += "COMMENT = 'Rev.2';";
			sql += "COMMIT;";
			MySqlCommand cmd = new MySqlCommand(sql, dbcon);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}



		///////////////////////////////////////////////////////////////////////
		//

		private Dictionary<string,string> CheckTables()
		{
			Dictionary<string,string> tableDic = new Dictionary<string,string>();

			lock (dbcon) {
				string sql = string.Empty;

				sql = "SELECT TABLE_NAME,TABLE_COMMENT FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=?dbname";
				MySqlCommand cmd = new MySqlCommand(sql, dbcon);
				cmd.Parameters.AddWithValue("?dbname", dbcon.Database);

				using (MySqlDataReader r = cmd.ExecuteReader()) {
					while (r.Read()) {
						try {
							string tableName = (string)r["TABLE_NAME"];
							string comment   = (string)r["TABLE_COMMENT"];
							tableDic.Add(tableName, comment);
						}
						catch (Exception e) {
							throw new Exception("[LOCALMIGRATION DATA]: Error checking tables" + e.ToString());
						}
					}
					r.Close();
				}

				cmd.Dispose();
				return tableDic;
			}
		}
	}

}
