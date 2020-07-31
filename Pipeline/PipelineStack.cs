using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodeCommit;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.Lambda;
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
                var code =  GitHubSourceAction.
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
                                ["commands"] = "npx cdk synth -o dist"
                            }
                        },
                        ["artifacts"] = new Dictionary<string, object>
                        {
                            ["base-directory"] = "dist",
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

                var lambdaBuild = new PipelineProject(this, "LambdaBuild", new PipelineProjectProps
                {
                    BuildSpec = BuildSpec.FromObject(new Dictionary<string, object>
                    {
                        ["version"] = "0.2",
                        ["phases"] = new Dictionary<string, object>
                        {
                            ["install"] = new Dictionary<string, object>
                            {
                                ["commands"] = new string[]
                                {
                                "cd Lambda",
                                "npm install",
                                "npm install typescript",
                                "dotnet tool install --global Amazon.Lambda.Tools" ,
                                 "dotnet restore"
                                }
                            },
                            ["build"] = new Dictionary<string, string>
                            {
                                ["commands"] = "dotnet lambda package -c TDFRPOC -o /RISAPI.zip"
                            }
                        },
                        ["artifacts"] = new Dictionary<string, object>
                        {
                            ["base-directory"] = "Lambda",
                            ["files"] = new string[]
                            {
                            "index.js",
                            "node_modules/**/*"
                            }
                        }
                    }),
                    Environment = new BuildEnvironment
                    {
                        BuildImage = LinuxBuildImage.STANDARD_2_0
                    }
                });

                var sourceOutput = new Artifact_();
                var cdkBuildOutput = new Artifact_("CdkBuildOutput");
                var lambdaBuildOutput = new Artifact_("LambdaBuildOutput");

                new Amazon.CDK.AWS.CodePipeline.Pipeline(this, "Pipeline", new PipelineProps
                {
                    Stages = new[]
                    {
                    new Amazon.CDK.AWS.CodePipeline.StageProps
                    {
                        StageName = "Source",
                        Actions = new []
                        {
                            new CodeCommitSourceAction(new CodeCommitSourceActionProps
                            {
                                ActionName = "Source",
                                Repository = code,
                                Output = sourceOutput
                            })
                        }
                    },
                    new Amazon.CDK.AWS.CodePipeline.StageProps
                    {
                        StageName = "Build",
                        Actions = new []
                        {
                            new CodeBuildAction(new CodeBuildActionProps
                            {
                                ActionName = "Lambda_Build",
                                Project = lambdaBuild,
                                Input = sourceOutput,
                                Outputs = new [] { lambdaBuildOutput }
                            }),
                            new CodeBuildAction(new CodeBuildActionProps
                            {
                                ActionName = "CDK_Build",
                                Project = cdkBuild,
                                Input = sourceOutput,
                                Outputs = new [] { cdkBuildOutput }
                            })
                        }
                    },
                    new Amazon.CDK.AWS.CodePipeline.StageProps
                    {
                        StageName = "Deploy",
                        Actions = new []
                        {
                            new CloudFormationCreateUpdateStackAction(new CloudFormationCreateUpdateStackActionProps {
                                ActionName = "Lambda_CFN_Deploy",
                                TemplatePath = cdkBuildOutput.AtPath("LambdaStack.template.json"),
                                StackName = "LambdaDeploymentStack",
                                AdminPermissions = true,
                                ParameterOverrides = props.LambdaCode.Assign(lambdaBuildOutput.S3Location),
                                ExtraInputs = new [] { lambdaBuildOutput }
                            })
                        }
                    }
                }
                });
            }
        }
    }