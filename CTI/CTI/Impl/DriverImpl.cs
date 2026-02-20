using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Zamasoft.CTI.Impl
{
    internal class DriverImpl : Driver 
    {
        public Session getSession(Uri uri, Hashtable props)
        {
            return new SessionImpl(uri, props);
        }
    }
}
