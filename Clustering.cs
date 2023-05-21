using System;
using System.IO;
using System.Collections.Generic;

namespace AIProg
{
    public class ClusterModel : Model
    {
        public ClusterModel(string name, int K) : base(name)
        {
            CategoryVar clusterVar = new CategoryVar("cluster");

            for (int i = 0; i < K; i++)
            {
                clusterVar.Domain.Add(i.ToString());
            }

            Variables.Add(clusterVar);
        }
    }

    public class SimpleClusterEngine
    {
        public void Cluster(ClusterModel CM, CategoryData D)
        {
        }
    }
}