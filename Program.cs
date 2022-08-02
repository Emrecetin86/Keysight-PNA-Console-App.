using Agilent.CommandExpert.ScpiNet.AgU2040_A_02_06;
using System;
using Agilent.CommandExpert.ScpiNet.AgPNA_A_09_00;
using Ivi.Visa.Interop;

namespace PNA_Console
{
    class Program
    {
        static double[] Power_Result = { };
        static double PS_Frequency = 2000000000D;
        private static double[] Frekans;
        private static double[] Isolation;
        private static double[] InputMatch;
        private static double[] Gain;
        private static double[] OutputMatch;


        static void Main(string[] args)
        {

            ///Start of measurement
            Console.WriteLine("Measurement Started");

            /// Power Sensor Commands
            AgU2040 SCPI_Xseries_Power = new AgU2040("USB0::0x2A8D::0x4801::MY58000117::0::INSTR");
            SCPI_Xseries_Power.SCPI.STATus.PRESet.Command();
            SCPI_Xseries_Power.SCPI.SENSe.DETector.FUNCtion.Command(1u, "AVERage");
            SCPI_Xseries_Power.SCPI.SENSe.FREQuency.CW.Command(null, PS_Frequency);
            SCPI_Xseries_Power.SCPI.FORMat.READings.DATA.Command("ASCii");
            SCPI_Xseries_Power.SCPI.MEASure.SCALar.POWer.AC.QueryAsciiReal(null, "DEFault", "DEFault", null, out Power_Result);
            
            ///VNA Commands in a seperate function
            void S_Parameter_Data(double StartFreq, double StopFreq, out double[] InputMatch, out double[] OutputMatch, out double[] Gain, out double[] Isolation, out double[] Frekans)
            {
                int opc = 0;
                int opc1 = 0;
                int Average_Bits = 0;
                // In order to use the following driver class, you need to reference this assembly : [C:\ProgramData\Keysight\Command Expert\ScpiNetDrivers\AgPNA_A_09_00.dll]

                AgPNA PNA = new AgPNA("TCPIP0::localhost::hislip2::INSTR");
                PNA.Connect();
                PNA.Transport.DefaultTimeout.Set(10000);
                PNA.SCPI.SYSTem.PRESet.Command();
                PNA.SCPI.OPC.Query(out opc);
                PNA.SCPI.SENSe.FREQuency.STARt.Command(null, StartFreq);
                PNA.SCPI.SENSe.FREQuency.STOP.Command(null, StopFreq);
                PNA.SCPI.SENSe.SWEep.POINts.Command(1u, 801);
                PNA.SCPI.SENSe.BWIDth.RESolution.Command(1u, 10e3);
                PNA.SCPI.FORMat.DATA.Command("REAL", 64);
                PNA.SCPI.FORMat.BORDer.Command("SWAPped");
                PNA.SCPI.CALCulate.PARameter.DEFine.EXTended.Command(1u, "InputMatch", "S11");
                PNA.SCPI.CALCulate.PARameter.DEFine.EXTended.Command(1u, "OutputMatch", "S22");
                PNA.SCPI.CALCulate.PARameter.DEFine.EXTended.Command(1u, "Gain", "S21");
                PNA.SCPI.CALCulate.PARameter.DEFine.EXTended.Command(1u, "Isolation", "S12");
                PNA.SCPI.CALCulate.PARameter.SELect.Command(null, "InputMatch");
                PNA.SCPI.ABORt.Command();
                PNA.SCPI.SENSe.AVERage.COUNt.Command(1u, 64);
                PNA.SCPI.SENSe.AVERage.STATe.Command(1u, true);

                //Change this
                PNA.SCPI.INITiate.CONTinuous.Command(null, false);


                //Change this
                for (int i=0; i<64; i++)
                {
                    PNA.SCPI.INITiate.IMMediate.Command(null);
                    PNA.SCPI.OPC.Query(out opc1);
                } 
                
                /*while (true)
                {
                    PNA.SCPI.STATus.OPERation.AVERaging.CONDition.Query(1u, out Average_Bits);
                    if (Average_Bits == 62)
                        break;
                } */ 


                Console.WriteLine("Average Completed");
                PNA.SCPI.SENSe.SWEep.MODE.Command(1u, "HOLD");
                PNA.SCPI.CALCulate.DATA.QueryBlockReal64(null, "FDATA", out InputMatch);
                PNA.SCPI.SENSe.X.VALues.QueryBlockReal64(null, out Frekans);
                PNA.SCPI.CALCulate.PARameter.SELect.Command(null, "OutputMatch");
                PNA.SCPI.CALCulate.DATA.QueryBlockReal64(null, "FDATA", out OutputMatch);
                PNA.SCPI.CALCulate.PARameter.SELect.Command(null, "Gain");
                PNA.SCPI.CALCulate.DATA.QueryBlockReal64(null, "FDATA", out Gain);
                PNA.SCPI.CALCulate.PARameter.SELect.Command(null, "Isolation");
                PNA.SCPI.CALCulate.DATA.QueryBlockReal64(null, "FDATA", out Isolation);
                PNA.Disconnect();
            }
            ///Calling function
            S_Parameter_Data(1e9, 10e9, out InputMatch, out OutputMatch, out Gain, out Isolation, out Frekans);

            ///Completition of measurement
            Console.WriteLine("Measurement Completed");

        }

        private static void IHislipInstr()
        {
            throw new NotImplementedException();
        }
    }
}
