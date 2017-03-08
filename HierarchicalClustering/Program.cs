using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        string[] lines = File.ReadAllLines(@"C:\Users\Leon\Documents\Data sets\Examples\chapter3\blogdata.txt");

        List<string> columnNames = lines[0].Split('\t').Skip(1).Select(x => x).ToList();
        var rowNames = new List<string>();
        var data = new List<List<int>>();

        foreach (string line in lines.Skip(1))
        {
            string[] rowParts = line.Split('\t');
            rowNames.Add(rowParts[0]);
            data.Add(rowParts.Skip(1).Select(int.Parse).ToList());
        }

        BiCluster biCluster = Cluster(data, PearsonCorrelation);

        #region Debug cluster

        PrintCluster(biCluster, rowNames);

        #endregion
    }

    private static float PearsonCorrelation(int[] set1, int[] set2)
    {
        if (set1.Length != set2.Length) throw new ArgumentOutOfRangeException(nameof(set1), "Sets are not of equal length.");

        int sum1 = 0;
        int sum2 = 0;
        float sumOfSquares1 = 0;
        float sumOfSquares2 = 0;
        int sumOfProducts = 0;

        foreach (int i in set1)
        {
            sum1 += i;
            sumOfSquares1 += (float)Math.Pow(i, 2);
        }
        foreach (int i in set2)
        {
            sum2 += i;
            sumOfSquares2 += (float)Math.Pow(i, 2);
        }

        for (int i = 0; i < set1.Length; i++)
        {
            sumOfProducts += set1[i] * set2[i];
        }

        int num = sumOfProducts - sum1 * sum2 / set1.Length;
        float den = (float)Math.Sqrt(
            (sumOfSquares1 - Math.Pow(sum1, 2) / set1.Length) *
            (sumOfSquares2 - Math.Pow(sum2, 2) / set2.Length));

        if (Math.Abs(den) < 0.00001) return 0;

        return 1.0F - num / den;
    }

    static BiCluster Cluster(List<List<int>> data, Func<int[], int[], float> algorithm)
    {
        var distances = new Dictionary<int, Dictionary<int, float>>();
        int currentClusterId = -1;

        var clust = new List<BiCluster>();
        for (int i = 0; i < data.Count; i++)
        {
            clust.Add(new BiCluster(data[i].ToArray()) { Id = i });
        }

        while (clust.Count > 1)
        {
            var lowestPair = new Tuple<int, int>(0, 1);
            float closest = algorithm(clust[0].Vec, clust[1].Vec);

            for (int i = 0; i < clust.Count; i++)
            {
                for (int j = i + 1; j < clust.Count; j++)
                {
                    int idI = clust[i].Id;
                    int idJ = clust[j].Id;
                    if (!distances.ContainsKey(idI)) distances.Add(idI, new Dictionary<int, float>());
                    if (!distances[idI].ContainsKey(idJ)) distances[i].Add(idJ, algorithm(clust[i].Vec, clust[j].Vec));

                    float distance = distances[idI][idJ];

                    if (distance < closest)
                    {
                        closest = distance;
                        lowestPair = new Tuple<int, int>(i, j);
                    }
                }
            }

            var mergeVec = new List<int>();
            for (int i = 0; i < clust[0].Vec.Length; i++)
            {
                int x = (clust[lowestPair.Item1].Vec[i] + clust[lowestPair.Item2].Vec[i]) / 2;
                mergeVec.Add(x);
            }

            var newCluster = new BiCluster(mergeVec.ToArray(), clust[lowestPair.Item1], clust[lowestPair.Item2], closest, currentClusterId);

            currentClusterId -= 1;
            clust.RemoveAt(lowestPair.Item2);
            clust.RemoveAt(lowestPair.Item1);
            clust.Append(newCluster);
        }

        return clust[0];
    }

    public static void PrintCluster(BiCluster cluster, List<string> labels, int n = 0)
    {
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine(" ");
        }

        if (cluster.Id < 0) Console.WriteLine("-");
        else
        {
            if (!labels.Any()) Console.WriteLine(cluster.Id);
            else Console.WriteLine(labels[cluster.Id]);
        }

        if (cluster.Left != null) PrintCluster(cluster.Left, labels, n + 1);
        if (cluster.Right != null) PrintCluster(cluster.Right, labels, n + 1);
    }

    public class BiCluster
    {
        public BiCluster(int[] vec)
        {
            Vec = vec;
        }

        public BiCluster(int[] vec, BiCluster left, BiCluster right, float distance, int id)
        {
            Vec = vec;
            Left = left;
            Right = right;
            Distance = distance;
            Id = id;
        }

        public BiCluster Left { get; set; }
        public BiCluster Right { get; set; }
        public int[] Vec { get; set; }
        public int Id { get; set; }
        public float Distance { get; set; }
    }
}