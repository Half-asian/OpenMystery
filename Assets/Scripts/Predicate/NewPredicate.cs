using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Assertions;
using System.Globalization;
public partial class NewPredicate
{



    interface IConstant
    {
    }

    abstract class Symbol
    {
        public abstract void print();
        public abstract string toString();
        public abstract int getPriority();
    }

    class SymbolConstantInteger : Symbol, IConstant //Identified by first char being a number
    {
        public int value;
        public override int getPriority()
        {
            return 0;
        }
        public SymbolConstantInteger(int _value)
        {
            value = _value;
        }
        public override void print()
        {
            Debug.Log("SymbolConstantInteger " + value);
        }
        public override string toString()
        {
            return value + " ";
        }
    }

    class SymbolConstantFloat : Symbol, IConstant //Identified by first char being a number
    {
        public float value;
        public override int getPriority()
        {
            return 0;
        }
        public SymbolConstantFloat(float _value)
        {
            value = _value;
        }
        public override void print()
        {
            Debug.Log("SymbolConstantFloat " + value);
        }
        public override string toString()
        {
            return value + " ";
        }
    }

    class SymbolConstantBool : Symbol, IConstant
    {
        public bool value;
        public override int getPriority()
        {
            return 0;
        }
        public SymbolConstantBool(bool _value)
        {
            value = _value;
        }
        public override void print()
        {
            Debug.Log("SymbolConstantBool " + value);
        }
        public override string toString()
        {
            return value + "";
        }
    }

    class SymbolConstantString : Symbol, IConstant
    {
        public string value;
        public override int getPriority()
        {
            return 0;
        }
        public SymbolConstantString(string _value)
        {
            value = _value;
        }
        public override void print()
        {
            Debug.Log("SymbolConstantString " + value);
        }
        public override string toString()
        {
            return value;
        }
        public static Dictionary<string, Delegate> functions = new Dictionary<string, Delegate>
        {
            { "find"                , new Func<SymbolConstantString, SymbolConstantString, SymbolConstantInteger, SymbolConstantBool, SymbolConstantInteger>(find) }

        };

        static SymbolConstantInteger find(SymbolConstantString base_string, SymbolConstantString pattern, SymbolConstantInteger start_index, SymbolConstantBool plain)
        {
            if (plain.value == false)
            {
                throw new Exception("String.Find doesn't have nonplain search available");
            }
            int index = base_string.value.IndexOf(pattern.value, start_index.value, StringComparison.Ordinal);
            return new SymbolConstantInteger(index);
        }
    }

    class SymbolConstantSeparator : Symbol, IConstant
    {
        public SymbolConstantSeparator()
        {
        }
        public override int getPriority()
        {
            return 0;
        }
        public override void print()
        {
            Debug.Log("SymbolConstantSeperator");
        }
        public override string toString()
        {
            return ",";
        }
    }

    class SymbolConstantNil : Symbol, IConstant
    {
        public override int getPriority()
        {
            return 0;
        }
        public override void print()
        {
            Debug.Log("SymbolConstantNil");
        }
        public override string toString()
        {
            return "Nil";
        }
    }


    class SymbolBrackets : Symbol
    {
        public SymbolBrackets parent;
        public  List<Symbol> children;
        public string function_name = null;
        public override int getPriority()
        {
            if (function_name != null && function_name.StartsWith(":"))
                return 6;
            return 7;
        }

        public SymbolBrackets(SymbolBrackets _parent)
        {
            parent = _parent;
            children = new List<Symbol>();
        }

        public void Add(Symbol child)
        {
            children.Add(child);
        }

        public override void print()
        {
            Debug.Log(toString());
        }
        public override string toString()
        {
            string str = "";

            if (function_name != null)
                str += function_name + "(";
            else
                str += "(";
            foreach (Symbol s in children)
                str += " " + s.toString();
            str += ")";
            return str;
        }
    }

