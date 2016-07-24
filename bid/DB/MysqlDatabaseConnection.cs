using System;
using System.Collections.Generic;
using System.Diagnostics;

using MySql.Data;
using MySql.Data.MySqlClient;


/* FIXME: many of these functions don't check if the page they operate on is locked.
 *        They probably should.
 * FIXME: CheckHash needs to make sure that we actually still have the lock.
 * TODO: Make batch version of AddHash.
 */


namespace DatabaseConnection
{
	public class MysqlDatabaseConnection:AbstractDatabaseConnection
	{
		private MySqlConnection connection;
		
		
		private static int GetProcessId()
		{
			return Process.GetCurrentProcess().Id;
		}
		

		public MysqlDatabaseConnection()
		{
			connection = null;
		}


		/*
		// DEBUG
		private string getConnectionString()
		{
			return "server=localhost;user=bids;database=bids;port=3306;password=burninghatred;";
		}
		*/

		public void Connect(ConnectionConfigurationSource configSource)
		{
			string connectionString = configSource.GetConnectionConfiguration();

			Connect();

			return;
		}

		public void Connect()
		{
			if (isConnected()) {
				Disconnect();
			}

			string connectionString = @"server=localhost;userid=root;password=123456;database=bid_extraction";

			try {
				connection = new MySqlConnection(connectionString);
				connection.Open();
			} catch (Exception e) {
				// TODO handle intelligently
				connection = null;

				throw new Exception("Could not connect to database: " +  e.Message, e);
			}

			return;
		}

		public override void Disconnect()
		{
			if (! isConnected()) {
				return;
			}

			// TODO: do we need to handle pending commands?
			connection.Close();

			connection = null;

			return;
		}

		private void throwIfNotConnected(string operation = null)
		{
			if (! isConnected()) {
				string message;
				if (operation == null) {
					message = "Operation attempted while not connected to database.";
				} else {
					message = "Attempted to " + operation + " while not connected to database.";
				}

				throw new Exception(message);
			}

			return;
		}

		public override bool isConnected()
		{
			if (connection == null) {
				return false;
			}

			return true;
		}


		/* I don't think this is going to work.
		 * I think this may need to happen "in-line" inside transaction sequences.
		public void CheckLock(Page page)
		{
			throwIfNotConnected("check a page lock");

			return;
		}
		*/


		/* Unneeded and annoying
		public override Page getPageForUrl(string url)
		{
			//TODO: come back to this.
			return null;
		}
		*/


		// Create an alert for the page?
		// This happens when a page needs to be *retrained*, as opposed
		//   to when the bids on a page need to be updated
		//
		// NEW: will release a lock on a page if a lock is held.
		//      will mark all 'seen' hashes as 'unseen'.
		//      actually don't *think* we need to do that, since getOldestPage resets all hash statuses.
		public override void markPageForRetraining(Page page)
		{
			throwIfNotConnected("mark a page for retraining");

			string sql = "UPDATE pages SET needs_retraining = true, when_locked = NULL, locked_by_pid = NULL WHERE id = @pid";

			MySqlCommand command = new MySqlCommand(sql, connection);
			command.Parameters.AddWithValue("@pid", page.id);

			command.ExecuteNonQuery();

			// TODO: check db to ensure that this actually happened?
			// TODO: handle locking and unlocking

			return;
		}


