using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace irDevelopers
{
	//By irDevelopers.com
    public static class ModifyInMemory
    {
        public static void ActivateMemoryPatching()
        {
            Assembly[] arr = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in arr)
            {
                if (assembly.FullName.StartsWith("DotNetBrowser,"))
                    ActivateForAssembly(assembly);
            }
            AppDomain.CurrentDomain.AssemblyLoad -= ActivateOnLoad;
            AppDomain.CurrentDomain.AssemblyLoad += ActivateOnLoad;
        }


        private static void ActivateOnLoad(object sender, AssemblyLoadEventArgs e)
        {
            if (e.LoadedAssembly.FullName.StartsWith("DotNetBrowser,"))
                ActivateForAssembly(e.LoadedAssembly);
        }


        private static void ActivateForAssembly(Assembly assembly)
        {
            MethodInfo myGetTrue = typeof(ModifyInMemory).GetMethod("GetTrue", BindingFlags.NonPublic | BindingFlags.Static);
            
            Type[] arrType = null;
            bool isFound = false;
            int nCount = 0;

            try
            {
                arrType = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException err)
            {
                arrType = err.Types;
            }

            Activator.CreateInstance(arrType.FirstOrDefault());

            foreach (var type in arrType)
            {
                if (isFound) break;

                if (type == null) continue;

                if(type?.BaseType?.FullName?.Contains("System.ComponentModel.License") == true)
                {
                    MethodInfo[] arrMInfo = ((System.Reflection.TypeInfo)type).DeclaredMethods
                        .Where(p=> p.ReturnType.FullName == "System.Boolean" &&
                        p.GetParameters().Count() == 0 && p.Attributes.HasFlag(MethodAttributes.Private)).ToArray();

                    foreach (MethodInfo methodInfo in arrMInfo)
                    {
                        if (isFound) break;

                        try
                        {
                            var methodBody = methodInfo.GetMethodBody();
                            if (methodBody.LocalVariables.Count(p=> p.LocalType.Name.Contains("BigInteger")) == 4 && 
                                methodBody.LocalVariables.Count() == 6)
                            {
                                MemoryPatching(methodInfo, myGetTrue);
                                nCount++;

                                isFound = true;
                                break;
                            }
                        }
                        catch
                        {
                            //throw new InvalidOperationException("MemoryPatching for \"" + assembly.FullName + "\" failed !");
                        }
                    }
                }
            }

        }


        private static bool GetTrue()
        {
            return true;
        }


        private static unsafe void MemoryPatching(MethodBase miEvaluation, MethodBase miLicensed)
        {
            IntPtr IntPtrEval = GetMemoryAddress(miEvaluation);
            IntPtr IntPtrLicensed = GetMemoryAddress(miLicensed);

            if (IntPtr.Size == 8)
                *((long*)IntPtrEval.ToPointer()) = *((long*)IntPtrLicensed.ToPointer());
            else
                *((int*)IntPtrEval.ToPointer()) = *((int*)IntPtrLicensed.ToPointer());
        }
        

        private static unsafe IntPtr GetMemoryAddress(MethodBase mb)
        {
            RuntimeHelpers.PrepareMethod(mb.MethodHandle);

            if ((Environment.Version.Major >= 4) || ((Environment.Version.Major == 2) && (Environment.Version.MinorRevision >= 3053)))
            {
                return new IntPtr(((int*)mb.MethodHandle.Value.ToPointer() + 2));
            }

            UInt64* location = (UInt64*)(mb.MethodHandle.Value.ToPointer());
            int index = (int)(((*location) >> 32) & 0xFF);
            if (IntPtr.Size == 8)
            {
                ulong* classStart = (ulong*)mb.DeclaringType.TypeHandle.Value.ToPointer();
                ulong* address = classStart + index + 10;
                return new IntPtr(address);
            }
            else
            {
                uint* classStart = (uint*)mb.DeclaringType.TypeHandle.Value.ToPointer();
                uint* address = classStart + index + 10;
                return new IntPtr(address);
            }
        }
    }
}



















































































































































//By irDevelopers.com