    static void parseChar(ref string predicate, ref string char_buf, ref SymbolBrackets symbols, ref int string_pointer, bool is_last_char)
    {
        if (char_buf == " ")
        {
            char_buf = "";
            return;
        }

        //BRACKET DETECTION

        if (char_buf.EndsWith("(")) //brackets!
        {
            SymbolBrackets s = new SymbolBrackets(symbols);
            symbols.Add(s);
            symbols = s;
            if (!String.IsNullOrEmpty(char_buf.Substring(0, char_buf.Length - 1)))
                s.function_name = char_buf.Substring(0, char_buf.Length - 1);
            char_buf = "";
            return;
        }
        else if (char_buf.Trim() == ")") //no more brackets!
        {
            symbols = symbols.parent;
            char_buf = "";
            return;
        }

        //OPERATOR DETECTION

        if (char_buf.Trim() == "+")
        {
            symbols.Add(new SymbolOperatorAdd());
            char_buf = "";
            return;
        }

        if (char_buf.Trim() == "-")
        {
            if (is_last_char == true || predicate[string_pointer + 1] != ' ')

                symbols.Add(new SymbolOperatorNeg());
            else
                symbols.Add(new SymbolOperatorSub());
            char_buf = "";
            return;
        }

        if (char_buf.Trim() == "*")
        {
            symbols.Add(new SymbolOperatorMult());
            char_buf = "";
            return;
        }

        if (char_buf.Trim() == ",")
        {
            symbols.Add(new SymbolConstantSeparator());
            char_buf = "";
            return;
        }

        if (char_buf.Trim() == "==")
        {
            symbols.Add(new SymbolOperatorEquals());
            char_buf = "";
            return;
        }

        if (char_buf.Trim().ToLower() == "not")
        {
            symbols.Add(new SymbolOperatorNot());
            char_buf = "";
            return;
        }

        if (char_buf.Trim() == "~=")
        {
            symbols.Add(new SymbolOperatorNotEquals());
            char_buf = "";
            return;
        }

        if (char_buf.ToLower() == "false")
        {
            symbols.Add(new SymbolConstantBool(false));
            char_buf = "";
            return;
        }
        if (char_buf.ToLower() == "true")
        {
            symbols.Add(new SymbolConstantBool(true));
            char_buf = "";
            return;
        }

        if (char_buf.Trim().ToLower() == "and")
        {
            if (is_last_char == true || predicate[string_pointer + 1] == ' ')
            {
                symbols.Add(new SymbolOperatorAnd());
                char_buf = "";
                return;
            }
        }

        if (char_buf.Trim().ToLower() == "or")
        {
            if (is_last_char == true || predicate[string_pointer + 1] == ' ')
            {
                symbols.Add(new SymbolOperatorOr());
                char_buf = "";
                return;
            }
        }

        if (char_buf.Trim() == ">")
        {
            if (is_last_char == true || predicate[string_pointer + 1] != '=')
            {
                symbols.Add(new SymbolOperatorGreaterThan());
                char_buf = "";
                return;
            }
        }

        if (char_buf.Trim() == ">=")
        {
            symbols.Add(new SymbolOperatorGreaterThanOrEqual());
            char_buf = "";
            return;
        }

        if (char_buf.Trim() == "<")
        {
            if (is_last_char == true || predicate[string_pointer + 1] != '=')
            {
                symbols.Add(new SymbolOperatorLesserThan());
                char_buf = "";
                return;
            }
        }

        if (char_buf.Trim() == "<=")
        {
            symbols.Add(new SymbolOperatorLesserThanOrEqual());
            char_buf = "";
            return;
        }

        //LAST CHAR GENERIC DETECTION

        if (is_last_char)
        {
            char_buf = char_buf.Trim(); //Remove any whitespace.

            if ((char.IsNumber(char_buf[0]) || char_buf[0] == '.') && !char.IsNumber(char_buf[char_buf.Length - 1])) //we know an int is finished if the next char is not a digit. Therefore, we leave that last digit alone
            {
                string s = char_buf.Trim();
                s = s.Substring(0, s.Length - 1);
                symbols.Add(new SymbolConstantInteger(int.Parse(s)));
                char_buf = "" + char_buf[char_buf.Length - 1];
                char_buf = char_buf.TrimStart(); //dont return. Clean this shit up later
            }
            else if (char.IsNumber(char_buf[0]) || char_buf[0] == '.')
            {
                if (char_buf.Contains("."))
                    symbols.Add(new SymbolConstantFloat(float.Parse(char_buf.Trim(), CultureInfo.InvariantCulture)));
                else
                    symbols.Add(new SymbolConstantInteger(int.Parse(char_buf.Trim(), CultureInfo.InvariantCulture)));
                char_buf = "";
                return;
            }
            if (char_buf.Trim() == ")") //no more brackets!
            {
                symbols = symbols.parent;
                char_buf = "";
                return;
            }
        }



        char starting_char;
        char ending_char;
        if (char_buf.Length > 1)
        {
            starting_char = char_buf[0];
            ending_char = char_buf[char_buf.Length - 1];
        }
        else
        {
            return;
        }

        //STRING DETECTION

        if (starting_char == '"')
        {
            if (ending_char == '"')
            {
                symbols.Add(new SymbolConstantString(char_buf.Substring(1, char_buf.Length - 2)));
                char_buf = "";
                return; //string
            }
            return;
        }



        //INTEGER DETECTION

        if ((char.IsNumber(starting_char) || starting_char == '.') && (!char.IsNumber(ending_char) && ending_char != '.') ) //we know an int is finished if the next char is not a digit. Therefore, we leave that last digit alone
        {
            string s = char_buf.Substring(0, char_buf.Length - 1);
            if (s.Contains("."))
                symbols.Add(new SymbolConstantFloat(float.Parse(s, CultureInfo.InvariantCulture)));
            else
                symbols.Add(new SymbolConstantInteger(int.Parse(s, CultureInfo.InvariantCulture)));
            char_buf = "";
            string_pointer--; // We used the last character to determine integer. Rerun that last character.
            return;
        }


        //VARIABLE DETECTION
        if (char_buf.EndsWith(" ") || is_last_char)
        {
            if (char_buf.Trim() == "isMemoryActiveWithTag" 
                || char_buf.Trim() == "nil" 
                || char_buf.Trim() == "inScenarioForTlsqWithTag" 
                || char_buf.Trim() == "isGreetingActiveWithScenarioTag" 
                || char_buf.Trim() == "inScenarioForHogwartsSidequest"
                || char_buf.Trim() == "isGreetingActiveForHogwartsSidequest"
                || char_buf.Trim() == "forceAdulthood"
                )
            {
                symbols.Add(new SymbolConstantNil());
                char_buf = "";
                return;
            }

            throw new Exception("Unknown variable " + char_buf);
        }

    }

