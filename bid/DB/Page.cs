using System;

namespace DatabaseConnection
{
	public class Page
	{
		private string _url;
		public string url {
			get { return _url; }
		}

		private int _id;
		public int id {
			get { return _id; }
		}

		private int _prefixOffset;
		public int prefixOffset {
			get { return _prefixOffset; }
		}

		private string _sampleLink;
		public string sampleLink {
			get { return _sampleLink; }
		}


		public Page (string url, int id, int prefixOffset, string sampleLink)
		{
			_url = url;
			_id = id;
			_prefixOffset = prefixOffset;
			_sampleLink = sampleLink;
		}
	}
}

