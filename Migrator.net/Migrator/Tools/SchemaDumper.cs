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

using System.IO;
using Migrator.Framework;

namespace Migrator.Tools
{
	public class SchemaDumper
	{
	    private readonly ITransformationProvider _provider;
		
		public SchemaDumper(string provider, string connectionString)
		{
			_provider = ProviderFactory.Create(provider, connectionString);
		}
		
		public string Dump()
		{
			StringWriter writer = new StringWriter();
			
			writer.WriteLine("using Migrator;\n");
			writer.WriteLine("[Migration(1)]");
			writer.WriteLine("public class SchemaDump : Migration");
			writer.WriteLine("{");
			writer.WriteLine("\tpublic override void Up()");
			writer.WriteLine("\t{");
			
			foreach (string table in _provider.GetTables())
			{
				writer.WriteLine("\t\tDatabase.AddTable(\"{0}\",", table);
				foreach (Column column in _provider.GetColumns(table))
				{
					writer.WriteLine("\t\t\tnew Column(\"{0}\", typeof({1})),", column.Name, column.Type);
				}
				writer.WriteLine("\t\t);");
			}
			
			writer.WriteLine("\t}\n");
			writer.WriteLine("\tpublic override void Down()");
			writer.WriteLine("\t{");
			
			foreach (string table in _provider.GetTables())
			{
				writer.WriteLine("\t\tDatabase.RemoveTable(\"{0}\");", table);
			}
			
			writer.WriteLine("\t}");
			writer.WriteLine("}");
			
			return writer.ToString();
		}
		
		public void DumpTo(string file)
		{
			using (StreamWriter writer = new StreamWriter(file))
			{
				writer.Write(Dump());
			}
		}
	}
}