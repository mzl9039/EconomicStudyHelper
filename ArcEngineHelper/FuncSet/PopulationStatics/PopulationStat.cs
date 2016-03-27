using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Common;

namespace DataHelper.FuncSet.PopulationStatics
{
    public class PopulationStat
    {
        public double GetDistanceAvg()
        {
            if (DistanceSum == 0)
                return 0;
            else
                return DistanceAvg = DistanceSum * 2 / (TableCount * (TableCount - 1));
        }

        public double GetPopulationAvg()
        {
            if (PopulationSum == 0)
                return 0;
            else
                return PopulationAvg = PopulationSum / TableCount;
        }

        public double GetXij()
        {
            return Xij = Sij / (PopulationSum * PopulationSum);
        }

        public double GetYij()
        {
            return Yij = Sij * (TableCount * TableCount) * (GetDistanceAvg() * DistanceAvg) / (PopulationSum * PopulationSum);
        }

        public double GetZij()
        {
            return Zij = 1 / (TableCount * DistanceAvg * DistanceAvg);
        }

        public List<Enterprise> Enterprises { get; set; }
        public int TableCount { get; set; }

        public double DistanceSum { get; set; }
        public double PopulationSum { get; set; }

        public double DistanceAvg { get; set; }
        public double PopulationAvg { get; set; }

        public double DistanceQuarter { get; set; }

        public double Sij { get; set; }
        public double Xij { get; set; }
        public double Yij { get; set; }
        public double Zij { get; set; }        
    }

}
