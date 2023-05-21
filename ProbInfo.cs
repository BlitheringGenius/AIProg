using System;
using System.IO;
using System.Collections.Generic;

namespace AIProg
{
    public class ProbInfo
    {
        public static double Info(double p)
        {
            if (p == 0)
            {
                throw new Exception("Infinite information");
            }

            return -Math.Log(p, 2.0);
        }
    }

    public class CategoryDist
    {
        private CategoryVar X;
        public double[] P;

        public CategoryDist(CategoryVar x)
        {
            X = x;
            P = new double[X.Domain.Count];
        }

        public double GetProb()
        {
            if (!X.Assigned)
            {
                throw new Exception("Variable not assigned: " + X.Name);
            }

            return P[X.ValueIndex];
        }

        public double GetInfo()
        {
            return ProbInfo.Info(GetProb());
        }
    }
}