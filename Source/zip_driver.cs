using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

// -o blah source

// DIR=C:\Users\RTCOUSENS\colt\projects\Laser\sql
// zip -o CT_2016 colt_tracking.2016.bcp ColtTrackingForMobius.sql
namespace NSZip {
    class Program {
        static void Main(string[] args) {
            //    const string DEFAULT_ZIP_NAME = "dummy.zip";
            int nargs, len, exitcode = 0;


            string anArg, zipName = null;
            bool showHelp = false;
            //     List<string> items = new List<string>();
            List<string> argsToProcess = new List<string>();
            ZipArchive za = null;
            ZipArchiveMode zam = ZipArchiveMode.Create;
            FileStream fs;

            if ((nargs = args.Length) > 0) {
                for (int i = 0; i < nargs; i++) {
                    anArg = args[i];
                    if ((len = anArg.Length) >= 2) {
                        if (anArg[0] == '-' || anArg[0] == '/') {
                            switch (anArg[1]) {
                                case 'o':
                                    if (len > 2)
                                        zipName = anArg.Substring(2).Trim();
                                    else { zipName = args[i + 1]; i++; }
                                    if (string.Compare(Path.GetExtension(zipName), ".zip", true) != 0)
                                        zipName = zipName + ".zip";
                                    break;
                                case 'h': showHelp = true; break;
                                case '?': showHelp = true; break;
                            }
                        } else {
                            argsToProcess.Add(anArg);
                        }
                    }
                }
                if (showHelp) {
                    Console.Out.WriteLine("user requested help.");
                    showUserHelp(Console.Out, Assembly.GetEntryAssembly());
                } else {
                    if (string.IsNullOrEmpty(zipName)) {
                        Console.Error.WriteLine("zip-name not specifified.");
                        showUserHelp(Console.Error, Assembly.GetEntryAssembly());
                        exitcode = 2;
                    } else if (argsToProcess.Count < 1) {
                        Console.Error.WriteLine("no files/directories to zip.");
                        showUserHelp(Console.Error, Assembly.GetEntryAssembly());
                        exitcode = 2;
                    } else {
                        zam = populateZipfile(zipName, argsToProcess, out za, zam, out fs);
                    }
                }
                if (za != null)
                    za.Dispose();
            } else {
                Console.Out.WriteLine("no arguments entered.");
                showUserHelp(Console.Out, Assembly.GetEntryAssembly());
            }
            Environment.Exit(exitcode);
        }

        private static ZipArchiveMode populateZipfile(string zipName, List<string> argsToProcess, out ZipArchive za, ZipArchiveMode zam, out FileStream fs) {
            if (File.Exists(zipName))
                zam = ZipArchiveMode.Update;
            fs = new FileStream(zipName, FileMode.OpenOrCreate);
            za = new ZipArchive(fs, zam);
            foreach (string anArgs in argsToProcess) {
                if (File.Exists(anArgs)) {
                    replaceEntryIfExists(za, anArgs);
                } else if (Directory.Exists(anArgs)) {
                    addDirectoryContentsTo(za, anArgs);
                }
            }

            return zam;
        }

        static void addDirectoryContentsTo(ZipArchive za, string dirName) {
            List<string> files = new List<string>();
            string[] kids;

            kids = findChildrenOf(dirName);
            if (kids.Length > 0)
                files.AddRange(kids);
            foreach (string aKid in kids)
                replaceEntryIfExists(za, aKid);
        }

        static void replaceEntryIfExists(ZipArchive za, string aKid) {
            ZipArchiveEntry z;

            if (za.Mode != ZipArchiveMode.Create)
                if ((z = za.GetEntry(aKid)) != null)
                    z.Delete();
            za.CreateEntryFromFile(aKid, aKid, CompressionLevel.Optimal);
        }

        static string[] findChildrenOf(string dirName) {
            List<string> ret = new List<string>();
            string[] tmp, kids;

            tmp = Directory.GetDirectories(dirName);
            foreach (string aDir in tmp) {
                kids = findChildrenOf(aDir);
                if (kids.Length > 0)
                    ret.AddRange(kids);
            }
            if ((tmp = Directory.GetFiles(dirName)).Length > 0)
                ret.AddRange(tmp);
            return ret.ToArray();
        }

        static void showUserHelp(TextWriter tw, Assembly a) {
            tw.WriteLine("usage:");
            // -D devexpress
            // -g generate-code
            // -isDebug phibro-style
            // -s simple

            tw.WriteLine("\t" + Path.GetFileNameWithoutExtension(a.Location) +
                ": -o out_zipfile file_or_directory [...file_or_directory]");
        }
    }
}