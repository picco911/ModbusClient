using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ModusClient.UnitTest
{
    [TestClass]
    public class ReadWriteTest
    {

        [TestMethod]
        public void CanConnect()
        {
            var mb = new ModbusClient.ModbusClient("127.0.0.1", 502);
            mb.Connect();
            Assert.IsTrue(mb.connected);
            mb.Disconnect();
        }

        [TestMethod]
        public void CanWriteReadCoil()
        {
            var mb = new ModbusClient.ModbusClient("127.0.0.1", 502);
            mb.Connect();
            mb.WriteCoil(0, true);
            var r = mb.ReadCoils(0, 1);
            mb.Disconnect();
            Assert.IsTrue(r[0]);
        }

        [TestMethod]
        public void CanWriteReadRegister()
        {
            var mb = new ModbusClient.ModbusClient("127.0.0.1", 502);
            mb.Connect();
            mb.WriteSingleRegister(0, 5);
            var r = mb.ReadHoldingRegisters(0, 1);
            mb.Disconnect();
            Assert.IsTrue(r[0] == 5);
        }

    }
}