    static void activateOperator(ref SymbolBrackets symbols, int operator_index)
    {
        IOperator op = (IOperator)symbols.children[operator_index];
        if (op.getOpType() == IOperator.OpType.SingleParam)
            activateOperatorSingle(ref symbols, operator_index);
        else if (op.getOpType() == IOperator.OpType.DoubleParam)
            activateOperatorDouble(ref symbols, operator_index);
    }

    static void activateOperatorSingle(ref SymbolBrackets symbols, int operator_index)
    {
        Symbol param1 = symbols.children[operator_index + 1];

        Symbol result = ((IOperator)symbols.children[operator_index]).eval(param1);
        symbols.children[operator_index] = result;
        symbols.children.RemoveAt(operator_index + 1);
    }
    static void activateOperatorDouble(ref SymbolBrackets symbols, int operator_index)
    {
        Symbol param1;
        if (operator_index == 0)
            param1 = null;
        else
            param1 = symbols.children[operator_index - 1];
        Symbol param2 = symbols.children[operator_index + 1];

        Symbol result = ((IOperator)symbols.children[operator_index]).eval(param1, param2);
        symbols.children[operator_index] = result;
        symbols.children.RemoveAt(operator_index + 1);
        if (param1 != null)
            symbols.children.RemoveAt(operator_index - 1);
    }


