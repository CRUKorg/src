using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipeline
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            var lambdaStack = new LambdaStack(app, "LambdaStack", new StackProps
            {
                StackName= "TDFRPOCLambdaStack",
                
                Env = new Amazon.CDK.Environment
                {
                    Account = "026188114773",
                    Region = "eu-west-1",               
                                      
                }                
            }            
                );
            new PipelineStack(app, "PipelineDeployingLambdaStack", new PipelineStackProps{
                Env = new Amazon.CDK.Environment {
                    Account = "026188114773",
                    Region = "eu-west-1"
                },
             LambdaCode= lambdaStack.lambdaCode
            });
              
            app.Synth();
        
        }
    }
}
