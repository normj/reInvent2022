using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQSBatchExample
{
    public class MessageReader
    {
        public IEnumerable<string> Read()
        {
            for(int i = 0; i < 100; i++)
            {
                yield return $"Sample message {i}";
            }
        }
    }
}
