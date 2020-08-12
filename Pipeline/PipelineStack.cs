using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodeCommit;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using System.Collections.Generic;

namespace Pipeline
{
    public class PipelineStackProps : StackProps
    {
        public CfnParametersCode LambdaCode { get; set; }
        public string RepoName { get; set; }
    }

    public class PipelineStack : Stack
    {        
        public PipelineStack(Construct scope, string id, PipelineStackProps props = null) :
            base(scope, id, props)
        {      

            var sourceOutput = new Artifact_();

            var sourceAction = new GitHubSourceAction(new GitHubSourceActionProps
            {
                ActionName = "GitHub_Source",
                Owner = "CRUKorg",
                Repo = "tdfr-poc-api",
                OauthToken = SecretValue.PlainText("tdfr-github-token"),                
                Output = sourceOutput,
                Branch = "master",
                Trigger = GitHubTrigger.WEBHOOK
            }) ;

            var deploymentPipelineArtifactBucket = new Bucket(this, "pipelineArtifactBucket",new BucketProps {
                    BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
                    BucketName= "pipelineartifactbucket",
                    Encryption= BucketEncryption.KMS_MANAGED,
                    RemovalPolicy= RemovalPolicy.DESTROY
            });

            var cdkBuild = new PipelineProject(this, "CDKBuild", new PipelineProjectProps
                {
                    BuildSpec = BuildSpec.FromObject(new Dictionary<string, object>
                    {
                        ["version"] = "0.2",
                        ["phases"] = new Dictionary<string, object>
                        {
                            ["install"] = new Dictionary<string, object>
                            {
                                ["commands"] = "npm install aws-cdk"
                            },
                            ["build"] = new Dictionary<string, object>
                            {
                                ["commands"] = "npx cdk synth -o ./pipeline/cdk.out"
                            }
                        },
                        ["artifacts"] = new Dictionary<string, object>
                        {
                            ["base-directory"] = "./pipeline/cdk.out",
                            ["files"] = new string[]
                        {
                            "LambdaStack.template.json"
                        }
                        }
                    }),
                Environment = new BuildEnvironment
                {
                    BuildImage = WindowsBuildImage.WINDOWS_BASE_2_0
                }
            });

            //var lambdaBuild = new PipelineProject(this, "LambdaBuild", new PipelineProjectProps
            //    {
            //        BuildSpec = BuildSpec.FromObject(new Dictionary<string, object>
            //        {
            //            ["version"] = "0.2",
            //            ["phases"] = new Dictionary<string, object>
            //            {
            //                ["install"] = new Dictionary<string, object>
            //                {
            //                    ["commands"] = new string[]
            //                    {
            //                    "cd TDFRPOCRISLambda",
            //                    "npm install",                              
            //                    "dotnet tool install --global Amazon.Lambda.Tools" ,
            //                    "dotnet tool install --global dotnet-cli-zip",
            //                    }
            //                },
            //                ["Pre_build"] = new Dictionary<string, object>
            //                {
            //                    ["commands"] = new string[]
            //                    {
            //                    "dotnet restore tdfr-poc-api/TDFRPOCRIS.sln"                                 
            //                    }
            //                },
            //                ["build"] = new Dictionary<string, object>
            //                {
            //                    ["commands"] = new string[]
            //                    {                                             
                                
            //                        "dotnet lambda package -c TDFRPOC -o /RISAPI.zip" ,
            //                        "aws s3 cp  ./RISAPI.zip s3://tdfr-poc-bucket"
            //                    }
            //                }                          
            //            },
            //            ["artifacts"] = new Dictionary<string, object>
            //            {
            //                ["base-directory"] = "TDFRPOCRISLambda",
            //                ["files"] = new string[]
            //                {
            //                 "serverless.template.json"
            //                }
            //            }
            //        }),
            //        Environment = new BuildEnvironment
            //        {
            //            BuildImage = LinuxBuildImage.STANDARD_2_0
            //        }
            //    });               
                    var cdkBuildOutput = new Artifact_("CdkBuildOutput");
                    //var lambdaBuildOutput = new Artifact_("LambdaBuildOutput");
                                      
            var pipelineStageBuildProp = new Amazon.CDK.AWS.CodePipeline.StageProps
            {
                StageName = "Build",
                Actions = new[]
                        {
                            //new CodeBuildAction(new CodeBuildActionProps
                            //{
                            //    ActionName = "Lambda_Build",
                            //    Project = lambdaBuild,
                            //    Input = sourceOutput,
                            //    Outputs = new [] { lambdaBuildOutput }
                            //}),
                            new CodeBuildAction(new CodeBuildActionProps
                            {
                                ActionName = "CDK_Build",
                                Project = cdkBuild,
                                Input = sourceOutput,
                                Outputs = new [] { cdkBuildOutput }
                            })
                        }
            };
            var pipelineStageDeployProp = new Amazon.CDK.AWS.CodePipeline.StageProps
            {
                StageName = "Deploy",
                Actions = new[]
                       {
                            new CloudFormationCreateUpdateStackAction(new CloudFormationCreateUpdateStackActionProps {
                                ActionName = "Lambda_CFN_Deploy",
                                TemplatePath = cdkBuildOutput.AtPath("LambdaStack.template.json"),
                                StackName = "LambdaDeploymentStack",
                                AdminPermissions = true
                                //ParameterOverrides = props.LambdaCode.Assign(lambdaBuildOutput.S3Location),
                                //ExtraInputs = new [] { lambdaBuildOutput }
                            })
                        }
            };
           
            var pipelineprop = new PipelineProps
            {
                ArtifactBucket = deploymentPipelineArtifactBucket,
                Stages = new[]
                {
                    new Amazon.CDK.AWS.CodePipeline.StageProps
                    {

                        StageName = "Source",
                        Actions = new []
                        {
                           sourceAction
                        }
                    },pipelineStageBuildProp,pipelineStageDeployProp
                }
            };
            var deploymentPipeline = new Amazon.CDK.AWS.CodePipeline.Pipeline(this, "deploymentPipeline", pipelineprop);



            }
        }
    }