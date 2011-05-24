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

namespace Migrator.Framework.SchemaBuilder
{
	public class ForeignKey
	{
		private string _primaryTable;
		private string _primaryKey;

		public ForeignKey(string primaryTable, string primaryKey)
		{
			_primaryTable = primaryTable;
			_primaryKey = primaryKey;
		}

		public string PrimaryTable
		{
			get { return _primaryTable; }
			set { _primaryTable = value; }
		}

		public string PrimaryKey
		{
			get { return _primaryKey; }
			set { _primaryKey = value; }
		}
	}
}