
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CodeDeploy;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.CXAPI;

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
                BucketNameParam = new Amazon.CDK.CfnParameter(this, "BucketName", new CfnParameterProps { MinLength = 2,Default="tdfr-poc-bucket" }),
                ObjectKeyParam = new Amazon.CDK.CfnParameter(this, "ObjectKey", new CfnParameterProps {Default= "TDFRPOCRISLambda/Get-CodeUri-637323964004593538-637323964234356273.zip" })

            });

            var vpc = Vpc.FromLookup(this, "VPC", new VpcLookupOptions
            {
                IsDefault = false,
                VpcName = "tdfrpoc-vpc"
            });

            //var importedSecurityGroup = SecurityGroup.FromSecurityGroupId(this, "securityGroup", "sg-0b8a803012ced8eff", new SecurityGroupImportOptions {Mutable= false}) ;

            //var subnetList = new IVpcSubnet[] { new VpcSubnet { SubnetId = pubSubNets.Subnets.GetValue(0).ToString() }, new VpcSubnet { SubnetId = pubSubNets.Subnets.GetValue(1).ToString() } };
            new CfnOutput(this, "publicsubnet1", new CfnOutputProps
            {
                Value = vpc.PublicSubnets[0].SubnetId.ToString()
            }); ;
            new CfnOutput(this, "publicsubnet2", new CfnOutputProps
            {
                Value = vpc.PublicSubnets[1].SubnetId.ToString()
            }); ;
            new CfnOutput(this, "VPCOut", new CfnOutputProps
            {
                Value = vpc.VpcId
            }); ;
            new CfnOutput(this, "importedSecurityGroupOut", new CfnOutputProps
            {
                Value = vpc.VpcId
            });


            SubnetSelection funcSubnetSelection = new SubnetSelection {                       
              Subnets= vpc.PublicSubnets
            };

            new CfnOutput(this, "subnetSelectionOutOut", new CfnOutputProps
            {
                Value =  funcSubnetSelection.Subnets.ToString()
            });

            var subnet1 = PrivateSubnet.FromSubnetAttributes(this, "Subnet1", new SubnetAttributes { SubnetId = vpc.PublicSubnets[0].SubnetId });
            //var func = new Function(this, "Lambda", new FunctionProps
            //{
            //    Tracing = Tracing.ACTIVE,
            //    Code = lambdaCode,
            //    Timeout = Duration.Seconds(30),
            //    MemorySize = 256,
            //    FunctionName = "GetPallet",
            //    Handler = " DFRPOCRISLambda.GetPalletFunction",
            //    Runtime = Runtime.DOTNET_CORE_3_1,
            //    VpcSubnets = funcSubnetSelection,
            //    Vpc = vpc,
            //    SecurityGroups = funcSecurityGroups
            //});

            var customRole = new Role(this, "Role1", new RoleProps
            {
                RoleName = "TDFRPOCLambdaRole",
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
               ManagedPolicies = new IManagedPolicy[] { ManagedPolicy.FromManagedPolicyArn(this, "managedpolicy1", "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"), ManagedPolicy.FromManagedPolicyArn(this,"managedpolicy2", "arn:aws:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole") }
            });

           /// cdk does not allow lambda to be place in public subnet, so  I have to add vpc manually from console also can not 
            var func = new Function(this, "Lambda", new FunctionProps
            {
                Tracing = Tracing.ACTIVE,
                Code = lambdaCode,
                Timeout = Duration.Seconds(30),
                MemorySize = 256,
                FunctionName = "TDFRPOCRISLambda",
                Handler = "TDFRPOCRISLambda::TDFRPOCRISLambda.GetPalletFunction::GetPallet",
                Runtime = Runtime.DOTNET_CORE_3_1,
                Role= customRole
            });

          
            RestApi api = new LambdaRestApi(this, "TDFRPOCLambdaAPI", new LambdaRestApiProps
            {
                Handler = func,
                RestApiName = "TDFRPOCApiThroughPipeline",
                ApiKeySourceType= ApiKeySourceType.HEADER,
                CloudWatchRole = true,
                Deploy=true,
                EndpointConfiguration= new EndpointConfiguration { Types = new EndpointType[]{EndpointType.EDGE } },
             
                Description = "GetPalletFromRestAPi",
                Proxy=false
            });
            //var pallets = api.Root.AddResource("GetPallet");
            //pallets.AddMethod("GET");
            var pallets = api.Root.AddResource("GetPallet");      
            var pallet = pallets.AddResource("{palletId}");
            pallet.AddMethod("GET");

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