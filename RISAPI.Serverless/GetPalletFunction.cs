using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RISAPI.Database;
using RISAPI.BLL.Services;
using RISAPI.Client;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]


namespace RISAPI.Serverless
{
    public class GetPalletFunction
    {

        public APIGatewayProxyResponse GetPallet(APIGatewayProxyRequest request)
        {
            if (request.PathParameters != null && request.PathParameters.ContainsKey("palletId")
                && int.TryParse(request.PathParameters["palletId"], out var palletId))
            {

                var context = new RISAPIContext();
                var palletService = new PalletService(context);
                var getPalletResponse = palletService.GetPallet(palletId);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = JsonConvert.SerializeObject(getPalletResponse)
                };

            }

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound
            };
        }
    }
}

