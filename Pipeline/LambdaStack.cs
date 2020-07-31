
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CodeDeploy;
using Amazon.CDK.AWS.Lambda;
using RISAPI.Serverless;

namespace Pipeline
{
    public class LambdaStack : Stack
    {
        public readonly CfnParametersCode lambdaCode;

        public LambdaStack(Construct scope, string id, StackProps props = null) :
            base(scope, id, props)
        {
            lambdaCode = Code.FromCfnParameters(new CfnParametersCodeProps
            {
                BucketNameParam = new Amazon.CDK.CfnParameter(this, "BucketName", new CfnParameterProps { MinLength = 2 }),
                ObjectKeyParam = new Amazon.CDK.CfnParameter(this, "ObjectKey", new CfnParameterProps { MinValue = 2 })

            });

            var func = new Function(this, "Lambda", new FunctionProps
            {
                Tracing = Tracing.ACTIVE,
                Code = lambdaCode ,
                Timeout = Duration.Seconds(30),
                MemorySize= 256,
                FunctionName= "GetPallet",             
                Handler = "func.handler",
                Runtime = Runtime.DOTNET_CORE_3_1
            });
            new LambdaRestApi(this, "Endpoint", new LambdaRestApiProps
            {
                Handler = func
            });  
            //var version = func.LatestVersion;
            //var alias = new Alias(this, "LambdaAlias", new AliasProps
            //{
            //    AliasName = "Prod",
            //    Version = version
            //});

            //new LambdaDeploymentGroup(this, "DeploymentGroup", new LambdaDeploymentGroupProps
            //{
            //    Alias = alias,
            //    DeploymentConfig = LambdaDeploymentConfig.LINEAR_10PERCENT_EVERY_1MINUTE
            //});
        }
    }
}