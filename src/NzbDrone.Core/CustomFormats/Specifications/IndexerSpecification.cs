using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class IndexerSpecificationValidator : AbstractValidator<IndexerSpecification>
    {
        public IndexerSpecificationValidator(/*IIndexerFactory indexerFactory*/)
        {
            RuleFor(c => c.Value).NotEmpty();
            /*
            RuleFor(c => c.Value).Custom((indexerId, context) =>
            {
                if (indexerId != 0 && !indexerFactory.Exists(indexerId))
                {
                    context.AddFailure($"Invalid indexer value: {indexerId}");
                }
            });
            */
        }
    }

    public class IndexerSpecification : CustomFormatSpecificationBase
    {
        private static readonly IndexerSpecificationValidator Validator = new ();

        public override int Order => 11;
        public override string ImplementationName => "Indexer";

        [FieldDefinition(1, Label = "CustomFormatsSpecificationIndexer", Type = FieldType.Select, SelectOptionsProviderAction = "getIndexersList")]
        public int Value { get; set; }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return input.IndexerId == Value;
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
