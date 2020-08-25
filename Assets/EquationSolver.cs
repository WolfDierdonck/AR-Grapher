using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

class Node {

    public string value;

    public Node left;
    public Node right;

    public Node(string value = null, Node left = null, Node right = null) {
        this.value = value;
        this.left = left;
        this.right = right;
    }
}

public class EquationSolver {

    string[] functions = new string[8] {"sqrt", "sin", "asin", "cos", "acos", "tan", "atan", "log"};
    string[] operators = new string[5] {"+", "-", "*", "/", "^"};
    string expression;
    bool zLeft;

    List<string> operatorStack;
    List<int> zPath;

    Dictionary<string, string> opposites = new Dictionary<string, string>() {
        {"+","-"}, {"-","+"}, {"*","/"}, {"/","*"}, {"sin","asin"}, {"cos","acos"}, {"tan","atan"}
    };

    public double SolveEquation(string input) {
        VariableSetup(input);

        Node[] roots = CreateTree();
        Node leftRoot = roots[0];
        Node rightRoot = roots[1];

        Node zRoot = zLeft ? leftRoot : rightRoot;
        Node nonZRoot = !zLeft ? leftRoot : rightRoot;

        Node newRoot;

        FindZPath(zRoot, new List<int>());

        foreach(int side in zPath) {
            Node otherSide = (side == 0) ? zRoot.right : zRoot.left;

            if (zRoot.value == "log") {
                if (side == 0) {
                    newRoot = new Node("^", otherSide, nonZRoot);
                } else {
                    newRoot = new Node("sqrt", otherSide, nonZRoot);
                }
            }
            else if (zRoot.value == "^") {
                if (side == 0) {
                    newRoot = new Node("sqrt", nonZRoot, otherSide);
                } else {
                    newRoot = new Node("log", nonZRoot, otherSide);
                }
            }
            else if (zRoot.value == "sqrt") {
                if (side == 0) {
                    newRoot = new Node("^", nonZRoot, otherSide);
                } else {
                    newRoot = new Node("log", otherSide, nonZRoot);
                }
            }
            else {
                newRoot = new Node(opposites[zRoot.value], nonZRoot, otherSide);
            }

            zRoot = (side == 0) ? zRoot.left : zRoot.right;
            nonZRoot = newRoot;
        }

        return CalculateNode(nonZRoot);
    }


    private void VariableSetup(string input) { //called once
        expression = input;
        operatorStack = new List<string>();
        zPath = new List<int>();
        zLeft = false;

        expression = expression.Replace("pi", "3.14159265358979323846");
        expression = expression.Replace("e", "2.71828182845904523536");
        expression = expression.Replace("+-", "-");
        expression = expression.Replace("-+", "-");
        expression = expression.Replace("--", "+");
        expression = expression.Replace("-z", "+(z*(-1))");
        expression = expression.Replace("z","(z)");
    }

    private Node[] CreateTree() { //called once
        List<string>[] tokens = GetTokens();
        List<string> leftTokens = tokens[0];
        List<string> rightTokens = tokens[1];
        
        Node leftRoot = ConvertToTree(leftTokens);

        Node rightRoot = ConvertToTree(rightTokens);

        return new Node[2]{leftRoot, rightRoot};
    }

    private List<string>[] GetTokens() { //remove
        string pattern = @"(\b(sqrt|sin|asin|cos|acos|tan|atan|log|z)\b)|([0-9]+\.[0-9]+)|([0-9]+)|[(-+]|[---]|[/-/]|[\^-\^]|[=-=]|[,-,]";
        Regex mainRegex = new Regex(pattern, RegexOptions.IgnoreCase);
        Regex numRegex = new Regex(@"([0-9]+\.[0-9]+)|([0-9]+)", RegexOptions.IgnoreCase);
        List<string> tokens = new List<string>();

        string prevMatch = "";

        foreach (Match match in mainRegex.Matches(expression)) {
            string current = match.Value;

            if ((current == "-" || current == "+") && (prevMatch == "(" || prevMatch == "")) {
                tokens.Add("0");
            }

            foreach (Match numMatch in numRegex.Matches(prevMatch)) {
                if (current == "(") {
                    tokens.Add("*");
                }
            }

            tokens.Add(current);
            prevMatch = current;
        }

        List<string> leftTokens = new List<string>();
        List<string> rightTokens = new List<string>();

        int i = 0;
        while (tokens[i] != "=") {
            if (tokens[i] == "z") {
                zLeft = true;
            }
            leftTokens.Add(tokens[i]);
            i++;
        }

        i++;

        while (i < tokens.Count()) {
            rightTokens.Add(tokens[i]);
            i++;
        }

        return new List<string>[2] {leftTokens, rightTokens};
        
    }

