using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OBS.Test
{
    public class MockHttpSession : HttpSessionStateBase
    {
        Dictionary<string, object> _sessionDictionnaty = new Dictionary<string, object>();

        public override object this[string name]
        {
            get { return _sessionDictionnaty[name]; }
            set { _sessionDictionnaty[name] = value; }
        }
    }
}
