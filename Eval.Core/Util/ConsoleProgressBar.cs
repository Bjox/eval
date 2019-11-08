using System;
using System.Text;

namespace Eval.Core.Util
{
    [Serializable]
    public class ConsoleProgressBar
    {

        public static StringBuilder BuildProgressBar(StringBuilder sb, int length, double current, double max)
        {
            double percent = current / max;
            int intpercent = (int)(percent * 100);
            if (sb == null)
                sb = new StringBuilder();
            sb.Append("[");
            int pos = (int)(length * percent);
            double part = (length * percent) - pos;
            bool filled = false;
            for (int i = 0; i < length; i++)
            {
                if (i < pos)
                {
                    sb.Append("=");
                }
                else if (part > 0.5 && !filled)
                {
                    sb.Append("-");
                    filled = true;
                }
                else
                {
                    sb.Append(" ");
                }
            }
            sb.Append("] ");
            sb.Append(string.Format("{0,3}", intpercent));
            sb.Append("% ");

            return sb;
        }

    }
}
