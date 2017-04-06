using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibXamHelp
{
    public interface IAppInfo
    {
        int AppVersion { get; }
        String AppName { get; }
    }
}  
