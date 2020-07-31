using Amazon.CDK;
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

            var lambdaStack = new LambdaStack(app, "LambdaStack");
            new PipelineStack(app, "PipelineDeployingLambdaStack", new PipelineStackProps
            {
                LambdaCode = lambdaStack.lambdaCode,
                RepoName = CODECOMMIT_REPO_NAME
            });
            app.Synth();
        }
    }
}
