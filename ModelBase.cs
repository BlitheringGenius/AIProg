using System;
using System.IO;
using System.Collections.Generic;

namespace AIProg
{
    public enum VarType { Category, Measure }

    public abstract class Variable
    {
        public string Name;
        public VarType Type;

        protected Variable(string s, VarType t)
        {
            Name = s;
            Type = t;
        }

        public abstract bool Assigned { get; }

        public abstract void ClearValue();

        public abstract void PrintState(StreamWriter W);
    }

    public class CategoryVar : Variable
    {
        public List<string> Domain;
        public int ValueIndex;

        public CategoryVar(string s) : base(s, VarType.Category)
        {
            Domain = new List<string>();
            ValueIndex = -1;
        }

        public string GetValue()
        {
            if (ValueIndex == -1)
            {
                return null;
            }
            else
            {
                return Domain[ValueIndex];
            }
        }

        public void Assign(string val)
        {
            ValueIndex = GetDomainIndex(val);
        }

        public int GetDomainIndex(string s)
        {
            if (s == null)
            {
                return -1;
            }

            for (int i = 0; i < Domain.Count; i++)
            {
                if (Domain[i] == s)
                {
                    return i;
                }
            }

            throw new Exception("Value not in domain[" + Name + "] : " + s);
        }

        public override bool Assigned
        {
            get { return ValueIndex != -1; }
        }

        public override void ClearValue()
        {
            ValueIndex = -1;
        }

        public override void PrintState(StreamWriter W)
        {
            if (ValueIndex == -1)
            {
                W.WriteLine(Name + " : ?");
            }
            else
            {
                W.WriteLine(Name + " : " + Domain[ValueIndex]);
            }
        }
    }

    public class MeasureVar : Variable
    {
        public double? LowerBound;
        public double? UpperBound;
        public double? Value;

        public MeasureVar(string s) : base(s, VarType.Measure)
        {
            LowerBound = null;
            UpperBound = null;
            Value = null;
        }

        public override bool Assigned
        {
            get { return Value != null; }
        }

        public override void ClearValue()
        {
            Value = null;
        }

        public override void PrintState(StreamWriter W)
        {
            W.WriteLine(Name + " : " + Value.ToString());
        }
    }

    public abstract class Model
    {
        public string Name;
        public List<Variable> Variables;

        protected Model(string s)
        {
            Name = s;
            Variables = new List<Variable>();
        }

        protected Model()
        {
            Name = null;
            Variables = new List<Variable>();
        }

        public Variable GetVar(string name)
        {
            foreach (Variable X in Variables)
            {
                if (X.Name == name)
                {
                    return X;
                }
            }

            throw new Exception("Variable not found: " + name);
        }

        public int NumVars
        {
            get { return Variables.Count; }
        }

        public void ClearState()
        {
            foreach (Variable X in Variables)
            {
                X.ClearValue();
            }
        }

        public void PrintState(StreamWriter W)
        {
            W.WriteLine(Name + " {");

            foreach (Variable X in Variables)
            {
                X.PrintState(W);
            }

            W.WriteLine("}");
        }

        public virtual void PrettyPrint(StreamWriter W)
        {
            PrintState(W);
        }
    }

    public class CategoryData
    {
        public List<CategoryVar> Variables;
        private List<int[]> Data;
        private StreamWriter InfoWriter;

        public CategoryData(StreamWriter infoWriter)
        {
            Variables = new List<CategoryVar>();
            Data = new List<int[]>();
            InfoWriter = infoWriter;
        }

        public void Clear()
        {
            for (int i = 0; i < Variables.Count; i++)
            {
                Variables[i].ClearValue();
            }

            Data.Clear();
        }

        public void Load(string filePath)
        {
            Clear();

            InfoWriter.WriteLine("Loading data from: " + filePath);

            using (StreamReader reader = new StreamReader(filePath, true))
            {
                string line = reader.ReadLine();

                int N = Variables.Count;

                while (line != null)
                {
                    line = line.Trim();

                    string[] vals = line.Split(',');

                    if (vals.Length != N)
                    {
                        throw new Exception("Invalid line in data file: " + line);
                    }

                    int[] datum = new int[N];

                    for (int i = 0; i < N; i++)
                    {
                        Variables[i].Assign(vals[i]);

                        datum[i] = Variables[i].ValueIndex;
                    }

                    Data.Add(datum);

                    if (Data.Count % 1000 == 0)
                    {
                        InfoWriter.WriteLine(Data.Count.ToString() + " data loaded...");
                    }

                    line = reader.ReadLine();
                }
            }

            InfoWriter.WriteLine(Data.Count.ToString() + " data loaded total");

            Clear();
        }
    }
}