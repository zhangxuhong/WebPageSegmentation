using System;

namespace DatabaseConnection
{
	public abstract class ConnectionConfigurationSource
	{
		public abstract string GetConnectionConfiguration();
	}
}

