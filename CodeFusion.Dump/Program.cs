using System;
using System.Collections.Generic;
using System.IO;
using CodeFusion.VM;

namespace CodeFusion.Dump;
public class Program
{
    public static void Main(string[] args)
    {
        string path = null;
        bool mainHeader = false;
        bool relocatableHeader = false;
        bool symbols = false;
        bool addresses = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-h")
            {
                mainHeader = true;
            }
            else if (args[i] == "-r")
            {
                relocatableHeader = true;
            }
            else if (args[i] == "-s")
            {
                symbols = true;
            }
            else if (args[i] == "-a")
            {
                addresses = true;
            }
            else
            {
                path = args[i];
            }
        }

        if (path == null)
        {
            Console.Error.WriteLine("Missing file");
            Environment.Exit(1);
        }

        BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open));
        Metadata metadata = Loader.ReadMainHeader(ref reader);
        if (mainHeader)
        {
            Console.WriteLine("Main Header:");
            Console.WriteLine("{0, 15}: {1, -30}", "EntryPoint", metadata.entryPoint);
            Console.WriteLine("{0, 15}: {1, -30}", "VM Version", metadata.version);
            Console.WriteLine("{0, 15}: {1, -30}", "VM Flags", Convert.ToString(metadata.flags, 2));
            Console.WriteLine("{0, 15}: {1, -30}", "Instructions", metadata.programSize);
            Console.WriteLine("{0, 15}: {1, -30}", "Pools", metadata.poolSize);
            Console.WriteLine();
        }

        if ((metadata.flags & Metadata.RELOCATABLE) == Metadata.RELOCATABLE)
        {
            RelocatableMetadata relocatable = Loader.ReadRelocatableHeader(ref reader);

            if (relocatableHeader)
            {
                Console.WriteLine("Relocatable Header:");
                Console.WriteLine("{0, 15}: {1, -30}", "Labels", relocatable.symbolCount);
                Console.WriteLine("{0, 15}: {1, -30}", "Missing", relocatable.missingCount);
                Console.WriteLine("{0, 15}: {1, -30}", "Addresses", relocatable.addressCount);
                Console.WriteLine();
            }

            Dictionary<string, ulong> symbolsDictonary = new Dictionary<string, ulong>();

            for (int i = 0; i < relocatable.symbolCount; i++)
            {
                string name = "";
                byte b;

                do
                {
                    b = reader.ReadByte();
                    if (b != 0)
                    {
                        name += (char)b;
                    }
                } while (b != 0);

                symbolsDictonary.Add(name, BitConverter.ToUInt64(reader.ReadBytes(8)));
            }

            if (symbols)
            {
                Console.WriteLine("Symbols:");
                foreach (KeyValuePair<string, ulong> item in symbolsDictonary)
                {
                    Console.WriteLine("{0, 15}: 0x{1, -30}", item.Key, item.Value.ToString("X"));
                }

                Console.WriteLine();
            }

            for (int i = 0; i < relocatable.missingCount; i++)
            {
                byte b;

                do
                {
                    b = reader.ReadByte();
                } while (b != 0);

                reader.ReadBytes(8);
            }

            List<ulong> addresseList = new List<ulong>();
            for (int i = 0; i < relocatable.addressCount; i++)
            {
                addresseList.Add(BitConverter.ToUInt64(reader.ReadBytes(8)));
            }

            if (addresses)
            {
                Console.WriteLine("Relatives:");

                foreach (ulong address in addresseList)
                {
                    Console.WriteLine("{0, 15}", "0x" + address.ToString("X"));
                }

                Console.WriteLine();
            }
        }
    }
}
