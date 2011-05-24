#region License
//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.
#endregion

using System;

namespace Migrator.Framework
{
	/// <summary>
	/// Base class for migration errors.
	/// </summary>
	public class MigrationException : Exception
	{
	    public MigrationException(string message)
			: base(message) {}
			
		public MigrationException(string message, Exception cause)
			: base(message, cause) {}
			
		public MigrationException(string migration, int version, Exception innerException)
			: base(String.Format("Exception in migration {0} (#{1})", migration, version), innerException) {}
	}
}
