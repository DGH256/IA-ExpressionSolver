using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sandbox_Winforms
{
    public partial class Form1 : Form
    {
        private List<string> words = new List<string>();

        private List<string> words_reversed = new List<string>();

        HashSet<char> letters = new HashSet<char>();

        private string jessTemplatePath = Directory.GetCurrentDirectory() + "\\jessTemplate.txt";

        int wordMaxLength = 0;

        bool trailingExpressions = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Saving the words && the solution

            wordMaxLength = 0; words = new List<string>(); words_reversed = new List<string>(); letters = new HashSet<char>();
                       
            char[] separators = new char[] { '+', '=' };

            var tokens = richTextBox2.Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            for(int i=0;i<tokens.Length;i++)
            {
                words.Add(tokens[i].Trim().ToUpper());

                wordMaxLength = System.Math.Max(words[i].Length, wordMaxLength); //finding the maximum string length of all words
            }

            //Generating reverse words and adding them in new list
            for (int i = 0; i < words.Count(); i++)
            {
                String word_reversed = "";

                for(int j=words[i].Length-1;j>=0;j--)
                {
                    word_reversed += words[i][j];

                    if(!letters.Contains(words[i][j]))
                    {
                        letters.Add(words[i][j]);
                    }
                }

                words_reversed.Add(word_reversed);
            }

            string jessTemplate = "";

            using (StreamReader reader = new StreamReader(File.OpenRead(jessTemplatePath)))
            {
                jessTemplate = reader.ReadToEnd();
            }        

            jessTemplate=jessTemplate.Replace("{comment_problemInput}", generate_commentProblemInput());

            jessTemplate = jessTemplate.Replace("{startup_printOut}", generate_startupPrintout());

            jessTemplate = jessTemplate.Replace("{startup_letters}", generate_startupLetters());

            jessTemplate = jessTemplate.Replace("{rule_distinctLetters}", generate_ruleDistinctLetters());

            jessTemplate = jessTemplate.Replace("{rule_additionOperations}", generate_ruleAdditionOperations());

            jessTemplate = jessTemplate.Replace("{printout_solution1}", generate_printoutSolution1());

            jessTemplate = jessTemplate.Replace("{printout_solution2}", generate_printoutSolution2());

            richTextBox1.Text = jessTemplate;
        }

        string generate_printoutSolution1()
        {
            string result = "";

            var orderedLetters = letters.OrderBy(p => p);

            foreach(var letter in orderedLetters)
            {
                result += string.Format("(printout t \"  {0} = \" ?{1} )", Char.ToUpper(letter), Char.ToLower(letter)) + Environment.NewLine;
            }

            return result;

        }

        string generate_printoutSolution2()
        {
            string result = "";

            int bufferLength = wordMaxLength + 2;

            for(int i=0;i<words.Count;i++)
            {
                int length_word = words[i].Length;

                int decorator_length = bufferLength - length_word;

                string decorator_sign = "+";

                string wordVariable = "";

                if(i==0)
                {
                    decorator_sign = " ";
                }

                if(i==words.Count-1)
                {
                    result += string.Format("(printout t \"{0}\" \"{1}\" crlf)", decorator_emptyString(bufferLength - wordMaxLength-1), decorator_dashedLine(wordMaxLength+1)) + Environment.NewLine;
                    decorator_sign = "=";
                }

                for(int j=0;j< words[i].Length;j++)
                {
                    wordVariable += string.Format(" ?{0}", Char.ToLower(words[i][j]));
                }

                result += string.Format("(printout t \"{0}{1}\" {2} crlf)", decorator_emptyString(decorator_length), decorator_sign, wordVariable) + Environment.NewLine;
            }

            var orderedLetters = letters.OrderBy(p => p);

            return result;

        }
        string generate_commentProblemInput()
        {
            int bufferLength = wordMaxLength + 7;

            string comment_problemInput = Environment.NewLine + bufferedString("", bufferLength, true) + Environment.NewLine;

            string decorator = "";

            for (int i = 0; i < words.Count - 1; i++)
            {
                if (i != 0) decorator = "+";

                comment_problemInput += bufferedString(decorator + words[i], bufferLength, true) + Environment.NewLine;
            }

            comment_problemInput += bufferedString(decorator_dashedLine(wordMaxLength), bufferLength, true) + Environment.NewLine;

            comment_problemInput += bufferedString("="+words[words.Count - 1], bufferLength, true) + Environment.NewLine;

            comment_problemInput += bufferedString("", bufferLength, true) + Environment.NewLine;

            return comment_problemInput;
        }

        string generate_ruleAdditionOperations()
        {
            string result = "", expression2="0", trailing_expression_token = null;

            List<string> l = new List<string>();
            List<string> l_prev = new List<string>();

            for (int j = 0; j < words_reversed.Count; j++)
            {
                l.Add("0"); l_prev.Add("0");
            }

            for (int i = 0; i < wordMaxLength; i++)
            {
                for (int j = 0; j < words_reversed.Count; j++)
                {
                    l_prev[j] = l[j];
                }

                for (int j = 0; j < words_reversed.Count; j++)
                {
                    l[j] = "0";

                    if (i < words_reversed[j].Length)
                    {
                        l[j] = "?" + Char.ToLower(words_reversed[j][i]);    
                    }
                }


                string expression1 = string.Format("(+ {0} {1})", l[0], l[1]);

                if (words.Count>3)
                {
                    for(int j=2;j<words.Count-1;j++)
                    {
                        expression1 = string.Format("(+ {0} {1})", expression1, l[j]);                     
                    }
                }

                if (trailing_expression_token != null)
                {
                    if (expression2 == null)
                    {
                        expression2 = trailing_expression_token;
                    }
                    else
                    {
                        expression2 = string.Format("(+ {1} (/ {0} 10) )", expression2, trailing_expression_token);
                    }
                }

                trailing_expression_token = expression1;

                string l1 = l[words.Count - 1];

                //First digit should be < 10
                if (i == wordMaxLength - 1)
                {
                    result += string.Format("(combination (letter {0}) (number {1}&:(<  (+ {2} (/ {3} 10))  10)))", l1.Substring(1).ToUpper(), l1, expression1, expression2) + Environment.NewLine;
                }

                result += string.Format("(combination (letter {0}) (number {1}&:(= (mod (+ {2} (/ {3} 10)) 10) {4}))) ", l1.Substring(1).ToUpper(), l1, expression1, expression2, l1) + Environment.NewLine;


            }

            return result;
        }

        string generate_ruleDistinctLetters()
        {
            string result = "";

            var orderedLetters = letters.OrderBy(p => p);

            for(int i=0;i<orderedLetters.Count();i++)
            {
                char letter = orderedLetters.ElementAt(i);

                string decorator_difference = "";

                for(int j=0;j<i;j++)
                {
                    decorator_difference += "&~?" + Char.ToLower(orderedLetters.ElementAt(j));
                }

                result += string.Format("(combination (letter {0}) (number ?{1}{2}))", Char.ToUpper(letter), Char.ToLower(letter), decorator_difference) + Environment.NewLine;
            }

            return result;
        }

        string generate_startupLetters()
        {
            string result = "";

            foreach(char letter in letters.OrderBy(p=>p))
            {
                result += (string.Format("(letter {0})", Char.ToUpper(letter)));
            }

            return result;
        }

        string generate_startupPrintout()
        {
            int bufferLength = wordMaxLength + 3;

            string result = "";

            result +=string.Format("(printout t \"{0}\" crlf)", "The problem is") + Environment.NewLine;

            string decorator = "";

            for (int i = 0; i < words.Count - 1; i++)
            {
                if (i != 0) decorator = "+";

                result += string.Format("(printout t \"{0}\" crlf)", bufferedString(decorator + words[i], bufferLength)) + Environment.NewLine;
            }

            result += string.Format("(printout t \"{0}\" crlf)", bufferedString(decorator_dashedLine(wordMaxLength), bufferLength)) + Environment.NewLine;

            result += string.Format("(printout t \"{0}\" crlf)", bufferedString("=" + words[words.Count - 1], bufferLength)) + Environment.NewLine;           

            return result;
        }

        string bufferedString(string str, int bufferLength, bool addComments=false)
        {
            StringBuilder bufferedStr = new StringBuilder();

            for(int i=0;i<bufferLength;i++)
            {
                bufferedStr.Append(" ");
            }

            int contor = 0;

            while(contor<str.Length && contor<bufferLength-1)
            {
                bufferedStr[bufferLength - 1 - contor] = str[str.Length - 1 - contor];

                contor++;
            }

            if(addComments)
            {
                for (int i = 0; i < 3; i++)
                    bufferedStr[i] = ';';
            }

            return bufferedStr.ToString();
        }


        string decorator_dashedLine(int length)
        {
            String str = "";

            for (int i = 0; i < length; i++)
            {
                str += "-";
            }

            return str;
        }

        string decorator_emptyString(int length)
        {
            String str = "";

            for (int i = 0; i < length; i++)
            {
                str += " ";
            }

            return str;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Dictionary<char, char> dictionary = new Dictionary<char, char>();

            Random rand = new Random();

            int maxValue = 'z' - 'a';

            for(int i=0;i<=9;i++)
            {
                char value = (char)('A' + rand.Next(0, maxValue));

                dictionary.Add(i.ToString()[0], value);
            }

            string result = richTextBox2.Text;

            if (!result.Contains('='))
            {
                char[] separators = new char[] { '+', '=', ' ' };

                var tokens = richTextBox2.Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                Int64 sum = 0;

                foreach (var token in tokens)
                {
                    try
                    {
                        sum += Convert.ToInt64(token);
                    }
                    catch(Exception ex) { }
                }

                result += " = " + sum;

                richTextBox2.Text = result;
            }

            for (int i=0;i<=9;i++)
            {
                result = result.Replace(i.ToString()[0], dictionary[i.ToString()[0]]);
            }

            richTextBox1.Text = result;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            int max_Digits = 8;

            int max_Numbers = 2;

            int nr_digits, nr_numbers;

            List<string> generated_numbers = new List<string>();

            try { max_Digits = Convert.ToInt32(textBox1.Text); } catch(Exception ex) { }
            try { max_Numbers = Convert.ToInt32(textBox2.Text); } catch (Exception ex) { }


            Random rand = new Random();

            if(max_Numbers<2 || max_Digits<3)
            {
                MessageBox.Show("Error. Max.no of digits should be at least 3, Max.no of numbers should be at least 2.");

                if (max_Numbers < 2) { max_Numbers = 2; }
                if (max_Digits < 3) { max_Digits = 3; }
            }

            //There should be at least 2 numbers to add up
            nr_numbers = rand.Next(2, max_Numbers);

            for (int i=0; i<nr_numbers; i++)
            {
                //How many digits  new number has
                nr_digits = rand.Next(3, max_Digits);

                string newNumber = "";

                for (int j = 1; j < nr_digits; j++)
                    newNumber += (char)('0' + rand.Next(0, 9));

                generated_numbers.Add(newNumber);           
            }

            generated_numbers = generated_numbers.OrderByDescending(p => p.Length).ToList();

            string output_expression = "";
            
            for(int i=0;i<generated_numbers.Count;i++)
            {
                output_expression += generated_numbers[i] + " ";

                if (i < nr_numbers - 1)
                {
                    output_expression += "+ ";
                }
            }

            richTextBox2.Text = output_expression;

            button2_Click(null, null);

            string aux = richTextBox1.Text;
            string oldNrValue = richTextBox2.Text;
            richTextBox1.Text = richTextBox2.Text;
            richTextBox2.Text = aux;

            button1_Click(null, null);

            richTextBox2.Text += Environment.NewLine + Environment.NewLine + oldNrValue;


        }
    }


}
