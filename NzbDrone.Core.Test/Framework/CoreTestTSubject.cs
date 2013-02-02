namespace NzbDrone.Core.Test.Framework
{
    public abstract class SqlCeTest<TSubject> : SqlCeTest where TSubject : class
    {

        private TSubject _subject;


        protected TSubject Subject
        {
            get
            {
                if (_subject == null)
                {
                    _subject = Mocker.Resolve<TSubject>();
                }

                return _subject;

            }
        }


        protected void InitiateSubject()
        {
            _subject = Mocker.Resolve<TSubject>();
        }
    }
}
