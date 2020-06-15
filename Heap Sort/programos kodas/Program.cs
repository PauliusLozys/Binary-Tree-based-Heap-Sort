using System;
using System.Diagnostics;
using System.IO;

namespace Heap_Sort
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] data = { 1000, 2_000, 4_000, 8_000, 16_000, 32_000, 64_000, 128_000};
            foreach (var count in data)
            {
                string rez = $"rez{count}.dat";

                if (File.Exists(rez)) File.Delete(rez);
                FileStream output = new FileStream($"rez{count}.dat", FileMode.Create, FileAccess.Write);
                Stopwatch t = new Stopwatch();

                CreateFile("data.dat", count);
                t.Start();
                HeapSort(@"data.dat", output);
                t.Stop();
                Console.WriteLine("Nr. of elements: {0,-10}  Time: {1,10}  Ticks: {2}", count, t.Elapsed, t.ElapsedTicks);
                output.Close();
                File.Delete(rez);
            }
        }

        #region Failo rašymas/skaitymas
        static void CreateFile(string fileName, int ElementCount)
        {
            Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            BinarySearchTree t = new BinarySearchTree();

            for (int i = 0; i < ElementCount; i++)
            {
                t.Insert((char)rand.Next(65, 91), Math.Round(rand.NextDouble()*10,2));
            }
            t.WriteToFile(fileName, ElementCount);
        }
        static void ReadFile(string fileName)
        {
            byte[] dataBlock = new byte[10];

            using (var fs = new FileStream(fileName,FileMode.Open,FileAccess.Read))
            {
                for (int i = 0; i < fs.Length/10; i++)
                {
                    fs.Read(dataBlock, 0, 10);
                    Console.WriteLine($"{i+1}|char: {BitConverter.ToChar(dataBlock,0)} -- double: {BitConverter.ToDouble(dataBlock,2)}");
                }
                
            }
        }
        #endregion

        #region Heap Sort metodai
        static void ReadBlock(FileStream fs, int offset, byte[] dataBlock)
        {
            fs.Seek(offset * 18, SeekOrigin.Begin);
            fs.Read(dataBlock, 0, 18);
        }
        static void Swap(FileStream fs, byte[] leaf, (char, double) parent, int parentPosition, int leafPosition)
        {
            // Change parrent
            fs.Seek(parentPosition * 18 + 4, SeekOrigin.Begin); // Offset it to the char area
            fs.Write(leaf, 4, 10);

            // Change leaf
            fs.Seek(leafPosition * 18 + 4, SeekOrigin.Begin); // Offset it to the char area
            fs.Write(BitConverter.GetBytes(parent.Item1), 0, 2);
            fs.Write(BitConverter.GetBytes(parent.Item2), 0, 8);
            
            // Re-reads the changed leaf data for recursive call after the swap 
            ReadBlock(fs, leafPosition, leaf);
        }
        static void HeapSort(string fileName, FileStream output)
        {
            byte[] dataBlock = new byte[18];
            int root = 0;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                fs.Read(dataBlock, 0, 18); // Reads root node
                BuildMinHeap(fs, dataBlock, ref root); // Builds min binary tree

                for (var i = fs.Length / 18 - 1; i >= 0; i--)
                {
                    ReadBlock(fs, 0, dataBlock); // read root
                    ExtractRoot(fs, dataBlock, 0, output); 

                    ReadBlock(fs, 0, dataBlock); // read root
                    Heapify(fs, dataBlock,ref root);
                }
            }
        }
        static void ExtractRoot(FileStream fs, byte[] dataBlock, int currentPos, FileStream output)
        {
            int l = BitConverter.ToInt32(dataBlock, 0),
                r = BitConverter.ToInt32(dataBlock, 14);

            if (l != -1)
            {
                ReadBlock(fs, l, dataBlock); // left node
                if (BitConverter.ToInt32(dataBlock, 0) == -1 && BitConverter.ToInt32(dataBlock, 14) == -1)
                {
                    byte[] leaf = new byte[18]; // stores leaf data
                    dataBlock.CopyTo(leaf, 0);

                    ReadBlock(fs, 0, dataBlock); // reads root data

                    output.Write(dataBlock, 4, 10);
                    
                    fs.Seek(4, SeekOrigin.Begin);
                    fs.Write(leaf, 4, 10); // swap root to leaf

                    fs.Seek(l * 18 + 4, SeekOrigin.Begin);
                    fs.Write(dataBlock, 4, 10); // swap leaf to root

                    fs.Seek(currentPos * 18, SeekOrigin.Begin);
                    fs.Write(BitConverter.GetBytes(-1), 0, 4); // break the connection to leaf

                    return;
                }
                ExtractRoot(fs, dataBlock, l, output);
            }
            else // einama i desine puse
            {
                if (r == -1) // prints root node
                {
                    // Single root node left in the tree
                    output.Write(dataBlock, 4, 10);
                }
                else
                {
                    ReadBlock(fs, r, dataBlock); // right node
                    if (BitConverter.ToInt32(dataBlock, 0) == -1 && BitConverter.ToInt32(dataBlock, 14) == -1) // Checks if it's not a leaf node
                    {
                        byte[] leaf = new byte[18]; // stores leaf data
                        dataBlock.CopyTo(leaf, 0);
                        ReadBlock(fs, 0, dataBlock); // reads root data

                        output.Write(dataBlock, 4, 10);

                        fs.Seek(4, SeekOrigin.Begin);
                        fs.Write(leaf, 4, 10); // swap root to leaf

                        fs.Seek(r * 18 + 4, SeekOrigin.Begin);
                        fs.Write(dataBlock, 4, 10); // swap leaf to root

                        fs.Seek(currentPos * 18 + 14, SeekOrigin.Begin);
                        fs.Write(BitConverter.GetBytes(-1), 0, 4); // break the connection to leaf

                        return;
                    }
                    ExtractRoot(fs, dataBlock, r, output);
                }
            }

        }
        static void Heapify(FileStream fs, byte[] dataBlock,ref int currentPos)
        {
            int l = BitConverter.ToInt32(dataBlock, 0),
                r = BitConverter.ToInt32(dataBlock, 14);

            if (l == -1 && r == -1) // if its the node of the last layer - return
                return;
            // Get parent data
            char pC = BitConverter.ToChar(dataBlock, 4);
            double pD = BitConverter.ToDouble(dataBlock, 6);
            int smallest = currentPos; // smallest nodes postition

            // Left leaf
            if (l != -1)
            {
                ReadBlock(fs, l, dataBlock);
                char lC = BitConverter.ToChar(dataBlock, 4);
                double lD = BitConverter.ToDouble(dataBlock, 6);

                if (lC < pC || (lC == pC && lD < pD)) // if the left side is smaller
                {
                    smallest = l;
                }
            }
            // Right leaf
            if (r != -1)
            {
                ReadBlock(fs, r, dataBlock);
                char rC = BitConverter.ToChar(dataBlock, 4);
                double rD = BitConverter.ToDouble(dataBlock, 6);

                if (smallest == currentPos && (rC < pC || (rC == pC && rD < pD))) // if the right side is smaller than PARENT
                {
                    smallest = r;
                }
                else if (l != -1) // if the smallest one became LEFT leaf check if the right leaf is smaller
                {
                    ReadBlock(fs, l, dataBlock);
                    char lC = BitConverter.ToChar(dataBlock, 4);
                    double lD = BitConverter.ToDouble(dataBlock, 6);
                    if (rC < lC || (rC == lC && rD < lD)) // if the right side is smaller
                    {
                        smallest = r;
                    }
                }
            }

            if (smallest != currentPos) // if some node is bigger than the parent perform a swap
            {
                ReadBlock(fs, smallest, dataBlock); // Reads smallest leaf
                Swap(fs, dataBlock, (pC, pD), currentPos, smallest);
                Heapify(fs, dataBlock,ref smallest);
            }
        }
        static void BuildMinHeap(FileStream fs, byte[] dataBlock, ref int currentPos)
        {
            int l = BitConverter.ToInt32(dataBlock, 0),
                r = BitConverter.ToInt32(dataBlock, 14);

            if (l != -1)
            {
                ReadBlock(fs, l, dataBlock); // left node
                if (!(BitConverter.ToInt32(dataBlock, 0) == -1 && BitConverter.ToInt32(dataBlock, 14) == -1)) // Checks if it's not a leaf node
                    BuildMinHeap(fs, dataBlock, ref l);
            }
            if (r != -1)
            {
                ReadBlock(fs, r, dataBlock); // right node
                if (!(BitConverter.ToInt32(dataBlock, 0) == -1 && BitConverter.ToInt32(dataBlock, 14) == -1)) // Checks if it's not a leaf node
                    BuildMinHeap(fs, dataBlock, ref r);
            }

            // Get parent node
            ReadBlock(fs, currentPos, dataBlock);
            char pC = BitConverter.ToChar(dataBlock, 4);
            double pD = BitConverter.ToDouble(dataBlock, 6);

            // Reads leafs and compare to this
            int smallest = currentPos; // smallest nodes postition

            // Left leaf
            if (l != -1)
            {
                ReadBlock(fs, l, dataBlock);
                char lC = BitConverter.ToChar(dataBlock, 4);
                double lD = BitConverter.ToDouble(dataBlock, 6);

                if (lC < pC || (lC == pC && lD < pD)) // if the left side is smaller
                {
                    smallest = l;
                }
            }
            // Right leaf
            if (r != -1)
            {
                ReadBlock(fs, r, dataBlock);
                char rC = BitConverter.ToChar(dataBlock, 4);
                double rD = BitConverter.ToDouble(dataBlock, 6);

                if (smallest == currentPos && (rC < pC || (rC == pC && rD < pD))) // if the right side is smaller than PARENT
                {
                    smallest = r;
                }
                else if (smallest == l) // if the smallest one became LEFT leaf check if the right leaf is smaller
                {
                    ReadBlock(fs, l, dataBlock);
                    char lC = BitConverter.ToChar(dataBlock, 4);
                    double lD = BitConverter.ToDouble(dataBlock, 6);
                    if (rC < lC || (rC == lC && rD < lD)) // if the right side is smaller
                    {
                        smallest = r;
                    }
                }
            }

            if (smallest != currentPos) // if some node is smaller than the parent perform a swap
            {
                ReadBlock(fs, smallest, dataBlock); // Reads smaller leaf
                Swap(fs, dataBlock, (pC, pD), currentPos, smallest);
                Heapify(fs, dataBlock, ref smallest);
            }
        }
        #endregion
    }
}
