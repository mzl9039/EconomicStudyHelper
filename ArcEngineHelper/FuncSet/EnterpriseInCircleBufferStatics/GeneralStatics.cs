using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataHelper.BaseUtil;

namespace DataHelper.FuncSet.EnterpriseInCircleBufferStatics
{
    public class GeneralStatics
    {
        public GeneralStatics(List<string> excels) {
            this.excels = excels;
        }

        public void CircleBufferStatics() {
            CircleBuffer cb = new CircleBuffer();
            cb.ShowDialog();
            double radius = cb.radius;
            this.excels.ForEach(e =>
            {
                EnterpriseInCircleBufferStatics st = 
                    new EnterpriseInCircleBufferStatics(e, radius);
                st.WriteOutputFileName();
            });
        }

        public void CircleBufferStaticsInAllEnterprises()
        {
            CircleBuffer cb = new CircleBuffer();
            cb.ShowDialog();
            double radius = cb.radius;
            List<Enterprise> allEnterprises = DataProcess.ReadExcels(excels, Static.Table, true);
            this.excels.ForEach(e =>
            {
                CircleSearchInAllEnterprises csiae = new CircleSearchInAllEnterprises(e, radius, allEnterprises);
                csiae.CaculateStatics();
            });
        }

        public List<string> excels { get; set; }
    }
}
