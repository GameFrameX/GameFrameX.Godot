// Copyright 2025 Code Philosophy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeanAOT.ToCpp
{
    public class BasicBlock
    {
        public readonly List<Instruction> instructions = new List<Instruction>();

        public readonly List<BasicBlock> inBlocks = new List<BasicBlock>();

        public readonly List<BasicBlock> outBlocks = new List<BasicBlock>();

        public BasicBlock nextBlock;

        public uint StartOffset => instructions.First().Offset;


        public void AddTargetBasicBlock(BasicBlock target)
        {
            if (!outBlocks.Contains(target))
            {
                outBlocks.Add(target);
            }
            if (!target.inBlocks.Contains(this))
            {
                target.inBlocks.Add(this);
            }
        }
    }

    public class BasicBlockCollection
    {

        private readonly List<BasicBlock> _blocks = new List<BasicBlock>();
        private readonly Dictionary<Instruction, BasicBlock> _inst2BlockMap = new Dictionary<Instruction, BasicBlock>();

        public IList<BasicBlock> Blocks => _blocks;

        public BasicBlockCollection(CilBody methodBody)
        {
            HashSet<Instruction> splitPoints = BuildSplitPoint(methodBody);
            BuildBasicBlocks(methodBody, splitPoints);
            BuildInOutGraph(methodBody);
        }

        public BasicBlock GetBasicBlockByInstruction(Instruction inst)
        {
            return _inst2BlockMap[inst];
        }

        private HashSet<Instruction> BuildSplitPoint(CilBody method)
        {
            var insts = method.Instructions;
            var splitPoints = new HashSet<Instruction>();
            foreach (ExceptionHandler eh in method.ExceptionHandlers)
            {
                if (eh.TryStart != null)
                {
                    splitPoints.Add(eh.TryStart);
                }
                if (eh.TryEnd != null)
                {
                    splitPoints.Add(eh.TryEnd);
                }
                if (eh.HandlerStart != null)
                {
                    splitPoints.Add(eh.HandlerStart);
                }
                if (eh.HandlerEnd != null)
                {
                    splitPoints.Add(eh.HandlerEnd);
                }
                if (eh.FilterStart != null)
                {
                    splitPoints.Add(eh.FilterStart);
                }
            }

            for (int i = 0, n = insts.Count; i < n; i++)
            {
                Instruction curInst = insts[i];
                Instruction nextInst = i + 1 < n ? insts[i + 1] : null;
                switch (curInst.OpCode.FlowControl)
                {
                case FlowControl.Branch:
                {
                    if (nextInst != null)
                    {
                        splitPoints.Add(nextInst);
                    }
                    splitPoints.Add((Instruction)curInst.Operand);
                    break;
                }
                case FlowControl.Cond_Branch:
                {
                    if (nextInst != null)
                    {
                        splitPoints.Add(nextInst);
                    }
                    if (curInst.Operand is Instruction targetInst)
                    {
                        splitPoints.Add(targetInst);
                    }
                    else if (curInst.Operand is Instruction[] targetInsts)
                    {
                        foreach (var target in targetInsts)
                        {
                            splitPoints.Add(target);
                        }
                    }
                    break;
                }
                case FlowControl.Return:
                {
                    if (nextInst != null)
                    {
                        splitPoints.Add(nextInst);
                    }
                    break;
                }
                case FlowControl.Throw:
                {
                    if (nextInst != null)
                    {
                        splitPoints.Add(nextInst);
                    }
                    break;
                }
                }
            }
            return splitPoints;
        }


        private void BuildBasicBlocks(CilBody method, HashSet<Instruction> splitPoints)
        {
            var insts = method.Instructions;


            BasicBlock curBlock = new BasicBlock();
            foreach (Instruction inst in insts)
            {
                if (splitPoints.Contains(inst) && curBlock.instructions.Count > 0)
                {
                    _blocks.Add(curBlock);
                    BasicBlock preBlock = curBlock;
                    curBlock = new BasicBlock();
                    preBlock.nextBlock = curBlock;
                }
                curBlock.instructions.Add(inst);
                _inst2BlockMap.Add(inst, curBlock);
            }
            if (curBlock.instructions.Count > 0)
            {
                _blocks.Add(curBlock);
            }
        }

        private void BuildInOutGraph(CilBody method)
        {
            var insts = method.Instructions;
            for (int i = 0, n = _blocks.Count; i < n; i++)
            {
                BasicBlock curBlock = _blocks[i];
                BasicBlock nextBlock = i + 1 < n ? _blocks[i + 1] : null;
                Instruction lastInst = curBlock.instructions.Last();
                switch (lastInst.OpCode.FlowControl)
                {
                case FlowControl.Branch:
                {
                    Instruction targetInst = (Instruction)lastInst.Operand;
                    BasicBlock targetBlock = GetBasicBlockByInstruction(targetInst);
                    curBlock.AddTargetBasicBlock(targetBlock);
                    break;
                }
                case FlowControl.Cond_Branch:
                {
                    if (lastInst.Operand is Instruction targetInst)
                    {
                        BasicBlock targetBlock = GetBasicBlockByInstruction(targetInst);
                        curBlock.AddTargetBasicBlock(targetBlock);
                    }
                    else if (lastInst.Operand is Instruction[] targetInsts)
                    {
                        foreach (var target in targetInsts)
                        {
                            BasicBlock targetBlock = GetBasicBlockByInstruction(target);
                            curBlock.AddTargetBasicBlock(targetBlock);
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid operand type for conditional branch");
                    }
                    if (nextBlock != null)
                    {
                        curBlock.AddTargetBasicBlock(nextBlock);
                    }
                    break;
                }
                case FlowControl.Call:
                case FlowControl.Next:
                {
                    if (nextBlock != null)
                    {
                        curBlock.AddTargetBasicBlock(nextBlock);
                    }
                    break;
                }
                case FlowControl.Return:
                case FlowControl.Throw:
                {
                    break;
                }
                default: throw new NotSupportedException($"Unsupported flow control: {lastInst.OpCode.FlowControl} in method");
                }
            }
        }
    }
}
