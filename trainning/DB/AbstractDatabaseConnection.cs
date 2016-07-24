using System;
using System.Collections.Generic;

namespace DatabaseConnection
{
	public abstract class AbstractDatabaseConnection
	{

		// Don't want a virtual Connect, as every DB may require different parameters for a connection.
		// Also there's no need to require all classes to implement a connect.

		public abstract void Disconnect();

		public abstract bool isConnected();


		// Marks all MD5 hashes for Page as "volatile"
		//public abstract Page getPageForUrl(string url); // removed, unneeded
		// needs prefix offset
		// Also mark a page as locked and flag hashes as vulnerable?
		//   I'll assume so for now.


		// This happens when a page needs to be *retrained*, as opposed
		//   to when the bids on a page need to be updated
		//public abstract void markPageForRelable(Page page);
		public abstract void markPageForRetraining(Page page);
		// unlock page, mark all hashes dead, like if markPageAsVisited had been called

		// Don't need the below; since the sampleLink is all we train,
		//   we can mark the page as trained as soon as a sample link and
		//   offset are set.
		//public abstract void markPageAsTrained(Page page);

		public abstract Page addPage(string url, int offset, string sampleLink);

		public abstract void setSampleLink(Page page, int offset, string sampleLink);
		// If a page was marked as needs_training, we'll clear that flag when this function
		//   is called, since the offset and sampleLink are the only things we train.

		//public abstract void setPathToBidRegion(Page page, string path);

		//public abstract void clearSampleLinks(Page page);

		//public abstract void addSampleLinks(Page page, List<string> urls);

		//public abstract List<string> getSampleLinks(Page page);
		// list of urls as strings
		// Should we maybe instead store the pattern that we infer from the sample links
		//   instead of re-computing it each time?


		/*
		//public abstract List<Bid> getUncheckedUrls(string date);
		public abstract List<Page> getUncheckedPages(string date_time);
		//public abstract List<Md5Sums> getMd5sForPage(Page page);
		// bid can be PDF or html Page url

		public abstract bool checkIfMd5New(Page page, string url, string Md5Sum, bool addIfNew = true);
		// adds Md5 if new?
		// also alert!
		// update age of existing MD5 sums
		//   add timestamp to bid, unused for now

		public abstract void markPageAsVisited(Page page);
		*/


		public abstract Page getOldestPage();
		// Marks all MD5 hashes for Page as "volatile"

		public abstract bool checkIfNewHash(Page page, string url, string Md5Hash, bool addIfNew = true);
		// Adds Md5Hash to known hashes if new
		// Adds alert if new
		// Marks seen hashes as "seen"

		public abstract void markPageAsVisited(Page page);
		// If Page has Md5Hashes marked "volatile",
		//   drop those hashes (or move them to a different table, or give them a label like "dead" and ignroe them in future)
		//   alarm on the URLs that pointed to them?
		
		public abstract void clearOldLocks(int max_age_minutes = 10);


		public abstract List<Alert> getAlerts();

		public abstract void clearAlert(Alert alert);


		//public abstract void addBid();

		//public abstract void updateBid();

		//public abstract void removeBid();


		//public abstract List<PdfFile> getPdfFilesOnPage(Page page);

	}
}

