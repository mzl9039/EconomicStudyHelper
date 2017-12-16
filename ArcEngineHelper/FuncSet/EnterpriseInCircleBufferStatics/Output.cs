using System;

namespace DataHelper.FuncSet.EnterpriseInCircleBufferStatics
{
    public class Output
    {
        public Output(string fileId, string circleId, string enterpriseIdInBuffer, double total)
        {
            this.fileId = fileId;
            this.circleId = circleId;
            this.enterpriseIdInBuffer = enterpriseIdInBuffer;
            this.total = total;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}, {3}", fileId, circleId, enterpriseIdInBuffer, total);
        }

        public string fileId { get; set; }
        public string circleId { get; set; }
        public string enterpriseIdInBuffer { get; set; }
        public double total { get; set; }
    }
}
