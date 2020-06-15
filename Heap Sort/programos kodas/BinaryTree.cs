using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heap_Sort
{
    class BinarySearchTree
    {
        public TreeNode Root { get; set; }
        int count;
        public BinarySearchTree()
        {
            this.Root = null;
            count = 0;
        }
        public void Insert(char x, double d)
        {
            this.Root = Insert(x, d, this.Root);
        }
        public void Print()
        {
            Print(this.Root);
        }
        public void WriteToFile(string filename, int n)
        {
            byte[][] bufTree = new byte[n][];
            BuildBufTree(bufTree, this.Root);
            if (File.Exists(filename)) File.Delete(filename);
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(filename,
                FileMode.Create)))
                {
                    foreach (var item in bufTree)
                    {
                        for (int j = 0; j < item.Length; j++)
                            writer.Write(item[j]);
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private void BuildBufTree(byte[][] bufTree, TreeNode t)
        {
            int mn = -1; // Null space
            if (t == null)
            {
                return;
            }
            else
            {
                BuildBufTree(bufTree, t.Left);
                int i = t.ElementNum;
                bufTree[i] = new byte[18];

                if (t.Left != null) // Rasomas kaires puses indeksas
                    BitConverter.GetBytes(t.Left.ElementNum).CopyTo(bufTree[i], 0);
                else
                    BitConverter.GetBytes(mn).CopyTo(bufTree[i], 0);
                
                // Rasomi duomenys (char po to double)
                BitConverter.GetBytes(t.ElementOne).CopyTo(bufTree[i], 4);
                BitConverter.GetBytes(t.ElementTwo).CopyTo(bufTree[i], 6);

                if (t.Right != null) // Rasoma desines puses indeksas
                    BitConverter.GetBytes(t.Right.ElementNum).CopyTo(bufTree[i], 14);
                else
                    BitConverter.GetBytes(mn).CopyTo(bufTree[i], 14);

                BuildBufTree(bufTree, t.Right);
            }
        }
        protected TreeNode Insert(char x, double d, TreeNode t)
        {
            if (t == null)
            {
                t = new TreeNode(x, d, count++);
            }
            else if (x.CompareTo(t.ElementOne) < 0)
            {
                t.Left = Insert(x,d, t.Left);
            }
            else if (x.CompareTo(t.ElementOne) > 0)
            {
                t.Right = Insert(x,d, t.Right);
            }
            else
            {
                if(d.CompareTo(t.ElementTwo) < 0)
                {
                    t.Left = Insert(x, d, t.Left);
                }
                else if (d.CompareTo(t.ElementTwo) >= 0)
                {
                    t.Right = Insert(x, d, t.Right);
                }
                //else
                //{
                //    //throw new Exception("Duplikatas medije");
                //}
            }
            return t;
        }
        private void Print(TreeNode t)
        {
            if (t == null)
            {
                return;
            }
            else
            {
                Print(t.Left);
                if (t.Left != null) Console.Write("{0,3:N0} <<- ",
                t.Left.ElementNum);
                else Console.Write(" ");
                Console.Write("{0,3:N0} char:{1} double:{2}", t.ElementNum, t.ElementOne, t.ElementTwo);
                if (t.Right != null) Console.WriteLine(" ->> {0,3:N0}",
                t.Right.ElementNum);
                else Console.WriteLine(" ");
                Print(t.Right);
            }
        }
    }

class TreeNode
    {
        public char ElementOne { get; set; }
        public double ElementTwo { get; set; }
        public TreeNode Left { get; set; }
        public TreeNode Right { get; set; }
        public int ElementNum { get; set; }
        public TreeNode(char elementOne, double elementTwo, int num)
        {
            this.ElementOne = elementOne;
            this.ElementTwo = elementTwo;
            this.ElementNum = num;
        }
    }
}
