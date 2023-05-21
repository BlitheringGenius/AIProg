using System;
using System.IO;
using System.Collections.Generic;

namespace AIProg
{
    public class CondMap<T>
    {
        public List<CategoryVar> Conds;

        private T[] Map;

        public CondMap(List<CategoryVar> C)
        {
            Conds = C;

            int Z = 1;

            for (int i = 0; i < Conds.Count; i++)
            {
                CategoryVar X = Conds[i];

                Z *= X.Domain.Count;
            }

            Map = new T[Z];
        }

        public void Put(T thing)
        {
            Map[GetMapIndex()] = thing;
        }
    
        public T Get()
        {
            return Map[GetMapIndex()];
        }

        private int GetMapIndex()
        {
            int Z = 1;

            for (int i = 0; i < Conds.Count; i++)
            {
                CategoryVar X = Conds[i];

                if (!X.Assigned)
                {
                    throw new Exception("Variable not assigned: " + X.Name);
                }

                Z *= X.ValueIndex;
            }

            return Z;
        }
    }

    public class BayesNet : Model
    {
        public List<CondMap<CategoryDist>> CPTables;

        // variables must be ordered

        public BayesNet(string name) : base(name)
        {
            CPTables = new List<CondMap<CategoryDist>>();
        }

        public double Info()
        {
            // information of current joint distribution

            double info = 0.0;

            for (int i = 0; i < Variables.Count; i++)
            {
                CategoryDist D = CPTables[i].Get();

                info += D.GetInfo();
            }

            return info;
        }
    }

    public class Bucket
    {
        public List<CategoryVar> Scheme;

        public List<CondMap<double>> Inputs;

        public CondMap<double> Output;

        public Bucket()
        {
            Scheme = new List<CategoryVar>();
            Inputs = new List<CondMap<double>>();
        }
    }

    public class BayesNetInference
    {
        public BayesNet BN;
        private StreamWriter InfoWriter;
        public List<Bucket> Buckets;

        public BayesNetInference(BayesNet bn, StreamWriter infoWriter)
        {
            BN = bn;
            InfoWriter = infoWriter;
            Buckets = new List<Bucket>();
        }

        public double Info()
        {
            // returns the joint information of assigned variables, eliminating others

            Buckets.Clear();

            double info = 0.0;

            // TO DO: implement bucket elimination algorithm
            
            for (int i = 0; i < BN.Variables.Count; i++)
            {
                CondMap<CategoryDist> CP = BN.CPTables[i];

                Bucket B = new Bucket();

                B.Scheme.Add((CategoryVar)BN.Variables[i]);

                for (int j = 0; j <= i; j++)
                {
                    
                }
            }

            return info;
        }
    }
}
