using System.Text;

namespace Akzin.Crm.EarlyBoundGenerator.Helpers
{
    class Writer
    {
        readonly StringBuilder sb = new StringBuilder();
        private int indent;

        public void TabPush()
        {
            indent++;
        }

        public void TabPop()
        {
            indent--;
        }

        public void AppendLine()
        {
            sb.AppendLine();
        }

        private void AppendIndent()
        {
            sb.Append(' ', indent * 4);
        }

        public void AppendLine(string line)
        {
            AppendIndent();
            sb.AppendLine(line);
        }

        public override string ToString()
        {
            return sb.ToString();
        }
    }
}