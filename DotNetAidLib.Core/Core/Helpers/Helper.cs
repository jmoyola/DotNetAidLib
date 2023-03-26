using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using DotNetAidLib.Core.Develop;

namespace DotNetAidLib.Core.Helpers
{
    [Serializable]
    public class RetryTimesExpiredException : Exception
    {
        public RetryTimesExpiredException()
        {
        }

        public RetryTimesExpiredException(string message) : base(message)
        {
        }

        public RetryTimesExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RetryTimesExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public static class Helper
    {
        private static Assembly entryAssemblyInstance;

        public static IDictionary<K, V> EnumToDictionary<K, V>(Type enumType,
            Func<KeyValuePair<string, object>, KeyValuePair<K, V>> enumConvert)
        {
            Assert.NotNull(enumType, nameof(enumType));
            Assert.When(enumType, v => v.IsEnum, "Must be a enum type", nameof(enumType));
            Assert.NotNull(enumConvert, nameof(enumConvert));

            return Enum.GetNames(enumType).Select(n =>
                    enumConvert.Invoke(new KeyValuePair<string, object>(n, Enum.Parse(enumType, n))))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }


        public static R With<T, R>(T value, Func<T, R> withFunction)
        {
            return withFunction.Invoke(value);
        }

        public static R If<T, R>(T value, Func<T, bool> predicate, Func<T, R> ifFunction,
            Func<T, R> elseFunction = null)
        {
            var ret = default(R);
            if (predicate(value))
                ret = ifFunction(value);
            else if (elseFunction != null)
                ret = elseFunction(value);

            return ret;
        }

        public static T IfNull<T>(T value, T ifNullValue)
        {
            if ((object) value is null)
                return ifNullValue;
            return value;
        }

        public static R IfNull<T, R>(T value, Func<T, R> ifNullValue, Func<T, R> ifNotNullValue)
        {
            if ((object) value is null)
                return ifNullValue(value);
            return ifNotNullValue(value);
        }

        public static T IfNotNull<T>(T value, T ifNotNullValue)
        {
            if (!((object) value is null))
                return ifNotNullValue;
            return value;
        }

        public static bool IsValidPath(string path)
        {
            try
            {
                var p = new FileInfo(path);
                return true;
            }
            catch
            {
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
            return TryFunc(function, default);
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

        public static IEnumerable<T> GetEnumMembers<T>()
        {
            if (!typeof(T).IsEnum)
                throw new AssertException("T must be a enum type.");
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static IDictionary<T, int> GetEnumDictionaryMembers<T>()
        {
            if (!typeof(T).IsEnum)
                throw new AssertException("T must be a enum type.");
            return Enum.GetValues(typeof(T)).Cast<T>()
                .ToDictionary(v => v, v => (int) Enum.Parse(typeof(T), v.ToString()));
        }

        public static T Retry<T>(object instance, string methodName, int retryTimes, int retryDelay,
            params object[] parameters)
        {
            return (T) Retry(instance, methodName, retryTimes, retryDelay, parameters);
        }

        public static object Retry(object instance, string methodName, int retryTimes, int retryDelay,
            params object[] parameters)
        {
            Assert.NotNull(instance, nameof(instance));
            Assert.NotNullOrEmpty(methodName, nameof(methodName));
            Assert.GreaterThan(retryTimes, 0, nameof(retryTimes));
            Assert.GreaterThan(retryDelay, 0, nameof(retryDelay));

            while (true)
                try
                {
                    var t = instance.GetType();
                    var method = t.GetMethod(methodName);
                    if (method == null)
                        throw new HelperException("Can't fins method with name '" + methodName + " in type '" + t.Name +
                                                  "'.");
                    return method.Invoke(instance, parameters);
                }
                catch (Exception ex)
                {
                    retryTimes--;
                    if (retryTimes == 0)
                        throw new RetryTimesExpiredException(
                            "Retry times (" + retryTimes + ") expired invoking method '" + methodName + "'.", ex);
                    Thread.Sleep(retryDelay);
                }
        }

        public static void Retry(Action action, Action<Exception> onRetryError = null, int retryTimes = 3,
            int retryDelay = 1)
        {
            Assert.NotNull(action, nameof(action));
            Assert.GreaterThan(retryTimes, 0, nameof(retryTimes));
            Assert.GreaterThan(retryDelay, 0, nameof(retryDelay));


            while (true)
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
                        throw new RetryTimesExpiredException(
                            "Retry times (" + retryTimes + ") expired invoking method.", ex);
                    Thread.Sleep(retryDelay);
                }
        }

        public static R Retry<R>(Func<R> function, Action<Exception> onRetryError = null, int retryTimes = 3,
            int retryDelay = 1)
        {
            Assert.NotNull(function, nameof(function));
            Assert.GreaterThan(retryTimes, 0, nameof(retryTimes));
            Assert.GreaterThan(retryDelay, 0, nameof(retryDelay));

            while (true)
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
                        throw new RetryTimesExpiredException(
                            "Retry times (" + retryTimes + ") expired invoking method.", ex);
                    Thread.Sleep(retryDelay);
                }
        }

        public static object CreateGenericInstance(Type baseGenericType, Type typeArgument,
            params object[] constructorArguments)
        {
            return CreateGenericInstance(baseGenericType, new[] {typeArgument}, constructorArguments);
        }

        public static object CreateGenericInstance(Type baseGenericType, Type[] typeArguments,
            params object[] constructorArguments)
        {
            object ret = null;

            var genericType = baseGenericType.MakeGenericType(typeArguments);
            ret = Activator.CreateInstance(genericType, constructorArguments);

            return ret;
        }

        public static DirectoryInfo BaseDirectory()
        {
            DirectoryInfo baseDirectory = null;
            try
            {
                baseDirectory = ServiceBaseDirectory();
                if (!baseDirectory.Exists)
                    baseDirectory = BinBaseDirectory();
            }
            catch
            {
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
            cmdLine = Assembly.GetEntryAssembly().Location;

            var workDir = Path.GetDirectoryName(cmdLine);
            return new DirectoryInfo(workDir);
        }

        public static void SetEntryAssembly(Assembly assembly)
        {
            entryAssemblyInstance = assembly;
        }

        public static Assembly GetEntryAssembly()
        {
            if (entryAssemblyInstance == null)
                entryAssemblyInstance = Assembly.GetEntryAssembly();

            return entryAssemblyInstance;
        }

        public static void TryTimes(Action action, int tryTimes = 1, int retryDelay = 100)
        {
            Exception lastException = null;

            Assert.NotNull(action, nameof(action));
            Assert.GreaterThan(tryTimes, 0, nameof(tryTimes));
            Assert.GreaterOrEqualThan(retryDelay, 100, nameof(retryDelay));

            var tryCounter = 1;

            while (tryCounter <= tryTimes)
                try
                {
                    action.Invoke();
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    tryCounter++;
                    Thread.Sleep(retryDelay);
                }

            if (tryCounter > tryTimes)
                throw lastException;
        }

        public static T TryTimes<T>(Func<T> function, int tryTimes = 1, int retryDelay = 100)
        {
            var ret = default(T);
            Exception lastException = null;

            Assert.NotNull(function, nameof(function));
            Assert.GreaterThan(tryTimes, 0, nameof(tryTimes));
            Assert.GreaterOrEqualThan(retryDelay, 100, nameof(retryDelay));

            var tryCounter = 1;

            while (tryCounter <= tryTimes)
                try
                {
                    ret = function.Invoke();
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    tryCounter++;
                    Thread.Sleep(retryDelay);
                }

            if (tryCounter > tryTimes)
                throw lastException;
            return ret;
        }
    }
}