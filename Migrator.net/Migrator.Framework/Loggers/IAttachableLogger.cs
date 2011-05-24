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

namespace Migrator.Framework.Loggers
{
	/// <summary>
	/// ILogger interface. 
	/// Implicit in this interface is that the logger will delegate actual
	/// logging to the <see cref="ILogWriter"/>(s) that have been attached
	/// </summary>
	public interface IAttachableLogger: ILogger
	{
		/// <summary>
		/// Attach an <see cref="ILogWriter"/>
		/// </summary>
		/// <param name="writer"></param>
		void Attach(ILogWriter writer);

		/// <summary>
		/// Detach an <see cref="ILogWriter"/>
		/// </summary>
		/// <param name="writer"></param>
		void Detach(ILogWriter writer);
	}
}
