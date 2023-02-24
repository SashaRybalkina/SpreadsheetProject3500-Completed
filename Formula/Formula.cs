/// <summary>
/// Author:    Sasha Rybalkina
/// Partner:   None
/// Date:      Febuary 3, 2023
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Sasha Rybalkina - This work may not 
///            be copied for use in Academic Coursework.
///
/// I, Sasha Rybalkina, certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// File Contents
/// Two Formula class constructors, one for using delegates and the other for not using delegates.
/// The Evaluate method, which evaluates the formula given
/// One ToString method, which converts the entire class into a string
/// One Equals method, which evaluates the equality of two Formula classes
/// Two operators, == and !=, which use the logic of the Equals method
/// A GetHashCode method, which gets the hash code of each class
/// A GetTokens method, which turns the given formula into a list
/// Methods for creating a FormulaError and a FormulaFormatException
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// This class takes in a string formula and evaluates the formula by splitting
    /// it into string tokens and then parsing and normalizing all doubles and variables.
    /// This class can be compared with the "==" and "!=" operators, which act as an
    /// alternative to the Equals method for this class and as an alternative to !Equals.
    /// This class also contains a ToString method, which returns the infix expression
    /// given with all numerical values parsed into doubles and all variables normalized
    /// according to the rules of the "normalize" delegate provided.
    /// </summary>
    public class Formula
    {
        private List<string> variables = new();
        private List<string> tokens = new();
        private string formula = "";
        /// <summary>
        /// This is the constructor that is used if a user provides a string expression
        /// without providing any delegates. The constructor is responsible for checking
        /// for syntax errors in the expression provided and throws an exception whenever
        /// such an error is found. The constructor also builds the string that is
        /// meant to represent the Formula class in string form, as well as building a
        /// list for all of the variables and a list for all of the tokens in the given
        /// expressoion after being parsed and normalized for later use.
        /// </summary>
        /// <param name="formula">The string expression given</param>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }
        /// <summary>
        /// This is the constructor that is used if a user provides a delegate for
        /// normalizing a variable and a delegate for checking if the normalized
        /// variable is valid as well as providig a string expression. It functions
        /// exactly like the first constructor above.
        /// </summary>
        /// <param name="formula">The string expression given</param>
        /// <param name="normalize">The delegate for normalizing all variables</param>
        /// <param name="isValid">The delegate for checking is a normalization of a
        /// variable is valid</param>
        /// <exception cref="FormulaFormatException"></exception>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            if (formula == null || formula == "")
            {
                throw new FormulaFormatException("Cannot have a null or empty formula");
            }
            List<string> formulaArray = GetTokens(formula).ToList();
            int lastIndex = formulaArray.Count() - 1;
            var left = formula.Count(x => x == '(');
            var right = formula.Count(x => x == ')');
            if (left != right)
            {
                throw new FormulaFormatException("Cannot have open or unbalanced parentheses");
            }
            for (int i = 0; i < lastIndex + 1; i++)
            {
                ///This if statement checks for all errors associated with operators.
                ///Two operators cannot be next to each other, an operator cannot
                ///precede a right parenthesis, and a division by zero cannot occur.
                if (formulaArray[i] == "+" || formulaArray[i] == "-" || formulaArray[i] == "*" || formulaArray[i] == "/")
                {
                    if (i == 0 || i == lastIndex)
                    {
                        throw new FormulaFormatException("Cannot have trailing operators");
                    }
                    ///If an operator is right next to another operator or a right
                    ///parenthesis, throws an exception.
                    if (i < lastIndex && (formulaArray[i + 1] == "+" || formulaArray[i + 1] == "-" ||
                                          formulaArray[i + 1] == "*" || formulaArray[i + 1] == "/" || formulaArray[i + 1] == ")"))
                    {
                        throw new FormulaFormatException("Cannot have two consecutive opperators or an opperator outside of parentheses.");
                    }
                    ///Builds the formula string and tokens list for later use.
                    this.formula = this.formula + formulaArray[i];
                    tokens.Add(formulaArray[i]);
                }

                ///This if statement checks for all errors associated with integers.
                ///Two integers cannot be next to each other, an integer cannot be right
                ///next to a variable, and an integer cannot be outside of parentheses.
                else if (Double.TryParse(formulaArray[i], result: out double Result))
                {
                    if (i < lastIndex && (Double.TryParse(formulaArray[i + 1], result: out double Result2)))
                    {
                        throw new FormulaFormatException("Cannot have two consecutive numbers in expression.");
                    }
                    else if (i < lastIndex && Regex.IsMatch(formulaArray[i+1], "^[a-z|A-Z|_][a-z|A-Z|0-9|_]*$"))
                    {
                        throw new FormulaFormatException("Cannot have an integer right next to a variable.");
                    }
                    else if (i < lastIndex && (formulaArray[i + 1] == "("))
                    {
                        throw new FormulaFormatException("Cannot have a variable right outside of parentheses.");
                    }

                    this.formula = this.formula + Result;
                    tokens.Add(formulaArray[i]);
                }

                ///This if statement checks for all errors associated with the right
                ///parenthesis. An integer cannot be outside of parentheses, a variable
                ///caanot be outside of parentheses, and an operator cannot be outside
                ///of parentheses.
                else if (formulaArray[i] == ")")
                {
                    if (i < lastIndex && Double.TryParse(formulaArray[i + 1], result: out Result))
                    {
                        throw new FormulaFormatException("Cannot have an integer right outisde of parentheses.");
                    }
                    else if (i < lastIndex && Regex.IsMatch(formulaArray[i], "^[a-z|A-Z|_][a-z|A-Z|0-9|_]*$"))
                    {
                        throw new FormulaFormatException("Cannot have a variable right outisde of parentheses.");
                    }
                    this.formula = this.formula + formulaArray[i];
                    tokens.Add(formulaArray[i]);
                }

                ///This if statement checks for all errors associated with the left
                ///parenthesis. An operator cannot come after a right parenthesis
                ///and closed parentheses cannot be empty.
                else if (formulaArray[i] == "(")
                {
                    if (i < lastIndex && (formulaArray[i + 1] == "+" || formulaArray[i + 1] == "-" ||
                                          formulaArray[i + 1] == "*" || formulaArray[i + 1] == "/"))
                    {
                        throw new FormulaFormatException("Cannot have an operator after a left parenthesis.");
                    }
                    if (i < lastIndex && (formulaArray[i + 1] == ")"))
                    {
                        throw new FormulaFormatException("Cannot have empty parentheses.");
                    }

                    this.formula = this.formula + formulaArray[i];
                    tokens.Add(normalize(formulaArray[i]));
                }

                ///This is where all errors associated with variables are handled.
                ///If a variable is invalid, or is next to another variable or integer,
                ///or if the variable is outside of parentheses, throws an exception.
                else
                {
                    if (!Regex.IsMatch(formulaArray[i], "^[a-z|A-Z|_][a-z|A-Z|0-9|_]*$"))
                    {
                        throw new FormulaFormatException("The variable entered must have one integer and one character.");
                    }
                    else if (!isValid(normalize(formulaArray[i])))
                    {
                        throw new FormulaFormatException("The variable entered is not valid.");
                    }
                    else if (i < lastIndex && Double.TryParse(formulaArray[i + 1], result: out Result))
                    {
                        throw new FormulaFormatException("Cannot have an integer right next to a variable.");
                    }
                    else if (i < lastIndex && Regex.IsMatch(formulaArray[i+1], "^[a-z|A-Z|_][a-z|A-Z|0-9|_]*$"))
                    {
                        throw new FormulaFormatException("Cannot have two consecutive variables in expression");
                    }
                    else if (i < lastIndex && formulaArray[i + 1] == "(")
                    {
                        throw new FormulaFormatException("Cannot have a variable outised of parentheses");
                    }

                    if (!variables.Contains(normalize(formulaArray[i])))
                    {
                        variables.Add(normalize(formulaArray[i]));
                    }
                    this.formula = this.formula + normalize(formulaArray[i]);
                    tokens.Add(normalize(formulaArray[i]));
                }
            }
        }
        /// <summary>
        /// Private helper method for adding and subtracting integers.
        /// </summary>
        /// <param name="value1">First integer for evaluation</param>
        /// <param name="value2">Second integer for evaluation</param>
        /// <param name="op">The operator to be used</param>
        /// <returns>The evaluation of the two integers based on the operator given.</returns>
        private static double AddOrSubtract(double value1, double value2, string op)
        {
            if (op == "+")
            {
                return value1 + value2;
            }
            else
            {
                return value1 - value2;
            }
        }
        /// <summary>
        /// Private helper method for multiplying and dividing integers.
        /// </summary>
        /// <param name="value1">First integer for evaluation</param>
        /// <param name="value2">Second integer for evaluation</param>
        /// <param name="op">The operator to be used</param>
        /// <returns>The evaluation of the two integers</returns>
        private static double MultiplyOrDivide(double value1, double value2, string op)
        {
            if (op == "*")
            {
                return value1 * value2;
            }
            else
            {
                if (value1 / value2 == double.PositiveInfinity || value1 / value2 == double.NegativeInfinity)
                {
                    throw new DivideByZeroException();
                }
                return value1 / value2;
            }
        }
        /// <summary>
        /// This method evaluates the expression that was provided in the constructor
        /// by putting the doubles and operators into two seperate stacks and then
        /// popping the stacks when the evaluating algorithm calls for it. If a token
        /// in the formula is a variable, then its numerical value will be looked up
        /// by using the "lookup" delegate and parsed as a double.
        /// </summary>
        /// <param name="lookup">The delegate for looking up the values of variables</param>
        /// <returns>The evaluation of the formula given in the constructor</returns>
        public object Evaluate(Func<string, double> lookup)
        {
            Stack<Double> ValueStack = new System.Collections.Generic.Stack<Double>();
            Stack<string> OperatorStack = new System.Collections.Generic.Stack<string>();
            if (lookup == null)
            {
                return new FormulaError("Parameter 'lookup' cannot be null");
            }
            foreach (string token in tokens)
            {
                ///This if statement sets up the operator stack for later use. Only the "(", "*" and "/"
                ///operators should be added to the stack
                if (token == "(" || token == "*" || token == "/")
                {
                    OperatorStack.Push(token);
                }
                ///This if statement determines if the string being worked with is an integer. If it is,
                ///then checks how the integer should be treated based on the operators being used.
                else if (Double.TryParse(token, result: out double Result))
                {

                    if (OperatorStack.Count() != 0 && (OperatorStack.Peek() == "*" ||
                        OperatorStack.Peek() == "/"))
                    {
                        try
                        {
                            ValueStack.Push(MultiplyOrDivide(ValueStack.Pop(), Result,
                                OperatorStack.Pop()));
                        }
                        catch
                        {
                            return new FormulaError("Division by zero occurred");
                        }
                    }
                    else
                    {
                        ValueStack.Push(Result);
                    }
                    
                }

                ///Repeats the process of the previous if statement, except with variables. A
                ///delegate is used to look up the value of a variable so that it can be evaluated.
                else if (Regex.IsMatch(token, "^[a-z|A-Z|_][a-z|A-Z|0-9|_]*$"))
                {
                    double LookedUp = 0;
                    try
                    {
                        LookedUp = (double)lookup(token);
                    }
                    catch
                    {
                        return new FormulaError("Unable to look up variable");
                    }
                    if (OperatorStack.Count() != 0 && (OperatorStack.Peek() == "*" ||
                        OperatorStack.Peek() == "/"))
                    {
                        try
                        {
                            ValueStack.Push(MultiplyOrDivide(ValueStack.Pop(), LookedUp,
                                                             OperatorStack.Pop()));
                        }
                        catch
                        {
                            return new FormulaError("Division by zero occurred");
                        }
                    }
                    else
                    {
                        ValueStack.Push(LookedUp);
                    }

                }

                ///This if statement works with the "+" and "-" operators. If there is already
                ///a "+" or "-" at the top of the operator stack, then two integers from the
                ///value stack are evaluated using one of the operators. The operator then gets
                ///pushed onto the operator stack.
                else if (token == "+" || token == "-")
                {
                    if (OperatorStack.Count() != 0 && (OperatorStack.Peek() == "+" ||
                        OperatorStack.Peek() == "-"))
                    {
                        double value2 = ValueStack.Pop();
                        double value1 = ValueStack.Pop();
                        ValueStack.Push(AddOrSubtract(value1, value2, OperatorStack.Pop()));
                    }
                    OperatorStack.Push(token);
                }

                ///This if statement works with right parentheses. It evaluates the integers in the
                ///value stack depending on the operator at the top of the operator stack.
                else if (token == ")")
                {
                    if (OperatorStack.Count() != 0 && (OperatorStack.Peek() == "+" ||
                        OperatorStack.Peek() == "-"))
                    {
                        double value2 = ValueStack.Pop();
                        double value1 = ValueStack.Pop();
                        ValueStack.Push(AddOrSubtract(value1, value2, OperatorStack.Pop()));
                    }

                    OperatorStack.Pop();

                    if (OperatorStack.Count() != 0 && (OperatorStack.Peek() == "*" ||
                        OperatorStack.Peek() == "/"))
                    {
                        double value2 = ValueStack.Pop();
                        double value1 = ValueStack.Pop();
                        try
                        {
                            ValueStack.Push(MultiplyOrDivide(value1, value2, OperatorStack.Pop()));
                        }
                        catch
                        {
                            return new FormulaError("Division by zero occurred");
                        }
                    }

                }
            }

            double result = 0;

            ///If there is a remaining expression that didn't get resolved while the
            ///for loop was running, this is where this expression gets handled.
            if (ValueStack.Count() > 1)
            {
                double value2 = ValueStack.Pop();
                double value1 = ValueStack.Pop();

                if (OperatorStack.Peek() == "*" || OperatorStack.Peek() == "/")
                {
                    try
                    {
                        result = MultiplyOrDivide(value1, value2, OperatorStack.Pop());
                    }
                    catch
                    {
                        return new FormulaError("Division by zero occurred");
                    }
                }

                if (OperatorStack.Peek() == "+" || OperatorStack.Peek() == "-")
                {
                    result = AddOrSubtract(value1, value2, OperatorStack.Pop());
                }
            }
            else
            {
                result = ValueStack.Pop();
            }
            return result;
        }
        /// <summary>
        /// Returns all of the the variables in a given formula. If the formula
        /// entered is "x5 + y6 + X5" and a normalizer is given that capitalizes
        /// the character in a given variable, then this method should return the
        /// list {X5, Y6}. If there is no normalizer, then the list should be
        /// {x5, y6, X5}.
        /// </summary>
        /// <returns>All variables in the formula</returns>
        public IEnumerable<String> GetVariables()
        {
            return variables;
        }
        /// <summary>
        /// Returns the string representation of the Formula object that was built
        /// in the constructor, with all of the tokens parsed and normalized (if a
        /// normalization delegate is given).
        /// </summary>
        /// <returns>The Formula object in string form</returns>
        public override string ToString()
        {
            return formula;
        }
        /// <summary>
        /// This method compares two Formula objects and determines if they are
        /// equal based on the equality of their string forms.
        /// </summary>
        /// <param name="obj">The object that this Formula object is comapred to.</param>
        /// <returns>True if the objects' strings are equal, false otherwise.</returns>
        public override bool Equals(object? obj)
        {
            String string1 = this.ToString();
            String string2 = obj.ToString();
            return string1.Equals(string2);
        }
        /// <summary>
        /// This method creates a "==" operator for the Formula class by comparing
        /// two objects based on the rules of the Equals method.
        /// </summary>
        /// <param name="f1">The first object to be compared</param>
        /// <param name="f2">The second object to be compared</param>
        /// <returns>True if the two objects are equal, false otherwise.</returns>
        public static bool operator ==(Formula f1, Formula f2)
        {
            return f1.Equals(f2);
        }
        /// <summary>
        /// This method creates a "!=" operator that serevs as an opposite of the
        /// "==" operator and compared two objects to determine of they are unequal.
        /// </summary>
        /// <param name="f1">The first object to be compared</param>
        /// <param name="f2">The second object to be compared</param>
        /// <returns>True if the two objects are unequal, false otherwise.</returns>
        public static bool operator !=(Formula f1, Formula f2)
        {
            return !f1.Equals(f2);
        }
        /// <summary>
        /// Returns a hash code for the class based on the hash code of the string
        /// of the formula
        /// </summary>
        /// <returns>The hash code of the class</returns>
        public override int GetHashCode()
        {
            return formula.GetHashCode();
        }
        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are 
        /// left paren;
        /// right paren; one of the four operator symbols; a string consisting of a 
        /// letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal;
        /// and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token 
        /// contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";
            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) |  ({5})",
            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);
            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }
    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }
    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }
        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}
