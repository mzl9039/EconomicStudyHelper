﻿/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-18
 * 时间: 16:10
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Data;

namespace DataHelper
{
    public class Const
    {
        public const string AllCaculatedPath = "计算全部中位数";
    }

    public enum FunctionType
    {
        Default = 0,
        Kd = 1,
        PopulationStatics = 2,
        EGIndex = 3,
        EGIndexRobust = 4,
        KdEachTable = 5,
        KdEachTablbCara = 6,
        KdEachTableCircle = 7
    }
}
