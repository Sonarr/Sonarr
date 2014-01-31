using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public static class MigrationExtension
    {
        public static ICreateTableColumnOptionOrWithColumnSyntax TableForModel(this ICreateExpressionRoot expressionRoot, string name)
        {
            return expressionRoot.Table(name).WithColumn("Id").AsInt32().PrimaryKey().Identity();
        }

        public static void AddParameter(this System.Data.IDbCommand command, object value)
        {
            var parameter = command.CreateParameter();
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
}