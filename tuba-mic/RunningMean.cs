using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tuba_mic
{
    // I am pretty sure the functionality of this class
    // already exists within the buitin Queue class, but
    // I am unsure of how efficient it is. The efficiency
    // of the `Average` function may be a concern
    class RunningMean
    {
        private float sum = 0;
        private int maxSize;
        private Queue<float> vals;

        public RunningMean(int maxSize)
        {
            if (maxSize <= 0)
            {
                // don't do this
            }
            this.maxSize = maxSize;
            vals = new Queue<float>(maxSize);

        }

        public void add(float val)
        {
            if (vals.Count == maxSize)
            {
                sum -= vals.Dequeue();
            }
            vals.Enqueue(val);
            sum += val;
        }

        public float avg()
        {
            return sum / maxSize;
        }
    }
}
