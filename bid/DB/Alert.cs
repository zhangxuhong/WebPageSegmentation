using System;

namespace DatabaseConnection
{
	public class Alert
	{
		private int _hashId;
		public int hashId {
			get { return _hashId; }
		}

		private string _pageUrl;
		public string pageUrl {
			get { return _pageUrl; }
		}

		private DateTime _detected;
		public DateTime detected {
			get { return _detected; }
		}

		private DateTime _alarmed;
		public DateTime alarmed {
			get { return _alarmed; }
		}

		// NB the following StackOverflows:
		//  - Building DateTimes from MySql
		//    http://stackoverflow.com/questions/3633262/convert-datetime-for-mysql-using-c-sharp
		//  - Cloning DateTimes (they're immutable value types, so it's easy but you don't
		//    really need to anyway).
		//    http://stackoverflow.com/questions/4265399/how-can-i-clone-a-datetime-object-in-c


		public Alert(int hashId, string pageUrl, string detectedSqlString, string alarmedSqlString)
		{
			_hashId = hashId;
			_pageUrl = pageUrl;

			// How timestamps appear to look as strings in MySql:
			// 2013-04-05 17:33:59
			_detected = DateTime.Parse(detectedSqlString);
			_alarmed = DateTime.Parse(alarmedSqlString);
		}

		public Alert(int hashId, string pageUrl, DateTime detected, DateTime alarmed)
		{
			_hashId = hashId;
			_pageUrl = pageUrl;

			// How timestamps appear to look as strings in MySql:
			// 2013-04-05 17:33:59
			_detected = detected;
			_alarmed = alarmed;
		}
	}
}

