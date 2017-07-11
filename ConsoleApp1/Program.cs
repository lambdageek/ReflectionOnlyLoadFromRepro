using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace ConsoleApp1
{
        class Program
        {
                static readonly Dictionary<string, string> asmPaths = new Dictionary<string, string>();

                static void Main(string[] args)
                {
                        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;

                        var path = Path.GetDirectoryName(args[0]);

                        foreach (var asmPath in Directory.EnumerateFiles(path, "*.dll"))
                        {
                                if (asmPath == args[0])
                                {
                                        continue;
                                }

                                try
                                {
                                        using (var stream = File.OpenRead(asmPath))
                                        {
                                                using (var peReader = new PEReader(stream))
                                                {
                                                        var mr = peReader.GetMetadataReader();
                                                        var ad = mr.GetAssemblyDefinition();

                                                        var asmName = new AssemblyName
                                                        {
                                                                Name = mr.GetString(ad.Name),
                                                                Version = ad.Version,
                                                                CultureInfo = CultureInfo.GetCultureInfo(mr.GetString(ad.Culture)),
                                                        };

                                                        var publicKey = mr.GetBlobBytes(ad.PublicKey);
                                                        if (publicKey != null)
                                                                asmName.SetPublicKey(publicKey);

                                                        asmPaths.Add(asmName.ToString(), asmPath);
                                                        Console.WriteLine($"Got assembly name {asmName} for path {asmPath}.");
                                                }
                                        }
                                }
                                catch (Exception e)
                                {
                                        //Console.WriteLine($"Couldn't get assembly name for {asmPath}: {e.Message}.");
                                }
                        }

                        var asm = Assembly.ReflectionOnlyLoadFrom(args[0]);

                        try
                        {
                                asm.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                                foreach (var ex in e.LoaderExceptions)
                                        Console.WriteLine(ex);
                        }

                        try
                        {
                                var type = asm.GetType("ClassLibrary1.TestClass, ClassLibrary1");
                                Console.WriteLine(type.FullName);
                        }
                        catch (Exception e)
                        {
                                Console.WriteLine($"Could not load type by name: {e.Message}");
                        }
                }

                private static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
                {
                        Console.WriteLine($"Asked to resolve {args.Name}.");

                        if (asmPaths.TryGetValue(args.Name, out var path))
                                return Assembly.ReflectionOnlyLoadFrom(path);

                        var asmName = new AssemblyName(args.Name);

                        if (asmName.Name == "System.Runtime")
                        {
                                // Find system.runtime w/o version.
                                var asmPath = asmPaths.SingleOrDefault(kvp => new AssemblyName(kvp.Key).Name == "System.Runtime").Value;

                                if (asmPath != null)
                                        return Assembly.ReflectionOnlyLoadFrom(asmPath);
                        }

                        Console.WriteLine($"Couldn't find assembly {args.Name}.");
                        return null;
                }
        }
}
