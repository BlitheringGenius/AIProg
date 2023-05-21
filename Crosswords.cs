using System;
using System.IO;
using System.Collections.Generic;

namespace AIProg
{
    public class CrosswordVariable : CategoryVar
    {
        public bool IsAcross;
        public int AcrossIndex;
        public int DownIndex;
        public int Length;

        public CrosswordVariable(string name, int aIndex, int dIndex, bool across, int length, EnglishDict dict)
            : base(name)
        {
            IsAcross = across;
            AcrossIndex = aIndex;
            DownIndex = dIndex;
            Length = length;

            dict.GetWords(length, Domain);
        }

        public static bool Cross(CrosswordVariable A, CrosswordVariable D)
        {
            return D.AcrossIndex >= A.AcrossIndex
                && D.AcrossIndex < A.AcrossIndex + A.Length
                && A.DownIndex >= D.DownIndex
                && A.DownIndex < D.DownIndex + D.Length;
        }
    }

    public class CrosswordConstraint : Constraint
    {
        private int AcrossIndex;
        private int DownIndex;

        public CrosswordConstraint(CategoryVar A, CategoryVar D, int a, int d) : base()
        {
            Scheme.Add(A);
            Scheme.Add(D);

            AcrossIndex = a;
            DownIndex = d;
        }

        public override bool Satisfied()
        {
            CrosswordVariable A = (CrosswordVariable)Scheme[0];
            CrosswordVariable B = (CrosswordVariable)Scheme[1];

            string aval = A.GetValue();
            string bval = B.GetValue();
        
            if (aval == null)
            {
                return false;
            }
            else if (bval == null)
            {
                return false;
            }
            else if (aval[AcrossIndex - A.AcrossIndex] == bval[DownIndex - B.DownIndex])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class NotSameConstraint : Constraint
    {
        public NotSameConstraint(CategoryVar A, CategoryVar B) : base()
        {
            Scheme.Add(A);
            Scheme.Add(B);
        }

        public override bool Satisfied()
        {
            CategoryVar A = (CategoryVar)Scheme[0];
            CategoryVar B = (CategoryVar)Scheme[1];

            string aval = A.GetValue();
            string bval = B.GetValue();
        
            if (aval == null)
            {
                return false;
            }
            else if (bval == null)
            {
                return false;
            }
            else if (aval == bval)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class CrosswordPuzzle : ConstraintSystem
    {
        List<char[]> Template;
        List<CrosswordVariable> Across;
        List<CrosswordVariable> Down;

        public CrosswordPuzzle() : base("crossword")
        {
            Template = new List<char[]>();
            Across = new List<CrosswordVariable>();
            Down = new List<CrosswordVariable>();
        }

        public void LoadTemplate(string filePath, EnglishDict dict)
        {
            Template.Clear();
            Across.Clear();
            Down.Clear();

            int width = 0;

            using (StreamReader reader = new StreamReader(filePath, true))
            {
                string line = reader.ReadLine();

                line = line.Trim();

                width = line.Length;

                while (line != null)
                {
                    line = line.Trim();

                    if (line.Length != width)
                    {
                        throw new Exception("Invalid puzzle template");
                    }

                    char[] X = new char[width];

                    for (int i = 0; i < width; i++)
                    {
                        X[i] = line[i];
                    }

                    Template.Add(X);

                    line = reader.ReadLine();
                }
            }

            int height = Template.Count;

            // get across variables

            for (int d = 0; d < height; d++)
            {
                char[] X = Template[d];

                int n = 0;

                for (int a = 0; a < width; a++)
                {
                    if (X[a] == '#')
                    {
                        if (n > 1)
                        {
                            CrosswordVariable A = new CrosswordVariable("A-" + Across.Count.ToString(), a - n, d, true, n, dict);

                            Variables.Add(A);

                            Across.Add(A);
                        }

                        n = 0;
                    }
                    else
                    {
                        n += 1;
                    }
                }

                if (n > 1)
                {
                    CrosswordVariable A = new CrosswordVariable("A-" + Across.Count.ToString(), width - n, d, true, n, dict);

                    Variables.Add(A);

                    Across.Add(A);
                }
            }

            // get down variables

            for (int a = 0; a < width; a++)
            {
                int n = 0;

                for (int d = 0; d < height; d++)
                {
                    char c = Template[d][a];

                    if (c == '#')
                    {
                        if (n > 1)
                        {
                            CrosswordVariable D = new CrosswordVariable("D-" + Down.Count.ToString(), a, d - n, false, n, dict);

                            Variables.Add(D);

                            Down.Add(D);
                        }

                        n = 0;
                    }
                    else
                    {
                        n += 1;
                    }
                }

                if (n > 1)
                {
                    CrosswordVariable D = new CrosswordVariable("D-" + Down.Count.ToString(), a, height - n, false, n, dict);

                    Variables.Add(D);

                    Down.Add(D);
                }
            }

            foreach (CrosswordVariable A in Across)
            {
                foreach (CrosswordVariable D in Down)
                {
                    if (CrosswordVariable.Cross(A, D))
                    {
                        CrosswordConstraint C = new CrosswordConstraint(A, D, D.AcrossIndex, A.DownIndex);

                        Constraints.Add(C);
                    }
                }
            }

            for (int i = 0; i < NumVars; i++)
            {
                for (int j = i + 1; j < NumVars; j++)
                {
                    CrosswordVariable X = (CrosswordVariable)Variables[i];
                    CrosswordVariable Y = (CrosswordVariable)Variables[j];

                    if (X.Length == Y.Length)
                    {
                        Constraints.Add(new NotSameConstraint(X, Y));
                    }
                }
            }
        }

        public override void PrettyPrint(StreamWriter writer)
        {
            foreach (CrosswordVariable A in Across)
            {
                char[] line = Template[A.DownIndex];
                string S = A.GetValue();

                for (int i = 0; i < A.Length; i++)
                {
                    line[A.AcrossIndex + i] = S[i];
                }
            }

            foreach (CrosswordVariable D in Down)
            {
                string S = D.GetValue();

                for (int i = 0; i < D.Length; i++)
                {
                    Template[D.DownIndex + i][D.AcrossIndex] = S[i];
                }
            }

            foreach (char[] line in Template)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    writer.Write(line[i]);
                }
                writer.WriteLine("");
            }

            writer.WriteLine("Across");

            int a = 1;

            foreach (CrosswordVariable A in Across)
            {
                writer.WriteLine((a++).ToString() + " " + A.GetValue());
            }

            writer.WriteLine("Down");

            int d = 1;

            foreach (CrosswordVariable D in Down)
            {
                writer.WriteLine((d++).ToString() + " " + D.GetValue());
            }

        }
    }

    public class EnglishDict
    {
        private List<string>[] WordLists;

        public EnglishDict()
        {
            WordLists = new List<string>[100];
        }

        public void Load(string filePath)
        {
            StreamReader reader = new StreamReader(filePath, true);

            string line = reader.ReadLine();

            while (line != null)
            {
                line = line.Trim();

                int i = line.Length;

                if (i > 100)
                {
                    throw new Exception("Invalid word: " + line);
                }

                if (WordLists[i] == null)
                {
                    WordLists[i] = new List<string>();
                }

                WordLists[i].Add(line);

                line = reader.ReadLine();
            }
        }

        public void GetWords(int n, List<string> words)
        {
           if (WordLists[n] == null)
           {
               return;
           }

           foreach (string s in WordLists[n])
           {
               words.Add(s);
           }
        }
    }

    // is crossword puzzle generation NP-complete?
}