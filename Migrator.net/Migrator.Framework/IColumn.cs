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

using System.Data;

namespace Migrator.Framework
{
	public interface IColumn
	{
		ColumnProperty ColumnProperty { get; set; }

		string Name { get; set; }

		DbType Type { get; set; }

		int Size { get; set; }

		bool IsIdentity { get; }

		bool IsPrimaryKey { get; }

		object DefaultValue { get; set; }
	}
}