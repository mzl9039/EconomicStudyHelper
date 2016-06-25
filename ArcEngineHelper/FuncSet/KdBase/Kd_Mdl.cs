using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataHelper.FuncSet.KdBase
{
    /// <summary>
    /// 设置模拟次数，可以弹出窗口，并保存要设置的要模拟的次数
    /// </summary>
    public class Kd_Mdl
    {
        public static int N { get; set; }

        /// <summary>
        /// 经过距离过滤后，获取的Dij的个数
        /// </summary>
        /// <param name="num"></param>
        public static void SetN(int num)
        {
            Kd_Mdl.N = num;
        }

        public static int SimulateTimes { get; set; }

        public static void SetSimulateTimes()
        {
            FuncSet.SimulateTimes.SimulateTimes time = new FuncSet.SimulateTimes.SimulateTimes();
            time.ShowDialog();
            Kd_Mdl.SimulateTimes = time.SimulateTime;
        }
    }
}
