using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class NewPredicate
{
    interface IOperator
    {
        public enum OpType
        {
            SingleParam,
            DoubleParam,
        }
        public OpType getOpType();
        public abstract Symbol eval(Symbol v1);
        public abstract Symbol eval(Symbol v1, Symbol v2);
    }
    class SymbolOperatorEquals : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 2;
        }
        public Symbol eval(Symbol v1) { throw new NotImplementedException(); }
        public Symbol eval(Symbol v1, Symbol v2)
        {
            Type t1 = v1.GetType();
            Type t2 = v2.GetType();

            if (t1.Equals(typeof(SymbolConstantInteger)) && t2.Equals(typeof(SymbolConstantInteger)))
            {
                SymbolConstantInteger i1 = (SymbolConstantInteger)v1;
                SymbolConstantInteger i2 = (SymbolConstantInteger)v2;
                return new SymbolConstantBool(i1.value == i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantBool)) && t2.Equals(typeof(SymbolConstantBool)))
            {
                SymbolConstantBool i1 = (SymbolConstantBool)v1;
                SymbolConstantBool i2 = (SymbolConstantBool)v2;
                return new SymbolConstantBool(i1.value == i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantString)) && t2.Equals(typeof(SymbolConstantString)))
            {
                SymbolConstantString i1 = (SymbolConstantString)v1;
                SymbolConstantString i2 = (SymbolConstantString)v2;
                return new SymbolConstantBool(i1.value == i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantNil)) && t2.Equals(typeof(SymbolConstantNil)))
            {
                return new SymbolConstantBool(true);
            }
            else if (t1.Equals(typeof(SymbolConstantFloat)) && t2.Equals(typeof(SymbolConstantFloat)))
            {
                SymbolConstantFloat i1 = (SymbolConstantFloat)v1;
                SymbolConstantFloat i2 = (SymbolConstantFloat)v2;
                return new SymbolConstantBool(i1.value == i2.value);
            }

            throw new System.Exception("Invalid equals comparison between " + v1.GetType() + " and " + v2.GetType());
        }
        public override void print()
        {
            Debug.Log("SymbolOperatorEquals");
        }
        public override string toString()
        {
            return "==";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.DoubleParam;
        }
    }
    class SymbolOperatorNotEquals : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 2;
        }
        public Symbol eval(Symbol v1) { throw new NotImplementedException(); }
        public Symbol eval(Symbol v1, Symbol v2)
        {
            Type t1 = v1.GetType();
            Type t2 = v2.GetType();

            if (t1.Equals(typeof(SymbolConstantInteger)) && t2.Equals(typeof(SymbolConstantInteger)))
            {
                SymbolConstantInteger i1 = (SymbolConstantInteger)v1;
                SymbolConstantInteger i2 = (SymbolConstantInteger)v2;
                return new SymbolConstantBool(i1.value != i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantBool)) && t2.Equals(typeof(SymbolConstantBool)))
            {
                SymbolConstantBool i1 = (SymbolConstantBool)v1;
                SymbolConstantBool i2 = (SymbolConstantBool)v2;
                return new SymbolConstantBool(i1.value != i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantString)) && t2.Equals(typeof(SymbolConstantString)))
            {
                SymbolConstantString i1 = (SymbolConstantString)v1;
                SymbolConstantString i2 = (SymbolConstantString)v2;
                return new SymbolConstantBool(i1.value != i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantFloat)) && t2.Equals(typeof(SymbolConstantFloat)))
            {
                SymbolConstantFloat i1 = (SymbolConstantFloat)v1;
                SymbolConstantFloat i2 = (SymbolConstantFloat)v2;
                return new SymbolConstantBool(i1.value != i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantNil)) && t2.Equals(typeof(SymbolConstantNil)))
            {
                return new SymbolConstantBool(false);
            }
            throw new System.Exception("Invalid not equals comparison between " + v1.GetType() + " and " + v2.GetType());
        }
        public override void print()
        {
            Debug.Log("SymbolOperatorNotEquals");
        }
        public override string toString()
        {
            return "!=";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.DoubleParam;
        }
    }



    class SymbolOperatorNot : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 7;
        }
        public Symbol eval(Symbol v1)
        {
            Type t1 = v1.GetType();

            if (t1.Equals(typeof(SymbolConstantBool)))
            {
                SymbolConstantBool i1 = (SymbolConstantBool)v1;
                return new SymbolConstantBool(!i1.value);
            }

            throw new System.Exception("Invalid not on " + v1.GetType());
        }
        public Symbol eval(Symbol v1, Symbol v2) { throw new NotImplementedException(); }


        public override void print()
        {
            Debug.Log("SymbolOperatorNot");
        }
        public override string toString()
        {
            return "!";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.SingleParam;
        }
    }

    class SymbolOperatorGreaterThan : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 3;
        }
        public Symbol eval(Symbol v1) { throw new NotImplementedException(); }
        public Symbol eval(Symbol v1, Symbol v2)
        {
            Type t1 = v1.GetType();
            Type t2 = v2.GetType();

            if (t1.Equals(typeof(SymbolConstantInteger)) && t2.Equals(typeof(SymbolConstantInteger)))
            {
                SymbolConstantInteger i1 = (SymbolConstantInteger)v1;
                SymbolConstantInteger i2 = (SymbolConstantInteger)v2;
                return new SymbolConstantBool(i1.value > i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantFloat)) && t2.Equals(typeof(SymbolConstantFloat)))
            {
                SymbolConstantFloat i1 = (SymbolConstantFloat)v1;
                SymbolConstantFloat i2 = (SymbolConstantFloat)v2;
                return new SymbolConstantBool(i1.value > i2.value);
            }
            throw new System.Exception("Invalid greater than comparison between " + v1.GetType() + " and " + v2.GetType());
        }
        public override void print()
        {
            Debug.Log("SymbolOperatorGreaterThan");
        }
        public override string toString()
        {
            return ">";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.DoubleParam;
        }
    }
    class SymbolOperatorGreaterThanOrEqual : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 3;
        }
        public Symbol eval(Symbol v1) { throw new NotImplementedException(); }

        public Symbol eval(Symbol v1, Symbol v2)
        {
            Type t1 = v1.GetType();
            Type t2 = v2.GetType();

            if (t1.Equals(typeof(SymbolConstantInteger)) && t2.Equals(typeof(SymbolConstantInteger)))
            {
                SymbolConstantInteger i1 = (SymbolConstantInteger)v1;
                SymbolConstantInteger i2 = (SymbolConstantInteger)v2;
                return new SymbolConstantBool(i1.value >= i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantFloat)) && t2.Equals(typeof(SymbolConstantFloat)))
            {
                SymbolConstantFloat i1 = (SymbolConstantFloat)v1;
                SymbolConstantFloat i2 = (SymbolConstantFloat)v2;
                return new SymbolConstantBool(i1.value >= i2.value);
            }
            throw new System.Exception("Invalid greater than or equal comparison between " + v1.GetType() + " and " + v2.GetType());
        }
        public override void print()
        {
            Debug.Log("SymbolOperatorGreaterThanOrEqual");
        }
        public override string toString()
        {
            return ">=";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.DoubleParam;
        }
    }
    class SymbolOperatorLesserThan : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 3;
        }
        public Symbol eval(Symbol v1) { throw new NotImplementedException(); }

        public Symbol eval(Symbol v1, Symbol v2)
        {
            Type t1 = v1.GetType();
            Type t2 = v2.GetType();

            if (t1.Equals(typeof(SymbolConstantInteger)) && t2.Equals(typeof(SymbolConstantInteger)))
            {
                SymbolConstantInteger i1 = (SymbolConstantInteger)v1;
                SymbolConstantInteger i2 = (SymbolConstantInteger)v2;
                return new SymbolConstantBool(i1.value < i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantFloat)) && t2.Equals(typeof(SymbolConstantFloat)))
            {
                SymbolConstantFloat i1 = (SymbolConstantFloat)v1;
                SymbolConstantFloat i2 = (SymbolConstantFloat)v2;
                return new SymbolConstantBool(i1.value < i2.value);
            }
            throw new System.Exception("Invalid lesser than comparison between " + v1.GetType() + " and " + v2.GetType());
        }
        public override void print()
        {
            Debug.Log("SymbolOperatorLesserThan");
        }
        public override string toString()
        {
            return "<";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.DoubleParam;
        }
    }
    class SymbolOperatorLesserThanOrEqual : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 3;
        }
        public Symbol eval(Symbol v1) { throw new NotImplementedException(); }

        public Symbol eval(Symbol v1, Symbol v2)
        {
            Type t1 = v1.GetType();
            Type t2 = v2.GetType();

            if (t1.Equals(typeof(SymbolConstantInteger)) && t2.Equals(typeof(SymbolConstantInteger)))
            {
                SymbolConstantInteger i1 = (SymbolConstantInteger)v1;
                SymbolConstantInteger i2 = (SymbolConstantInteger)v2;
                return new SymbolConstantBool(i1.value <= i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantFloat)) && t2.Equals(typeof(SymbolConstantFloat)))
            {
                SymbolConstantFloat i1 = (SymbolConstantFloat)v1;
                SymbolConstantFloat i2 = (SymbolConstantFloat)v2;
                return new SymbolConstantBool(i1.value <= i2.value);
            }
            throw new System.Exception("Invalid lesser than or equal comparison between " + v1.GetType() + " and " + v2.GetType());
        }
        public override void print()
        {
            Debug.Log("SymbolOperatorLesserThanOrEqual");
        }
        public override string toString()
        {
            return "<=";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.DoubleParam;
        }
    }
    class SymbolOperatorAnd : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 1;
        }
        public Symbol eval(Symbol v1) { throw new NotImplementedException(); }

        public Symbol eval(Symbol v1, Symbol v2)
        {
            Type t1 = v1.GetType();
            Type t2 = v2.GetType();

            if (t1.Equals(typeof(SymbolConstantBool)) && t2.Equals(typeof(SymbolConstantBool)))
            {
                SymbolConstantBool i1 = (SymbolConstantBool)v1;
                SymbolConstantBool i2 = (SymbolConstantBool)v2;
                return new SymbolConstantBool(i1.value && i2.value);
            }
            throw new System.Exception("Invalid AND comparison between " + v1.GetType() + " and " + v2.GetType());
        }
        public override void print()
        {
            Debug.Log("SymbolOperatorAnd");
        }
        public override string toString()
        {
            return "&&";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.DoubleParam;
        }
    }

    class SymbolOperatorOr : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 1;
        }
        public Symbol eval(Symbol v1) { throw new NotImplementedException(); }

        public Symbol eval(Symbol v1, Symbol v2)
        {
            Type t1 = v1.GetType();
            Type t2 = v2.GetType();

            if (t1.Equals(typeof(SymbolConstantBool)) && t2.Equals(typeof(SymbolConstantBool)))
            {
                SymbolConstantBool i1 = (SymbolConstantBool)v1;
                SymbolConstantBool i2 = (SymbolConstantBool)v2;
                return new SymbolConstantBool(i1.value || i2.value);
            }
            throw new System.Exception("Invalid OR comparison between " + v1.GetType() + " and " + v2.GetType());
        }
        public override void print()
        {
            Debug.Log("SymbolOperatorOr");
        }
        public override string toString()
        {
            return "||";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.DoubleParam;
        }
    }

    class SymbolOperatorAdd : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 4;
        }
        public Symbol eval(Symbol v1) { throw new NotImplementedException(); }

        public Symbol eval(Symbol v1, Symbol v2)
        {
            Type t1 = v1.GetType();
            Type t2 = v2.GetType();

            if (t1.Equals(typeof(SymbolConstantInteger)) && t2.Equals(typeof(SymbolConstantInteger)))
            {
                SymbolConstantInteger i1 = (SymbolConstantInteger)v1;
                SymbolConstantInteger i2 = (SymbolConstantInteger)v2;
                return new SymbolConstantInteger(i1.value + i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantFloat)) && t2.Equals(typeof(SymbolConstantFloat)))
            {
                SymbolConstantFloat i1 = (SymbolConstantFloat)v1;
                SymbolConstantFloat i2 = (SymbolConstantFloat)v2;
                return new SymbolConstantFloat(i1.value + i2.value);
            }
            throw new System.Exception("Invalid addition between " + v1.GetType() + " and " + v2.GetType());
        }
        public override void print()
        {
            Debug.Log("SymbolOperatorAdd");
        }
        public override string toString()
        {
            return "+";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.DoubleParam;
        }
    }
    class SymbolOperatorSub : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 4;
        }
        public Symbol eval(Symbol v1) { throw new NotImplementedException(); }

        public Symbol eval(Symbol v1, Symbol v2)
        {
            Type t1 = v1.GetType();
            Type t2 = v2.GetType();

            if (t1.Equals(typeof(SymbolConstantInteger)) && t2.Equals(typeof(SymbolConstantInteger)))
            {
                SymbolConstantInteger i1 = (SymbolConstantInteger)v1;
                SymbolConstantInteger i2 = (SymbolConstantInteger)v2;
                return new SymbolConstantInteger(i1.value - i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantFloat)) && t2.Equals(typeof(SymbolConstantFloat)))
            {
                SymbolConstantFloat i1 = (SymbolConstantFloat)v1;
                SymbolConstantFloat i2 = (SymbolConstantFloat)v2;
                return new SymbolConstantFloat(i1.value - i2.value);
            }
            throw new System.Exception("Invalid subtraction between " + v1.GetType() + " and " + v2.GetType());
        }
        public override void print()
        {
            Debug.Log("SymbolOperatorSubSingleParam");
        }
        public override string toString()
        {
            return "-";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.DoubleParam;
        }
    }

    class SymbolOperatorNeg : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 7;
        }
        public Symbol eval(Symbol v1)
        {
            Type t1 = v1.GetType();

            if (t1.Equals(typeof(SymbolConstantInteger)))
            {
                SymbolConstantInteger i1 = (SymbolConstantInteger)v1;
                return new SymbolConstantInteger(-i1.value);
            }
            if (t1.Equals(typeof(SymbolConstantFloat)))
            {
                SymbolConstantFloat i1 = (SymbolConstantFloat)v1;
                return new SymbolConstantFloat(-i1.value);
            }
            throw new System.Exception("Invalid negative of " + v1.GetType());
        }
        public Symbol eval(Symbol v1, Symbol v2) { throw new NotImplementedException(); }

        public override void print()
        {
            Debug.Log("SymbolOperatorSub");
        }
        public override string toString()
        {
            return "-";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.SingleParam;
        }
    }
    class SymbolOperatorMult : Symbol, IOperator
    {
        public override int getPriority()
        {
            return 5;
        }
        public Symbol eval(Symbol v1) { throw new NotImplementedException(); }
        public Symbol eval(Symbol v1, Symbol v2)
        {
            Type t1 = v1.GetType();
            Type t2 = v2.GetType();

            if (t1.Equals(typeof(SymbolConstantInteger)) && t2.Equals(typeof(SymbolConstantInteger)))
            {
                SymbolConstantInteger i1 = (SymbolConstantInteger)v1;
                SymbolConstantInteger i2 = (SymbolConstantInteger)v2;
                return new SymbolConstantInteger(i1.value * i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantFloat)) && t2.Equals(typeof(SymbolConstantFloat)))
            {
                SymbolConstantFloat i1 = (SymbolConstantFloat)v1;
                SymbolConstantFloat i2 = (SymbolConstantFloat)v2;
                return new SymbolConstantFloat(i1.value * i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantFloat)) && t2.Equals(typeof(SymbolConstantInteger)))
            {
                SymbolConstantFloat i1 = (SymbolConstantFloat)v1;
                SymbolConstantInteger i2 = (SymbolConstantInteger)v2;
                return new SymbolConstantFloat(i1.value * i2.value);
            }
            else if (t1.Equals(typeof(SymbolConstantInteger)) && t2.Equals(typeof(SymbolConstantFloat)))
            {
                SymbolConstantInteger i1 = (SymbolConstantInteger)v1;
                SymbolConstantFloat i2 = (SymbolConstantFloat)v2;
                return new SymbolConstantFloat(i1.value * i2.value);
            }

            throw new System.Exception("Invalid multiplication between " + v1.GetType() + " and " + v2.GetType());
        }
        public override void print()
        {
            Debug.Log("SymbolOperatorMult");
        }
        public override string toString()
        {
            return "*";
        }
        public IOperator.OpType getOpType()
        {
            return IOperator.OpType.DoubleParam;
        }
    }

}
