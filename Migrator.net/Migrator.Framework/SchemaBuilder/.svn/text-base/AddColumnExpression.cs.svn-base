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
	public class AddColumnExpression : ISchemaBuilderExpression
	{
		private IFluentColumn _column;
		private string _toTable;


		public AddColumnExpression(string toTable, IFluentColumn column)
		{
			_column = column;
			_toTable = toTable;
		}
		public void Create(ITransformationProvider provider)
		{
			provider.AddColumn(_toTable, _column.Name, _column.Type, _column.Size, _column.ColumnProperty, _column.DefaultValue);

			if (_column.ForeignKey != null)
			{
				provider.AddForeignKey(
					"FK_" + _toTable + "_" + _column.Name + "_" + _column.ForeignKey.PrimaryTable + "_" +
					_column.ForeignKey.PrimaryKey,
					_toTable, _column.Name, _column.ForeignKey.PrimaryTable, _column.ForeignKey.PrimaryKey, _column.Constraint);
			}
		}
	}
}