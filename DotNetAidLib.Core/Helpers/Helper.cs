using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using DotNetAidLib.Core.Collections;
using System.Globalization;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    [Serializable]
    public class RetryTimesExpiredException : Exception
    {
        public RetryTimesExpiredException(){}

        public RetryTimesExpiredException(string message) : base(message){}

        public RetryTimesExpiredException(string message, Exception innerException) : base(message, innerException){}

        protected RetryTimesExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public static class Helper
    {
        public static IDictionary<K, V> EnumToDictionary<K, V>(Type enumType, Func<KeyValuePair<String, Object>, KeyValuePair<K,V>> enumConvert)
        {
            Assert.NotNull(enumType,nameof(enumType));
            Assert.When(enumType, v=>v.IsEnum, "Must be a enum type", nameof(enumType));
            Assert.NotNull(enumConvert, nameof(enumConvert));
            
            return Enum.GetNames(enumType).Select(n =>
                    enumConvert.Invoke(new KeyValuePair<string, object>(n, Enum.Parse(enumType, n))))
                .ToDictionary(kv => kv.Key, kv=> kv.Value);
        }

        public static bool IsUserInteractive(){
            bool ret;

            if (IsMonoRuntime ()) {
                if (IsWindowsSO ())
                    ret = System.Environment.UserInteractive;
                else {
                    ret = Mono.Unix.Native.Syscall.isatty (0);
                }
            }else{
                ret = System.Environment.UserInteractive;
            }

            return ret;
        }

        public static R With<T, R>(T value, Func<T, R> withFunction){
            return withFunction.Invoke(value);
        }

        public static R If<T, R>(T value, Func<T, bool> predicate, Func<T, R> ifFunction, Func<T, R> elseFunction=null)
        {
            R ret=default(R);
            if (predicate(value))
                ret=ifFunction(value);
            else if(elseFunction!=null)
                ret=elseFunction(value);

            return ret;
        }

        public static T IfNull<T>(T value, T ifNullValue)
        {
            if ((object)value is null)
                return ifNullValue;
            else
                return value;
        }

        public static R IfNull<T, R>(T value, Func<T,R> ifNullValue, Func<T, R> ifNotNullValue)
        {
            if ((object)value is null)
                return ifNullValue(value);
            else
                return ifNotNullValue(value);
        }

        public static T IfNotNull<T>(T value, T ifNotNullValue)
        {
            if (!((object)value is null))
                return ifNotNullValue;
            else
                return value;
        }

        public static bool IsValidPath (String path) {
            try {
                System.IO.FileInfo p=new System.IO.FileInfo (path);
                return true;
            }
            catch {
                return false;
            }
        }

        public static bool TryAction(Action action)
        {
            Assert.NotNull(action, nameof(action));

            try
            {
                action.Invoke();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static R TryFunc<R>(Func<R> function)
        {
            return TryFunc(function, default(R));
        }

        public static R TryFunc<R>(Func<R> function, R returnIfError)
        {
            Assert.NotNull(function, nameof(function));

            try
            {
                return function.Invoke();
            }
            catch
            {
                return returnIfError;
            }
        }

        public static IEnumerable<T> GetEnumMembers<T>(){
            if (!typeof(T).IsEnum)
                throw new AssertException("T must be a enum type.");
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static IDictionary<T, int> GetEnumDictionaryMembers<T>()
        {
            if (!typeof(T).IsEnum)
                throw new AssertException("T must be a enum type.");
            return Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(v=>v, v=>(int)(Enum.Parse(typeof(T),v.ToString())));
        }

        public static T Retry<T>(Object instance, String methodName, int retryTimes, int retryDelay, params Object[] parameters) {
            return (T)Retry(instance, methodName, retryTimes, retryDelay, parameters);
        }

        public static Object Retry(Object instance, String methodName, int retryTimes, int retryDelay, params Object[] parameters)
        {
            Assert.NotNull(instance, nameof(instance));
            Assert.NotNullOrEmpty(methodName, nameof(methodName));
            Assert.GreaterThan(retryTimes, 0, nameof(retryTimes));
            Assert.GreaterThan(retryDelay, 0, nameof(retryDelay));

            while (true){
                try{
                    Type t = instance.GetType();
                    MethodInfo method = t.GetMethod(methodName);
                    if (method == null)
                        throw new HelperException("Can't fins method with name '" + methodName + " in type '" + t.Name + "'.");
                    return method.Invoke(instance, parameters);
                }
                catch (Exception ex){
                    retryTimes--;
                    if (retryTimes == 0)
                        throw new RetryTimesExpiredException("Retry times (" + retryTimes + ") expired invoking method '" + methodName + "'.", ex);
                    Thread.Sleep(retryDelay);
                }
            }
        }

        public static void Retry(Action action, Action<Exception> onRetryError=null, int retryTimes=3, int retryDelay=1)
        {
            Assert.NotNull(action, nameof(action));
            Assert.GreaterThan(retryTimes, 0, nameof(retryTimes));
            Assert.GreaterThan(retryDelay, 0, nameof(retryDelay));


            while (true)
            {
                try
                {
                    action.Invoke();
                    break;
                }
                catch (Exception ex)
                {
                    if (onRetryError != null)
                        onRetryError.Invoke(ex);

                    retryTimes--;
                    if (retryTimes == 0)
                        throw new RetryTimesExpiredException("Retry times (" + retryTimes + ") expired invoking method.", ex);
                    Thread.Sleep(retryDelay);
                }
            }
        }

        public static R Retry<R>(Func<R> function, Action<Exception> onRetryError = null, int retryTimes = 3, int retryDelay = 1)
        {
            Assert.NotNull(function, nameof(function));
            Assert.GreaterThan(retryTimes, 0, nameof(retryTimes));
            Assert.GreaterThan(retryDelay, 0, nameof(retryDelay));

            while (true)
            {
                try
                {
                    return function.Invoke();
                }
                catch (Exception ex)
                {
                    if (onRetryError != null)
                        onRetryError.Invoke(ex);

                    retryTimes--;
                    if (retryTimes == 0)
                        throw new RetryTimesExpiredException("Retry times (" + retryTimes + ") expired invoking method.", ex);
                    Thread.Sleep(retryDelay);
                }
            }
        }

        public static Object CreateGenericInstance(Type baseGenericType, Type typeArgument, params Object[] constructorArguments){
			return CreateGenericInstance (baseGenericType, new Type[]{typeArgument}, constructorArguments);
		}

		public static Object CreateGenericInstance(Type baseGenericType, Type[] typeArguments, params Object[] constructorArguments){
			Object ret = null;

			Type genericType = baseGenericType.MakeGenericType(typeArguments);
			ret = Activator.CreateInstance(genericType, constructorArguments);

			return ret;
		}

        public static DirectoryInfo BaseDirectory() {
			DirectoryInfo baseDirectory = null;
			try
			{
				baseDirectory = ServiceBaseDirectory();
				if (!baseDirectory.Exists)
					baseDirectory = BinBaseDirectory();

			}
			catch { 
				baseDirectory = BinBaseDirectory();
			}
			return baseDirectory;
		}

		public static DirectoryInfo BinBaseDirectory()
		{
            return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
		}

		public static DirectoryInfo ServiceBaseDirectory()
		{
            string cmdLine = null;

            //cmdLine=Environment.CommandLine.Remove(Environment.CommandLine.Length - 2, 2).Remove(0, 1);
            cmdLine = System.Reflection.Assembly.GetEntryAssembly().Location;

            string workDir = Path.GetDirectoryName(cmdLine);
			return new DirectoryInfo(workDir);
		}
        private static Assembly entryAssemblyInstance = null;
        public static void SetEntryAssembly(Assembly assembly){
            entryAssemblyInstance = assembly;
        }

        public static Assembly GetEntryAssembly()
        {
            if (entryAssemblyInstance == null)
                entryAssemblyInstance = Assembly.GetEntryAssembly();
            
            return entryAssemblyInstance;
        }

        public static bool IsCompiledInDebugMode(){
			bool ret=false;

            if (IsMonoRuntime())
            {

#if (DEBUG)
                ret = true;
#endif
            }
            else
                ret = IsCompiledInDebugMode(GetEntryAssembly());
			return ret;

			//Assembly entryAssembly = Helpers.Helper.GetEntryAssembly();
			//return IsCompiledInDebugMode (entryAssembly);
		}

		public static bool IsCompiledInDebugMode(Assembly assembly){
			bool ret = false;

			DebuggableAttribute debugAtt=assembly.GetCustomAttribute<DebuggableAttribute>();
			if (debugAtt != null)
				ret = debugAtt.IsJITOptimizerDisabled;

			return ret;
		}

		public static bool IsWindowsSO(){
			return Environment.OSVersion.ToString ().IndexOf ("win", StringComparison.InvariantCultureIgnoreCase) > -1;
		}

		public static bool IsMonoRuntime(){
			return Type.GetType ("Mono.Runtime") != null;
		}
        
        public static void TryTimes(Action action, int tryTimes = 1, int retryDelay=100) {
            Exception lastException = null;

            Assert.NotNull(action, nameof(action));
            Assert.GreaterThan(tryTimes, 0, nameof(tryTimes));
            Assert.GreaterOrEqualThan(retryDelay, 100, nameof(retryDelay));

            int tryCounter = 1;

            while (tryCounter <= tryTimes)
            {
                try
                {
                    action.Invoke();
                    break;
                }
                catch (Exception ex){
                    lastException = ex;
                    tryCounter++;
                    Thread.Sleep(retryDelay);
                }
            }

            if (tryCounter > tryTimes)
                throw lastException;
        }

        public static T TryTimes<T>(Func<T> function, int tryTimes = 1, int retryDelay = 100)
        {
            T ret = default(T);
            Exception lastException = null;

            Assert.NotNull(function, nameof(function));
            Assert.GreaterThan(tryTimes, 0, nameof(tryTimes));
            Assert.GreaterOrEqualThan(retryDelay, 100, nameof(retryDelay));

            int tryCounter = 1;

            while (tryCounter <= tryTimes)
            {
                try{
                    ret = function.Invoke();
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    tryCounter++;
                    Thread.Sleep(retryDelay);
                }
            }

            if (tryCounter > tryTimes)
                throw lastException;
            else
                return ret;
        }



    }
}

