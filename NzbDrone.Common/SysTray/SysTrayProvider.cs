using System.Linq;
using System.Windows.Forms;

namespace NzbDrone.Common.SysTray
{
    public class SysTrayProvider
    {
        private readonly SysTrayApp _sysTrayApp;

        public SysTrayProvider(SysTrayApp sysTrayApp)
        {
            _sysTrayApp = sysTrayApp;
        }

        public SysTrayProvider()
        {
        }

        public virtual void Start()
        {
            _sysTrayApp.Create();

            Application.Run(_sysTrayApp);
        }
    }
}