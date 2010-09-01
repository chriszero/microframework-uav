using System;

using System.Threading;


namespace IMU_Test {
    class Program {
        static void Main(string[] args) {
            RazorImuMod imu = new RazorImuMod("com12");
            Console.ReadLine();
        }

    }
}
