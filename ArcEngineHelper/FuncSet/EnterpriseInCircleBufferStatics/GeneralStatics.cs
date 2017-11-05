using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public List<string> excels { get; set; }
    }
}