    private Node ConvertToTree(List<string> tokens) { //keep (called 2 times)
        List<Node> nodes = new List<Node>();
        Node leftNode;
        Node rightNode;

        foreach (string token in tokens) {
            double n;

            if (Double.TryParse(token, out n)) {
                nodes.Add(new Node(n.ToString()));

            } 
            else if (token == "z") {
                nodes.Add(new Node(token));
            }

            else if (token == ",") {
                operatorStack.Add(token);
            }

            else if (functions.Contains(token)) {
                operatorStack.Add(token);
            }
            else if (operators.Contains(token)) {
                string top = GetLast();

                while (top != null && (GreaterPrecedence(top,token) || (EqualPrecedence(top,token) && token != "^")) && top != "(") {
                    string popped = operatorStack[operatorStack.Count-1];
                    operatorStack.RemoveAt(operatorStack.Count-1);

                    rightNode = nodes[nodes.Count-1];
                    nodes.RemoveAt(nodes.Count-1);
                    leftNode = nodes[nodes.Count-1];
                    nodes.RemoveAt(nodes.Count-1);

                    nodes.Add(new Node(popped, leftNode, rightNode));

                    top = GetLast();
                }

                operatorStack.Add(token);
            }
            else if (token == "(") {
                operatorStack.Add(token);

            } else if (token == ")") {
                while (GetLast() != "(" && GetLast() != null && GetLast() != ",") {
                    string popped = operatorStack[operatorStack.Count-1];
                    operatorStack.RemoveAt(operatorStack.Count-1);

                    rightNode = nodes[nodes.Count-1];
                    nodes.RemoveAt(nodes.Count-1);
                    leftNode = nodes[nodes.Count-1];
                    nodes.RemoveAt(nodes.Count-1);

                    nodes.Add(new Node(popped, leftNode, rightNode));
                }

                Node modifier = new Node();

                if (GetLast() == ",") {
                    operatorStack.RemoveAt(operatorStack.Count-1);
                    modifier = nodes[nodes.Count-1];
                    nodes.RemoveAt(nodes.Count-1);
                }

                if (GetLast() == "(") {
                    operatorStack.RemoveAt(operatorStack.Count-1);
                }

                if (functions.Contains(GetLast())) {
                    string popped = operatorStack[operatorStack.Count-1];
                    operatorStack.RemoveAt(operatorStack.Count-1);
                    
                    leftNode = nodes[nodes.Count-1];
                    nodes.RemoveAt(nodes.Count-1);

                    if (modifier.value != null) {
                        nodes.Add(new Node(popped, modifier, leftNode));
                    }
                    else {
                        nodes.Add(new Node(popped, leftNode));
                    }
                    
                }

            }
        }

        while (GetLast() != null) {
            string popped = operatorStack[operatorStack.Count-1];
            operatorStack.RemoveAt(operatorStack.Count-1);

            if (functions.Contains(popped)) {
                leftNode = nodes[nodes.Count-1];
                nodes.RemoveAt(nodes.Count-1);

                nodes.Add(new Node(popped, leftNode));
            } else {
                rightNode = nodes[nodes.Count-1];
                nodes.RemoveAt(nodes.Count-1);
                leftNode = nodes[nodes.Count-1];
                nodes.RemoveAt(nodes.Count-1);

                nodes.Add(new Node(popped, leftNode, rightNode));
            }
        }

        return nodes[0];
    }

    private string GetLast() { //keep (called 9 times)
        if (operatorStack.Count > 0) {
            return operatorStack[operatorStack.Count-1];
        }
        else {
            return null;
        }
    }

    private bool GreaterPrecedence(string op1, string op2) { //remove
        if (operators.Contains(op1) && operators.Contains(op2)) {
            Dictionary<string, int> precedences = new Dictionary<string, int>()
            {
                {"+", 1},
                {"-", 1},
                {"*",2},
                {"/",2},
                {"^",3}
            };
            if (precedences[op1] > precedences[op2]) {
                return true;
            }
        }

        return false;
    }

    private bool EqualPrecedence(string op1, string op2) { //remove
        if (operators.Contains(op1) && operators.Contains(op2)) {
            Dictionary<string, int> precedences = new Dictionary<string, int>()
            {
                {"+", 1},
                {"-", 1},
                {"*",2},
                {"/",2},
                {"^",3}
            };
            if (precedences[op1] == precedences[op2]) {
                return true;
            }
        }

        return false;
    }

    private void FindZPath(Node node, List<int> path) { //keep (recursion)
        if (node.left == null && node.right == null) {
            if (node.value == "z") {
                foreach (int value in path) {
                    zPath.Add(value);
                }
            }
        }
        if (node.left != null) {
            path.Add(0);
            FindZPath(node.left, path);
            path.RemoveAt(path.Count-1);
        }
        if (node.right != null) {
            path.Add(1);
            FindZPath(node.right, path);
            path.RemoveAt(path.Count-1);
        }

        return;

    }

    private double CalculateNode(Node node) { //keep (recursion)

        if (node.left == null && node.right == null) {
            return Convert.ToDouble(node.value);
        }

        if (node.value == "+") {
            return CalculateNode(node.left)+CalculateNode(node.right);
        }
        else if (node.value == "-") {
            return CalculateNode(node.left)-CalculateNode(node.right);
        }
        else if (node.value == "*") {
            return CalculateNode(node.left)*CalculateNode(node.right);
        }
        else if (node.value == "/") {
            return CalculateNode(node.left)/CalculateNode(node.right);
        }
        else if (node.value == "^") {
            return Math.Pow(CalculateNode(node.left),CalculateNode(node.right));
        }
        else if (node.value == "sqrt") {
            return Math.Pow(CalculateNode(node.left), (double) 1/Convert.ToDouble(node.right.value));
        }
        else if (node.value == "sin") {
            return Math.Sin(CalculateNode(node.left));
        }
        else if (node.value == "asin") {
            return Math.Asin(CalculateNode(node.left));
        }
        else if (node.value == "cos") {
            return Math.Cos(CalculateNode(node.left));
        }
        else if (node.value == "acos") {
            return Math.Acos(CalculateNode(node.left));
        }
        else if (node.value == "tan") {
            return Math.Tan(CalculateNode(node.left));
        }
        else if (node.value == "atan") {
            return Math.Atan(CalculateNode(node.left));
        }
        else if (node.value == "log") {
            return Math.Log(CalculateNode(node.left), Convert.ToDouble(node.right.value));
        }

        return 0.0;
    }

}