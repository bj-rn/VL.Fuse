﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fuse.compute;
using VL.Core;

namespace Fuse.regions
{
    public abstract class AbstractRegion : ShaderNode<GpuVoid>
    {
        protected Group InCall;
        protected Group CrossLinkCall;

        protected int SubContextIds;

        protected AbstractRegion(NodeContext nodeContext, string theName) : base(nodeContext, theName)
        {
            OptionalOutputs = new List<AbstractShaderNode>();
        }


        private static IEnumerable<AbstractShaderNode> ResolveVoidCrossLink(AbstractShaderNode theCrossLink)
        {
            var myResult = new List<AbstractShaderNode>();

            theCrossLink.Children.ForEach(child =>
            {
                if (!(child is AbstractShaderNode input)) return;
                switch (input)
                {
                    case ShaderNode<GpuVoid> shaderNode:
                        myResult.AddRange(ResolveVoidCrossLink(shaderNode));
                        break;
                    default:
                        myResult.Add(input);
                        break;
                }
            });
            return myResult;
        }

        public void SetupRegion(
            ref List<AbstractShaderNode> theInputs,
            ref List<AbstractShaderNode> theOutputs,
            ref List<AbstractShaderNode> theCrossLinks)
        {
            SubContextIds = 0;
            OptionalOutputs.Clear();

            var inputs = theInputs.ToList();
            var outputs = theOutputs.ToList();
            var crossLinks = theCrossLinks.ToList();
            
            if (inputs.Count != outputs.Count)
            {
                SetInputs(inputs);
                return;
            }

            var myCrossLinks = new List<AbstractShaderNode>();
            crossLinks.ForEach(c =>
            {
                if (c == null) return;
                switch (c)
                {
                    case ShaderNode<GpuVoid> shaderNode:
                        myCrossLinks.AddRange(ResolveVoidCrossLink(shaderNode));
                        //myCrossLinks.Add(AbstractCreation.AbstractShaderNodePassThrough(c));
                        break;
                    default:
                        //myCrossLinks.Add(AbstractCreation.AbstractDeclareValueAssigned(c));
                        //myCrossLinks.Add(AbstractCreation.AbstractShaderNodePassThrough(c));
                        myCrossLinks.Add(c);
                        break;
                }
            });

            var myInputs = new List<AbstractShaderNode>();
            var myOutputs = new List<AbstractShaderNode>();

            for (var i = 0; i < outputs.Count; i++)
            {
                switch (outputs[i])
                {
                    case ShaderNode<GpuVoid> shaderNode:
                        var myInputVoid = inputs[i];
                        myInputs.Add(myInputVoid);
                        myOutputs.Add(shaderNode);

                        break;
                    default:
                        var myInput = i >= inputs.Count ? AbstractCreation.AbstractConstant(NodeContext, SubContextIds++, outputs[i], 0f) : inputs[i];
                        var myDeclareValue = AbstractCreation.AbstractDeclareValueAssigned(NodeContext, SubContextIds++, myInput);
                        myInputs.Add(myDeclareValue);

                        var myOutput = i >= outputs.Count
                            ? AbstractCreation.AbstractConstant(NodeContext, SubContextIds++, inputs[i], 0f)
                            : outputs[i];
                        var myAssign = AbstractCreation.AbstractAssignNode(NodeContext, SubContextIds++, myDeclareValue, myOutput);
                        myOutputs.Add(myAssign);

                        break;
                }
            }

            InCall = new Group(ShaderNodesUtil.GetContext(NodeContext, SubContextIds++));
            InCall.SetInput(myInputs);

            CrossLinkCall = new Group(ShaderNodesUtil.GetContext(NodeContext, SubContextIds++));
            CrossLinkCall.SetInput(myCrossLinks);

            for (var i = 0; i < outputs.Count; i++)
            {
                switch (outputs[i])
                {
                    case ShaderNode<GpuVoid> shaderNode:
                        var myInputVoid = myInputs[i];
                        var outputVoid = AbstractCreation.AbstractOutput(NodeContext, SubContextIds++, this, myInputVoid);
                        OptionalOutputs.Add(outputVoid);

                        break;
                    default:
                        var myInput = myInputs[i];
                        var output = AbstractCreation.AbstractOutput(NodeContext, SubContextIds++, this, myInput);
                        OptionalOutputs.Add(output);

                        break;
                }
            }

            theInputs = myInputs;
            theOutputs = myOutputs;
            theCrossLinks = myCrossLinks;
        }

        protected internal override void BuildSource(StringBuilder theSourceBuilder, HashSet<uint> theHashes)
        {
            var nodes = new List<AbstractShaderNode>();
            foreach (var child in Children)
            {
                if (!(child is ShaderTree input))
                {
                    return;
                }

                nodes.Add(input.Node);
                input.Node.BuildSource(theSourceBuilder, theHashes);
            }


            var source = SourceCode;
            if (!string.IsNullOrWhiteSpace(source) && theHashes.Add(HashCode))
            {
                theSourceBuilder.Append("        " + source + Environment.NewLine);
            }
        }

        protected override string SourceTemplate()
        {
            return "";
        }
    }
}