using System;
using System.IO;
using System.Collections.Generic;

namespace AIProg
{
    public abstract class Constraint
    {
        public List<Variable> Scheme;

        protected Constraint()
        {
            Scheme = new List<Variable>();
        }

        public bool CanEvaluate()
        {
            foreach (Variable X in Scheme)
            {
                if (!X.Assigned)
                {
                    return false;
                }
            }

            return true;
        }

        public bool DependsOn(string s)
        {
            foreach (Variable X in Scheme)
            {
                if (X.Name == s)
                {
                    return true;
                }
            }

            return false;
        }

        public abstract bool Satisfied();

        public bool Violated()
        {
            if (!CanEvaluate())
            {
                return false;
            }
            else if (Satisfied())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public abstract class ConstraintSystem : Model
    {
        public List<Constraint> Constraints;

        public ConstraintSystem(string s) : base(s)
        {
            Constraints = new List<Constraint>();
        }

        public bool Satisfied()
        {
            foreach (Constraint C in Constraints)
            {
                if (!C.Satisfied())
                {
                    return false;
                }
            }

            return true;
        }

        public bool Violated()
        {
            foreach (Constraint C in Constraints)
            {
                if (C.Violated())
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class SimpleConstraintSat
    {
        public int SolutionCount;
        public int StepCount;
        private ConstraintSystem System;
        private StreamWriter Writer;

        public SimpleConstraintSat(ConstraintSystem S, StreamWriter W)
        {
            System = S;
            Writer = W;
        }

        public bool Satisfy()
        {
            System.ClearState();

            StepCount = 0;

            return SatisfyStep(0);
        }

        private bool SatisfyStep(int depth)
        {
            StepCount += 1;

            if (StepCount % 1000 == 0)
            {
                Writer.WriteLine("Current step count: " + StepCount.ToString());
            }

            if (depth == System.NumVars)
            {
                return true;
            }
            else
            {
                CategoryVar X = (CategoryVar)System.Variables[depth];

                foreach (string val in X.Domain)
                {
                    X.Assign(val);

                    if (!System.Violated() && SatisfyStep(depth + 1))
                    {
                        return true;
                    }
                }

                X.ClearValue();

                return false;
            }
        }

        public void Enumerate(int maxSolutions)
        {
            System.ClearState();

            SolutionCount = 0;

            StepCount = 0;

            Writer.WriteLine("Enumerating solutions for: " + System.Name);

            EnumerateStep(0, maxSolutions);

            Writer.WriteLine(SolutionCount.ToString() + " solutions found");
            Writer.WriteLine(StepCount.ToString() + " steps executed");
        }

        public void EnumerateStep(int depth, int maxCount)
        {
            StepCount += 1;

            if (StepCount % 1000 == 0)
            {
                Writer.WriteLine("Current step count: " + StepCount.ToString());
            }

            if (SolutionCount == maxCount)
            {
                return;
            }
            else if (depth == System.NumVars)
            {
                SolutionCount += 1;

                Writer.WriteLine("Solution " + SolutionCount.ToString());
                
                System.PrettyPrint(Writer);
            }
            else
            {
                CategoryVar X = (CategoryVar)System.Variables[depth];

                foreach (string val in X.Domain)
                {
                    X.Assign(val);

                    if (!System.Violated())
                    {
                        EnumerateStep(depth + 1, maxCount);
                    }

                    if (SolutionCount == maxCount)
                    {
                        break;
                    }
                }

                X.ClearValue();
            }
        }
    }

    public class VarSearchState
    {
        public CategoryVar Variable;
        public DynamicSet DomainState;
        public Stack<int> RestorePoints;
        public List<Constraint> Constraints;
        public int AssignIndex;

        public VarSearchState(CategoryVar X, ConstraintSystem S)
        {
            Variable = X;
            DomainState = new DynamicSet(X.Domain.Count);
            RestorePoints = new Stack<int>();
            Constraints = new List<Constraint>();

            foreach (Constraint C in S.Constraints)
            {
                if (C.DependsOn(X.Name))
                {
                    Constraints.Add(C);
                }
            }
        }

        public void Init()
        {
            DomainState.Init();
            RestorePoints.Clear();
        }

        public int CurrentDomainSize
        {
            get { return DomainState.Count; }
        }

        public void Assign(int i)
        {
            Variable.ValueIndex = DomainState.Get(i);
        }

        public void Prune()
        {
            RestorePoints.Push(DomainState.Count);

            int i = DomainState.Count;

            while (i-- > 0)
            {
                Variable.ValueIndex = DomainState.Get(i);

                // is any constraint violated?

                bool violated = false;

                for (int c = 0; c < Constraints.Count; c++)
                {
                    if (Constraints[c].Violated())
                    {
                        violated = true;

                        break;
                    }
                }

                if (violated)
                {
                    DomainState.Remove(i);
                }
            }

            Variable.ValueIndex = -1;
        }

        public void Restore()
        {
            DomainState.Restore(RestorePoints.Pop());
        }
    }

    public class ProactiveConstraintSat
    {
        public int SolutionCount;
        public int StepCount;

        private ConstraintSystem System;
        private List<VarSearchState> VarStates;
        private StreamWriter Writer;

        private DynamicSet VarMap;

        public ProactiveConstraintSat(ConstraintSystem S, StreamWriter W)
        {
            System = S;
            Writer = W;

            VarStates = new List<VarSearchState>();

            foreach (CategoryVar X in S.Variables)
            {
                VarStates.Add(new VarSearchState(X, S));
            }

            VarMap = new DynamicSet(S.Variables.Count);
        }

        public bool Satisfy()
        {
            System.ClearState();

            StepCount = 0;

            foreach (VarSearchState V in VarStates)
            {
                V.Init();
            }

            VarMap.Init();
            
            return SatisfyStep(0);
        }

        private bool SatisfyStep(int depth)
        {
            StepCount += 1;

            if (StepCount % 1000 == 0)
            {
                Writer.WriteLine("Current step count: " + StepCount.ToString());
            }

            if (depth == VarStates.Count)
            {
                return true;
            }
            else
            {
                int lowest = 0;
                int lowestIndex = -1;

                for (int j = 0; j < VarMap.Count; j++)
                {
                    int v = VarMap.Get(j);

                    VarSearchState V = VarStates[v];

                    V.Prune();

                    if (j == 0)
                    {
                        lowest = V.CurrentDomainSize;
                        lowestIndex = j;
                    }
                    else if (V.CurrentDomainSize < lowest)
                    {
                        lowest = V.CurrentDomainSize;
                        lowestIndex = j;
                    }
                }

                if (lowest > 0)
                {
                    int v = VarMap.Get(lowestIndex);

                    VarMap.Remove(lowestIndex);

                    VarSearchState V = VarStates[v];

                    int n = V.CurrentDomainSize;

                    for (int d = 0; d < n; d++)
                    {
                        V.Assign(d);

                        if (SatisfyStep(depth + 1))
                        {
                            return true;
                        }

                        V.Variable.ClearValue();
                    }

                    VarMap.Restore(VarMap.Count + 1);
                }
                       
                for (int j = 0; j < VarMap.Count; j++)
                {
                    int v = VarMap.Get(j);

                    VarSearchState V = VarStates[v];

                    V.Restore();
                }

                return false;
            }
        }

        public void Enumerate(int maxSolutions)
        {
            System.ClearState();

            StepCount = 0;

            SolutionCount = 0;

            foreach (VarSearchState V in VarStates)
            {
                V.Init();
            }

            VarMap.Init();
            
            Writer.WriteLine("Enumerating solutions for: " + System.Name);

            EnumerateStep(0, maxSolutions);

            System.ClearState();

            Writer.WriteLine(SolutionCount.ToString() + " solutions found");
            Writer.WriteLine(StepCount.ToString() + " steps executed");
        }

        private void EnumerateStep(int depth, int maxSolutions)
        {
            StepCount += 1;

            if (StepCount % 1000 == 0)
            {
                Writer.WriteLine("Current step count: " + StepCount.ToString());
            }

            if (SolutionCount >= maxSolutions)
            {
                return;
            }
            else if (depth == VarStates.Count)
            {
                SolutionCount += 1;

                Writer.WriteLine("Solution " + SolutionCount.ToString());
                
                //System.PrintState(Writer);
                System.PrettyPrint(Writer);
            }
            else
            {
                int lowest = 0;
                int lowestIndex = -1;

                for (int j = 0; j < VarMap.Count; j++)
                {
                    int v = VarMap.Get(j);

                    VarSearchState V = VarStates[v];

                    V.Prune();

                    if (j == 0)
                    {
                        lowest = V.CurrentDomainSize;
                        lowestIndex = j;
                    }
                    else if (V.CurrentDomainSize < lowest)
                    {
                        lowest = V.CurrentDomainSize;
                        lowestIndex = j;
                    }
                }

                if (lowest > 0)
                {
                    int v = VarMap.Get(lowestIndex);

                    VarMap.Remove(lowestIndex);

                    VarSearchState V = VarStates[v];

                    int n = V.CurrentDomainSize;

                    for (int d = 0; d < n; d++)
                    {
                        V.Assign(d);

                        EnumerateStep(depth + 1, maxSolutions);

                        V.Variable.ClearValue();
                    }

                    VarMap.Restore(VarMap.Count + 1);
                }
                       
                for (int j = 0; j < VarMap.Count; j++)
                {
                    int v = VarMap.Get(j);

                    VarSearchState V = VarStates[v];

                    V.Restore();
                }
            }
        }
    }
}