﻿using System;
using System.Collections.Generic;

namespace VL.ShaderFXtension.Tests
{
    public class TestControl
    {
        public static void TestBooleanOperator()
        {
            var gpuValue0 = new GPUInput<float>();
            var gpuValue1 = new GPUInput<float>();
            
            var compare = new OperatorNode<float, bool>(gpuValue0.Output, gpuValue1.Output,new ConstantValue<bool>(true),">");
            Console.WriteLine(compare.BuildSourceCode());
            
            var compareNull = new OperatorNode<float, bool>(gpuValue0.Output, null,new ConstantValue<bool>(true),">");
            Console.WriteLine(compareNull.BuildSourceCode());
        }
        
        public static void TestBooleanSwitch()
        {
            var gpuValue0 = new GPUInput<float>();
            var gpuValue1 = new GPUInput<float>();
            
            var compare = new OperatorNode<float, bool>(gpuValue0.Output, null,new ConstantValue<bool>(true),">");
            
            var switchVal = new BooleanSwitchNode<float>(compare.Output, gpuValue0.Output, gpuValue1.Output,new ConstantValue<float>(0));
            Console.WriteLine(switchVal.BuildSourceCode());
            
            var switchValNull = new BooleanSwitchNode<float>(compare.Output, gpuValue0.Output, null,new ConstantValue<float>(0));
            Console.WriteLine(switchValNull.BuildSourceCode());
        }
        
        public static void TestNumericSwitch()
        {    
            var gpuValueCheck = new GPUInput<int>();
            var gpuValue0 = new GPUInput<float>();
            var gpuValue1 = new GPUInput<float>();
            var gpuValue2 = new GPUInput<float>();
            var gpuValue3 = new GPUInput<float>();
            
            var compare = new OperatorNode<float, bool>(gpuValue0.Output, gpuValue1.Output,new ConstantValue<bool>(false),">");
            Console.WriteLine(compare.BuildSourceCode());
            
            var switchVal = new NumericSwitchNode<float>(gpuValueCheck.Output, new List<GpuValue<float>>(){gpuValue0.Output, gpuValue1.Output, gpuValue2.Output}, gpuValue3.Output);
            Console.WriteLine(switchVal.BuildSourceCode());
        }
    }
}