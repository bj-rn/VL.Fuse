﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stride.Core.Extensions;
using VL.Stride.Shaders.ShaderFX;

namespace VL.ShaderFXtension
{
    public class GpuValueMember<TIn, TOut> : ShaderNode<TOut>
    {
        private const string ShaderCode = "${resultType} ${resultName} = ${input}.${member};";

        public GpuValueMember(GpuValue<TIn> theInput, string theMember) : base("Member")
        {
            Output = new GpuValue<TOut>("member");

            var sourceCode = ShaderTemplateEvaluator.Evaluate(ShaderCode, new Dictionary<string, string>
            {
                {"resultName", Output.ID},
                {"resultType",TypeHelpers.GetNameForType<TOut>().ToLower()},
                {"input",theInput.ID},
                {"member",theMember}
            });
           Setup(sourceCode, ShaderNodesUtil.BuildInputs(theInput));
        }
    }
}