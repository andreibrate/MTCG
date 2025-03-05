using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Http.Endpoints
{
    internal interface IHttpEndpoint
    {
        bool HandleRequest(HttpRequest request, HttpResponse response);
    }
}
