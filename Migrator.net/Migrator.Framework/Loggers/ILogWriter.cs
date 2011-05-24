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
	/// Handles writing a message to the log medium (i.e. file, console)
	/// </summary>
	public interface ILogWriter
	{
		/// <summary>
		/// Write this message
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		void Write(string message, params object[] args);

		/// <summary>
		/// Write this message, as a line
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		void WriteLine(string message, params object[] args);
	}
}
