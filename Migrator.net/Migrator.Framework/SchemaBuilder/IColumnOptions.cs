using System.Data;

namespace Migrator.Framework.SchemaBuilder
{
	public interface IColumnOptions
	{
		SchemaBuilder OfType(DbType dbType);

		SchemaBuilder WithSize(int size);

		IForeignKeyOptions AsForeignKey();
	}
}