using System;
using System.IO;

namespace Heap_Sort
{
    static class BinaryTreeHeapSort
    {
        static readonly int TotalBlockSize = 18;
        static readonly short FirstVariableSize = 2;
        static readonly short SecondVariableSize = 8;

        static public void HeapSort(string fileName, FileStream output)
        {
            byte[] dataBlock = new byte[TotalBlockSize];
            int root = 0;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                fs.Read(dataBlock, 0, TotalBlockSize); // Reads root node
                BuildMinHeap(fs, dataBlock, ref root); // Builds min binary tree

                for (var i = fs.Length / TotalBlockSize - 1; i >= 0; i--)
                {
                    ReadBlock(fs, 0, dataBlock); // read root
                    ExtractRoot(fs, dataBlock, 0, output);

                    ReadBlock(fs, 0, dataBlock); // read root
                    Heapify(fs, dataBlock, ref root);
                }
            }
            Console.WriteLine("Sorting finished");
        }
        static void ExtractRoot(FileStream fs, byte[] dataBlock, int currentPos, FileStream output)
        {
            int l = BitConverter.ToInt32(dataBlock, 0),
                r = BitConverter.ToInt32(dataBlock, TotalBlockSize - 4);

            if (l != -1)
            {
                ReadBlock(fs, l, dataBlock); // left node
                if (BitConverter.ToInt32(dataBlock, 0) == -1 && BitConverter.ToInt32(dataBlock, TotalBlockSize - 4) == -1)
                {
                    byte[] leaf = new byte[TotalBlockSize]; // stores leaf data
                    dataBlock.CopyTo(leaf, 0);

                    ReadBlock(fs, 0, dataBlock); // reads root data

                    output.Write(dataBlock, 4, FirstVariableSize + SecondVariableSize);

                    fs.Seek(4, SeekOrigin.Begin);
                    fs.Write(leaf, 4, FirstVariableSize + SecondVariableSize); // swap root to leaf

                    fs.Seek(l * TotalBlockSize + 4, SeekOrigin.Begin);
                    fs.Write(dataBlock, 4, FirstVariableSize + SecondVariableSize); // swap leaf to root

                    fs.Seek(currentPos * TotalBlockSize, SeekOrigin.Begin);
                    fs.Write(BitConverter.GetBytes(-1), 0, 4); // break the connection to leaf

                    return;
                }
                ExtractRoot(fs, dataBlock, l, output);
            }
            else // going to the right side
            {
                if (r == -1) // prints root node
                {
                    // Single root node left in the tree
                    output.Write(dataBlock, 4, FirstVariableSize + SecondVariableSize);
                }
                else
                {
                    ReadBlock(fs, r, dataBlock); // right node
                    if (BitConverter.ToInt32(dataBlock, 0) == -1 && BitConverter.ToInt32(dataBlock, TotalBlockSize - 4) == -1) // Checks if it's not a leaf node
                    {
                        byte[] leaf = new byte[TotalBlockSize]; // stores leaf data
                        dataBlock.CopyTo(leaf, 0);
                        ReadBlock(fs, 0, dataBlock); // reads root data

                        output.Write(dataBlock, 4, FirstVariableSize + SecondVariableSize);

                        fs.Seek(4, SeekOrigin.Begin);
                        fs.Write(leaf, 4, FirstVariableSize + SecondVariableSize); // swap root to leaf

                        fs.Seek(r * TotalBlockSize + 4, SeekOrigin.Begin);
                        fs.Write(dataBlock, 4, FirstVariableSize + SecondVariableSize); // swap leaf to root

                        fs.Seek(currentPos * TotalBlockSize + TotalBlockSize - 4, SeekOrigin.Begin);
                        fs.Write(BitConverter.GetBytes(-1), 0, 4); // break the connection to leaf

                        return;
                    }
                    ExtractRoot(fs, dataBlock, r, output);
                }
            }

        }
        static void Heapify(FileStream fs, byte[] dataBlock, ref int currentPos)
        {
            int l = BitConverter.ToInt32(dataBlock, 0),
                r = BitConverter.ToInt32(dataBlock, TotalBlockSize - 4);

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
                Heapify(fs, dataBlock, ref smallest);
            }
        }
        static void BuildMinHeap(FileStream fs, byte[] dataBlock, ref int currentPos)
        {
            int l = BitConverter.ToInt32(dataBlock, 0),
                r = BitConverter.ToInt32(dataBlock, TotalBlockSize - 4);

            if (l != -1)
            {
                ReadBlock(fs, l, dataBlock); // left node
                if (!(BitConverter.ToInt32(dataBlock, 0) == -1 && BitConverter.ToInt32(dataBlock, TotalBlockSize - 4) == -1)) // Checks if it's not a leaf node
                    BuildMinHeap(fs, dataBlock, ref l);
            }
            if (r != -1)
            {
                ReadBlock(fs, r, dataBlock); // right node
                if (!(BitConverter.ToInt32(dataBlock, 0) == -1 && BitConverter.ToInt32(dataBlock, TotalBlockSize - 4) == -1)) // Checks if it's not a leaf node
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

        #region Utility
        static void ReadBlock(FileStream fs, int offset, byte[] dataBlock)
        {
            fs.Seek(offset * TotalBlockSize, SeekOrigin.Begin);
            fs.Read(dataBlock, 0, TotalBlockSize);
        }
        static void Swap(FileStream fs, byte[] leaf, (char, double) parent, int parentPosition, int leafPosition)
        {
            // Change parrent
            fs.Seek(parentPosition * TotalBlockSize + 4, SeekOrigin.Begin); // Offset it to the char area
            fs.Write(leaf, 4, FirstVariableSize + SecondVariableSize);

            // Change leaf
            fs.Seek(leafPosition * TotalBlockSize + 4, SeekOrigin.Begin); // Offset it to the char area
            fs.Write(BitConverter.GetBytes(parent.Item1), 0, FirstVariableSize);
            fs.Write(BitConverter.GetBytes(parent.Item2), 0, SecondVariableSize);

            // Re-reads the changed leaf data for recursive call after the swap 
            ReadBlock(fs, leafPosition, leaf);
        }
        #endregion
    }
}
