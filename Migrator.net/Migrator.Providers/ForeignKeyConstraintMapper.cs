using Migrator.Framework;

namespace Migrator.Providers
{
	public class ForeignKeyConstraintMapper
	{
		public string SqlForConstraint(ForeignKeyConstraint constraint)
		{
			switch(constraint)
			{
				case ForeignKeyConstraint.Cascade:
                    return "CASCADE";
				case ForeignKeyConstraint.Restrict:
                    return "RESTRICT";
				case ForeignKeyConstraint.SetDefault:
                    return "SET DEFAULT";
				case ForeignKeyConstraint.SetNull:
                    return "SET NULL";
				default:
                    return "NO ACTION";
			}
		}
	}
}
