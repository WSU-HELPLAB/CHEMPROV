using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChemProV.Core
{
    public class Expression
    {
        private string m_original;

        private INode m_root;

        private Dictionary<string, double> m_syms = new Dictionary<string, double>();
        
        private Expression(string original, INode root)
        {
            m_original = original;
            m_root = root;
        }

        public int CountVariables()
        {
            int count = 0;
            Stack<INode> nodes = new Stack<INode>();
            nodes.Push(m_root);
            while (nodes.Count > 0)
            {
                INode temp = nodes.Pop();
                if (temp is SymbolNode)
                {
                    count++;
                }
                else if (temp is BinaryOpNode)
                {
                    nodes.Push((temp as BinaryOpNode).Left);
                    nodes.Push((temp as BinaryOpNode).Right);
                }
                else if (temp is NegationNode)
                {
                    nodes.Push((temp as NegationNode).Child);
                }
            }

            return count;
        }

        public static Expression Create(string expression)
        {
            string err;
            return Create(expression, out err);
        }
        
        public static Expression Create(string expression, out string errors)
        {
            // We can't do anything will a null or empty expression
            if (string.IsNullOrEmpty(expression))
            {
                errors = "Cannot parse a null or empty expression";
                return null;
            }

            errors = null;
            INode root = Parse(expression, 0, ref errors);
            if (null == root)
            {
                return null;
            }
            return new Expression(expression, root);
        }

        public double Evaluate()
        {
            return m_root.Evaluate(m_syms);
        }

        private static INode Parse(string expression, int indexInOriginal, ref string errors)
        {
            if (string.IsNullOrEmpty(expression))
            {
                errors = "Cannot parse a null or empty expression";
                return null;
            }

            // Ignore leading or trailing spaces
            if (' ' == expression[0])
            {
                return Parse(expression.Substring(1), indexInOriginal + 1, ref errors);
            }
            if (' ' == expression[expression.Length - 1])
            {
                return Parse(expression.Substring(0, expression.Length - 1), indexInOriginal, ref errors);
            }
            
            int i;
            char[] chars = expression.ToCharArray();
            
            // Incremented when we see '(' and decremented when we see ')'.
            int paren = 0;

            // First check if we start with '(' and end with the matching ')'
            if ('(' == chars[0])
            {
                paren = 1;
                for (i = 1; i < chars.Length - 1; i++)
                {
                    if ('(' == chars[i])
                    {
                        paren++;
                    }
                    else if (')' == chars[i])
                    {
                        paren--;

                        if (paren < 0)
                        {
                            errors = "Unmatched \")\" at index " + (i + indexInOriginal).ToString();
                            return null;
                        }
                    }
                }

                if (')' == chars[i] && 1 == paren)
                {
                    return Parse(expression.Substring(1, chars.Length - 2), indexInOriginal + 1, ref errors);
                }
            }

            char[] ops = new char[] { '+', '-', '*', '/' };
            BinaryOpNode.Type[] types = new BinaryOpNode.Type[] {
                BinaryOpNode.Type.Add, BinaryOpNode.Type.Subtract, 
                BinaryOpNode.Type.Multiply, BinaryOpNode.Type.Divide};

            // Look for binary operators
            for (int index = 0; index < 4; index++)
            {
                for (i = 0; i < chars.Length; i++)
                {
                    if ('(' == chars[i])
                    {
                        paren++;
                    }
                    else if (')' == chars[i])
                    {
                        paren--;

                        if (paren < 0)
                        {
                            errors = "Unmatched \")\" at index " + (i + indexInOriginal).ToString();
                            return null;
                        }
                    }
                    else if (0 == paren && ops[index] == chars[i])
                    {
                        // Make sure the operator character isn't the first or last in the expression
                        if (0 == i)
                        {
                            // Special case for subtraction: ignore this and process it as a negation layer
                            if ('-' == ops[index])
                            {
                                continue;
                            }
                            
                            errors = ops[index].ToString() + " operator is missing left hand side argument";
                            return null;
                        }
                        if (chars.Length - 1 == i)
                        {
                            errors = ops[index].ToString() + " operator is missing right hand side argument";
                            return null;
                        }

                        INode left = Parse(expression.Substring(0, i), indexInOriginal, ref errors);
                        if (null == left)
                        {
                            return null;
                        }
                        INode right = Parse(
                            expression.Substring(i + 1, chars.Length - (i + 1)),
                            indexInOriginal + i + 1,
                            ref errors);
                        if (null == right)
                        {
                            return null;
                        }
                        return new BinaryOpNode(types[index], left, right);
                    }
                }
            }

            // See if we have something that's just a constant numerical value
            double num;
            if (double.TryParse(expression, out num))
            {
                return new ConstantValueNode(num);
            }

            // At this point we have no operators, so if we have a '-' as the first character, then 
            // it is a negation
            if ('-' == chars[0] && chars.Length > 1)
            {
                return new NegationNode(Parse(expression.Substring(1), indexInOriginal + 1, ref errors));
            }

            return new SymbolNode(expression, indexInOriginal);
        }

        public void SetSymbolValue(string symbol, double value)
        {
            m_syms[symbol] = value;
        }

        #region Node declarations

        private interface INode
        {
            double Evaluate(Dictionary<string, double> syms);
        }

        private class BinaryOpNode : INode
        {
            public enum Type
            {
                Add,
                Subtract,
                Multiply,
                Divide
            }

            private INode m_left;

            private INode m_right;

            private Type m_type;

            public BinaryOpNode(Type type, INode left, INode right)
            {
                m_type = type;
                m_left = left;
                m_right = right;
            }

            public double Evaluate(Dictionary<string, double> syms)
            {
                switch (m_type)
                {
                    case Type.Add:
                        return m_left.Evaluate(syms) + m_right.Evaluate(syms);

                    case Type.Divide:
                        return m_left.Evaluate(syms) / m_right.Evaluate(syms);

                    case Type.Multiply:
                        return m_left.Evaluate(syms) * m_right.Evaluate(syms);

                    case Type.Subtract:
                        return m_left.Evaluate(syms) - m_right.Evaluate(syms);

                    default:
                        throw new InvalidOperationException();
                }
            }

            public INode Left
            {
                get { return m_left; }
            }

            public INode Right
            {
                get { return m_right; }
            }
        }

        /// <summary>
        /// Represents a constant numerical value
        /// </summary>
        private class ConstantValueNode : INode
        {
            private double m_value;

            public ConstantValueNode(double value)
            {
                m_value = value;
            }

            public double Evaluate(Dictionary<string, double> syms)
            {
                return m_value;
            }

            public override string ToString()
            {
                return m_value.ToString();
            }
        }

        private class NegationNode : INode
        {
            private INode m_child;

            public NegationNode(INode child)
            {
                m_child = child;
            }

            public INode Child
            {
                get
                {
                    return m_child;
                }
            }

            public double Evaluate(Dictionary<string, double> symbols)
            {
                return -m_child.Evaluate(symbols);
            }

            public override string ToString()
            {
                return "-(" + m_child.ToString() + ")";
            }
        }

        private class SymbolNode : INode
        {
            private int m_index;

            private string m_symbol;

            public SymbolNode(string symbol, int indexInExpression)
            {
                m_index = indexInExpression;
                m_symbol = symbol;
            }

            public double Evaluate(Dictionary<string, double> syms)
            {
                if (!syms.ContainsKey(m_symbol))
                {
                    throw new InvalidOperationException("Symbol \"" + m_symbol + "\" is undefined");
                }

                return syms[m_symbol];
            }

            public override string ToString()
            {
                return m_symbol;
            }
        }

        #endregion
    }
}
