/// Binary tree based file Heap Sort
/// Made by Paulius Ložys 2020
///
/// This program performs a HeapSort on a file that is written in a form of a binary form (for better understanding check the "Data structure and reading" png file)
/// This sorting is based on a two variable sort (char and double)
/// To adabt this code to diffrent sorting, make sure the byte sizes are correct
/// Current tree "node" structure is like this:
/// int | char | double | int
/// two ints are the pointers to another place in the file which in total are 8 bytes
/// char and double in total are 2+8 = 10 bytes
/// in total the whole "node" size is 18 bytes
/// depending what variables are used the total size will change, so make sure they are all correct
/// also change what variables are used in the swap parameters

using System;
using System.Diagnostics;
using System.IO;

namespace Heap_Sort
{
    class Program
    {
        static void Main(string[] args)
        {
            //BatchTesting();
            Test(50);
        }

        private static void Test(int size)
        {
            string rez = $"rez.dat";

            if (File.Exists(rez)) File.Delete(rez);
            FileStream output = new FileStream($"rez.dat", FileMode.Create, FileAccess.Write);
            Stopwatch t = new Stopwatch();

            CreateFile("data.dat", size); // Creates a binary tree based data file
            t.Start();
            BinaryTreeHeapSort.HeapSort(@"data.dat", output);
            t.Stop();
            Console.WriteLine("Nr. of elements: {0,-10}  Time: {1,10}  Ticks: {2}", size, t.Elapsed, t.ElapsedTicks);
            output.Close();
            ReadFile(rez);

            File.Delete(rez);
        }
        private static void BatchTesting()
        {
            // Testing with various amounts of data
            int[] data = { 1000, 2_000, 4_000, 8_000, 16_000, 32_000, 64_000, 128_000 };
            foreach (var count in data)
            {
                string rez = $"rez{count}.dat";

                if (File.Exists(rez)) File.Delete(rez);
                FileStream output = new FileStream($"rez{count}.dat", FileMode.Create, FileAccess.Write);
                Stopwatch t = new Stopwatch();

                CreateFile("data.dat", count); // Creates a binary tree based data file
                t.Start();
                BinaryTreeHeapSort.HeapSort(@"data.dat", output);
                t.Stop();
                Console.WriteLine("Nr. of elements: {0,-10}  Time: {1,10}  Ticks: {2}", count, t.Elapsed, t.ElapsedTicks);
                output.Close();
                File.Delete(rez);
            }
        }

        #region File reading/writing
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
    }
}