		// FIXME: you could use this page to checkHashes, which is inappropriate now (the page isn't locked)
		//        should we lock the page before returning it?
		// TODO: God in Heaven, there is some nasty code here.
		public override Page addPage(string url, int offset, string sampleLink)
		{
			throwIfNotConnected("add a bid-page");

			//string sql = "INSERT INTO pages(page_url, link_prefix_length, sample_link, last_checked, is_locked, needs_retraining) VALUES "
			//	+ "(@url, @preflen, @sample, NULL, false, false);";
			string sql = "INSERT INTO pages(page_url, link_prefix_length, sample_link, last_checked, when_locked, locked_by_pid, needs_retraining) VALUES "
				+ "(@url, @preflen, @sample, NULL, NULL, NULL, false);";
			sql += "\nSELECT LAST_INSERT_ID();"; // http://stackoverflow.com/questions/7230116/returning-last-inserted-id-from-mysql

			MySqlCommand command = new MySqlCommand(sql, connection);
			command.Parameters.AddWithValue("@url", url);
			command.Parameters.AddWithValue("@preflen", offset);
			command.Parameters.AddWithValue("@sample", sampleLink);
			
			// TODO: is ExecuteReader the right call?
			//       changed from ExecuteReader without testing.
			object insertId = command.ExecuteScalar();
			if (insertId == null) {
				// TODO: handle exception (better)
				throw new Exception("Failed to add page to database.");
			}

			// may throw?
			//int i = Convert.ToInt32(id);

			// NOTE: getting everything back from dbase again to make sure
			//       Page object agrees with dbase contents.
			sql = "SELECT page_url, link_prefix_length, sample_link FROM pages WHERE id = @id;";
			command = new MySqlCommand(sql, connection);
			command.Parameters.AddWithValue("@id", insertId); // just throw id back in unconverted?  I hope so.

			MySqlDataReader reader = command.ExecuteReader();

			if (! reader.Read()) {
				throw new Exception("Couldn't find new page in database after adding it (potentially a bad internal error!)");
			}

			//int pageId = Convert.ToInt32(reader[""]); // Hate this very explicit type specification
			// ^ so terrible that Banshee died.

			// FIXME: these can throw!
			Int32 pageId = Convert.ToInt32(insertId); // types!

			string pageUrl = Convert.ToString(reader["page_url"]);

			string pageSampleLink = Convert.ToString(reader["sample_link"]);

			Int32 pagePrefixLength = Convert.ToInt32(reader["link_prefix_length"]); // types!
			
			reader.Close(); // TODO: unchecked if this method exists

			return new Page(pageUrl, pageId, pagePrefixLength, pageSampleLink); // types!  Int32 -> int
		}

		public override void setSampleLink(Page page, int offset, string sampleLink)
		{
			throwIfNotConnected("set the sample link for a page");
			
			string sql = "UPDATE pages SET sample_link = @slink, link_prefix_length = @len WHERE id = @pid;";
			
			MySqlCommand command = new MySqlCommand(sql, connection);
			command.Parameters.AddWithValue("@slink", sampleLink);
			command.Parameters.AddWithValue("@len", offset);
			command.Parameters.AddWithValue("@pid", page.id);
			
			command.ExecuteNonQuery();
			
			// TODO: ensure that this actually happened.
			
			return;
		}


		// Return and lock a page with a NULL last_checked time,
		//   or the oldest last_checked time if no NULLs.
		// Marks all MD5 hashes for Page as "volatile"
		public override Page getOldestPage()
		{
			throwIfNotConnected("get oldest page");

			// Turns out MySql sorts NULLs ahead of values, so I don't need to do any
			//   explicit switching on whether or not NULL-valued last-checked fields exist.


			string sql = "START TRANSACTION;\n"
				+ "SELECT id, page_url, link_prefix_length, sample_link FROM pages WHERE when_locked IS NULL and needs_retraining = FALSE ORDER BY last_checked FOR UPDATE;";

			MySqlCommand command = new MySqlCommand(sql, connection);
			MySqlDataReader reader = command.ExecuteReader();

			// TODO: better exception handling
			if (! reader.HasRows) {
				reader.Close();
				throw new Exception("Database contains no usable pages!");
			}

			reader.Read(); // shouldn't fail if reader.HasRows

			Int32 page_id = Convert.ToInt32(reader["id"]);
			string page_url = Convert.ToString(reader["page_url"]);
			string sample_link = Convert.ToString(reader["sample_link"]);
			Int32 link_prefix_length = Convert.ToInt32(reader["link_prefix_length"]);

			Page page = new Page(page_url, page_id, link_prefix_length, sample_link);

			reader.Close();

			// Mark the page as checked-out before returning it.
			
			int process_id = GetProcessId();
			
			sql = "UPDATE pages SET when_locked = NOW(), locked_by_pid = @proc_id where ID = @pid;\n"
				+ "UPDATE hashes SET status = 'unseen' WHERE status != 'dead' AND page_id = @pid;\n"
				+ "COMMIT;";

			MySqlCommand lockCommand = new MySqlCommand(sql, connection);
			lockCommand.Parameters.AddWithValue("@pid", page.id);
			lockCommand.Parameters.AddWithValue("@proc_id", process_id);

			lockCommand.ExecuteNonQuery();

			return page;
		}

