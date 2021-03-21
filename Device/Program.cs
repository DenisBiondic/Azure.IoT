using System;
using System.Threading.Tasks;

namespace Device
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // Parse application parameters
            var enrollmentId = System.Environment.GetEnvironmentVariable("ENROLLMENT_ID");
            var enrollmentPrimaryKey = System.Environment.GetEnvironmentVariable("ENROLLMENT_PRIMARY_KEY");
            var enrollmentIdScope = System.Environment.GetEnvironmentVariable("ENROLLMENT_ID_SCOPE");

            var sample = new DeviceSimulator(enrollmentId, enrollmentPrimaryKey, enrollmentIdScope);
            await sample.RunSampleAsync();

            return 0;
        }
    }
}
