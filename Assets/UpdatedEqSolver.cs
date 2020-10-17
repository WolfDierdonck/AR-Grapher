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

public class UpdatedEqSolver {
    string inputEquation;
    Node root;
    double x;
    double y;

    string[] functions = new string[8] {"sqrt", "sin", "asin", "cos", "acos", "tan", "atan", "log"};
    string[] operators = new string[5] {"+", "-", "*", "/", "^"};
    Dictionary<string, string> opposites = new Dictionary<string, string>() {
        {"+","-"}, {"-","+"}, {"*","/"}, {"/","*"}, {"sin","asin"}, {"cos","acos"}, {"tan","atan"}
    };
    bool variableIsLeftSide;
    List<string> operatorStack;
    List<int> zPath;

    public UpdatedEqSolver(string inputEquation) {
        this.inputEquation = inputEquation;
        operatorStack = new List<string>();
        zPath = new List<int>();
    }

    public void ParseEquation() {
        #region replace bogus syntax
        inputEquation = inputEquation.Replace("pi", "3.14159265358979323846");
        inputEquation = inputEquation.Replace("e", "2.71828182845904523536");
        inputEquation = inputEquation.Replace("+-", "-");
        inputEquation = inputEquation.Replace("-+", "-");
        inputEquation = inputEquation.Replace("--", "+");
        inputEquation = inputEquation.Replace("-z", "+(z*(-1))");
        inputEquation = inputEquation.Replace("z","(z)");
        inputEquation = inputEquation.Replace("-x", "+(x*(-1))");
        inputEquation = inputEquation.Replace("x","(x)");
        inputEquation = inputEquation.Replace("-y", "+(y*(-1))");
        inputEquation = inputEquation.Replace("y","(y)");
        #endregion

        List<string>[] tokens = GetTokens();
        
        Node leftRoot = ConvertToTree(tokens[0]);
        Node rightRoot = ConvertToTree(tokens[1]);

        if (!variableIsLeftSide) { //ensures z is always on left side
            Node tempRoot = leftRoot;
            leftRoot = rightRoot;
            rightRoot = tempRoot;
        }

        FindZPath(leftRoot, new List<int>());
        Node newRoot;

        foreach(int side in zPath) { //Isolates for z
            Node otherSide = (side == 0) ? leftRoot.right : leftRoot.left;

            if (leftRoot.value == "log") {
                if (side == 0) {
                    newRoot = new Node("^", otherSide, rightRoot);
                } else {
                    newRoot = new Node("sqrt", otherSide, rightRoot);
                }
            }
            else if (leftRoot.value == "^") {
                if (side == 0) {
                    newRoot = new Node("sqrt", rightRoot, otherSide);
                } else {
                    newRoot = new Node("log", rightRoot, otherSide);
                }
            }
            else if (leftRoot.value == "sqrt") {
                if (side == 0) {
                    newRoot = new Node("^", rightRoot, otherSide);
                } else {
                    newRoot = new Node("log", otherSide, rightRoot);
                }
            }
            else {
                newRoot = new Node(opposites[leftRoot.value], rightRoot, otherSide);
            }

            leftRoot = (side == 0) ? leftRoot.left : leftRoot.right;
            rightRoot = newRoot;
        }

        root = rightRoot;

    }

    private List<string>[] GetTokens() {
        string pattern = @"(\b(sqrt|sin|asin|cos|acos|tan|atan|log|x|y|z)\b)|([0-9]+\.[0-9]+)|([0-9]+)|[(-+]|[---]|[/-/]|[\^-\^]|[=-=]|[,-,]";
        Regex mainRegex = new Regex(pattern, RegexOptions.IgnoreCase);
        Regex numRegex = new Regex(@"([0-9]+\.[0-9]+)|([0-9]+)", RegexOptions.IgnoreCase);
        List<string> tokens = new List<string>();

        string prevMatch = "";

        foreach (Match match in mainRegex.Matches(inputEquation)) {
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
        variableIsLeftSide = false;

        int i = 0;
        while (tokens[i] != "=") {
            if (tokens[i] == "z") {
                variableIsLeftSide = true;
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
            else if (token == "z" || token == "x" || token == "y") {
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


    public double CalculateFunction(double x, double y) {
        this.x = x;
        this.y = y;
        return CalculateNode(root);
    }

    private double CalculateNode(Node node) {
        if (node.left == null && node.right == null) {
            if (node.value == "x") {
                return x;
            } else if (node.value == "y") {
                return y;
            }
            else {
                return Convert.ToDouble(node.value);
            }
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
            return Math.Pow(CalculateNode(node.left), (double) 1/CalculateNode(node.right));
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
            return Math.Log(CalculateNode(node.left), CalculateNode(node.right));
        }

        return 0.0;
    }
}