		// Adds Md5Hash to known hashes if new
		// Adds alert if new
		// Marks seen hashes as "seen"
		public override bool checkIfNewHash(Page page, string url, string Md5Hash, bool addIfNew = true)
		{
			throwIfNotConnected("check if hash existed");
			
			// TODO: refactor, recall email sent to Xuhong.
			
			/* See if the hash exists (SELECT)
			 * if it exists
			 *   Mark it seen
			 * else
			 *   Insert it (marked seen)
			 *   Add an alert on that hash (and url and page?)
			 * 
			 * Is there a quicker way to do this, possibly pushing this logic
			 *   into the SQL server?
			 * 
			 * It looks like there is, using ON DUPLICATE KEY UPDATE.
			 * It's a little thorny, and shouldn't be used with tables with more than one
			 *   index (which our's doesn't have).
			 * 
			 * However, it looks like there isn't an easy way to find out if you INSERTED a new
			 *   row or UPDATED an existing row, so it might be necessary to just do this in three
			 *   steps. :/
			 * There also ends up not being a particularly easy way to figure out the ID of the row
			 *   that was updated (I think), at least no without doing more work than just putting
			 *   the switching logic in my code.
			 * 
			 * Note that, as of right now, I don't check which URL led to a given hash, and I have no
			 *   particular way to do that.
			 * TODO: I need to fix that.
			 */
			
			// TODO: should probably use a transaction to avoid having an
			//       exception thrown in the middle of this fuck up our DB.


			// make sure that we still have the lock
			// TODO: should probably actually use a transaction and a lock.
			string sql = "SELECT when_locked, locked_by_pid FROM pages WHERE id = @page_id;";
			MySqlCommand lockCheckCommand = new MySqlCommand(sql, connection);
			lockCheckCommand.Parameters.AddWithValue("@page_id", page.id);

			MySqlDataReader lockCheckReader = lockCheckCommand.ExecuteReader();

			if (! lockCheckReader.HasRows) {
				throw new Exception("Attempted to check has for non-existant page.");
			}

			lockCheckReader.Read();

			if (Convert.IsDBNull(lockCheckReader["when_locked"])) {
				throw new Exception("Attempted to check hash for page that was not locked.");
			}

			if (Convert.IsDBNull(lockCheckReader["locked_by_pid"]) || Convert.ToInt32(lockCheckReader["locked_by_pid"]) != Convert.ToInt32(GetProcessId())) {
				throw new Exception("Attempted to check hash for page that current process has not locked.");
			}

			lockCheckReader.Close();


			sql = "SELECT id FROM hashes WHERE page_id = @pid AND hash = @hash AND url = @url;";
			
			MySqlCommand checkCommand = new MySqlCommand(sql, connection);
			checkCommand.Parameters.AddWithValue("@pid", page.id);
			checkCommand.Parameters.AddWithValue("@hash", Md5Hash);
			checkCommand.Parameters.AddWithValue("@url", url);

			//Console.WriteLine(checkCommand.CommandText);
			
			MySqlDataReader checkReader = checkCommand.ExecuteReader();
			
			/*
			if (! reader.Read()) { // MARK: what does this do with NULL result?
				throw new Exception("Couldn't find new page in database after adding it (potentially a bad internal error!)");
			}
			*/
		
			bool hashIsNew = true;
			
			// MARK: THIS IS PROBABLY WRONG
			if (checkReader.HasRows) {
				hashIsNew = false; // if the hash existed, it isn't new.
				
				// mark the hash as seen
				
				checkReader.Read();
				object hashId = checkReader["id"];
				checkReader.Close();
				
				sql = "UPDATE hashes SET status = 'seen', detected = NOW() WHERE id = @id;";
				MySqlCommand updateCommand = new MySqlCommand(sql, connection);
				updateCommand.Parameters.AddWithValue("@id", hashId);
				
				updateCommand.ExecuteNonQuery();
			} else {
				//Console.WriteLine("Values " + Convert.ToString(Md5Hash) + " " + Convert.ToString(url) + " not found.");
				hashIsNew = true;
				
				checkReader.Close();
				
				// add the hash, and set alert on it
				
				sql = "INSERT INTO hashes(page_id, hash, status, url, alarmed, detected) VALUES \n"
                      + "(@pid, @hash, 'seen', @url, NOW(), NOW());";
				
				MySqlCommand insertCommand = new MySqlCommand(sql, connection);
				insertCommand.Parameters.AddWithValue("@pid", page.id);
				insertCommand.Parameters.AddWithValue("@hash", Md5Hash);
				insertCommand.Parameters.AddWithValue("@url", url);
				
				insertCommand.ExecuteNonQuery();
			}
			
			return hashIsNew;
		}

