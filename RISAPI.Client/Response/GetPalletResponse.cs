using System;
using System.Collections.Generic;
using System.Text;

namespace RISAPI.Client.Response
{
    public class GetPalletResponse
    {
        public decimal Weight { get; set; }
        public SizeDto Size { get; set; }
    }
}
