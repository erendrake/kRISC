﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kOS
{
    #region Base classes

    public class Opcode
    {
        private static int _lastId = 0;
        private int _id = ++_lastId;
        
        public virtual string Name { get { return string.Empty; } }
        public int Id { get { return _id; } }
        public int DeltaInstructionPointer = 1;
        public string Label = string.Empty;
        public string DestinationLabel;
        public int InstructionId;

        public virtual void Execute(CPU cpu)
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class BinaryOpcode : Opcode
    {
        protected object argument1 = null;
        protected object argument2 = null;

        public override void Execute(CPU cpu)
        {
            argument2 = cpu.PopValue();
            argument1 = cpu.PopValue();

            Calculator calc = GetCalculator();
            object result = ExecuteCalculation(calc);
            cpu.PushStack(result);
        }

        protected Calculator GetCalculator()
        {
            int intCount = 0;
            int doubleCount = 0;
            int stringCount = 0;
            int specialCount = 0;
            int boolCount = 0;

            // convert floats to doubles
            if (argument1 is float) argument1 = Convert.ToDouble(argument1);
            if (argument2 is float) argument2 = Convert.ToDouble(argument2);

            if (argument1 is int) intCount++;
            if (argument1 is double) doubleCount++;
            if (argument1 is string) stringCount++;
            if (argument1 is SpecialValue) specialCount++;
            if (argument1 is bool) boolCount++;
            if (argument2 is int) intCount++;
            if (argument2 is double) doubleCount++;
            if (argument2 is string) stringCount++;
            if (argument2 is SpecialValue) specialCount++;
            if (argument2 is bool) boolCount++;

            if (intCount == 2) return new CalculatorIntInt();
            if (doubleCount == 2) return new CalculatorDoubleDouble();
            if (intCount == 1 && doubleCount == 1) return new CalculatorIntDouble();
            if (stringCount > 0) return new CalculatorString();
            if (boolCount > 0) return new CalculatorBool();
            if (specialCount > 0) return new CalculatorSpecialValue();

            throw new NotImplementedException(string.Format("Can't operate types {0} and {1}", argument1.GetType(), argument2.GetType()));
        }

        protected virtual object ExecuteCalculation(Calculator calc)
        {
            return null;
        }
    }

    #endregion

    #region General

    public class OpcodePush : Opcode
    {
        public object argument;

        public override string Name { get { return "push"; } }

        public OpcodePush(object argument)
        {
            this.argument = argument;
        }

        public override void Execute(CPU cpu)
        {
            cpu.PushStack(argument);
        }

        public override string ToString()
        {
            string argumentString = argument != null ? argument.ToString() : "null";
            return Name + " " + argumentString;
        }
    }

    public class OpcodeStore : Opcode
    {
        public override string Name { get { return "store"; } }

        public override void Execute(CPU cpu)
        {
            object value = cpu.PopValue();
            string identifier = (string)cpu.PopStack();
            cpu.SetValue(identifier, value);
        }
    }

    public class OpcodeEOF : Opcode
    {
        public override string Name { get { return "EOF"; } }
    }

    public class OpcodeEOP : Opcode
    {
        public override string Name { get { return "EOP"; } }
    }

    public class OpcodeNOP : Opcode
    {
        public override string Name { get { return "nop"; } }
    }

    #endregion

    #region Branch

    public class BranchOpcode : Opcode
    {
        public int distance = 0;

        public override string ToString()
        {
            return Name + " " + distance.ToString();
        }
    }

    public class OpcodeBranchIfFalse : BranchOpcode
    {
        public override string Name { get { return "br.false"; } }

        public override void Execute(CPU cpu)
        {
            bool condition = Convert.ToBoolean(cpu.PopValue());
            if (!condition) DeltaInstructionPointer = distance;
            else DeltaInstructionPointer = 1;
        }
    }

    public class OpcodeBranchJump : BranchOpcode
    {
        public override string Name { get { return "jump"; } }

        public override void Execute(CPU cpu)
        {
            DeltaInstructionPointer = distance;
        }
    }

    #endregion

    #region Compare

    public class OpcodeCompareGT : BinaryOpcode
    {
        public override string Name { get { return "gt"; } }

        protected override object ExecuteCalculation(Calculator calc)
        {
            return calc.GreaterThan(argument1, argument2);
        }
    }

    public class OpcodeCompareLT : BinaryOpcode
    {
        public override string Name { get { return "lt"; } }

        protected override object ExecuteCalculation(Calculator calc)
        {
            return calc.LessThan(argument1, argument2);
        }
    }

    public class OpcodeCompareGTE : BinaryOpcode
    {
        public override string Name { get { return "gte"; } }

        protected override object ExecuteCalculation(Calculator calc)
        {
            return calc.GreaterThanEqual(argument1, argument2);
        }
    }

    public class OpcodeCompareLTE : BinaryOpcode
    {
        public override string Name { get { return "lte"; } }

        protected override object ExecuteCalculation(Calculator calc)
        {
            return calc.LessThanEqual(argument1, argument2);
        }
    }

    public class OpcodeCompareEqual : BinaryOpcode
    {
        public override string Name { get { return "eq"; } }

        protected override object ExecuteCalculation(Calculator calc)
        {
            return calc.Equal(argument1, argument2);
        }
    }

    #endregion

    #region Math

    public class OpcodeMathAdd : BinaryOpcode
    {
        public override string Name { get { return "add"; } }

        protected override object ExecuteCalculation(Calculator calc)
        {
            object result = calc.Add(argument1, argument2);
            if (result == null) throw new ArgumentException("Can't add ....");
            return result;
        }
    }

    public class OpcodeMathSubtract : BinaryOpcode
    {
        public override string Name { get { return "sub"; } }

        protected override object ExecuteCalculation(Calculator calc)
        {
            return calc.Subtract(argument1, argument2);
        }
    }

    public class OpcodeMathMultiply : BinaryOpcode
    {
        public override string Name { get { return "mult"; } }

        protected override object ExecuteCalculation(Calculator calc)
        {
            return calc.Multiply(argument1, argument2);
        }
    }

    public class OpcodeMathDivide : BinaryOpcode
    {
        public override string Name { get { return "div"; } }

        protected override object ExecuteCalculation(Calculator calc)
        {
            return calc.Divide(argument1, argument2);
        }
    }

    public class OpcodeMathPower : BinaryOpcode
    {
        public override string Name { get { return "pow"; } }

        protected override object ExecuteCalculation(Calculator calc)
        {
            return calc.Power(argument1, argument2);
        }
    }

    #endregion

    #region Logic

    public class OpcodeLogicToBool : Opcode
    {
        public override string Name { get { return "bool"; } }

        public override void Execute(CPU cpu)
        {
            object value = cpu.PopValue();
            bool result = Convert.ToBoolean(value);
            cpu.PushStack(result);
        }
    }

    public class OpcodeLogicNot : Opcode
    {
        public override string Name { get { return "not"; } }

        public override void Execute(CPU cpu)
        {
            object value = cpu.PopValue();
            object result;

            if (value is bool)
                result = !((bool)value);
            else if (value is int)
                result = -((int)value);
            else if (value is double)
                result = -((double)value);
            else
                throw new ArgumentException(string.Format("Can't negate object {0} of type {1}", value, value.GetType()));

            cpu.PushStack(result);
        }
    }

    public class OpcodeLogicAnd : Opcode
    {
        public override string Name { get { return "and"; } }

        public override void Execute(CPU cpu)
        {
            bool argument2 = Convert.ToBoolean(cpu.PopValue());
            bool argument1 = Convert.ToBoolean(cpu.PopValue());
            object result = argument1 & argument2;
            cpu.PushStack(result);
        }
    }

    public class OpcodeLogicOr : Opcode
    {
        public override string Name { get { return "or"; } }

        public override void Execute(CPU cpu)
        {
            bool argument2 = Convert.ToBoolean(cpu.PopValue());
            bool argument1 = Convert.ToBoolean(cpu.PopValue());
            object result = argument1 | argument2;
            cpu.PushStack(result);
        }
    }

    #endregion

    #region Call

    public class OpcodeCall : Opcode
    {
        public object destination;

        public override string Name { get { return "call"; } }

        public OpcodeCall(object destination)
        {
            this.destination = destination;
        }

        public override void Execute(CPU cpu)
        {
            object functionPointer = cpu.GetValue(destination);
            if (functionPointer is int)
            {
                int currentPointer = cpu.InstructionPointer;
                DeltaInstructionPointer = (int)functionPointer - currentPointer;
                cpu.PushStack(currentPointer + 1);
                cpu.MoveStackPointer(-1);
            }
            else
            {
                if (functionPointer is string)
                {
                    string functionName = (string)functionPointer;
                    functionName = functionName.Substring(0, functionName.Length - 2);
                    cpu.CallBuiltinFunction(functionName);
                }
            }
        }

        public override string ToString()
        {
            return Name + " " + destination.ToString();
        }
    }

    public class OpcodeReturn : Opcode
    {
        public override string Name { get { return "return"; } }

        public override void Execute(CPU cpu)
        {
            cpu.MoveStackPointer(1);
            int destinationPointer = (int)cpu.PopValue();
            int currentPointer = cpu.InstructionPointer;
            DeltaInstructionPointer = destinationPointer - currentPointer;
        }
    }

    #endregion

    #region Stack
    
    public class OpcodeDup : Opcode
    {
        public override string Name { get { return "dup"; } }

        public override void Execute(CPU cpu)
        {
            object value = cpu.PopStack();
            cpu.PushStack(value);
            cpu.PushStack(value);
        }
    }

    public class OpcodeSwap : Opcode
    {
        public override string Name { get { return "swap"; } }

        public override void Execute(CPU cpu)
        {
            object value1 = cpu.PopStack();
            object value2 = cpu.PopStack();
            cpu.PushStack(value1);
            cpu.PushStack(value2);
        }
    }

    #endregion

    #region Wait / Trigger

    public class OpcodeAddTrigger : Opcode
    {
        public bool shouldWait;
        
        public override string Name { get { return "addtrigger"; } }

        public OpcodeAddTrigger(bool shouldWait)
        {
            this.shouldWait = shouldWait;
        }

        public override void Execute(CPU cpu)
        {
            int functionPointer = (int)cpu.PopValue();
            cpu.AddTrigger(functionPointer);
            if (shouldWait)
                cpu.StartWait(0);
        }

        public override string ToString()
        {
            return Name + " " + shouldWait.ToString().ToLower();
        }
    }

    public class OpcodeRemoveTrigger : Opcode
    {
        public override string Name { get { return "removetrigger"; } }

        public override void Execute(CPU cpu)
        {
            int functionPointer = (int)cpu.PopValue();
            cpu.RemoveTrigger(functionPointer);
        }
    }

    public class OpcodeWait : Opcode
    {
        public override string Name { get { return "wait"; } }

        public override void Execute(CPU cpu)
        {
            object waitTime = cpu.PopValue();
            if (waitTime is double)
                cpu.StartWait((double)waitTime);
            else if (waitTime is int)
                cpu.StartWait((int)waitTime);
        }
    }

    public class OpcodeEndWait : Opcode
    {  
        public override string Name { get { return "endwait"; } }

        public override void Execute(CPU cpu)
        {
            cpu.EndWait();
        }
    }

    #endregion

}