    static Symbol[] bracketSetToFunctionParameters(SymbolBrackets sb)
    {
        List<SymbolBrackets> parameters_to_solve = new List<SymbolBrackets>();
        List<Symbol> parameters = new List<Symbol>();
        SymbolBrackets current = new SymbolBrackets(null);
        foreach (Symbol s in sb.children)
        {
            if (s.GetType() == typeof(SymbolConstantSeparator))
            {
                parameters_to_solve.Add(current);
                current = new SymbolBrackets(null);
            }
            else
                current.Add(s);
        }
        if (current.children.Count != 0)
        parameters_to_solve.Add(current);


        foreach (SymbolBrackets p in parameters_to_solve)
        {
            parameters.Add(runBracketSet(p));
        }
        return parameters.ToArray();
    }

    static Symbol runBracketSet(SymbolBrackets base_symbol)
    {
        int priority = 7;

        while (priority > 0)
        {
            bool did_do_something = false;

            for (int i = base_symbol.children.Count - 1; i >= 0; i--)
            {
                if (base_symbol.children[i].getPriority() == priority)
                {
                    Type t = base_symbol.children[i].GetType();
                    if (t.Equals(typeof(SymbolBrackets)))
                    {
                        SymbolBrackets sb = (SymbolBrackets) base_symbol.children[i];

                        if (sb.function_name != null)
                        {
                            if (sb.function_name.StartsWith("alias."))
                            {
                                string aliasId = sb.function_name.Substring(6);
                                if (!Configs.predicate_alias_dict.ContainsKey(aliasId))
                                    throw new Exception("Unknown predicate alias " + aliasId);
                                Debug.Log("Predicate alias: " + Configs.predicate_alias_dict[aliasId].aliasedPredicate);
                                Symbol result = new SymbolConstantBool(parsePredicate(Configs.predicate_alias_dict[aliasId].aliasedPredicate));
                                base_symbol.children[i] = result;
                            }
                            else if (sb.function_name.StartsWith(":"))
                            {
                                Symbol operatingSymbol = base_symbol.children[i - 1];
                                Symbol result;
                                List<object> args = new List<object> { operatingSymbol };
                                args.AddRange(bracketSetToFunctionParameters(sb));
                                try
                                {
                                    if (operatingSymbol is SymbolConstantString)
                                    {
                                        result = (Symbol)SymbolConstantString.functions[sb.function_name.Substring(1)].DynamicInvoke(args.ToArray());
                                    }
                                    else
                                    {
                                        throw new Exception("Unimplemented class function for predicate symbol");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(e.Message);
                                    result = new SymbolConstantBool(true);
                                }
                                base_symbol.children[i - 1] = result;
                                base_symbol.children.RemoveAt(i);
                            }
                            else if (functions.ContainsKey(sb.function_name))
                            {
                                if (sb.function_name == "isExclusive" && sb.children.Count == 0)
                                    sb.function_name = "isExclusiveEmpty";
                                Symbol result;
                                //try
                                //{
                                    result = (Symbol)functions[sb.function_name].DynamicInvoke(bracketSetToFunctionParameters(sb));
                                //}
                                //catch (Exception e)
                                //{
                                  //  Debug.LogError(e.Message);
                                   // result = new SymbolConstantBool(true);
                                //}
                                base_symbol.children[i] = result; 
                            }
                            else
                            {
                                throw new Exception("Unknown function \"" + sb.function_name + "\"");
                            }
                        }
                        else
                        {
                            base_symbol.children[i] = runBracketSet(sb); //Replace the brackset with the result
                        }
                    }
                    else
                    {
                        activateOperator(ref base_symbol, i);
                    }
                    did_do_something = true;
                }
            }
            if (did_do_something == false)
                priority--;
        }

        if (base_symbol.children.Count == 1)
        {
            return base_symbol.children[0];
        }
        else
        {
            base_symbol.print();
            throw new System.Exception("Could not resolve bracketset");
        }
    }

    public static bool parsePredicate(string predicate)
    {
        SymbolBrackets base_symbol = new SymbolBrackets(null);
        string char_buf = "";
        int string_pointer = 0;

        //weird quote symbols.
        predicate = predicate.Replace('“', '"');
        predicate = predicate.Replace('”', '"');
        predicate = predicate.Replace('\'', '"');
        predicate = predicate.Replace("\n", "");

        predicate = predicate.Replace("Regular", "true"); //Dumb mistake in script event QuidditchS1C10P5_centerCamOutro

        while (string_pointer < predicate.Length)
        {
            char_buf += predicate[string_pointer];
            parseChar(ref predicate, ref char_buf, ref base_symbol, ref string_pointer, string_pointer == predicate.Length - 1);
            string_pointer++;
        }

        if (char_buf.Trim() != "")
        {
            throw new System.Exception("unresolved predicate trail: " + char_buf);
        }

        Symbol result = runBracketSet(base_symbol);

        Type t1 = result.GetType();

        if (!t1.Equals(typeof(SymbolConstantBool))){
            throw new Exception("Result was not a boolean");
        }

        if (((SymbolConstantBool)result).value == true)
        {
            return true;
        }
        return false;
    }

    static Symbol parsePredicateNoOutput(string predicate)
    {
        predicate = predicate.Replace(".", "\\.");

        SymbolBrackets base_symbol = new SymbolBrackets(null);
        string char_buf = "";
        int string_pointer = 0;

        while (string_pointer < predicate.Length)
        {
            char_buf += predicate[string_pointer];
            parseChar(ref predicate, ref char_buf, ref base_symbol, ref string_pointer, string_pointer == predicate.Length - 1);
            string_pointer++;
        }

        if (char_buf.Trim() != "")
        {
            throw new System.Exception("unresolved predicate trail: " + char_buf);
        }

        Symbol result = runBracketSet(base_symbol);
        return result;
    }

    public static void startupTest()
    {
        Assert.AreEqual(
            parsePredicateNoOutput("( 51 ==    512     )   ==    (true==false       )").toString(),
            new SymbolConstantBool(true).toString()
            );
        Assert.AreEqual(
            parsePredicateNoOutput("5 + 12 ~= 5 + 12").toString(),
            new SymbolConstantBool(false).toString()
        );
        Assert.AreEqual(
            parsePredicateNoOutput("\"HELLO\" == \"BELLO\"").toString(),
            new SymbolConstantBool(false).toString()
        );
        Assert.AreEqual(
            parsePredicateNoOutput("3 + 5 *6 ").toString(),
            new SymbolConstantInteger(33).toString()
        );
        Assert.AreEqual(
            parsePredicateNoOutput("(3 + 5) *6 ").toString(),
            new SymbolConstantInteger(48).toString()
        );
        Assert.AreEqual(
            parsePredicateNoOutput("multiplyStuff(5 * 2, 2 + 2)").toString(),
            new SymbolConstantInteger(40).toString()
        );
        Assert.AreEqual(
            parsePredicateNoOutput("   20    - 25 ==    -5 ").toString(),
            new SymbolConstantBool(true).toString()
        );
        Assert.AreEqual(
            parsePredicateNoOutput("-(5+10) == -15").toString(),
            new SymbolConstantBool(true).toString()
        );
        Assert.AreEqual(
            parsePredicateNoOutput("true and false == false").toString(),
            new SymbolConstantBool(true).toString()
        );
        Assert.AreEqual(
            parsePredicateNoOutput("true or false == true").toString(),
            new SymbolConstantBool(true).toString()
        );
    }


}
