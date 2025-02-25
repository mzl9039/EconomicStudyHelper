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
        public const string AllCaculatedPath = "所有表的数据";
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
        KdEachTableCircle = 7,
        KdEachTableMultiCircle = 8,
        TransportationCirble = 9
    }

    public enum KdType
    {
        // 原来的 Kd 函数 [5/8/2016 mzl]
        KdClassic = 0,
        // 对企业规模进行计算的 Kd 函数 [5/8/2016 mzl]
        KdScale = 1
    }

    public enum DensityType
    {
        // 以圆的半径来计算浓度 [5/15/2016 21:52:10 mzl]
        Diameter = 0,
        // 以圆内企业的总人数来计算浓度 [5/15/2016 21:52:35 mzl]
        Scale = 1
    }
}