		// If Page has Md5Hashes marked "volatile",
		//   drop those hashes (or move them to a different table, or give them a label like "dead" and ignroe them in future)
		//   alarm on the URLs that pointed to them?
		//
		// To account for changes to check-out system, we'll make sure that we *still* have this page locked.
		public override void markPageAsVisited(Page page)
		{
			throwIfNotConnected("mark a page as visited");
			
			// Ensure that we still have the lock.
			string sql = "START TRANSACTION;\n" + 
				         "SELECT when_locked, locked_by_pid FROM pages WHERE id = @page_id FOR UPDATE;";
			MySqlCommand command = new MySqlCommand(sql, connection);
			command.Parameters.AddWithValue("@page_id", page.id);
			
			MySqlDataReader reader = command.ExecuteReader();

			if (! reader.HasRows) {
				reader.Close();
				throw new Exception("Requested page not present in database.");
			}

			reader.Read();
			
			if (Convert.IsDBNull(reader["when_locked"])) {
				throw new Exception("Lost lock on specified page.");
			}
			
			if (Convert.ToInt32(reader["locked_by_pid"]) != Convert.ToInt32(GetProcessId())) {
				throw new Exception("Lost lock on specified page.");
			}
			
			reader.Close();
			

			sql = "UPDATE hashes SET status = 'dead' WHERE status = 'unseen' AND page_id = @pid;";
			
			command = new MySqlCommand(sql, connection);
			command.Parameters.AddWithValue("@pid", page.id);
			
			command.ExecuteNonQuery();
			
			
			sql = "UPDATE pages SET when_locked = NULL, locked_by_pid = NULL, last_checked = NOW() WHERE id = @pid;\n" + 
				  "COMMIT;";
			
			command = new MySqlCommand(sql, connection);
			command.Parameters.AddWithValue("@pid", page.id);
			
			command.ExecuteNonQuery();
			
			return;
		}

		public override void clearOldLocks(int max_age_minutes = 10)
		{
			string sql = "START TRANSACTION;\n" + 
					     "UPDATE pages SET when_locked = NULL, locked_by_pid = NULL WHERE when_locked < (NOW() - INTERVAL @min MINUTE);\n" +
					     "COMMIT;";
			
			MySqlCommand command = new MySqlCommand(sql, connection);
			command.Parameters.AddWithValue("@min", max_age_minutes);
			
			command.ExecuteNonQuery();
			
			return;
		}
			


		// TODO: add locking to getAlert and clearAlert
		public override List<Alert> getAlerts()
		{
			throwIfNotConnected("get a list of alerts");

			string sql = "SELECT id, page_id, hash, url, alarmed, detected FROM hashes WHERE alarmed IS NOT NULL;";

			MySqlCommand command = new MySqlCommand(sql, connection);

			MySqlDataReader reader = command.ExecuteReader();

			if (! reader.HasRows) {
				reader.Close();
				return null; // Or an empty list?
			}

			List<Alert> alerts = new List<Alert>();

			while (reader.Read()) {
				Int32 hash_id = Convert.ToInt32(reader["id"]);
				string url = Convert.ToString(reader["url"]);

				string detectedString = Convert.ToString(reader["detected"]);
				string alarmedString = Convert.ToString(reader["alarmed"]);

				Alert currAlert = new Alert(hash_id, url, detectedString, alarmedString);

				alerts.Add(currAlert);
			}

			reader.Close();

			return alerts;
		}

		public override void clearAlert(Alert alert)
		{
			throwIfNotConnected("clear an alert");

			string sql = "UPDATE hashes SET alarmed = NULL WHERE id = @hid;";

			MySqlCommand command = new MySqlCommand(sql, connection);
			command.Parameters.AddWithValue("@hid", alert.hashId);

			command.ExecuteNonQuery();

			return;
		}
	}
}

