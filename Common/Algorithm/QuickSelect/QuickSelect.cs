using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class QuickSelect
    {
        private int num_array = 0;
        private int num_med_array = 0;
        //private double array [num_array];
        double[] midian_array = null;

        /// <summary>
        /// 快选算法的初始化
        /// </summary>
        /// <param name="array_length"></param>
        public QuickSelect(int array_length)
        {
            num_array = array_length;
            num_med_array = num_array / 5 + (num_array % 5 == 0 ? 0 : 1);
            midian_array = new double[num_med_array];
        }

        /// <summary>
        /// 快速选择算法
        /// </summary>
        /// <param name="array"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public double QSelect(double[] array, int left, int right, int k)
        {
            // 寻找中位数的中位数
            double median = FindMedian(array, left, right);

            // 将中位数的中位数与最右元素交换
            int index = FindIndex(array, left, right, median);
            Swap(ref array[index], ref array[right]);

            double pivot = array[right];

            // 申请两个移动指针关初始化
            int i = left;
            int j = right - 1;

            // 根据枢纽元素的值对数组进行一次划分，保证数组左边的数全部小于pivot,数组右边的数全部大于privoi
            // 注意这里左边并非恰好是数组的左1/2，可能多一些，也可能少一些，总之保证存在一个中间位置，使左边
            // 右边分别小于/大于pivot即可
            while (true)
            {
                while (i <= array.Length-2 && array[i] < pivot)
                    i++;
                while (j >= 0 && array[j] >= pivot)
                    j--;

                if (i < j)
                    Swap(ref array[i], ref array[j]);
                else
                    break;
            }
            Swap(ref array[i], ref array[right]);

            /* 对三种情况进行处理：（m = i - left + 1）
             1、如果m = k，即返回的主元即为我们要找的第k小的元素，那么直接返回主元a[i]即可；
             2、如果m > k，那第接下来要到低区间A[0....m-1]中寻找，选择高区间；
             3、如果m < k，那第接下来要到高区间A[0....m-1]中寻找，选择低区间；
             */
            int m = i - left + 1;
            if (m == k)
                return array[i];
            else if (m > k)
                return QSelect(array, left, i - 1, k);
            else
                return QSelect(array, i + 1, right, k - m);
        }

        private void InsertSort(double[] array, int left, int loop_times )
        {
            for (int j = left + 1; j <= left + loop_times; j++)
            {
                double key = array[j];
                int i = j - 1;
                while (i >= left && array[i] > key)
                {
                    array[i + 1] = array[i];
                    i--;
                }
                array[i + 1] = key;
            }
        }

        private double FindMedian(double[] array, int left, int right)
        {
            if (left == right)
                return array[left];

            int index;
            for (index = left; index < right - 5; index+=5)
            {
                InsertSort(array, index, 4);
                int num = index - left;
                midian_array[num / 5] = array[index + 2];
            }

            //处理剩余元素
            int remain_num = right - index + 1;
            if (remain_num > 0)
            {
                InsertSort(array, index, remain_num - 1);
                int num = index - left;
                // index -1 确保index指向的是上一节（每5个元素为一节）的末尾，
                // 而(remain_num + 1) / 2确保取到剩余元素的中位数在本节的位置
                midian_array[num/5] = array[index - 1 + (remain_num + 1) / 2];
            }

            int elem_aux_array = (right - left) / 5 - ((right - left) % 5 == 0 ? 1 : 0);

            //如果剩余一个元素则返回，否则继续递归
            if (elem_aux_array <= 0)
                return midian_array[0];
            else
                return FindMedian(midian_array, 0, elem_aux_array);
        }

        private int FindIndex(double[] array, int left, int right, double median)
        {
            for (int i = left; i <= right; i++)
            {
                if (array[i] == median)
                    return i;
            }
            return -1;
        }        

        private void Swap(ref double first, ref double right)
        {
            double temp = first;
            first = right;
            right = temp;
        }
    }
}
