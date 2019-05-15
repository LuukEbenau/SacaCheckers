using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalLayer.Api
{
    public enum ApiPath
    {
        Login = 1,
        Register,
        Logout,

        AccountExists,
        GetAccountID,

        RegisterDevice,
        DeviceExists,

        PollStillLoggedIn
    }
}
